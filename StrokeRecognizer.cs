using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// ストローク認識機能を提供するクラス。
    /// Step1：認識対象の点列を等間隔な64個の点列に変換する。
    /// Step2：回転して始点と重心を結ぶ直線をそろえる。
    /// Step3：拡大縮小してサイズをそろえ、重心を原点とする座標系に変換する。
    /// Step4：テンプレートと各点を比較して類似度を計算する。
    /// </summary>
    public class StrokeRecognizer
    {
        #region フィールド・定数・プロパティ

        private List<Template> templateList = new List<Template>();
        private const char Comma = ',';
        private const char Space = ' ';


        /// <summary>
        /// リサンプルによって得られる点の数
        /// </summary>
        public const int ResampleLength = 64;

        /// <summary>
        /// サイズ変換によって大きさを合わせる正方形のサイズ
        /// </summary>
        public const float SquareSize = 50;

        /// <summary>
        /// 原点
        /// </summary>
        private static readonly PointF Origin = new PointF(0, 0);

        /// <summary>
        /// 黄金比
        /// </summary>
        private static readonly double GoldenRatio = 0.5 * (-1 + Math.Sqrt(5));

        /// <summary>
        /// 最良の角度を求めるときの初期上限角度（45度）
        /// </summary>
        private const double MaxAngle = Math.PI / 4;

        /// <summary>
        /// 最良の角度を求めるときの初期下限角度（-45度）
        /// </summary>
        private const double MinAngle = -(Math.PI / 4);

        /// <summary>
        /// 最良の角度を求めるときの終了判定の閾値角度（2度）
        /// </summary>
        private const double AngleThreshold = Math.PI * 2 / 180;

        /// <summary>
        /// Step4の比較結果の最大値
        /// </summary>
        private static readonly double MaxDistance =
            Math.Sqrt(SquareSize * SquareSize + SquareSize * SquareSize) / 2;


        /// <summary>
        /// テンプレートの配列を取得する。
        /// </summary>
        public Template[] Templates
        {
            get { return templateList.ToArray(); }
        }

        #endregion


        #region コンストラクタ・テンプレートファイル処理

        /// <summary>
        /// テンプレートを読み込みストローク認識の初期化を行う。
        /// </summary>
        public StrokeRecognizer()
        {
            LoadTemplateList();
        }


        /// <summary>
        /// テンプレートリストを読み込む。
        /// </summary>
        private void LoadTemplateList()
        {
            if (File.Exists(PhotoChat.StrokeTemplateFile))
            {
                using (StreamReader sr = new StreamReader(PhotoChat.StrokeTemplateFile))
                {
                    // テンプレート名、点列の順に読み取る
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string name = line;
                        PointF[] points = new PointF[ResampleLength];
                        int i = 0;
                        foreach (string str in sr.ReadLine().Split(
                            new Char[] { Space }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            // 各点の読み取り
                            int index = str.IndexOf(Comma);
                            float x = float.Parse(str.Substring(0, index));
                            float y = float.Parse(str.Substring(index + 1));
                            points[i++] = new PointF(x, y);
                        }

                        // テンプレートリストに追加
                        templateList.Add(new Template(name, points));
                    }
                }
            }
        }


        /// <summary>
        /// テンプレートリストを保存する。
        /// </summary>
        private void SaveTemplateList()
        {
            using (StreamWriter sw = new StreamWriter(PhotoChat.StrokeTemplateFile))
            {
                // テンプレート名、点列の順に各テンプレートを書き込む
                foreach (Template template in templateList)
                {
                    sw.WriteLine(template.Name);
                    foreach (PointF p in template.Points)
                        sw.Write(p.X.ToString() + Comma + p.Y.ToString() + Space);
                    sw.WriteLine();
                }
                sw.Flush();
            }
        }

        #endregion


        #region ストローク認識、テンプレート追加・取得・削除

        /// <summary>
        /// ストローク認識を行う。
        /// </summary>
        /// <param name="points">ストロークの点列</param>
        /// <returns>認識結果（テンプレート名とスコア）</returns>
        public Result Recognize(PointF[] points)
        {
            // 点列の長さが２以上のときのみ認識処理
            if (points.Length < 2)
                return new Result(string.Empty, 0);

            // Step1：リサンプル
            PointF[] candidate = Resample(points, ResampleLength);

            // Step2：回転
            PointF centroid = Centroid(candidate);
            candidate = Rotate(candidate, centroid);

            // Step3：リサイズ・座標変換
            candidate = ScaleToSquare(candidate, SquareSize);
            centroid = Centroid(candidate);
            candidate = TranslateToOrigin(candidate, centroid);

            // Step4：テンプレートと比較
            double best = double.MaxValue;
            string name = string.Empty;
            foreach (Template template in templateList)
            {
                double distance = DistanceAtBestAngle(candidate, template.Points);
                if (best > distance)
                {
                    best = distance;
                    name = template.Name;
                }
            }
            return new Result(name, 1 - best / MaxDistance);
        }


        /// <summary>
        /// ストローク認識のテンプレートを追加する。
        /// </summary>
        /// <param name="name">追加するテンプレート名</param>
        /// <param name="points">テンプレートの点列</param>
        public void AddTemplate(string name, PointF[] points)
        {
            // Step1：リサンプル
            PointF[] template = Resample(points, ResampleLength);

            // Step2：回転
            PointF centroid = Centroid(template);
            template = Rotate(template, centroid);

            // Step3：リサイズ・座標変換
            template = ScaleToSquare(template, SquareSize);
            centroid = Centroid(template);
            template = TranslateToOrigin(template, centroid);

            // テンプレート追加
            templateList.Add(new Template(name, template));
            SaveTemplateList();
        }


        /// <summary>
        /// テンプレートリストから指定した名前と一致する最初に見つかったテンプレートを返す。
        /// </summary>
        /// <param name="name">テンプレート名</param>
        /// <returns>最初に見つかったテンプレート。見つからなかった場合はEmptyを返す。</returns>
        public Template GetTemplate(string name)
        {
            for (int i = 0; i < templateList.Count; i++)
            {
                if (templateList[i].Name == name)
                    return templateList[i];
            }
            return Template.Empty;
        }


        /// <summary>
        /// テンプレートリストから指定した名前のテンプレートを削除する。
        /// </summary>
        /// <param name="name">削除するテンプレート名</param>
        /// <returns>削除した場合はtrueを返す。</returns>
        public bool RemoveTemplate(string name)
        {
            for (int i = 0; i < templateList.Count; i++)
            {
                if (templateList[i].Name == name)
                {
                    templateList.RemoveAt(i);
                    SaveTemplateList();
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// テンプレートリストから指定した位置にあるテンプレートを削除する。
        /// </summary>
        /// <param name="index">削除するテンプレートのインデックス</param>
        public void RemoveTemplateAt(int index)
        {
            templateList.RemoveAt(index);
            SaveTemplateList();
        }


        /// <summary>
        /// リサンプルとリサイズを行った画像作成用ストローク点列を取得する。
        /// </summary>
        /// <param name="points">元のストローク点列</param>
        /// <returns>画像作成用のストローク点列</returns>
        public PointF[] GetPointsForImage(PointF[] points)
        {
            // リサンプル
            PointF[] newPoints = Resample(points, ResampleLength);

            // リサイズ
            newPoints = ScaleToSquare(newPoints, SquareSize);

            // 描画のための座標変換
            PointF centroid = Centroid(newPoints);
            centroid = new PointF(centroid.X - 25, centroid.Y - 25);
            return TranslateToOrigin(newPoints, centroid);
        }

        #endregion


        #region Step1：リサンプル

        /// <summary>
        /// 点列をnum個の等間隔な点列に変換する。
        /// </summary>
        /// <param name="points">元の点列</param>
        /// <param name="num">変換後の点の数</param>
        /// <returns>変換した点列</returns>
        private static PointF[] Resample(PointF[] points, int num)
        {
            // 新しい点列用配列の作成と各点の距離を計算
            PointF[] newPoints = new PointF[num];
            double space = PathLength(points) / (num - 1);

            // 点列の変換
            newPoints[0] = points[0];
            int index = 1, i = 1;
            double stock = 0;
            while (index <= num - 2)
            {
                // 距離の蓄積が変換後の点の距離を越えたら新たな点を求める
                double distance = Distance(points[i - 1], points[i]);
                if ((stock + distance) >= space)
                {
                    // 新たな点の作成
                    double scale = (space - stock) / distance;
                    double x = points[i - 1].X + ((points[i].X - points[i - 1].X) * scale);
                    double y = points[i - 1].Y + ((points[i].Y - points[i - 1].Y) * scale);
                    PointF point = new PointF((float)x, (float)y);

                    // 点の追加
                    newPoints[index++] = point;

                    // 次の点を求める準備
                    points[i - 1] = point;
                    stock = 0;
                }
                else
                {
                    // 変換後の点の距離を超えるまで距離を蓄積
                    stock += distance;
                    i++;
                }
            }
            newPoints[num - 1] = points[points.Length - 1];

            return newPoints;
        }


        /// <summary>
        /// 点列全体の長さを求める。
        /// </summary>
        /// <param name="points">点列</param>
        /// <returns>点列全体の長さ</returns>
        private static double PathLength(PointF[] points)
        {
            double length = 0;
            for (int i = 1; i < points.Length; i++)
                length += Distance(points[i - 1], points[i]);
            return length;
        }

        #endregion


        #region Step2：回転

        /// <summary>
        /// 始点と重心を結ぶ直線の角度が0度になるよう点列を回転する。
        /// </summary>
        /// <param name="points">点列</param>
        /// <param name="centroid">点列の重心</param>
        /// <returns>回転した点列</returns>
        private static PointF[] Rotate(PointF[] points, PointF centroid)
        {
            // 始点と重心を結ぶ直線の角度を求める
            double theta = Math.Atan2(points[0].Y - centroid.Y, points[0].X - centroid.X);

            // 角度が0度になるよう回転
            return RotateBy(points, centroid, -theta);
        }


        /// <summary>
        /// 点列を指定した角度だけ回転する。
        /// </summary>
        /// <param name="points">点列</param>
        /// <param name="center">回転の中心となる点</param>
        /// <param name="theta">回転する角度（ラジアン）</param>
        /// <returns>回転した点列</returns>
        private static PointF[] RotateBy(PointF[] points, PointF center, double theta)
        {
            PointF[] newPoints = new PointF[points.Length];
            double cos = Math.Cos(theta);
            double sin = Math.Sin(theta);
            for (int i = 0; i < points.Length; i++)
            {
                // 各点を中心点を軸に回転
                double dx = points[i].X - center.X;
                double dy = points[i].Y - center.Y;
                double x = dx * cos - dy * sin + center.X;
                double y = dx * sin + dy * cos + center.Y;
                newPoints[i] = new PointF((float)x, (float)y);
            }
            return newPoints;
        }

        #endregion


        #region Step3：リサイズ・座標変換

        /// <summary>
        /// 点列を指定したサイズの正方形に合うようリサイズする。
        /// </summary>
        /// <param name="points">点列</param>
        /// <param name="size">正方形のサイズ</param>
        /// <returns>リサイズした点列</returns>
        private static PointF[] ScaleToSquare(PointF[] points, float size)
        {
            // 拡大縮小倍率を求める
            SizeF boundingSize = BoundingSize(points);
            float scaleX = size / boundingSize.Width;
            float scaleY = size / boundingSize.Height;

            // リサイズ
            PointF[] newPoints = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                newPoints[i] = new PointF(points[i].X * scaleX, points[i].Y * scaleY);
            return newPoints;
        }


        /// <summary>
        /// 点列を囲む四角形のサイズを求める
        /// </summary>
        /// <param name="points">点列</param>
        /// <returns>点列を囲む四角形のサイズ</returns>
        private static SizeF BoundingSize(PointF[] points)
        {
            float minX = points[0].X, minY = points[0].Y;
            float maxX = minX, maxY = minY;
            for (int i = 1; i < points.Length; i++)
            {
                // X軸方向
                if (minX > points[i].X)
                    minX = points[i].X;
                else if (maxX < points[i].X)
                    maxX = points[i].X;

                // Y軸方向
                if (minY > points[i].Y)
                    minY = points[i].Y;
                else if (maxY < points[i].Y)
                    maxY = points[i].Y;
            }
            return new SizeF(maxX - minX, maxY - minY);
        }


        /// <summary>
        /// 点列をoriginを原点とする座標系に変換する。
        /// </summary>
        /// <param name="points">点列</param>
        /// <param name="origin">原点とする点</param>
        /// <returns>変換した点列</returns>
        private static PointF[] TranslateToOrigin(PointF[] points, PointF origin)
        {
            PointF[] newPoints = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                newPoints[i] = new PointF(points[i].X - origin.X, points[i].Y - origin.Y);
            return newPoints;
        }

        #endregion


        #region Step4：比較

        /// <summary>
        /// ２つの点列が最も一致する角度を黄金分割法(Golden Section Search)により探索し、
        /// その角度における各点のずれの大きさを返す。
        /// </summary>
        /// <param name="a">点列A</param>
        /// <param name="b">点列B</param>
        /// <returns>比較結果（小さいほど良い結果）</returns>
        private static double DistanceAtBestAngle(PointF[] a, PointF[] b)
        {
            // 初期化
            double floorAngle = MinAngle;
            double ceilingAngle = MaxAngle;
            double angle1 = GoldenRatio * floorAngle + (1 - GoldenRatio) * ceilingAngle;
            double distanceAtAngle1 = PathDistance(RotateBy(a, Origin, angle1), b);
            double angle2 = (1 - GoldenRatio) * floorAngle + GoldenRatio * ceilingAngle;
            double distanceAtAngle2 = PathDistance(RotateBy(a, Origin, angle2), b);

            // 黄金分割探索で最良の値を求める
            while ((ceilingAngle - floorAngle) > AngleThreshold)
            {
                if (distanceAtAngle1 < distanceAtAngle2)
                {
                    // 上限を移動
                    ceilingAngle = angle2;
                    angle2 = angle1;
                    distanceAtAngle2 = distanceAtAngle1;
                    angle1 = GoldenRatio * floorAngle + (1 - GoldenRatio) * ceilingAngle;
                    distanceAtAngle1 = PathDistance(RotateBy(a, Origin, angle1), b);
                }
                else
                {
                    // 下限を移動
                    floorAngle = angle1;
                    angle1 = angle2;
                    distanceAtAngle1 = distanceAtAngle2;
                    angle2 = (1 - GoldenRatio) * floorAngle + GoldenRatio * ceilingAngle;
                    distanceAtAngle2 = PathDistance(RotateBy(a, Origin, angle2), b);
                }
            }
            return Math.Min(distanceAtAngle1, distanceAtAngle2);
        }


        /// <summary>
        /// ２つの点列を比較しずれの大きさを求める。
        /// </summary>
        /// <param name="a">点列A</param>
        /// <param name="b">点列B</param>
        /// <returns>比較結果（大きいほどずれが大きい）</returns>
        private static double PathDistance(PointF[] a, PointF[] b)
        {
            double distance = 0;
            for (int i = 0; i < a.Length; i++)
                distance += Distance(a[i], b[i]);
            return distance / a.Length;
        }

        #endregion


        #region 距離・重心

        /// <summary>
        /// ２点間の距離を求める。
        /// </summary>
        /// <param name="p1">点１</param>
        /// <param name="p2">点２</param>
        /// <returns>２点間の距離</returns>
        private static double Distance(PointF p1, PointF p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }


        /// <summary>
        /// 点列の重心を求める。
        /// </summary>
        /// <param name="points">点列</param>
        /// <returns>重心</returns>
        private static PointF Centroid(PointF[] points)
        {
            float x = 0;
            float y = 0;
            for (int i = 0; i < points.Length; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            return new PointF(x / points.Length, y / points.Length);
        }

        #endregion


        #region 認識結果構造体

        /// <summary>
        /// ストローク認識の結果を格納する構造体
        /// </summary>
        public struct Result
        {
            private string name;
            private double score;

            /// <summary>
            /// 認識結果構造体を作成する。
            /// </summary>
            /// <param name="name">最もスコアの高かったテンプレートの名前</param>
            /// <param name="score">スコア</param>
            public Result(string name, double score)
            {
                this.name = name;
                this.score = score;
            }

            /// <summary>
            /// 最もスコアの高かったテンプレートの名前を取得する。
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// スコアを取得する。
            /// </summary>
            public double Score
            {
                get { return score; }
            }
        }

        #endregion


        #region テンプレート構造体

        public struct Template
        {
            public static readonly Template Empty = new Template(string.Empty, new PointF[0]);
            private string name;
            private PointF[] points;

            /// <summary>
            /// テンプレート構造体を作成する。
            /// </summary>
            /// <param name="name">テンプレート名</param>
            /// <param name="points">テンプレート点列</param>
            public Template(string name, PointF[] points)
            {
                this.name = name;
                this.points = points;
            }

            /// <summary>
            /// テンプレート名を取得する。
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// テンプレートの点列を取得する。
            /// </summary>
            public PointF[] Points
            {
                get { return points; }
            }
        }

        #endregion
    }
}
