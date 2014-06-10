using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace ConversationFieldDetector
{
    /// <summary>
    /// 音声処理を統括するクラス。
    /// </summary>
    public class SoundControl
    {
        #region 定数

        //Wavフォーマット
        public const int SamplesPerSec = 16000;
        public const int BitsPerSample = 16;
        public const int Channels = 2; //mono
        public const int BlockAlign = Channels * BitsPerSample / 8;
        public const int AvgBytesPerSec = SamplesPerSec * BlockAlign;

        /// <summary>
        /// 近接関係にあると判定する音声類似度の閾値
        /// </summary>
        public const double ProximityThreshold = 0.8;

        /// <summary>
        /// ログなどを保存するシステムディレクトリのパス
        /// </summary>
        public static string SystemDirectory;

        /// <summary>
        /// ログファイルのパス
        /// </summary>
        private static string LogFile;

        /// <summary>
        /// リングバッファ中でNotifyを行う回数
        /// </summary>
        private const int NotificationTimes = 3;

        #endregion

        #region フィールド・プロパティ
        private string id;

        public delegate void SendBackSignalDelegate(ConversationFieldDetector.ISendable soundData);
        private SendBackSignalDelegate sendBackDeligate;
        private AsyncCallback completeSendBack;

        private DataBuffer dataBuffer;
        private Thread recordingThread;
        private volatile bool isActive = false;

        // 有声・無声判定関連
        private double volumeAverage = 5000;
        private double denominator = 0;

        // 近接センサ
        private int sendInterval;
        private int proximityKeepCycle;
        private Dictionary<string, int> userDictionary = new Dictionary<string, int>();
        private List<string> nearUserList = new List<string>();

        // captureの位置
        private int captureBufferSize;
        private int nextCaptureOffset;
        // 一度に録音するデータサイズ
        private int notifySize;
        // Notifyイベント
        private Notify applicationNotify;
        // Notifyの位置
        private BufferPositionNotify[] positionNotify = new BufferPositionNotify[NotificationTimes];
        // Notifyを起こすイベント
        private AutoResetEvent notificationEvent;
        // 録音バッファ
        private CaptureBuffer applicationBuffer;
        // デバイス
        private Capture applicationDevice;
        // Wavのフォーマット
        private WaveFormat inputFormat;

        private static string soundDirectory;
        private static string errorFile;

        private static Dictionary<string, long> diffTimeDictonary;

        public static string SoundDirectory
        {
            get { return soundDirectory; }
            set { soundDirectory = value; }
        }

        /// <summary>
        /// 近接センシング用データの送信周期を取得する。
        /// </summary>
        public int SendInterval
        {
            get { return sendInterval; }
        }

        /// <summary>
        /// 一度の近接判定が影響を及ぼす周期数を取得する。
        /// </summary>
        public int ProximityKeepCycle
        {
            get { return proximityKeepCycle; }
        }

        /// <summary>
        /// 端末IDを取得する。
        /// </summary>
        public string ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// 近接関係にあるユーザの端末IDの配列を取得する。
        /// </summary>
        /// <returns>近接関係にある端末IDの配列</returns>
        public string[] GetNearUserID()
        {
            return nearUserList.ToArray();
        }

        /// <summary>
        /// SoundControlが起動中かどうかを取得する。
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
        }

        #endregion


        #region コンストラクタ、初期化・終了処理

        /// <summary>
        /// 音声処理管理部を作成する。
        /// </summary>
        /// <param name="id">この端末のID(どの端末が近くにいるのかの表示に利用)</param>
        /// <param name="sendBack">比較用データ取得deligate</param>
        /// <param name="sendInterval">近接センシング用データ送信周期(ミリ秒, 7000ms以下とする)</param>
        /// <param name="fftLength">比較をかけるサウンドデータのバイト数(32000/s)</param>
        /// <param name="proximityKeepCycle">一度のTrue判定が影響を及ぼす周期数</param>
        /// <param name="soundDirectory">音声ログ(16kbps, 16bit)保存のためのディレクトリ</param>
        /// <param name="errorFile">エラーログを書き出すファイル</param>
        public SoundControl(string id, SendBackSignalDelegate sendBack, int fftLength, int sendInterval, int proximityKeepCycle, string soundDirectory, string errorFile)
        {
            try
            {
                this.id = id;
                SystemDirectory = Path.Combine(soundDirectory, "system");
                LogFile = Path.Combine(SystemDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                diffTimeDictonary = new Dictionary<string, long>();

                sendBackDeligate = sendBack;
                completeSendBack = new AsyncCallback(CompleteSendBackMethod);
                SoundControl.soundDirectory = soundDirectory;
                SoundControl.errorFile = errorFile;

                if (sendInterval > 7000) sendInterval = 7000;
                CreateDirectory();
                this.sendInterval = sendInterval;
                this.proximityKeepCycle = proximityKeepCycle;
                dataBuffer = new DataBuffer(this, fftLength);


                //音量の設定
                int mixer;
                Mixer.init(out mixer); //Mixer初期化
                Mixer.SetMainVolume(mixer, 50); //Mainのボリュームを50%に設定
                Mixer.SetWaveOutVolume(mixer, 50); //WavOutを50%に設定
                Mixer.SetMicRecordVolume(mixer, 100); //マイク録音を100%に設定

                // デバイス初期化
                CaptureDevicesCollection devices = new CaptureDevicesCollection();
                applicationDevice = new Capture(devices[0].DriverGuid);
                if (applicationDevice != null)
                {
                    inputFormat = new WaveFormat();
                    inputFormat.BitsPerSample = BitsPerSample;
                    inputFormat.Channels = Channels;
                    inputFormat.SamplesPerSecond = SamplesPerSec;
                    inputFormat.FormatTag = WaveFormatTag.Pcm;
                    inputFormat.BlockAlign = BlockAlign;
                    inputFormat.AverageBytesPerSecond = AvgBytesPerSec;

                    recordingThread = new Thread(new ThreadStart(RecordWorker));
                    recordingThread.IsBackground = true;
                    CreateCaptureBuffer();
                    isActive = true;
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サウンドデータ保存用ディレクトリを作成する。
        /// </summary>
        private void CreateDirectory()
        {
            if (!Directory.Exists(soundDirectory))
                Directory.CreateDirectory(soundDirectory);
            if (!Directory.Exists(SystemDirectory))
                Directory.CreateDirectory(SystemDirectory);
        }


        /// <summary>
        /// 録音バッファを初期化する。
        /// </summary>
        private void CreateCaptureBuffer()
        {
            notificationEvent = new AutoResetEvent(false);
            notifySize = DataBuffer.PacketSize;
            notifySize -= notifySize % inputFormat.BlockAlign;
            captureBufferSize = notifySize * NotificationTimes;

            for (int i = 0; i < NotificationTimes; i++)
            {
                positionNotify[i].Offset = (notifySize * i) + notifySize - 1;
                positionNotify[i].EventNotifyHandle = notificationEvent.SafeWaitHandle.DangerousGetHandle();
            }

            CaptureBufferDescription captureDescription = new CaptureBufferDescription();
            captureDescription.BufferBytes = captureBufferSize;
            captureDescription.Format = inputFormat;

            applicationBuffer = new CaptureBuffer(captureDescription, applicationDevice);
            applicationNotify = new Notify(applicationBuffer);
            applicationNotify.SetNotificationPositions(positionNotify, NotificationTimes);
            nextCaptureOffset = 0;
        }


        /// <summary>
        /// 音声処理部を閉じる。
        /// </summary>
        public void Close()
        {
            if (!isActive) return;
            isActive = false;

            try
            {
                Stop();
                dataBuffer.Close();
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ユーザ追加

        /// <summary>
        /// ユーザを追加する。
        /// </summary>
        /// <param name="id">追加する端末ID</param>
        /// <param name="diffTime">端末間の時間差(相手の時計が進んでいる場合は正の値, 遅れていれば不の値のTicks)</param>
        public void AddUser(string id, long diffTime)
        {
            try
            {
                if (userDictionary.ContainsKey(id))
                {
                    userDictionary[id] = 0;
                }
                else
                {
                    userDictionary.Add(id, 0);
                }

                if (diffTimeDictonary.ContainsKey(id))
                {
                    diffTimeDictonary[id] = diffTime;
                }
                else
                {
                    diffTimeDictonary.Add(id, diffTime);
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// ユーザを削除する。
        /// </summary>
        /// <param name="id">削除する端末ID</param>
        public void RemoveUser(string id)
        {
            try
            {
                userDictionary.Remove(id);
                nearUserList.Remove(id);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 常時録音

        /// <summary>
        /// 常時録音を開始する。
        /// </summary>
        public void Start()
        {
            try
            {
                recordingThread.Start();
                applicationBuffer.Start(true);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 常時録音を終了する。
        /// </summary>
        public void Stop()
        {
            try
            {
                applicationBuffer.Stop();
                if (notificationEvent != null)
                    notificationEvent.Set();
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 常時音声録音に使う各値をリセットする。
        /// </summary>
        public void Reset()
        {
            dataBuffer.StopRecording();
            volumeAverage = 5000;
            denominator = 0;
        }


        /// <summary>
        /// 常時録音処理を行う。
        /// </summary>
        private void RecordWorker()
        {
            short[] buffer = new short[DataBuffer.PacketSize / sizeof(short)];
            while (isActive)
            {
                try
                {
                    // キャプチャバッファが埋まるまで待機
                    notificationEvent.WaitOne(Timeout.Infinite, true);

                    // キャプチャサイズを算出
                    int capturePosition, readPosition;
                    applicationBuffer.GetCurrentPosition(out capturePosition, out readPosition);
                    int lockSize = readPosition - nextCaptureOffset;
                    if (lockSize < 0)
                        lockSize += captureBufferSize;
                    lockSize -= lockSize % notifySize;
                    if (lockSize == 0)
                        continue;

                    // キャプチャデータ取得
                    byte[] captureData = (byte[])applicationBuffer.Read(
                        nextCaptureOffset, typeof(byte), LockFlag.None, lockSize);

                    // パケットに切り分けて処理
                    for (int i = 0; i < captureData.Length / DataBuffer.PacketSize; i++)
                    {
                        System.Buffer.BlockCopy(captureData,
                            DataBuffer.PacketSize * i, buffer, 0, DataBuffer.PacketSize);
                        dataBuffer.AddData(buffer, DetectTalking(buffer), DateTime.Now.Ticks);
                    }

                    // 次回のオフセットを計算
                    nextCaptureOffset += captureData.Length;
                    if (nextCaptureOffset >= captureBufferSize)
                        nextCaptureOffset -= captureBufferSize;
                }
                catch (Exception e)
                {
                    SoundControl.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// 音量平均と比較して有声区間かどうかを判定する。
        /// </summary>
        /// <param name="data">音声データ</param>
        /// <returns>有声区間であればtrue, 無声区間であればfalseを返す。</returns>
        private bool DetectTalking(short[] data)
        {
            bool result = false;
            short max = 0;

            // 音量の最大値を調べる
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == short.MinValue)
                    continue;
                short abs = Math.Abs(data[i]);
                if (max < abs) max = abs;
            }

            // 音量平均より大きな音があれば有声区間と判定
            if (max > volumeAverage * 1.5)
                result = true;

            // 音量平均の更新
            UpdateVolumeAverage(max);

            return result;
        }


        /// <summary>
        /// 音量平均を更新する。
        /// </summary>
        /// <param name="value">新たに取得された音量</param>
        private void UpdateVolumeAverage(double value)
        {
            denominator += 1;
            volumeAverage += (value - volumeAverage) / denominator;
        }

        #endregion


        #region ボイスメール

        /// <summary>
        /// ボイスメールの録音を開始する。
        /// </summary>
        public void StartVoiceMail()
        {
            dataBuffer.StartVoiceMail();
        }

        /// <summary>
        /// ボイスメールの録音を停止する。
        /// </summary>
        /// <returns>ボイスメールファイルのパス</returns>
        public string EndVoiceMail()
        {
            return dataBuffer.EndVoiceMail();
        }

        #endregion


        #region 近接センサ用データ送受信

        /// <summary>
        /// 近接センサ用音声データを送信する。
        /// </summary>
        /// <param name="soundData">送信する音声データ</param>
        public void SendAll(SoundData soundData)
        {
            try
            {
                sendBackDeligate.BeginInvoke((ConversationFieldDetector.ISendable)soundData, completeSendBack, null);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 受信した近接センサ用音声データを処理する。
        /// </summary>
        /// <param name="soundData">受信した音声データ</param>
        public void Receive(SoundData soundData)
        {
            try
            {
                if (!diffTimeDictonary.ContainsKey(soundData.ID)) return;

                soundData.Time -= diffTimeDictonary[soundData.ID];
                // コサイン類似度の計算
                double similarity = dataBuffer.Compair(soundData);

                if (similarity > ProximityThreshold)
                {
                    // 類似度が閾値以上なら近接ユーザリストに追加
                    userDictionary[soundData.ID] = proximityKeepCycle;
                    if (!nearUserList.Contains(soundData.ID))
                        nearUserList.Add(soundData.ID);
                }
                else
                {
                    int temp;
                    if (userDictionary.TryGetValue(soundData.ID, out temp))
                    {
                        if (temp > 0)
                        {
                            // 一定周期は近接判定を継続
                            userDictionary[soundData.ID] = temp - 1;
                            if (userDictionary[soundData.ID] == 0)
                                nearUserList.Remove(soundData.ID);
                        }
                    }
                }

                WriteLog(soundData.ID + " : " + similarity.ToString());
                WriteLogWindow(
                    "\r\n##Proximity##\r\n" + similarity.ToString() + " with " + soundData.ID);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region ログ

        /// <summary>
        /// ログファイルに追記する。
        /// </summary>
        /// <param name="log">追記するログ</param>
        public static void WriteLog(string log)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(LogFile))
                {
                    sw.WriteLine(log);
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ログウィンドウに出力する。
        /// </summary>
        /// <param name="log">出力するログ</param>
        public void WriteLogWindow(string log)
        {
            try
            {
                //client.Form.LogWindow.AddLine(log);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion

        public static void WriteErrorLog(string errorLog)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(errorFile, true, Encoding.UTF8))
                {
                    sw.WriteLine(errorLog);
                    sw.Flush();
                }
            }
            catch { }
        }

        /// <summary>
        /// IP追加が終了したときにコールバックされる
        /// </summary>
        /// <param name="asyncResult"></param>
        private void CompleteSendBackMethod(IAsyncResult asyncResult)
        {
            sendBackDeligate.EndInvoke(asyncResult);
        }
    }
}
