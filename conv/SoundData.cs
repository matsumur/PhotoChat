using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConversationFieldDetector
{
    /// <summary>
    /// 音声データ格納クラス
    /// </summary>
    public class SoundData:ConversationFieldDetector.ISendable
    {
        #region フィールド・プロパティ

        public const int TypeSoundData = 111;
        private long time;
        private double[] data;
        private string id;


        /// <summary>
        /// データタイプを取得する。
        /// </summary>
        public int Type
        {
            get { return TypeSoundData; }
        }

        /// <summary>
        /// データ時刻を取得する。
        /// </summary>
        public long Time
        {
            get { return time; }
            set { time = value; }
        }

        /// <summary>
        /// 音声データのバイト列を取得する。
        /// </summary>
        public double[] Data
        {
            get { return data; }
        }

        /// <summary>
        /// 端末IDを取得する。
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        #endregion


        #region コンストラクタ・インスタンス再構成

        /// <summary>
        /// 送信用音声データを作成する。
        /// </summary>
        /// <param name="time">データの録音開始時刻</param>
        /// <param name="data">音声データ</param>
        /// <param name="id">端末ID</param>
        public SoundData(long time, double[] data, string id)
        {
            this.time = time;
            this.id = id;
            this.data = new double[data.Length];
            data.CopyTo(this.data, 0);
        }


        /// <summary>
        /// バイト列からインスタンスを再構成する。
        /// </summary>
        /// <param name="dataBytes">音声データのバイト列</param>
        /// <returns>再構成したインスタンス</returns>
        public static SoundData CreateInstance(byte[] dataBytes)
        {
            try
            {
                // 時刻の読み取り
                long time = BitConverter.ToInt64(dataBytes, 0);
                int index = sizeof(long);

                // 端末IDの読み取り
                int idLength = BitConverter.ToInt32(dataBytes, index);
                index += sizeof(int);
                string id = Encoding.UTF8.GetString(dataBytes, index, idLength);
                index += idLength;

                // 音声データの読み取り
                int dataLength = (dataBytes.Length - index) / sizeof(double);
                double[] data = new double[dataLength];
                for (int i = 0; i < dataLength; i++)
                {
                    data[i] = BitConverter.ToDouble(dataBytes, index);
                    index += sizeof(double);
                }

                return new SoundData(time, data, id);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return null;
            }
        }

        #endregion


        /// <summary>
        /// 通信で送るデータに変換
        /// </summary>
        /// <returns>送信バイト列</returns>
        public byte[] GetDataBytes()
        {
            try
            {
                // 時刻と端末IDをバイト列に変換
                byte[] timeBytes = BitConverter.GetBytes(time);
                byte[] idBytes = Encoding.UTF8.GetBytes(id);
                byte[] idLengthBytes = BitConverter.GetBytes(idBytes.Length);

                // 全体のバイト列を作成
                int totalLength = timeBytes.Length + idLengthBytes.Length
                    + idBytes.Length + data.Length * sizeof(double);
                byte[] dataBytes = new byte[totalLength];

                // 時刻と端末IDをコピー
                int index = 0;
                timeBytes.CopyTo(dataBytes, index);
                index += timeBytes.Length;
                idLengthBytes.CopyTo(dataBytes, index);
                index += idLengthBytes.Length;
                idBytes.CopyTo(dataBytes, index);
                index += idBytes.Length;

                // 音声データをコピー
                for (int i = 0; i < data.Length; i++)
                {
                    BitConverter.GetBytes(data[i]).CopyTo(dataBytes, index);
                    index += sizeof(double);
                }

                return dataBytes;
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }
    }
}