using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// 共有ファイル転送クラス
    /// </summary>
    public class SharedFile : ISendable
    {
        #region フィールド･プロパティ

        public const int TypeSoundFile = 41;

        private int type;
        private string id;
        private long serialNumber;
        private string fileName;
        private byte[] data;

        /// <summary>
        /// タイプを取得する。
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        /// <summary>
        /// IDを取得する。
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// 通し番号を取得する。
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }

        /// <summary>
        /// ファイル名を取得する。
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// データを取得する。
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// 共有ファイル転送インスタンスを作成する。
        /// </summary>
        /// <param name="type">データタイプ</param>
        /// <param name="id">端末ID</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="data">データ</param>
        public SharedFile(int type, string id, long serialNumber, string fileName, byte[] data)
        {
            this.type = type;
            this.id = id;
            this.serialNumber = serialNumber;
            this.fileName = fileName;
            this.data = data;
        }


        /// <summary>
        /// ファイルからインスタンスを作成する。
        /// </summary>
        /// <param name="type">データタイプ</param>
        /// <param name="id">端末ID</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="filePath">ファイルパス</param>
        public SharedFile(int type, string id, long serialNumber, string filePath)
        {
            this.type = type;
            this.id = id;
            this.serialNumber = serialNumber;

            // ファイル読み込み
            FileInfo file = new FileInfo(filePath);
            this.fileName = file.Name;
            this.data = new byte[(int)file.Length];
            using (FileStream fs = file.OpenRead())
            {
                fs.Read(data, 0, data.Length);
            }
        }


        /// <summary>
        /// バイト列からインスタンスを作成する。
        /// </summary>
        /// <param name="type">データタイプ</param>
        /// <param name="dataBytes">データのバイト列</param>
        public SharedFile(int type, byte[] dataBytes)
        {
            this.type = type;

            // データ文字列
            int length = BitConverter.ToInt32(dataBytes, 0);
            byte[] temp = new byte[length];
            Array.Copy(dataBytes, 4, temp, 0, length);
            InterpretDataString(PhotoChat.DefaultEncoding.GetString(temp));

            // データ
            int index = 4 + length;
            length = dataBytes.Length - index;
            data = new byte[length];
            Array.Copy(dataBytes, index, data, 0, length);
        }

        #endregion




        #region 送信バイト列処理

        /// <summary>
        /// データの送信バイト列を返す。
        /// </summary>
        /// <returns>送信バイト列</returns>
        public byte[] GetDataBytes()
        {
            try
            {
                // データ文字列をバイト列に変換
                byte[] fileNameBytes = PhotoChat.DefaultEncoding.GetBytes(GetDataString());
                byte[] lengthBytes = BitConverter.GetBytes(fileNameBytes.Length);

                // 全体のバイト列を作成
                int index1 = lengthBytes.Length;
                int index2 = index1 + fileNameBytes.Length;
                byte[] dataBytes = new byte[index2 + data.Length];
                lengthBytes.CopyTo(dataBytes, 0);
                fileNameBytes.CopyTo(dataBytes, index1);
                data.CopyTo(dataBytes, index2);

                return dataBytes;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }


        /// <summary>
        /// データ文字列を返す。
        /// </summary>
        /// <returns>データ文字列</returns>
        private string GetDataString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append("ID=").Append(id).Append(PhotoChat.Delimiter);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("FileName=").Append(fileName);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// データ文字列を読み取り各値を設定する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        private void InterpretDataString(string dataString)
        {
            try
            {
                DataStringDictionary dsd = new DataStringDictionary(dataString);
                this.id = dsd["ID"];
                this.serialNumber = long.Parse(dsd["SerialNumber"]);
                this.fileName = dsd["FileName"];
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region 保存

        /// <summary>
        /// ファイルを保存する。
        /// </summary>
        /// <param name="directory">保存先ディレクトリのパス</param>
        public void Save(string directory)
        {
            try
            {
                string file = Path.Combine(directory, fileName);
                using (FileStream fs = File.Create(file))
                {
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ユーザIndex文字列を返す。
        /// </summary>
        /// <returns>ユーザIndex文字列</returns>
        public string GetIndexString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(100);
                sb.Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(fileName);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }

        #endregion
    }
}
