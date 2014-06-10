using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;

namespace PhotoChat
{
    /// <summary>
    /// ストローク認識機能設定ウィンドウ
    /// </summary>
    public partial class RecognizerConfigWindow : Form
    {
        #region フィールド

        private StrokeRecognizer strokeRecognizer;
        private List<Bitmap> templateImages = new List<Bitmap>();
        private List<PointF> currentStroke = null;
        private List<PointF> roughStroke = null;
        private Pen currentPen;
        private Pen roughPen;
        private Brush foreBrush;
        private volatile bool isStroking = false;
        private string resultName = string.Empty;
        private Bitmap resultImage;
        private double resultScore;
        private TemplateLabel selectedLabel = null;

        #endregion


        #region コンストラクタ・タブ選択

        /// <summary>
        /// ストローク認識機能設定ウィンドウを作成する。
        /// </summary>
        /// <param name="strokeRecognizer">ストローク認識クラス</param>
        public RecognizerConfigWindow(StrokeRecognizer strokeRecognizer)
        {
            this.strokeRecognizer = strokeRecognizer;
            InitializeComponent();

            // 登録済みテンプレート画像の更新
            UpdateTemplateImagePanel();

            // ペン・ブラシの初期化
            currentPen = new Pen(Color.Red, 3);
            currentPen.StartCap = LineCap.Round;
            currentPen.EndCap = LineCap.Round;
            roughPen = new Pen(Color.Pink, 3);
            roughPen.StartCap = LineCap.Round;
            roughPen.EndCap = LineCap.Round;
            foreBrush = new SolidBrush(this.ForeColor);
        }

        /// <summary>
        /// タブ選択イベント
        /// </summary>
        private void myTabControl_Selected(object sender, TabControlEventArgs e)
        {
            // 共通の初期化
            currentStroke = null;
            roughStroke = null;
            resultName = string.Empty;

            // 選択されたタブに応じた初期化
            if (myTabControl.SelectedTab == testPage)
            {
                // 登録済みテンプレート画像の更新
                UpdateTemplateImagePanel();
            }
            else if (myTabControl.SelectedTab == addPage)
            {
                // 登録ボタンを無効化
                addInfoLabel.Text = string.Empty;
                addInfoLabel.Refresh();
                addButton.Enabled = false;
            }
            else if (myTabControl.SelectedTab == managePage)
            {
                // テンプレートリストの更新
                UpdateTemplateList();
                selectedLabel = null;
                deleteButton.Enabled = false;
            }
        }

        #endregion


        #region ストローク入力

        /// <summary>
        /// ストローク入力開始
        /// </summary>
        private void StrokeLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isStroking)
            {
                // 入力ストロークを初期化して開始点を追加
                isStroking = true;
                currentStroke = new List<PointF>();
                currentStroke.Add(e.Location);
                ((Control)sender).Refresh();
            }
        }

        /// <summary>
        /// ストローク入力中
        /// </summary>
        private void StrokeLabel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isStroking)
            {
                // 入力ストロークに点を追加
                currentStroke.Add(e.Location);
                ((Control)sender).Refresh();
            }
        }

        /// <summary>
        /// ストローク入力終了
        /// </summary>
        private void StrokeLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isStroking)
            {
                // ストローク入力終了時の処理
                if (myTabControl.SelectedTab == testPage)
                {
                    // 認識テストの場合は認識実行
                    Recognize();
                }
                else if (myTabControl.SelectedTab == addPage)
                {
                    // テンプレート追加の場合は登録準備ができていればプレビュー表示
                    if (nameTextBox.Text.Length != 0)
                        UpdateNewTemplate();
                }
                ((Control)sender).Refresh();
                isStroking = false;
            }
        }

        /// <summary>
        /// 入力ストロークの描画
        /// </summary>
        private void StrokeLabel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // 下書きストロークの描画
                if (roughStroke != null)
                {
                    for (int i = 1; i < roughStroke.Count; i++)
                        e.Graphics.DrawLine(roughPen, roughStroke[i - 1], roughStroke[i]);
                }

                // 入力ストロークの描画
                if (currentStroke != null)
                {
                    for (int i = 1; i < currentStroke.Count; i++)
                        e.Graphics.DrawLine(currentPen, currentStroke[i - 1], currentStroke[i]);
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region 認識テスト

        /// <summary>
        /// 登録済みテンプレート画像を更新する。
        /// </summary>
        private void UpdateTemplateImagePanel()
        {
            // テンプレート画像のリストを更新（最大９枚）
            templateImages.Clear();
            StrokeRecognizer.Template[] templates = strokeRecognizer.Templates;
            for (int i = 0; i < Math.Min(templates.Length, 9); i++)
            {
                string filePath = Path.Combine(
                    PhotoChat.StrokeRecognizerDirectory, templates[i].Name + ".jpg");
                if (File.Exists(filePath))
                    templateImages.Add(new Bitmap(filePath));
            }
            templateImagePanel.Invalidate();
        }

        /// <summary>
        /// 登録済みテンプレート画像を描画する。
        /// </summary>
        private void templateImagePanel_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < templateImages.Count; i++)
                e.Graphics.DrawImage(templateImages[i], (i % 3) * 75, (i / 3) * 75);
        }

        /// <summary>
        /// 認識結果を描画する。
        /// </summary>
        private void resultLabel_Paint(object sender, PaintEventArgs e)
        {
            if (resultName != string.Empty)
            {
                e.Graphics.DrawImage(resultImage, 0, 0);
                e.Graphics.DrawString(
                    "テンプレート名：" + resultName, this.Font, foreBrush, 60, 7);
                e.Graphics.DrawString(
                    "スコア：" + resultScore.ToString(), this.Font, foreBrush, 60, 30);
            }
        }

        /// <summary>
        /// ストロークの認識テストを実行する。
        /// </summary>
        private void Recognize()
        {
            StrokeRecognizer.Result result =
                strokeRecognizer.Recognize(currentStroke.ToArray());

            // 認識結果の表示
            if (result.Name != string.Empty)
            {
                string filePath = Path.Combine(
                    PhotoChat.StrokeRecognizerDirectory, result.Name + ".jpg");
                if (File.Exists(filePath))
                {
                    resultName = result.Name;
                    resultImage = new Bitmap(filePath);
                    resultScore = result.Score;
                    resultLabel.Refresh();
                }
            }
        }

        #endregion


        #region テンプレート登録

        /// <summary>
        /// テンプレート追加 > テンプレート名入力ボックス
        /// </summary>
        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            // テンプレート名とストロークが入力されていればプレビュー表示
            if (nameTextBox.Text.Length == 0)
                addButton.Enabled = false;
            else if (currentStroke != null)
                UpdateNewTemplate();
        }

        /// <summary>
        /// テンプレート追加 > テンプレート登録ボタン
        /// </summary>
        private void addButton_Click(object sender, EventArgs e)
        {
            if (PhotoChat.ContainsInvalidChars(nameTextBox.Text))
            {
                MessageBox.Show(PhotoChat.InvalidCharsMessage);
            }
            else
            {
                addButton.Enabled = false;

                // テンプレート登録
                strokeRecognizer.AddTemplate(nameTextBox.Text, currentStroke.ToArray());
                string filePath = Path.Combine(
                    PhotoChat.StrokeRecognizerDirectory, resultName + ".jpg");
                resultImage.Save(filePath, ImageFormat.Jpeg);

                // 登録完了メッセージ
                addInfoLabel.Text = "登録しました";
                addInfoLabel.Refresh();

                // ストローク消去
                currentStroke = null;
                roughStroke = null;
                nameTextBox.Clear();
                newStrokeLabel.Refresh();
            }
        }

        /// <summary>
        /// テンプレート追加 > 下書きボタン
        /// </summary>
        private void roughButton_Click(object sender, EventArgs e)
        {
            // 入力したストロークを下書きとして表示し、入力ストロークは消去
            roughStroke = currentStroke;
            currentStroke = null;
            addButton.Enabled = false;
            newStrokeLabel.Refresh();
        }

        /// <summary>
        /// テンプレート追加 > 消去ボタン
        /// </summary>
        private void clearButton_Click(object sender, EventArgs e)
        {
            // 下書き・入力ストロークともに消去
            currentStroke = null;
            roughStroke = null;
            addButton.Enabled = false;
            newStrokeLabel.Refresh();
        }

        /// <summary>
        /// 登録するテンプレートのプレビューを更新する。
        /// </summary>
        private void UpdateNewTemplate()
        {
            // 登録するテンプレートのプレビュー作成・表示
            resultName = nameTextBox.Text;
            PointF[] points = strokeRecognizer.GetPointsForImage(currentStroke.ToArray());
            int size = (int)StrokeRecognizer.SquareSize;
            resultImage = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(resultImage))
            {
                g.Clear(Color.White);
                for (int i = 1; i < points.Length; i++)
                    g.DrawLine(currentPen, points[i - 1], points[i]);
            }
            newTemplateLabel.Refresh();

            // 登録ボタンを有効化
            addInfoLabel.Text = string.Empty;
            addInfoLabel.Refresh();
            addButton.Enabled = true;
        }

        /// <summary>
        /// 登録するテンプレートのプレビューを表示する。
        /// </summary>
        private void newTemplateLabel_Paint(object sender, PaintEventArgs e)
        {
            if (resultName != string.Empty)
            {
                e.Graphics.DrawImage(resultImage, 0, 0);
                e.Graphics.DrawString(
                    "テンプレート名：" + resultName, this.Font, foreBrush, 60, 10);
            }
        }

        #endregion


        #region テンプレート整理

        /// <summary>
        /// テンプレートリストを更新する。
        /// </summary>
        private void UpdateTemplateList()
        {
            // リスト更新
            templatePanel.Controls.Clear();
            StrokeRecognizer.Template[] templates = strokeRecognizer.Templates;
            for (int i = 0; i < templates.Length; i++)
            {
                TemplateLabel label = new TemplateLabel(templates[i].Name);
                label.Click += new EventHandler(templateLabel_Click);
                templatePanel.Controls.Add(label);
            }
        }

        /// <summary>
        /// テンプレートの画像と名前を表示するラベル
        /// </summary>
        private class TemplateLabel : Label
        {
            private Bitmap image;
            private Brush brush;
            private bool selected;

            public TemplateLabel(string name)
            {
                this.Size = new Size(270, 56);
                this.Name = name;
                string filePath = Path.Combine(PhotoChat.StrokeRecognizerDirectory, name + ".jpg");
                if (File.Exists(filePath))
                    this.image = new Bitmap(filePath);
                this.brush = new SolidBrush(Color.Navy);
            }

            /// <summary>
            /// ラベルの描画
            /// </summary>
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (selected)
                    e.Graphics.Clear(Color.PowderBlue);
                if (image != null)
                    e.Graphics.DrawImage(this.image, 0, 3);
                e.Graphics.DrawString(this.Name, this.Font, brush, 60, 13);
            }

            /// <summary>
            /// ラベルが選択されているかどうかを設定する。
            /// </summary>
            public bool Selected
            {
                get { return selected; }
                set { selected = value; }
            }
        }

        /// <summary>
        /// ラベルの選択
        /// </summary>
        private void templateLabel_Click(object sender, EventArgs e)
        {
            if (selectedLabel != null)
                selectedLabel.Selected = false;
            selectedLabel = (TemplateLabel)sender;
            selectedLabel.Selected = true;
            deleteButton.Enabled = true;
            templatePanel.Refresh();
        }

        /// <summary>
        /// テンプレート整理 > 削除ボタン
        /// </summary>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            // 選択されているテンプレートを削除
            deleteButton.Enabled = false;
            strokeRecognizer.RemoveTemplate(selectedLabel.Name);
            selectedLabel = null;

            // テンプレートリスト更新
            UpdateTemplateList();
        }

        #endregion
    }
}