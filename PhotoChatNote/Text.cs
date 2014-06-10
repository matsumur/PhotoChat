using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// キーボード入力した書き込みを表す
    /// </summary>
    public class Text : PhotoChatNote
    {
        private SolidBrush brush;
        private Font font;
        private string bodyText;




        #region コンストラクタ・デストラクタ

        /// <summary>
        /// テキストインスタンスを作成する。
        /// </summary>
        /// <param name="author">タグ付けしたユーザ名</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="photoName">タグ付けした写真名</param>
        /// <param name="color">テキストの色</param>
        /// <param name="textFontSize">フォントサイズ</param>
        /// <param name="point">テキストの位置</param>
        /// <param name="bodyText">書き込んだ文字列</param>
        public Text(string author, long serialNumber, string photoName,
            Color color, float textFontSize, Point point, string bodyText)
        {
            this.type = TypeText;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.brush = new SolidBrush(color);
            this.font = new Font(PhotoChat.FontName, textFontSize, FontStyle.Bold);
            this.bodyText = bodyText;
            this.range = GetRange(point);
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// データ文字列からインスタンスを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        public Text(string dataString)
        {
            this.type = TypeText;
            InterpretDataString(dataString);
        }


        /// <summary>
        /// デストラクタ
        /// </summary>
        ~Text()
        {
            brush.Dispose();
            font.Dispose();
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
                sb.Append("Color=").Append(brush.Color.ToArgb()).Append(PhotoChat.Delimiter);
                sb.Append("Size=").Append(font.Size).Append(PhotoChat.Delimiter);
                sb.Append("Point=").Append(range.X).Append(
                    SubDelimiter).Append(range.Y).Append(PhotoChat.Delimiter);
                sb.Append("Body=").Append(bodyText.Replace(Environment.NewLine, "< BR >"));
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
                Color color = Color.FromArgb(int.Parse(dsd["Color"]));
                this.brush = new SolidBrush(color);
                int fontSize = int.Parse(dsd["Size"]);
                this.font = new Font(PhotoChat.FontName, fontSize, FontStyle.Bold);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);

                // テキストの位置の読み取り
                string str = dsd["Point"];
                int index = str.IndexOf(SubDelimiter);
                int x = int.Parse(str.Substring(0, index));
                int y = int.Parse(str.Substring(index + 1));
                Point point = new Point(x, y);

                // テキストの読み取り
                str = dsd["Body"];
                this.bodyText = str.Replace("< BR >", Environment.NewLine);

                // 書き込み領域を求める
                this.range = GetRange(point);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        /// <summary>
        /// 書き込み領域を計算する。
        /// </summary>
        /// <param name="point">テキストの左上の座標</param>
        /// <returns>書き込み領域</returns>
        private Rectangle GetRange(Point point)
        {
            try
            {
                Graphics g = Graphics.FromImage(new Bitmap(1, 1));
                SizeF size = g.MeasureString(bodyText, font);
                return new Rectangle(point, size.ToSize());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return Rectangle.Empty;
            }
        }


        /// <summary>
        /// テキストを移動する。
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
        /// テキストを描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public override void Paint(Graphics g)
        {
            try
            {
                g.DrawString(bodyText, font, brush, range.X, range.Y);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// テキストを拡大倍率に応じて描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public override void Paint(Graphics g, float scaleFactor)
        {
            try
            {
                g.DrawString(
                    bodyText,
                    new Font(font.Name, font.Size * scaleFactor, FontStyle.Bold),
                    brush,
                    range.X * scaleFactor,
                    range.Y * scaleFactor);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
