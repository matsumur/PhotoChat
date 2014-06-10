using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// ハイパーリンク書き込みを表す
    /// </summary>
    public class Hyperlink : PhotoChatNote
    {
        #region フィールド・プロパティ

        /// <summary>
        /// サムネイルの外枠を描くPen
        /// </summary>
        private static readonly Pen outlinePen = new Pen(Color.Blue);
        private string linkedPhotoName;
        private Bitmap thumbnailImage;

        /// <summary>
        /// 貼り付けられたサムネイルの写真名
        /// </summary>
        public string LinkedPhotoName
        {
            get { return linkedPhotoName; }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// ハイパーリンクを作成する。
        /// </summary>
        /// <param name="author">作成したユーザ名</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="point">ハイパーリンクの位置</param>
        /// <param name="photoName">ハイパーリンク元の写真名</param>
        /// <param name="linkedPhotoName">ハイパーリンク先の写真名</param>
        public Hyperlink(string author, long serialNumber,
            Point point, string photoName, string linkedPhotoName)
        {
            this.type = TypeHyperlink;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.linkedPhotoName = linkedPhotoName;
            this.range = new Rectangle(
                point.X, point.Y, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
            this.thumbnailImage = Thumbnail.GetImage(
                linkedPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// データ文字列からインスタンスを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        public Hyperlink(string dataString)
        {
            this.type = TypeHyperlink;
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
                sb.Append("Point=").Append(range.X).Append(
                    SubDelimiter).Append(range.Y).Append(PhotoChat.Delimiter);
                sb.Append("PhotoName=").Append(photoName).Append(PhotoChat.Delimiter);
                sb.Append("Link=").Append(linkedPhotoName);
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
                this.linkedPhotoName = dsd["Link"];
                this.thumbnailImage = Thumbnail.GetImage(
                    linkedPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);

                // 貼り付け位置の読み取り
                string str = dsd["Point"];
                int index = str.IndexOf(SubDelimiter);
                int x = int.Parse(str.Substring(0, index));
                int y = int.Parse(str.Substring(index + 1));
                this.range = new Rectangle(
                    x, y, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.ToString());
            }
        }

        #endregion




        /// <summary>
        /// 貼り付けたサムネイルを移動する。
        /// </summary>
        /// <param name="dx">右方向に移動する距離</param>
        /// <param name="dy">下方向に移動する距離</param>
        public override void Move(int dx, int dy)
        {
            range.X += dx;
            range.Y += dy;
        }




        #region 描画

        /// <summary>
        /// 貼り付けられたサムネイルを描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public override void Paint(Graphics g)
        {
            try
            {
                if (thumbnailImage == null)
                {
                    // サムネイル画像が取得できていないときは再読み込み
                    thumbnailImage = Thumbnail.GetImage(
                        linkedPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                    if (thumbnailImage != null)
                        g.DrawImage(thumbnailImage, range.X, range.Y);
                }
                else
                {
                    g.DrawImage(thumbnailImage, range.X, range.Y);
                }

                // 外枠描画
                g.DrawRectangle(outlinePen, range);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 貼り付けられたサムネイルを拡大倍率に応じて描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public override void Paint(Graphics g, float scaleFactor)
        {
            try
            {
                // 拡大
                float x = range.X * scaleFactor;
                float y = range.Y * scaleFactor;
                float width = range.Width * scaleFactor;
                float height = range.Height * scaleFactor;

                // サムネイル画像は取得できているときだけ描画
                if (thumbnailImage != null)
                    g.DrawImage(thumbnailImage, x, y, width, height);
                g.DrawRectangle(outlinePen, x, y, width, height);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
