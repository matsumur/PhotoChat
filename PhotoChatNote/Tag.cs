using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// タグ付け書き込みを表す
    /// </summary>
    public class Tag : PhotoChatNote
    {
        #region フィールド・プロパティ

        /// <summary>
        /// タグ文字列
        /// </summary>
        public string TagString
        {
            get { return tagString; }
        }
        private string tagString;

        /// <summary>
        /// 書き込みの範囲（常に0範囲）
        /// </summary>
        public override Rectangle Range
        {
            get { return new Rectangle(0, 0, 0, 0); }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// タグインスタンスを作成する。
        /// </summary>
        /// <param name="author">タグ付けしたユーザ名</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="photoName">タグ付けした写真名</param>
        /// <param name="tagString">タグ文字列</param>
        public Tag(string author, long serialNumber, string photoName, string tagString)
        {
            this.type = TypeTag;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.tagString = tagString;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// データ文字列からインスタンスを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        public Tag(string dataString)
        {
            this.type = TypeTag;
            InterpretDataString(dataString);
        }

        #endregion




        #region データ文字列処理

        /// <summary>
        /// 各値の情報をもつデータ文字列を返す。
        /// </summary>
        /// <returns>データ文字列</returns>
        public override string GetDataString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append("Author=").Append(author).Append(PhotoChat.Delimiter);
                sb.Append("ID=").Append(id).Append(PhotoChat.Delimiter);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("Ticks=").Append(date.Ticks).Append(PhotoChat.Delimiter);
                sb.Append("PhotoName=").Append(photoName).Append(PhotoChat.Delimiter);
                sb.Append("Tag=").Append(tagString);
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
                this.author = dsd["Author"];
                this.id = dsd["ID"];
                this.serialNumber = long.Parse(dsd["SerialNumber"]);
                this.date = new DateTime(long.Parse(dsd["Ticks"]));
                this.photoName = dsd["PhotoName"];
                this.tagString = dsd["Tag"];
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        /// <summary>
        /// 点が書き込みの範囲内にあるかどうかを判定する。
        /// </summary>
        /// <param name="point">点の座標</param>
        /// <returns>常にfalse</returns>
        public override bool Contains(Point point)
        {
            return false;
        }
    }
}
