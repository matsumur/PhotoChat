using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PhotoChat
{
    /// <summary>
    /// 画像データの抽象基本クラス
    /// </summary>
    public abstract class PhotoChatImage : IComparable, IComparable<PhotoChatImage>, ISendable
    {
        public const int TypePhoto = 11;


        #region フィールド・プロパティ

        protected int type;
        protected string author;
        protected string id;
        protected long serialNumber;
        protected DateTime date;
        protected string photoName;
        protected string infoText;
        protected Bitmap image;


        /// <summary>
        /// 画像データのタイプを取得する。
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        /// <summary>
        /// 撮影者名を取得する。
        /// </summary>
        public string Author
        {
            get { return author; }
            set {
                this.author = value;
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
        }

        /// <summary>
        /// 撮影した端末IDを取得する。
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
        /// 撮影時刻を取得・設定する。
        /// </summary>
        public DateTime Date
        {
            get { return date; }
            set {
                this.date = value;
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
        }

        /// <summary>
        /// 写真名（端末ID_通し番号）を取得する。
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }

        /// <summary>
        /// 画像の簡易情報文字列を取得する。
        /// </summary>
        public string InfoText
        {
            get { return infoText; }
        }

        /// <summary>
        /// 画像を取得する。
        /// </summary>
        public Bitmap Image
        {
            get { return image; }
        }

        #endregion




        #region デストラクタ

        /// <summary>
        /// リソースを解放する。
        /// </summary>
        public virtual void Dispose()
        {
            try
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region Set/Get

        /// <summary>
        /// ID・通し番号からファイル名を設定する。
        /// </summary>
        protected void SetPhotoName()
        {
            try
            {
                StringBuilder sb = new StringBuilder(id, 50);
                sb.Append('_').Append(serialNumber);
                photoName = sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 各値の情報をもつデータ文字列を返す。
        /// </summary>
        /// <returns>データ文字列</returns>
        public abstract string GetDataString();


        /// <summary>
        /// データのバイト列（データ文字列と画像データの連結）を返す。
        /// データ文字列の長さを表す4バイト＋データ文字列＋画像データ
        /// </summary>
        /// <returns>データのバイト列</returns>
        public abstract byte[] GetDataBytes();

        #endregion




        #region staticメソッド

        /// <summary>
        /// データのバイト列から画像データのインスタンスを作成する。
        /// データが不適切なものであった場合はnullを返す。
        /// </summary>
        /// <param name="type">画像データのタイプ</param>
        /// <param name="dataBytes">データのバイト列</param>
        /// <returns>作成したインスタンス。作成できなかったときはnullを返す。</returns>
        public static PhotoChatImage CreateInstance(int type, byte[] dataBytes)
        {
            try
            {
                string dataString;
                byte[] imageBytes;
                InterpretDataBytes(dataBytes, out dataString, out imageBytes);

                PhotoChatImage image = null;
                switch (type)
                {
                    case TypePhoto:
                        image = new Photo(dataString, imageBytes);
                        break;
                }
                return image;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// データのバイト列をデータ文字列と画像のバイナリに分離する。
        /// </summary>
        /// <param name="dataBytes">データのバイト列</param>
        /// <param name="dataString">分離したデータ文字列を格納するstring</param>
        /// <param name="imageBytes">分離した画像のバイナリを格納するbyte[]</param>
        private static void InterpretDataBytes(
            byte[] dataBytes, out string dataString, out byte[] imageBytes)
        {
            try
            {
                // データ文字列のコピー
                int length = BitConverter.ToInt32(dataBytes, 0);
                dataString = PhotoChat.DefaultEncoding.GetString(dataBytes, sizeof(int), length);

                // 画像データのコピー
                int index = sizeof(int) + length;
                length = dataBytes.Length - index;
                imageBytes = new byte[length];
                Array.Copy(dataBytes, index, imageBytes, 0, length);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                dataString = string.Empty;
                imageBytes = new byte[0];
            }
        }


        /// <summary>
        /// 範囲内に収まるように縮小した画像を作成する。
        /// もともと範囲内に収まっていても新たなインスタンスを作成する。
        /// </summary>
        /// <param name="original">縮小する画像</param>
        /// <param name="maxWidth">幅の上限</param>
        /// <param name="maxHeight">高さの上限</param>
        /// <returns>縮小した画像</returns>
        public static Bitmap ResizeImage(Image original, int maxWidth, int maxHeight)
        {
            if (original.Width > maxWidth || original.Height > maxHeight)
            {
                try
                {
                    // 縦横比を維持した縮小サイズを計算
                    float widthScale = (float)maxWidth / (float)original.Width;
                    float heightScale = (float)maxHeight / (float)original.Height;
                    float scaleFactor = Math.Min(widthScale, heightScale);
                    int width = (int)(original.Width * scaleFactor);
                    int height = (int)(original.Height * scaleFactor);

                    // 縮小画像の作成
                    return new Bitmap(original, width, height);
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
            return new Bitmap(original);
        }

        #endregion




        #region IComparable メンバ

        /// <summary>
        /// 現在のインスタンスを同じ型の別のオブジェクトと比較する。
        /// </summary>
        /// <param name="obj">このインスタンスと比較するオブジェクト</param>
        /// <returns>比較結果</returns>
        public int CompareTo(object obj)
        {
            // 型を調べる
            PhotoChatImage another = obj as PhotoChatImage;
            if (another == null)
            {
                throw new ArgumentException();
            }

            // 比較
            return CompareTo(another);
        }


        /// <summary>
        /// 現在のPhotoChatImageを別のPhotoChatImageと比較する。
        /// </summary>
        /// <param name="another">比較するPhotoChatImage</param>
        /// <returns>比較結果</returns>
        public int CompareTo(PhotoChatImage another)
        {
            int result = date.CompareTo(another.Date);
            if (result == 0)
                result = id.CompareTo(another.ID);
            return result;
        }

        #endregion
    }
}
