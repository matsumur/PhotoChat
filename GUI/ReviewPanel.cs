using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// 写真閲覧・編集パネル
    /// </summary>
    public class ReviewPanel : Panel
    {
        #region フィールド・プロパティ

        private enum ReviewModes
        {
            Review, Stroke, Text, Hyperlink, Remove, Removing, Link, Replay, Live
        };
        private ReviewModes reviewMode;

        private static readonly Cursor penCursor = new Cursor(
            new System.IO.MemoryStream(Properties.Resources.PenCursor));
        private static readonly Cursor eraserCursor = new Cursor(
            new System.IO.MemoryStream(Properties.Resources.EraserCursor));
        private static readonly Cursor liveCursor = new Cursor(
            new System.IO.MemoryStream(Properties.Resources.LiveCursor));
        private static readonly Bitmap soundImage = Properties.Resources.sound;
        private static readonly Pen soundLabelPen = new Pen(Color.Navy, 5);
        private const string DatePattern = "yyyyMMdd_HHmm";
        private const string MP3Pattern = "??.mp3";
        private static readonly Bitmap micImage = Properties.Resources.mic;

        private PhotoChatForm form;
        private Photo currentPhoto;
        private PictureBox imageBox;
        private Panel tagPanel;
        private int tagCount;
        private Label soundLabel;
        private List<SoundItem> soundItemList;
        private Color noteColor = Color.Red;
        private int strokeSize = PhotoChat.DefaultStrokeSize;
        private float textFontSize = PhotoChat.DefaultTextFontSize;
        private Stroke currentStroke;
        private TextBox textBox;
        private string[] historyList;
        private int historyFrom, historyEnd, historyIndex;
        private bool historyJumpFlag = false;
        private PhotoChatNote pressedLink;
        private Point draggingPoint = Point.Empty;
        private Bitmap draggingImage;
        private string draggingPhotoName;
        private Point soundDraggingPoint = Point.Empty;
        private SoundItem soundDraggingItem;


        /// <summary>
        /// 表示中の写真
        /// </summary>
        public Photo CurrentPhoto
        {
            get { return currentPhoto; }
        }

        /// <summary>
        /// 書き込みの色
        /// </summary>
        public Color NoteColor
        {
            get { return noteColor; }
            set { noteColor = value; }
        }

        /// <summary>
        /// ストロークの太さ
        /// </summary>
        public int StrokeSize
        {
            get { return strokeSize; }
            set { strokeSize = value; }
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public float TextFontSize
        {
            get { return textFontSize; }
            set { textFontSize = value; }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// 写真閲覧・編集パネルを初期化する
        /// </summary>
        /// <param name="form">親フォーム</param>
        public ReviewPanel(PhotoChatForm form)
        {
            this.form = form;
            this.SuspendLayout();

            // GUI設定
            this.BackColor = Color.LightGreen;
            this.Dock = DockStyle.Fill;
            this.Name = "ReviewPanel";
            this.Font = PhotoChat.BoldFont;
            this.AutoScroll = true;
            this.AllowDrop = true;
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.LostFocus += new EventHandler(ReviewPanel_LostFocus);

            InitializeImageBox();
            InitializeTagArea();
            InitializeSoundArea();
            InitializeTextBox();
            this.ResumeLayout(false);

            // 閲覧履歴の初期化
            historyList = new String[PhotoChat.HistorySize];
            historyIndex = -1;
            historyFrom = -1;
            historyEnd = -1;
        }


        /// <summary>
        /// フォーカスが無くなったときの処理
        /// </summary>
        private void ReviewPanel_LostFocus(object sender, EventArgs e)
        {
            if (reviewMode == ReviewModes.Remove)
            {
                reviewMode = ReviewModes.Review;
                form.SetPenMode();
            }
        }

        #endregion




        #region 写真の変更

        /// <summary>
        /// 表示する写真を変更する。
        /// </summary>
        /// <param name="photo">表示する写真</param>
        public void SetPhoto(Photo photo)
        {
            try
            {
                // 同じ写真なら何もしない
                if (currentPhoto != null
                    && photo.PhotoName == currentPhoto.PhotoName) return;
                Photo temp = currentPhoto;
                currentPhoto = photo;
                if (temp != null) temp.Dispose();

                // 閲覧履歴更新
                UpdateHistory(photo.PhotoName);

                // 現在のモードに応じた初期化
                ResetReviewMode();

                // 書き込み再生モードであれば再生
                //if (form.IsReplayMode)
                //    new DrawingReplay().start();

                // 描画バッファ・タグ表示・音声表示を更新して描画
                UpdateImageBox();
                ResetTagLabel();
                ResetSoundLabel();
                Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 現在のモードに応じて初期化を行う。
        /// </summary>
        private void ResetReviewMode()
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Text:
                        // テキスト入力エリアのリセット
                        HideTextBox();
                        break;

                    case ReviewModes.Remove:
                        // 書き込みモードに戻す
                        form.SetPenMode();
                        break;
                }
                reviewMode = ReviewModes.Review;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 初期状態にする。
        /// </summary>
        public void Clear()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(Clear));
                    return;
                }

                // フィールド値の初期化
                currentPhoto = null;
                historyIndex = -1;
                historyFrom = -1;
                historyEnd = -1;
                form.LeftButtonEnabled = false;
                form.RightButtonEnabled = false;

                // 表示の初期化
                using (Graphics g = Graphics.FromImage(imageBox.Image))
                {
                    g.Clear(Color.LightCyan);
                    g.DrawString("No Photo", new Font(PhotoChat.FontName, 20, FontStyle.Bold),
                        new SolidBrush(Color.Navy), new PointF(250, 220));
                }
                tagCount = 0;
                tagPanel.Controls.Clear();
                soundItemList.Clear();
                this.Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 閲覧履歴

        /// <summary>
        /// 閲覧履歴を更新する。
        /// </summary>
        /// <param name="photoName">新しく表示する写真の名前</param>
        private void UpdateHistory(String photoName)
        {
            try
            {
                // 閲覧履歴による表示の場合は更新しない
                if (historyJumpFlag)
                {
                    historyJumpFlag = false;
                    return;
                }

                // 新しい履歴の追加
                if (++historyIndex == PhotoChat.HistorySize) historyIndex = 0;
                historyList[historyIndex] = photoName;

                // 履歴の終点を更新
                historyEnd = historyIndex;
                form.RightButtonEnabled = false;

                // 履歴の始点を更新
                if (historyFrom == -1)
                    historyFrom = 0;
                else
                {
                    form.LeftButtonEnabled = true;
                    if (historyFrom == historyIndex)
                    {
                        if (++historyFrom == PhotoChat.HistorySize)
                            historyFrom = 0;
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 履歴の1つ前の写真を表示する。
        /// </summary>
        public void Back()
        {
            try
            {
                // 閲覧履歴のインデックスを1つ前に戻す
                if (--historyIndex == -1) historyIndex = PhotoChat.HistorySize - 1;
                form.RightButtonEnabled = true;

                // 履歴の先頭まで戻ったときは戻るボタン無効
                if (historyIndex == historyFrom)
                    form.LeftButtonEnabled = false;

                // 写真表示
                historyJumpFlag = true;
                form.ShowPhoto(historyList[historyIndex]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 履歴の1つ先の写真を表示する。
        /// </summary>
        public void Forward()
        {
            try
            {
                // 閲覧履歴のインデックスを1つ先に進める
                if (++historyIndex == PhotoChat.HistorySize) historyIndex = 0;
                form.LeftButtonEnabled = true;

                // 履歴の最後まで進んだときは進むボタン無効
                if (historyIndex == historyEnd)
                    form.RightButtonEnabled = false;

                // 写真表示
                historyJumpFlag = true;
                form.ShowPhoto(historyList[historyIndex]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 外部からの更新呼び出し・画像取得・モード変更

        /// <summary>
        /// 表示中の写真と書き込みを更新する。
        /// </summary>
        public void UpdateImage()
        {
            try
            {
                // ストローク中・再生中でなければイメージバッファを更新して再描画
                if (reviewMode != ReviewModes.Stroke && reviewMode != ReviewModes.Replay)
                {
                    UpdateImageBox();
                    imageBox.Invalidate();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 現在表示されている画像をコピーして返す。
        /// </summary>
        /// <returns>現在表示されている画像</returns>
        public Bitmap GetCurrentImage()
        {
            try
            {
                lock (imageBox.Image)
                {
                    return new Bitmap(imageBox.Image);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// サムネイル用の写真部分切り取り画像を作成して返す。
        /// </summary>
        /// <returns>サムネイル用画像</returns>
        public Bitmap GetThumbnailImage()
        {
            try
            {
                lock (imageBox.Image)
                {
                    Rectangle srcRect = new Rectangle(PhotoChat.LeftMargin, PhotoChat.TopMargin,
                        currentPhoto.Image.Width, currentPhoto.Image.Height);
                    Bitmap image = new Bitmap(currentPhoto.Image.Width, currentPhoto.Image.Height);
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.DrawImage(imageBox.Image, 0, 0, srcRect, GraphicsUnit.Pixel);
                    }
                    return image;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 書き込み数の割合を指定して書き込み再生描画を行う。
        /// </summary>
        /// <param name="percentage">描画する書き込み数の割合（％）</param>
        public void Replay(int percentage)
        {
            try
            {
                reviewMode = ReviewModes.Replay;
                PaintReplayImage(percentage);
                imageBox.Invalidate();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込み再生を終了しスライダーをリセットする。
        /// </summary>
        public void QuitReplay()
        {
            try
            {
                form.ResetSliderValue();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 閲覧モードに設定する。
        /// </summary>
        public void SetReviewMode()
        {
            try
            {
                reviewMode = ReviewModes.Review;
                imageBox.Cursor = penCursor;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 削除モードに設定する。
        /// </summary>
        public void SetRemoveMode()
        {
            try
            {
                reviewMode = ReviewModes.Remove;
                imageBox.Cursor = eraserCursor;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ライブモードに設定する。
        /// </summary>
        public void SetLiveMode()
        {
            try
            {
                reviewMode = ReviewModes.Live;
                imageBox.Cursor = liveCursor;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 写真・書き込み表示エリア

        /// <summary>
        /// 写真・書き込み表示エリアを初期化する。
        /// </summary>
        private void InitializeImageBox()
        {
            try
            {
                imageBox = new PictureBox();
                imageBox.SuspendLayout();
                imageBox.Bounds = new Rectangle(0, 0, PhotoChat.ReviewWidth, PhotoChat.ReviewHeight);
                imageBox.Image = new Bitmap(PhotoChat.ReviewWidth, PhotoChat.ReviewHeight);
                using (Graphics g = Graphics.FromImage(imageBox.Image))
                {
                    g.Clear(Color.LightCyan);
                    g.DrawString("No Photo", new Font(PhotoChat.FontName, 20, FontStyle.Bold),
                        new SolidBrush(Color.Navy), new PointF(250, 220));
                }
                imageBox.MouseDown += new MouseEventHandler(imageBox_MouseDown);
                imageBox.MouseClick += new MouseEventHandler(imageBox_MouseClick);
                imageBox.MouseUp += new MouseEventHandler(imageBox_MouseUp);
                imageBox.MouseMove += new MouseEventHandler(imageBox_MouseMove);
                imageBox.Paint += new PaintEventHandler(imageBox_Paint);
                imageBox.ResumeLayout(false);
                this.Controls.Add(imageBox);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// イメージの追加描画を行う。
        /// </summary>
        private void imageBox_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (currentPhoto == null) return;

                // 入力中のストロークがあれば描画
                if (currentStroke != null)
                {
                    currentStroke.PaintOutline(e.Graphics);
                    currentStroke.Paint(e.Graphics);
                }

                // ドラッグ中のハイパーリンク画像があれば描画
                else if (draggingImage != null)
                {
                    e.Graphics.DrawImage(draggingImage, draggingPoint);
                }

                // ドラッグ中の音声データがあれば描画
                else if (soundDraggingPoint != Point.Empty)
                {
                    e.Graphics.DrawImage(soundImage, soundDraggingPoint);
                }

                // テキスト入力中であれば枠を描画
                else if (textBox.Visible)
                {
                    e.Graphics.DrawRectangle(new Pen(noteColor),
                        textBox.Left - 1, textBox.Top - 1, textBox.Width + 1, textBox.Height + 1);
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// イメージを更新する（全て描画する）。
        /// </summary>
        private void UpdateImageBox()
        {
            try
            {
                lock (imageBox.Image)
                {
                    using (Graphics g = Graphics.FromImage(imageBox.Image))
                    {
                        g.Clear(Color.White);

                        // 写真の描画
                        g.DrawImage(currentPhoto.Image, PhotoChat.LeftMargin, PhotoChat.TopMargin);

                        // 書き込みの描画
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        currentPhoto.PaintNotes(g);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// イメージにnoteを描画して更新する。
        /// noteのデータ時刻が最新でなければレイヤーの順序がおかしくなる。
        /// またStrokeなどのグループ化を行う書き込みの場合は引数を取らないものを使う。
        /// </summary>
        /// <param name="note">描画する書き込み</param>
        private void UpdateImageBox(PhotoChatNote note)
        {
            try
            {
                lock (imageBox.Image)
                {
                    using (Graphics g = Graphics.FromImage(imageBox.Image))
                    {
                        note.Paint(g);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// イメージに再生中イメージを描画してから再描画する。
        /// </summary>
        /// <param name="percentage">再生中イメージに描画する書き込みの割合（％）</param>
        private void PaintReplayImage(int percentage)
        {
            try
            {
                lock (imageBox.Image)
                {
                    using (Graphics g = Graphics.FromImage(imageBox.Image))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(currentPhoto.Image, PhotoChat.LeftMargin, PhotoChat.TopMargin);
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        currentPhoto.PaintNotes(g, percentage);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region タグ表示エリア

        /// <summary>
        /// タグ表示エリアを初期化する。
        /// </summary>
        private void InitializeTagArea()
        {
            try
            {
                // タグタイトルの配置
                Label label = new Label();
                label.SuspendLayout();
                label.Text = "タグ";
                label.Bounds = new Rectangle(
                    PhotoChat.ReviewWidth, 0, PhotoChat.TagWidth, PhotoChat.TagHeight);
                label.BackColor = Color.LightGreen;
                label.ForeColor = Color.Violet;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.ResumeLayout(false);
                this.Controls.Add(label);

                // タグリンクパネルの初期化
                tagCount = 0;
                tagPanel = new Panel();
                tagPanel.SuspendLayout();
                tagPanel.Bounds = new Rectangle(PhotoChat.ReviewWidth, PhotoChat.TagHeight,
                    PhotoChat.TagWidth, PhotoChat.ReviewHeight - PhotoChat.TagHeight);
                tagPanel.BackColor = Color.LightGreen;
                tagPanel.ForeColor = Color.Navy;
                tagPanel.ResumeLayout(false);
                this.Controls.Add(tagPanel);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// タグパネルを現在の写真のタグで再設定する。
        /// </summary>
        private void ResetTagLabel()
        {
            try
            {
                tagCount = 0;
                tagPanel.SuspendLayout();
                tagPanel.Controls.Clear();
                foreach (string tag in currentPhoto.TagArray)
                    AddTag(tag);
                tagPanel.ResumeLayout(false);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// タグラベルにタグを追加する。
        /// </summary>
        /// <param name="tag">追加するタグ</param>
        public void AddTag(string tag)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<string>(AddTag), tag);
                    return;
                }

                tagPanel.SuspendLayout();
                LinkLabel label = new LinkLabel();
                label.Bounds = new Rectangle(0, PhotoChat.TagHeight * tagCount,
                    PhotoChat.TagWidth, PhotoChat.TagHeight);
                label.LinkColor = Color.Navy;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Text = tag;
                label.Links.Add(0, tag.Length, tag);
                label.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
                label.ResumeLayout(false);
                tagPanel.Controls.Add(label);
                tagCount++;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// タグラベルのリンクをクリックしたときの処理。
        /// </summary>
        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string tag = e.Link.LinkData as string;
                form.ShowPhotoListWindow(tag);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region 音声データ表示エリア

        /// <summary>
        /// 音声データ表示エリアを初期化する。
        /// </summary>
        private void InitializeSoundArea()
        {
            try
            {
                soundItemList = new List<SoundItem>();

                // 音声データ表示ラベルの初期化
                soundLabel = new Label();
                soundLabel.SuspendLayout();
                soundLabel.Bounds = new Rectangle(
                    0, PhotoChat.ReviewHeight, PhotoChat.ReviewWidth, PhotoChat.SoundHeight);
                soundLabel.BackColor = Color.LightGreen;
                soundLabel.Paint += new PaintEventHandler(soundLabel_Paint);
                soundLabel.MouseDown += new MouseEventHandler(soundLabel_MouseDown);
                soundLabel.MouseUp += new MouseEventHandler(soundLabel_MouseUp);
                soundLabel.MouseMove += new MouseEventHandler(soundLabel_MouseMove);
                soundLabel.ResumeLayout(false);
                this.Controls.Add(soundLabel);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 音声データ表示エリアの描画
        /// </summary>
        private void soundLabel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // 線の描画
                soundLabelPen.EndCap = LineCap.ArrowAnchor;
                int y = PhotoChat.SoundHeight / 2;
                e.Graphics.DrawLine(soundLabelPen, 10, y, soundLabel.Bounds.Right - 10, y);
                soundLabelPen.EndCap = LineCap.NoAnchor;
                int x = (soundLabel.Width / 3) + 20;
                e.Graphics.DrawLine(soundLabelPen, x, 3, x, soundLabel.Bounds.Bottom - 3);

                // 音声データ配置オブジェクトの描画
                lock (soundItemList)
                {
                    foreach (SoundItem item in soundItemList)
                        e.Graphics.DrawImage(soundImage, item.Bounds.Location);
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// 音声データ表示エリアをリセットする。
        /// </summary>
        private void ResetSoundLabel()
        {
            try
            {
                lock (soundItemList)
                {
                    soundItemList.Clear();

                    // 撮影時刻の付近の音声データを検索し音声リストに追加
                    int x = 20;
                    int gap = (soundLabel.Width - 40) / 6;
                    DateTime date;
                    string[] files;
                    for (int diff = -2; diff <= 3; diff++)
                    {
                        date = currentPhoto.Date.AddMinutes(diff);
                        files = Directory.GetFiles(
                            PhotoChat.SoundDirectory, date.ToString(DatePattern) + MP3Pattern);
                        for (int i = 0; i < files.Length; i++)
                            soundItemList.Add(new SoundItem(files[i], x + soundImage.Width * i));
                        x += gap;
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 音声データ配置オブジェクト
        /// </summary>
        public class SoundItem
        {
            private string filePath;
            private Rectangle bounds;

            /// <summary>
            /// 音声データのファイルパスを取得する。
            /// </summary>
            public string FilePath
            {
                get { return filePath; }
            }

            /// <summary>
            /// この配置オブジェクトの位置・サイズを取得する。
            /// </summary>
            public Rectangle Bounds
            {
                get { return bounds; }
            }

            /// <summary>
            /// 音声データ配置オブジェクトを作成する。
            /// </summary>
            /// <param name="filePath">音声データのファイルパス</param>
            /// <param name="x">配置位置のx座標</param>
            public SoundItem(string filePath, int x)
            {
                this.filePath = filePath;
                this.bounds = new Rectangle(x, 4, soundImage.Width, soundImage.Height);
            }
        }

        #endregion




        #region ストローク(Stroke)

        /// <summary>
        /// ストロークを開始する。
        /// </summary>
        /// <param name="point">開始点</param>
        private void StartStroke(Point point)
        {
            try
            {
                currentStroke = new Stroke(
                    form.Client.UserName, form.Client.GetNewSerialNumber(),
                    currentPhoto.PhotoName, noteColor, strokeSize, point);
                imageBox.Invalidate(currentStroke.Range);
                Update();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークを伸ばす。
        /// </summary>
        /// <param name="point">ストロークに追加する点</param>
        private void ExtendStroke(Point point)
        {
            try
            {
                currentStroke.AddPoint(point);
                imageBox.Invalidate(currentStroke.Range);
                Update();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ストロークを終了する。
        /// </summary>
        private void FinishStroke()
        {
            try
            {
                currentStroke.Fix();
                form.NewData(currentStroke);
                currentStroke = null;
                UpdateImageBox();
                imageBox.Invalidate();
                Update();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region テキスト(Text)

        // テキスト入力エリアの初期化
        private void InitializeTextBox()
        {
            try
            {
                textBox = new TextBox();
                textBox.SuspendLayout();
                textBox.Multiline = true;
                textBox.AcceptsTab = true;
                textBox.BorderStyle = BorderStyle.None;
                textBox.Width = 500;
                textBox.Height = 100;
                textBox.ImeMode = ImeMode.NoControl;
                textBox.Hide();
                textBox.ResumeLayout(false);

                imageBox.Controls.Add(textBox);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// テキスト入力を開始する。
        /// </summary>
        /// <param name="point">入力位置の座標</param>
        private void StartTextInput(Point point)
        {
            try
            {
                form.KeyPreview = false;

                // テキスト入力エリアの表示
                textBox.Location = point;
                if (textBox.Font != null)
                    textBox.Font.Dispose();
                textBox.Font = new Font(PhotoChat.FontName, textFontSize, FontStyle.Bold);
                textBox.ForeColor = noteColor;
                textBox.Show();
                imageBox.Refresh();
                textBox.Focus();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// テキスト入力を終了する。
        /// </summary>
        private void FinishTextInput()
        {
            try
            {
                string body = textBox.Text;
                if (body.Length != 0)
                {
                    Text text = new Text(form.Client.UserName, form.Client.GetNewSerialNumber(),
                        currentPhoto.PhotoName, noteColor, textFontSize, textBox.Location, body);
                    form.NewData(text);
                    UpdateImageBox(text);
                }

                //入力エリア後処理
                HideTextBox();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// テキストボックスをリセットして隠す。
        /// </summary>
        private void HideTextBox()
        {
            try
            {
                textBox.Clear();
                textBox.Hide();
                form.KeyPreview = true;
                Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region ハイパーリンク(Hyperlink)

        /// <summary>
        /// ハイパーリンクのドラッグ＆ドロップを受け入れる。
        /// </summary>
        private void DropHyperlink(DragEventArgs e)
        {
            try
            {
                Point point = this.PointToClient(new Point(e.X, e.Y));
                if (imageBox.Bounds.Contains(point))
                {
                    Hyperlink hyperlink = new Hyperlink(
                        form.Client.UserName, form.Client.GetNewSerialNumber(),
                        point, currentPhoto.PhotoName, draggingPhotoName);
                    form.NewData(hyperlink);
                    UpdateImageBox(hyperlink);
                }
                HyperlinkReset();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// ドラッグされてきたハイパーリンクの画像を取得する。
        /// </summary>
        /// <param name="e"></param>
        private void HyperlinkEnter(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(typeof(string)))
                {
                    // ドラッグデータが文字列であれば受け入れ
                    e.Effect = DragDropEffects.Copy;

                    // 同じ写真へのリンクの場合は拒否
                    draggingPhotoName = (string)e.Data.GetData(typeof(string));
                    if (draggingPhotoName == currentPhoto.PhotoName)
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }

                    // 画像の取得
                    draggingImage = Thumbnail.GetImage(
                        draggingPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                    draggingPoint = this.PointToClient(new Point(e.X, e.Y));
                    imageBox.Refresh();
                }
                else
                {
                    // ドラッグデータが文字列でなければ受け付けない
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// ハイパーリンクドラッグの情報を消去する。
        /// </summary>
        private void HyperlinkReset()
        {
            try
            {
                draggingPoint = Point.Empty;
                draggingPhotoName = null;
                if (draggingImage != null)
                {
                    draggingImage.Dispose();
                    draggingImage = null;
                }
                imageBox.Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ドラッグ移動に合わせてドラッグ位置の更新と再描画を行う。
        /// </summary>
        private void HyperlinkDragOver(DragEventArgs e)
        {
            try
            {
                Rectangle preRect = new Rectangle(draggingPoint.X, draggingPoint.Y,
                    PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                draggingPoint = this.PointToClient(new Point(e.X, e.Y));
                Rectangle newRect = new Rectangle(draggingPoint.X, draggingPoint.Y,
                    PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                imageBox.Invalidate(Rectangle.Union(preRect, newRect));
                Update();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region 音声リンク(Sound)

        /// <summary>
        /// 音声データドロップの処理。
        /// </summary>
        /// <param name="point">ドロップポイント</param>
        private void SoundDragDrop(Point point)
        {
            try
            {
                string fileName = form.Client.UserName + Path.GetFileName(soundDraggingItem.FilePath);
                // 音声ファイルの保存・送信
                FileInfo file = new FileInfo(soundDraggingItem.FilePath);
                byte[] soundData = new byte[(int)file.Length];
                using (FileStream fs = file.OpenRead())
                {
                    fs.Read(soundData, 0, soundData.Length);
                }
                form.NewData(new SharedFile(SharedFile.TypeSoundFile,
                    form.Client.ID, form.Client.GetNewSerialNumber(), fileName, soundData));

                // 音声リンクの作成
                Sound sound = new Sound(form.Client.UserName, form.Client.GetNewSerialNumber(),
                    currentPhoto.PhotoName, point, fileName);
                form.NewData(sound);
                UpdateImageBox(sound);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 音声データドラッグの処理。
        /// </summary>
        /// <param name="point">ドラッグポイント</param>
        private void SoundDragOver(Point point)
        {
            try
            {
                Rectangle preRect = new Rectangle(soundDraggingPoint, soundImage.Size);
                soundDraggingPoint = point;
                Rectangle newRect = new Rectangle(point, soundImage.Size);
                imageBox.Invalidate(Rectangle.Union(preRect, newRect));
                Update();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 音声データドラッグの情報をクリアする。
        /// </summary>
        private void SoundDragReset()
        {
            try
            {
                soundDraggingPoint = Point.Empty;
                soundDraggingItem = null;
                imageBox.Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 音声データ表示エリア上でのPointをイメージエリア上でのPointに変換する。
        /// </summary>
        /// <param name="soundPoint">音声データ表示エリア上でのPoint</param>
        /// <returns>イメージエリア上でのPoint</returns>
        private Point SoundPointToImagePoint(Point soundPoint)
        {
            soundPoint.Y += PhotoChat.ReviewHeight;
            return soundPoint;
        }


        /// <summary>
        /// 音声リンクの音声を再生する。
        /// </summary>
        /// <param name="sound">音声リンク</param>
        private void SoundPlay(Sound sound)
        {
            try
            {
                PhotoChat.AudioPlay(
                    Path.Combine(PhotoChat.SoundNoteDirectory, sound.SoundFileName));
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 削除(Removal)

        /// <summary>
        /// 書き込みを削除する。
        /// </summary>
        /// <param name="note">削除する書き込み</param>
        private void RemoveNote(PhotoChatNote note)
        {
            try
            {
                if (note == null) return;

                Removal removal = new Removal(
                    form.Client.UserName, form.Client.GetNewSerialNumber(),
                    note.PhotoName, note.ID, note.SerialNumber);
                form.NewData(removal);
                UpdateImageBox();
                imageBox.Invalidate();
                Update();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region マウスイベント:Panel

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Hyperlink:
                        // ハイパーリンクを構成
                        DropHyperlink(drgevent);
                        reviewMode = ReviewModes.Review;
                        break;
                }

                base.OnDragDrop(drgevent);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        protected override void OnDragOver(DragEventArgs drgevent)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Hyperlink:
                        // ハイパーリンクドラッグ移動
                        HyperlinkDragOver(drgevent);
                        break;
                }

                base.OnDragOver(drgevent);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Review:
                        // ハイパーリンクモードに入る
                        HyperlinkEnter(drgevent);
                        reviewMode = ReviewModes.Hyperlink;
                        break;
                }

                base.OnDragEnter(drgevent);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        protected override void OnDragLeave(EventArgs e)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Hyperlink:
                        // 通常閲覧モードに戻す
                        HyperlinkReset();
                        reviewMode = ReviewModes.Review;
                        break;
                }

                base.OnDragLeave(e);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region マウスイベント:imageBox

        private void imageBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (currentPhoto == null) return;

                switch (reviewMode)
                {
                    case ReviewModes.Review:
                        pressedLink = currentPhoto.GetPointedLink(e.Location);
                        if (pressedLink != null)
                        {
                            // リンク書き込み上で押された場合はリンク処理の準備
                            reviewMode = ReviewModes.Link;
                        }
                        else if (e.Button == MouseButtons.Left)
                        {
                            // 左ボタンの場合はストローク開始
                            StartStroke(e.Location);
                            reviewMode = ReviewModes.Stroke;
                        }
                        break;

                    case ReviewModes.Remove:
                        // 削除開始
                        reviewMode = ReviewModes.Removing;
                        break;

                    case ReviewModes.Replay:
                        // 再生中止
                        QuitReplay();
                        reviewMode = ReviewModes.Review;
                        break;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void imageBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (currentPhoto == null) return;

                switch (reviewMode)
                {
                    case ReviewModes.Review:
                        if (e.Button == MouseButtons.Right)
                        {
                            // 右クリックしたときはテキスト入力開始
                            StartTextInput(e.Location);
                            reviewMode = ReviewModes.Text;
                        }
                        break;

                    case ReviewModes.Text:
                        if (e.Button == MouseButtons.Left)
                        {
                            // テキスト入力中の左クリックは入力完了
                            FinishTextInput();
                            reviewMode = ReviewModes.Review;
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            // テキスト入力中の右クリックはキャンセル
                            HideTextBox();
                            reviewMode = ReviewModes.Review;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void imageBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Stroke:
                        // ストローク完了
                        reviewMode = ReviewModes.Review;
                        FinishStroke();
                        break;

                    case ReviewModes.Removing:
                        // 削除ドラッグ終了
                        reviewMode = ReviewModes.Remove;
                        break;

                    case ReviewModes.Link:
                        // リンク処理
                        reviewMode = ReviewModes.Review;
                        if (pressedLink.Type == PhotoChatNote.TypeHyperlink)
                            form.ShowPhoto(((Hyperlink)pressedLink).LinkedPhotoName);
                        else if (pressedLink.Type == PhotoChatNote.TypeSound)
                            SoundPlay((Sound)pressedLink);
                        break;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Stroke:
                        // ストロークを伸ばす
                        ExtendStroke(e.Location);
                        break;

                    case ReviewModes.Removing:
                        // 書き込み削除
                        RemoveNote(currentPhoto.GetPointedMyNote(
                            e.Location, form.Client.UserName));
                        break;

                    case ReviewModes.Link:
                        // リンク処理をキャンセル
                        if (!pressedLink.Contains(e.Location))
                            reviewMode = ReviewModes.Review;
                        break;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region マウスイベント:soundLabel

        private void soundLabel_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // 音声アイテム上で押されたときのみ処理
                foreach (SoundItem item in soundItemList)
                {
                    if (item.Bounds.Contains(e.Location))
                    {
                        soundDraggingItem = item;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void soundLabel_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (soundDraggingItem != null)
                {
                    if (soundDraggingPoint != Point.Empty)
                    {
                        // 音声ドラッグが開始されている場合
                        Point point = SoundPointToImagePoint(e.Location);
                        if (imageBox.Bounds.Contains(point))
                            SoundDragDrop(point);
                        SoundDragReset();
                    }
                    else if (soundDraggingItem.Bounds.Contains(e.Location))
                    {
                        // 同じ音声アイテム上でボタンが離されたら音声再生
                        PhotoChat.AudioPlay(soundDraggingItem.FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void soundLabel_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (soundDraggingItem != null)
                {
                    if (soundDraggingPoint != Point.Empty)
                    {
                        // 音声ドラッグ移動
                        SoundDragOver(SoundPointToImagePoint(e.Location));
                    }
                    else if (!soundDraggingItem.Bounds.Contains(e.Location))
                    {
                        // 音声ドラッグ開始
                        soundDraggingPoint = SoundPointToImagePoint(e.Location);
                    }
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion
    }
}
