using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PhotoChat
{
    /// <summary>
    /// パレットツールウィンドウ
    /// </summary>
    public partial class PaletteWindow : Form
    {
        #region フィールド

        private PhotoChatForm form;
        private int colorIndex = 0;
        private int strokeSizeIndex = 1;
        private Pen pen = new Pen(Color.Black);
        private const int ColorCellSize = 20;  // 色の正方形のサイズ
        private static readonly Color SelectedRectColor = Color.DarkTurquoise;
        private static readonly Color[] DefaultColorList = new Color[]
            {
                Color.Red, Color.Magenta, Color.Pink,
                Color.Purple, Color.MediumOrchid, Color.Plum,
                Color.Navy, Color.Blue, Color.Cyan,
                Color.Teal, Color.SkyBlue, Color.PaleTurquoise,
                Color.Green, Color.Lime, Color.LightGreen,
                Color.Olive, Color.OliveDrab, Color.YellowGreen,
                Color.OrangeRed, Color.Orange, Color.Yellow,
                Color.SaddleBrown, Color.Peru, Color.Wheat,
                Color.Black, Color.DimGray, Color.Silver,
                Color.DarkSeaGreen, Color.Lavender, Color.White
            };
        private static readonly int[] StrokeSizeList = new int[] { 1, 3, 5, 10, 15 };
        private static readonly string[] FontSizeList =
            new string[] { "12", "15", "20", "25", "30", "40", "50" };

        #endregion




        #region コンストラクタ・デストラクタ

        /// <summary>
        /// パレットツールウィンドウを作成する。
        /// </summary>
        public PaletteWindow(PhotoChatForm form)
        {
            this.form = form;
            InitializeComponent();

            // 初期色をランダムに設定
            if (form.ReviewPanel != null)
            {
                Random random = new Random();
                colorIndex = random.Next(DefaultColorList.Length);
                form.ReviewPanel.NoteColor = DefaultColorList[colorIndex];
                form.PenColor = DefaultColorList[colorIndex];
            }

            // 色選択ラベルの初期化
            this.colorLabel.Image = CreateColorLabelImage();

            // 太さ選択ラベルの初期化
            this.strokeSizeLabel.Image = CreateStrokeSizeLabelImage();

            // 文字サイズボックスの初期化
            this.fontSizeComboBox.Items.AddRange(FontSizeList);
            this.fontSizeComboBox.Text = PhotoChat.DefaultTextFontSize.ToString();
        }


        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                this.pen.Dispose();

                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 色選択

        /// <summary>
        /// 色選択ラベルの背景イメージを作成する。
        /// </summary>
        /// <returns>作成したイメージ</returns>
        private Bitmap CreateColorLabelImage()
        {
            Bitmap image = new Bitmap(64, 211);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(this.BackColor);

                // 各色の正方形を描画
                Rectangle rect = new Rectangle(0, 0, 20, 20);
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    for (int i = 0; i < DefaultColorList.Length; i++)
                    {
                        brush.Color = DefaultColorList[i];
                        rect.X = ((i % 3) * (ColorCellSize + 1)) + 1;
                        rect.Y = ((i / 3) * (ColorCellSize + 1)) + 1;
                        g.FillRectangle(brush, rect);
                    }
                }

                // 初期色の選択枠を描画
                pen.Color = SelectedRectColor;
                rect.X = (colorIndex % 3) * (ColorCellSize + 1);
                rect.Y = (colorIndex / 3) * (ColorCellSize + 1);
                rect.Size = new Size(21, 21);
                g.DrawRectangle(pen, rect);
            }
            return image;
        }


        /// <summary>
        /// 色選択ラベルが押されたときの処理。
        /// </summary>
        private void colorLabel_MouseDown(object sender, MouseEventArgs e)
        {
            // 選択色の列と行を算出
            int column = (e.X - 1) / (ColorCellSize + 1);
            int row = (e.Y - 1) / (ColorCellSize + 1);

            using (Graphics g = Graphics.FromImage(colorLabel.Image))
            {
                // これまでの選択枠を消す
                pen.Color = this.BackColor;
                int x = (colorIndex % 3) * (ColorCellSize + 1);
                int y = (colorIndex / 3) * (ColorCellSize + 1);
                Rectangle rect = new Rectangle(x, y, 21, 21);
                g.DrawRectangle(pen, rect);

                // 選択枠の描画
                pen.Color = SelectedRectColor;
                rect.X = column * (ColorCellSize + 1);
                rect.Y = row * (ColorCellSize + 1);
                g.DrawRectangle(pen, rect);
            }

            // 色設定
            colorIndex = row * 3 + column;
            form.ReviewPanel.NoteColor = DefaultColorList[colorIndex];
            form.PenColor = DefaultColorList[colorIndex];
            form.SetPenMode();
            colorLabel.Invalidate();
        }

        #endregion




        #region 太さ選択

        /// <summary>
        /// 太さ選択ラベルの背景イメージを作成する。
        /// </summary>
        /// <returns>作成したイメージ</returns>
        private Bitmap CreateStrokeSizeLabelImage()
        {
            Bitmap image = new Bitmap(64, 100);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(this.BackColor);

                // 太さ選択項目の描画
                pen.Color = Color.MidnightBlue;
                for (int i = 0; i < StrokeSizeList.Length; i++)
                {
                    pen.Width = StrokeSizeList[i];
                    int y = (i * 19) + 11;
                    g.DrawLine(pen, 7, y, 57, y);
                }

                // 初期選択枠の描画
                pen.Color = SelectedRectColor;
                pen.Width = 1;
                g.DrawRectangle(pen, 4, strokeSizeIndex * 19 + 2, 55, 18);
            }
            return image;
        }


        /// <summary>
        /// 太さ選択ラベルが押されたときの処理。
        /// </summary>
        private void strokeSizeLabel_MouseDown(object sender, MouseEventArgs e)
        {
            using (Graphics g = Graphics.FromImage(strokeSizeLabel.Image))
            {
                // これまでの選択枠を消す
                pen.Color = this.BackColor;
                g.DrawRectangle(pen, 4, strokeSizeIndex * 19 + 2, 55, 18);

                // 選択された太さの算出
                strokeSizeIndex = e.Y / 20;

                // 選択枠の描画
                pen.Color = SelectedRectColor;
                g.DrawRectangle(pen, 4, strokeSizeIndex * 19 + 2, 55, 18);
            }

            // 太さ設定
            form.ReviewPanel.StrokeSize = StrokeSizeList[strokeSizeIndex];
            form.SetPenMode();
            strokeSizeLabel.Invalidate();
        }

        #endregion




        #region 文字サイズ

        /// <summary>
        /// 文字サイズの設定に変更があったときの処理。
        /// </summary>
        private void fontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            form.ReviewPanel.TextFontSize = int.Parse(fontSizeComboBox.Text);
            form.SetPenMode();
        }

        #endregion




        #region キーイベント

        /// <summary>
        /// キー入力があったら親フォームのキーイベントを呼び出す。
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            form.DoKeyDown(e);
            base.OnKeyDown(e);
        }

        #endregion
    }
}