using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace ConversationFieldDetector
{
    /// <summary>
    /// 音声録音バッファを管理するクラス
    /// </summary>
    public class DataBuffer
    {
        #region 定数

        /// <summary>
        /// 録音周期(ミリ秒)
        /// </summary>
        public const int RecInterval = 30;

        /// <summary>
        /// 録音パケットのデータ長(バイト単位)
        /// </summary>
        public const int PacketSize = SoundControl.AvgBytesPerSec * RecInterval / 1000;

        /// <summary>
        /// 録音パケットをリングバッファのパケットに分けるときの分割数
        /// </summary>
        public const int PacketDivisor = 2;

        /// <summary>
        /// リングバッファに入れるパケットのデータサイズ(short単位)
        /// </summary>
        public const int BufferPacketSize = (PacketSize / sizeof(short)) / PacketDivisor;

        /// <summary>
        /// リングバッファの長さ(パケット数)
        /// RingBufferLength > SilentCandidateLength でなければならない
        /// </summary>
        public const int RingBufferLength = 300 * PacketDivisor;

        /// <summary>
        /// 録音停止のための無音区間判定に用いるブロックの長さ
        /// </summary>
        public const int SilentCandidateLength = (3000 / RecInterval) * PacketDivisor;

        /// <summary>
        /// 録音を停止する無声フラグの数の閾値
        /// </summary>
        public const int RecStopThreshold = (int)(SilentCandidateLength * 0.9);

        /// <summary>
        /// 録音開始のための有声区間判定に用いるブロックの長さ
        /// </summary>
        public const int SpeechCandidateLength = 30;

        /// <summary>
        /// 録音を開始する有声フラグの数の閾値。
        /// </summary>
        public const int RecStartThreshold = (int)(SpeechCandidateLength * 0.8);

        /// <summary>
        /// 音声比較時に端末間の時刻のズレを吸収するためのマージン（パケット数）
        /// </summary>
        public const int ComparisonMargin = 10;

        /// <summary>
        /// 音声比較に用いる音声データが無音であると判定する閾値
        /// </summary>
        public const int SilentDataThreshold = 10;

        #endregion


        #region フィールド・プロパティ

        private SoundControl soundControl;
        private Codec.mp3_Encoder encoder;
        private string soundFile;
        private volatile bool voiceMailFlag = false;
        private volatile bool isRecording;

        // FFT
        private int fftInterval;
        private int fftLength;  // FFTに用いる配列の長さ
        private int fftPacketCount;  // FFTに用いるパケット数
        private int fftCount;

        // リングバッファ
        private short[] soundBuffer = new short[RingBufferLength * BufferPacketSize];
        private bool[] talkFlagBuffer = new bool[RingBufferLength];
        private long[] timeBuffer = new long[RingBufferLength];
        private int currentPosition;

        // 有声・無声フラグカウント
        private int silentCount;
        private int silentJudgeHead;
        private int speechCount;
        private int speechJudgeHead;

        #endregion


        #region コンストラクタ、終了処理

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="soundControl">サウンドデータをやりとりするためのSoundControlオブジェクト</param>
        /// <param name="fftLength">比較にかける長さ(32000/秒)</param>
        public DataBuffer(SoundControl soundControl, int fftLength)
        {
            this.soundControl = soundControl;
            encoder = new Codec.mp3_Encoder();
            isRecording = false;

            // FFT初期化
            fftInterval = soundControl.SendInterval / RecInterval * PacketDivisor;
            this.fftLength = 2 ^ ((int)(Math.Log(fftLength) / Math.Log(2)) + 1);
            fftPacketCount = fftLength / BufferPacketSize + 1;
            fftCount = 0;

            // リングバッファ初期化
            Array.Clear(soundBuffer, 0, soundBuffer.Length);
            Array.Clear(talkFlagBuffer, 0, talkFlagBuffer.Length);
            Array.Clear(timeBuffer, 0, timeBuffer.Length);
            currentPosition = 0;

            // 有声・無声フラグカウント初期化
            silentCount = SilentCandidateLength;
            silentJudgeHead = RingBufferLength - SilentCandidateLength;
            speechCount = 0;
            speechJudgeHead = RingBufferLength - SpeechCandidateLength;
        }


        /// <summary>
        /// プログラム終了処理
        /// </summary>
        public void Close()
        {
            try
            {
                if (isRecording)
                {
                    isRecording = false;
                    encoder.end();
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ユーティリティメソッド

        /// <summary>
        /// リングバッファの現在位置を移動する。
        /// </summary>
        /// <param name="origin">基点</param>
        /// <param name="offset">移動量（絶対値はRingBufferLength以下）</param>
        /// <returns>新しい位置</returns>
        private int SeekPosition(int origin, int offset)
        {
            int position = origin + offset;
            if (position >= RingBufferLength)
                position -= RingBufferLength;
            else if (position < 0)
                position += RingBufferLength;

            return position;
        }


        /// <summary>
        /// 端末間の時刻のズレを吸収するために比較開始位置を計算する。
        /// </summary>
        /// <param name="origin">基点</param>
        /// <param name="remoteTalkFlags">比較する端末の有声フラグ配列</param>
        /// <returns>比較開始位置</returns>
        private int GetCompairPosition(int origin, bool[] remoteTalkFlags)
        {
            int result = origin;
            int max = 0;

            for (int i = -ComparisonMargin; i < ComparisonMargin; i++)
            {
                int position = SeekPosition(origin, i);
                bool[] localTalkFlags = GetTalkFlags(position, fftPacketCount);
                int score = CompairBoolArray(remoteTalkFlags, localTalkFlags);
                if (score > max)
                {
                    result = position;
                    max = score;
                }
            }
            return result;
        }


        /// <summary>
        /// 送信データの開始位置を取得する。
        /// 有声フラグが最も多い区間を選択する。
        /// ただし、無声区間であると判定されれば-1を返す。
        /// </summary>
        /// <param name="position">最新バッファ位置</param>
        /// <returns>送信データの開始位置。または、無声区間と判定されれば-1を返す。</returns>
        private int GetSendDataPosition(int position)
        {
            // 最も古い区間で初期化
            int index = SeekPosition(position, -fftInterval + 1);
            int next = SeekPosition(index, fftPacketCount);
            int count = CountBool(GetTalkFlags(index, fftPacketCount));
            int max = count;
            int result = index;

            // 前回の送信以降で有声フラグの最も多い区間を探す
            for (int i = 0; i < fftInterval - fftPacketCount; i++)
            {
                // 1ブロックずらして有声フラグ数を更新
                if (talkFlagBuffer[index++])
                    count--;
                if (talkFlagBuffer[next++])
                    count++;

                // 有声フラグ数が最大であればresultを更新
                if (count > max)
                {
                    max = count;
                    result = index;
                }
                if (max == fftPacketCount) break;

                // リングバッファのつなぎ目処理
                if (index == RingBufferLength)
                    index = 0;
                if (next == RingBufferLength)
                    next = 0;
            }

            // 有声フラグ数が一定値より小さければ無声区間と判定
            if (max < SilentDataThreshold)
                return -1;
            else
                return result;
        }


        /// <summary>
        /// ２つのbool列を比較し一致する要素を数える。
        /// </summary>
        /// <param name="a">bool列A</param>
        /// <param name="b">bool列B</param>
        /// <returns>一致した要素数</returns>
        public static int CompairBoolArray(bool[] a, bool[] b)
        {
            int count = 0;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                if (a[i] == b[i]) count++;
            }
            return count;
        }


        /// <summary>
        /// bool配列の中のtrue要素の数をカウントする。
        /// </summary>
        /// <param name="boolArray">bool配列</param>
        /// <returns>true要素の数</returns>
        public static int CountBool(bool[] boolArray)
        {
            int count = 0;
            for (int i = 0; i < boolArray.Length; i++)
                if (boolArray[i]) count++;
            return count;
        }


        /// <summary>
        /// 現在時刻をyyyyMMdd_HHmmssフォーマットで文字列にして返す。
        /// </summary>
        /// <returns>現在時刻の文字列</returns>
        public static string GetNowString()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        #endregion


        #region ボイスメール

        /// <summary>
        /// ボイスメール録音を開始する。
        /// </summary>
        public void StartVoiceMail()
        {
            voiceMailFlag = true;
            StopRecording();
            StartRecording(currentPosition);
        }


        /// <summary>
        /// ボイスメール録音を停止する。
        /// </summary>
        /// <returns>ボイスメールファイル名</returns>
        public string EndVoiceMail()
        {
            voiceMailFlag = false;
            return StopRecording();
        }

        #endregion


        #region データの追加

        /// <summary>
        /// リングバッファにデータを追加する。
        /// </summary>
        /// <param name="sound">音声データ</param>
        /// <param name="talkFlag">有声区間かどうか（無音ならfalse）</param>
        /// <param name="time">音声データの時刻</param>
        public void AddData(short[] sound, bool talkFlag, long time)
        {
            try
            {
                sound.CopyTo(soundBuffer, currentPosition * BufferPacketSize);

                // 分割して処理
                for (int i = 0; i < PacketDivisor; i++)
                {
                    talkFlagBuffer[currentPosition] = talkFlag;
                    timeBuffer[currentPosition] = time + (RecInterval / PacketDivisor) * i;
                    ProcessData(currentPosition);
                    currentPosition++;
                    if (currentPosition == RingBufferLength)
                        currentPosition = 0;
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 追加された音声データを保存し、近接センサ用データを送信する。
        /// </summary>
        /// <param name="currentPosition">最新データのバッファ位置</param>
        private void ProcessData(int currentPosition)
        {
            // 一定時間ごとに近接センサ用データを送信
            if (++fftCount == fftInterval)
            {
                SendFftData(currentPosition);
                fftCount = 0;
            }

            // 有声・無声フラグカウント更新
            UpdateJudgeCount(currentPosition);

            // 音声記録
            if (isRecording)
            {
                // 録音停止判定
                if (silentCount > RecStopThreshold && !voiceMailFlag)
                    StopRecording();
                else
                    Record(currentPosition);
            }
            else
            {
                // 録音開始判定
                if (speechCount > RecStartThreshold)
                    StartRecording(currentPosition);
            }
        }


        /// <summary>
        /// 有声・無声フラグカウントを更新する。
        /// </summary>
        /// <param name="currentPosition">最新データのバッファ位置</param>
        private void UpdateJudgeCount(int currentPosition)
        {
            // 範囲外に出るフラグ分の計算
            if (!talkFlagBuffer[silentJudgeHead++])
                silentCount--;
            if (talkFlagBuffer[speechJudgeHead++])
                speechCount--;

            // 現在位置のフラグ分の計算
            if (talkFlagBuffer[currentPosition])
                speechCount++;
            else
                silentCount++;

            // リングバッファの境界に達した場合の処理
            if (silentJudgeHead == RingBufferLength)
                silentJudgeHead = 0;
            if (speechJudgeHead == RingBufferLength)
                speechJudgeHead = 0;
        }

        #endregion


        #region 音声記録

        /// <summary>
        /// 録音を開始する。
        /// </summary>
        /// <param name="position">リングバッファの先頭</param>
        private void StartRecording(int position)
        {
            try
            {
                if (!isRecording)
                {
                    isRecording = true;
                    soundFile = Path.Combine(SoundControl.SoundDirectory, GetNowString());
                    encoder.setFileName(soundFile);
                    encoder.init(SoundControl.SamplesPerSec, 32, 64);
                    Record(position);
                    soundControl.WriteLogWindow("RecStart\r\n");
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 録音を停止する。
        /// </summary>
        /// <returns>音声ファイル名</returns>
        public string StopRecording()
        {
            try
            {
                if (isRecording)
                {
                    isRecording = false;
                    encoder.end();
                    soundControl.WriteLogWindow("RecStop\r\n");
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
            return soundFile + ".mp3";
        }


        /// <summary>
        /// 音声データをファイルに出力する。
        /// </summary>
        /// <param name="position">出力するリングバッファの位置</param>
        private void Record(int position)
        {
            try
            {
                int index = SeekPosition(position, -SpeechCandidateLength);
                short[] buffer = GetData(index, 1);
                encoder.encode(buffer, BufferPacketSize);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region リングバッファからのデータ取得

        /// <summary>
        /// 音声リングバッファからのデータを取り出す。
        /// </summary>
        /// <param name="index">開始位置（BufferPacketSize単位）</param>
        /// <param name="length">取り出すデータの長さ（BufferPacketSize単位）</param>
        /// <returns>音声データ配列</returns>
        private short[] GetData(int index, int length)
        {
            try
            {
                short[] buffer = new short[length * BufferPacketSize];

                // バッファからコピー
                if ((index + length) > RingBufferLength)
                {
                    // リングバッファの境界をまたぐ場合
                    int formerLength = RingBufferLength - index;
                    int latterLength = length - formerLength;
                    Array.Copy(soundBuffer, index * BufferPacketSize,
                        buffer, 0, formerLength * BufferPacketSize);
                    Array.Copy(soundBuffer, 0,
                        buffer, formerLength * BufferPacketSize, latterLength * BufferPacketSize);
                }
                else
                {
                    // 境界をまたぐ処理が必要ない場合
                    Array.Copy(soundBuffer, index * BufferPacketSize,
                        buffer, 0, length * BufferPacketSize);
                }

                return buffer;
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return new short[0];
            }
        }


        /// <summary>
        /// 有声フラグリングバッファからデータを取り出す。
        /// </summary>
        /// <param name="index">開始位置</param>
        /// <param name="length">取り出す長さ</param>
        /// <returns>有声フラグ配列</returns>
        private bool[] GetTalkFlags(int index, int length)
        {
            try
            {
                bool[] talkFlags = new bool[length];

                // バッファからコピー
                for (int i = 0; i < length; i++)
                {
                    talkFlags[i] = talkFlagBuffer[index++];
                    if (index == RingBufferLength)
                        index = 0;
                }

                return talkFlags;
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return new bool[0];
            }
        }

        #endregion


        #region 近接センサ

        /// <summary>
        /// 近接センサ用音声データを比較する。
        /// </summary>
        /// <param name="soundData">受信した音声データ</param>
        /// <returns>比較結果（類似度）</returns>
        public double Compair(SoundData soundData)
        {
            try
            {
                if (soundData.Data.Length > 1)
                {
                    // データ時刻が最も近いブロックを探す
                    int index = SeekPosition(currentPosition, 100);
                    while (index != currentPosition)
                    {
                        // 簡単に相手のデータ時刻を越えた瞬間とする
                        if (timeBuffer[index] > soundData.Time)
                            break;
                        if (++index == RingBufferLength)
                            index = 0;
                    }

                    // 対応する自分の音声バッファが有声区間であるか確認
                    bool[] localTalkingFlag = GetTalkFlags(index, fftPacketCount);
                    if (CountBool(localTalkingFlag) > SilentDataThreshold)
                    {
                        // 音声データの比較
                        short[] data = GetData(index, fftPacketCount);
                        double[] localFrequency = UseFFT.GetPower(data);
                        return UseFFT.Compair(localFrequency, soundData.Data);
                    }
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
            return 0;
        }


        /// <summary>
        /// 近接センサ用音声データを送信する。
        /// </summary>
        /// <param name="position">最新バッファ位置</param>
        private void SendFftData(int position)
        {
            try
            {
                int index = GetSendDataPosition(position);
                if (index == -1)
                {
                    soundControl.SendAll(
                        new SoundData(timeBuffer[position], new double[0], soundControl.ID));
                }
                else
                {
                    double[] fftData = UseFFT.GetPower(GetData(index, fftPacketCount));
                    soundControl.SendAll(
                        new SoundData(timeBuffer[index], fftData, soundControl.ID));
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}