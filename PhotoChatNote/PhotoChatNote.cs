using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// 書き込みデータの抽象基本クラス
    /// </summary>
    public abstract class PhotoChatNote : IComparable, IComparable<PhotoChatNote>, ISendable
    {
        #region フィールド・プロパティ

        public const int TypeStroke = 21;
        public const int TypeText = 22;
        public const int TypeHyperlink = 23;
        public const int TypeRemoval = 24;
        public const int TypeTag = 25;
        public const int TypeSound = 26;
        protected const char SubDelimiter = ',';
        protected static Brush infoBrush = new SolidBrush(PhotoChat.InfoColor);

        protected int type;
        protected string author;
        protected string id;
        protected long serialNumber;
        protected DateTime date;
        protected string photoName;
        protected string infoText;
        protected Rectangle range;


        /// <summary>
        /// 書き込みのタイプ
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        /// <summary>
        /// 書き込みを行ったユーザ名
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
        /// 書き込みが行われた端末のID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// 通し番号
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }

        /// <summary>
        /// 書き込みが行われた時刻
        /// </summary>
        public DateTime Date
        {
            get { return date; }
            set
            {
                this.date = value;
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
        }

        /// <summary>
        /// 書き込みが行われた写真名
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }

        /// <summary>
        /// 書き込みに関する情報文字列
        /// </summary>
        public string InfoText
        {
            get { return infoText; }
        }

        /// <summary>
        /// 書き込みの範囲
        /// </summary>
        public virtual Rectangle Range
        {
            get { return range; }
        }

        #endregion




        #region Set/Get

        /// <summary>
        /// 各値の情報をもつデータ文字列を返す。
        /// </summary>
        /// <returns>データ文字列</returns>
        public abstract string GetDataString();


        /// <summary>
        /// 書き込みデータ保存用文字列を返す。
        /// </summary>
        /// <returns>書き込みデータ保存用文字列</returns>
        public string GetSaveString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(300);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(GetDataString());
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
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
                StringBuilder sb = new StringBuilder(300);
                sb.Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(GetDataString());
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// データのバイト列（データ文字列をバイト列に変換したもの）を返す。
        /// </summary>
        /// <returns>データのバイト列</returns>
        public byte[] GetDataBytes()
        {
            return PhotoChat.DefaultEncoding.GetBytes(GetDataString());
        }

        #endregion




        #region staticメソッド

        /// <summary>
        /// データのバイト列から書き込みデータのインスタンスを作成する。
        /// データが不適切なものであったときはnullを返す。
        /// </summary>
        /// <param name="type">書き込みのタイプ</param>
        /// <param name="dataBytes">データのバイト列</param>
        /// <returns>作成したインスタンス。作成できなかったときはnullを返す。</returns>
        public static PhotoChatNote CreateInstance(int type, byte[] dataBytes)
        {
            try
            {
                string dataString = PhotoChat.DefaultEncoding.GetString(dataBytes);
                return CreateInstance(type, dataString);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// データ文字列から書き込みデータのインスタンスを作成する。
        /// データが不適切なものであったときはnullを返す。
        /// </summary>
        /// <param name="type">書き込みのタイプ</param>
        /// <param name="dataString">データ文字列</param>
        /// <returns>作成したインスタンス。作成できなかったときはnullを返す。</returns>
        public static PhotoChatNote CreateInstance(int type, string dataString)
        {
            try
            {
                PhotoChatNote note = null;
                switch (type)
                {
                    case TypeStroke:
                        note = new Stroke(dataString);
                        break;

                    case TypeRemoval:
                        note = new Removal(dataString);
                        break;

                    case TypeHyperlink:
                        note = new Hyperlink(dataString);
                        break;

                    case TypeTag:
                        note = new Tag(dataString);
                        break;

                    case TypeText:
                        note = new Text(dataString);
                        break;

                    case TypeSound:
                        note = new Sound(dataString);
                        break;
                }
                return note;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 書き込みファイルのパスを返す。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>書き込みファイルのパス</returns>
        public static string GetNotesFilePath(string photoName)
        {
            return Path.Combine(PhotoChat.NotesDirectory, photoName + ".dat");
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
            PhotoChatNote another = obj as PhotoChatNote;
            if(another == null)
            {
                throw new ArgumentException();
            }

            // 比較
            return CompareTo(another);
        }


        /// <summary>
        /// 現在のPhotoChatNoteを別のPhotoChatNoteと比較する。
        /// </summary>
        /// <param name="another">比較するPhotoChatNote</param>
        /// <returns>比較結果</returns>
        public int CompareTo(PhotoChatNote another)
        {
            int result = date.CompareTo(another.Date);
            if (result == 0)
                result = id.CompareTo(another.ID);
            return result;
        }

        #endregion




        /// <summary>
        /// 書き込みデータをファイルに書き込む。
        /// </summary>
        public void Save()
        {
            try
            {
                string filePath = GetNotesFilePath(photoName);
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine(GetSaveString());
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 点が書き込みの範囲内にあるかどうかを判定する。
        /// </summary>
        /// <param name="point">点の座標</param>
        /// <returns>書き込みの範囲内にあればtrue</returns>
        public virtual bool Contains(Point point)
        {
            return range.Contains(point);
        }


        /// <summary>
        /// 書き込みを移動する。
        /// サブクラスで実装されない場合は何もしない。
        /// </summary>
        /// <param name="dx">右方向に移動する距離</param>
        /// <param name="dy">下方向に移動する距離</param>
        public virtual void Move(int dx, int dy) { }




        #region 描画

        /// <summary>
        /// 書き込みを描画する。
        /// サブクラスで実装されない場合は何もしない。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public virtual void Paint(Graphics g) { }


        /// <summary>
        /// 書き込みを拡大倍率に応じて描画する。
        /// サブクラスで実装されない場合は何もしない。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public virtual void Paint(Graphics g, float scaleFactor) { }


        /// <summary>
        /// 書き込みの輪郭を描画する。
        /// サブクラスで実装されない場合は何もしない。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public virtual void PaintOutline(Graphics g) { }


        /// <summary>
        /// 書き込みの輪郭を拡大倍率に応じて描画する。
        /// サブクラスで実装されない場合は何もしない。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public virtual void PaintOutline(Graphics g, float scaleFactor) { }


        /// <summary>
        /// 書き込みに関する情報文字列を描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public void PaintInfoText(Graphics g)
        {
            try
            {
                g.DrawString(infoText, PhotoChat.RegularFont, infoBrush,
                    range.X, range.Y + range.Height + PhotoChat.InfoFontSize);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
