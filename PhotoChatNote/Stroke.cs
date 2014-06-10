using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PhotoChat
{
    /// <summary>
    /// ストローク（手書き）書き込みを表す
    /// </summary>
    public class Stroke : PhotoChatNote
    {
        #region フィールド・プロパティ

        private const char Space = ' ';
        private const int OutlineWidth = 4;
        private static readonly Color OutlineColor = Color.White;
        private Color color;
        private int strokeWidth;
        private List<Point> points;
        private Pen mainPen;
        private Pen outlinePen;
        private int penWidthHalf;


        /// <summary>
        /// ストロークの点列を取得する。
        /// </summary>
        public Point[] Points
        {
            get { return points.ToArray(); }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// ストロークインスタンスを初期化し、ストロークを開始する。
        /// </summary>
        /// <param name="author">書き込みを行ったユーザ名</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="photoName">書き込み対象の写真名</param>
        /// <param name="color">ペンの色</param>
        /// <param name="strokeWidth">ペンの太さ</param>
        /// <param name="p">ストロークの開始点</param>
        public Stroke(string author, long serialNumber,
            string photoName, Color color, int strokeWidth, Point p)
        {
            this.type = TypeStroke;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.color = color;
            this.strokeWidth = strokeWidth;
            this.points = new List<Point>();
            points.Add(p);
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            SetPen();
            FigureRange();
        }


        /// <summary>
        /// データ文字列からインスタンスを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        public Stroke(string dataString)
        {
            this.type = TypeStroke;
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
                sb.Append("Color=").Append(color.ToArgb()).Append(PhotoChat.Delimiter);
                sb.Append("Width=").Append(strokeWidth).Append(PhotoChat.Delimiter);

                sb.Append("Points=");
                foreach (Point p in points)
                {
                    sb.Append(p.X).Append(SubDelimiter);
                    sb.Append(p.Y).Append(Space);
                }

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
                this.color = Color.FromArgb(int.Parse(dsd["Color"]));
                this.strokeWidth = int.Parse(dsd["Width"]);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
                SetPen();

                // 点素データの読み取り
                string pointData = dsd["Points"];
                this.points = new List<Point>();
                int index, x, y;
                foreach (string str in pointData.Split(
                    new Char[] { Space }, StringSplitOptions.RemoveEmptyEntries))
                {
                    index = str.IndexOf(SubDelimiter);
                    x = int.Parse(str.Substring(0, index));
                    y = int.Parse(str.Substring(index + 1));
                    points.Add(new Point(x, y));
                }

                // ストロークを固定し範囲を計算する
                Fix();
                FigureRange();
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region ペン設定

        /// <summary>
        /// 2つのペンを設定する。
        /// </summary>
        private void SetPen()
        {
            try
            {
                // 主線のペン
                this.mainPen = new Pen(color, strokeWidth);
                mainPen.StartCap = LineCap.Round;
                mainPen.EndCap = LineCap.Round;

                // アウトラインのペン
                this.outlinePen = new Pen(OutlineColor, strokeWidth + OutlineWidth);
                outlinePen.StartCap = LineCap.Round;
                outlinePen.EndCap = LineCap.Round;

                // ペン幅の半分の長さを記憶
                this.penWidthHalf = (strokeWidth + OutlineWidth) / 2;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ストローク処理

        /// <summary>
        /// ストロークの点を追加する。
        /// </summary>
        /// <param name="p">追加する点の座標</param>
        public void AddPoint(Point p)
        {
            try
            {
                points.Add(p);

                // 横方向に範囲を広げる
                if (p.X - penWidthHalf < range.Left)
                {
                    range.Width += range.X - p.X + penWidthHalf;
                    range.X = p.X - penWidthHalf;
                }
                else if (range.Right < p.X + penWidthHalf)
                {
                    range.Width = p.X + penWidthHalf - range.X;
                }

                // 縦方向に範囲を広げる
                if (p.Y - penWidthHalf < range.Top)
                {
                    range.Height += range.Y - p.Y + penWidthHalf;
                    range.Y = p.Y - penWidthHalf;
                }
                else if (range.Bottom < p.Y + penWidthHalf)
                {
                    range.Height = p.Y + penWidthHalf - range.Y;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークを固定する。
        /// </summary>
        public void Fix()
        {
            try
            {
                // 1点しかないときはもう1点追加
                if (points.Count == 1)
                {
                    points.Add(new Point(points[0].X + 1, points[0].Y));
                }

                // 範囲計算（AddPointで範囲計算するなら必要ない）
                //FigureRange();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークの範囲を計算する。
        /// </summary>
        private void FigureRange()
        {
            try
            {
                if (points.Count == 0)
                {
                    this.range = new Rectangle(0, 0, 0, 0);
                    return;
                }

                int minX = int.MaxValue;
                int minY = int.MaxValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;
                foreach (Point p in points)
                {
                    if (p.X - penWidthHalf < minX) minX = p.X - penWidthHalf;
                    if (p.Y - penWidthHalf < minY) minY = p.Y - penWidthHalf;
                    if (p.X + penWidthHalf > maxX) maxX = p.X + penWidthHalf;
                    if (p.Y + penWidthHalf > maxY) maxY = p.Y + penWidthHalf;
                }
                this.range = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストローク書き込みを移動する。
        /// </summary>
        /// <param name="dx">右方向に移動する距離</param>
        /// <param name="dy">下方向に移動する距離</param>
        public override void Move(int dx, int dy)
        {
            try
            {
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Point(points[i].X + dx, points[i].Y + dy);
                }
                range.X += dx;
                range.Y += dy;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 描画

        /// <summary>
        /// ストロークを描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public override void Paint(Graphics g)
        {
            try
            {
                for (int i = 0; i < points.Count - 1; i++)
                    g.DrawLine(mainPen, points[i], points[i + 1]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークを拡大倍率に応じて描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public override void Paint(Graphics g, float scaleFactor)
        {
            try
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    g.DrawLine(mainPen,
                               points[i].X * scaleFactor,
                               points[i].Y * scaleFactor,
                               points[i + 1].X * scaleFactor,
                               points[i + 1].Y * scaleFactor);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークの縁取りを描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public override void PaintOutline(Graphics g)
        {
            try
            {
                for (int i = 0; i < points.Count - 1; i++)
                    g.DrawLine(outlinePen, points[i], points[i + 1]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークの縁取りを拡大倍率に応じて描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public override void PaintOutline(Graphics g, float scaleFactor)
        {
            try
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    g.DrawLine(outlinePen,
                               points[i].X * scaleFactor,
                               points[i].Y * scaleFactor,
                               points[i + 1].X * scaleFactor,
                               points[i + 1].Y * scaleFactor);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
