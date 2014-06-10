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
    /// �ʐ^�{���E�ҏW�p�l��
    /// </summary>
    public class ReviewPanel : Panel
    {
        #region �t�B�[���h�E�v���p�e�B

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
        /// �\�����̎ʐ^
        /// </summary>
        public Photo CurrentPhoto
        {
            get { return currentPhoto; }
        }

        /// <summary>
        /// �������݂̐F
        /// </summary>
        public Color NoteColor
        {
            get { return noteColor; }
            set { noteColor = value; }
        }

        /// <summary>
        /// �X�g���[�N�̑���
        /// </summary>
        public int StrokeSize
        {
            get { return strokeSize; }
            set { strokeSize = value; }
        }

        /// <summary>
        /// �t�H���g�T�C�Y
        /// </summary>
        public float TextFontSize
        {
            get { return textFontSize; }
            set { textFontSize = value; }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// �ʐ^�{���E�ҏW�p�l��������������
        /// </summary>
        /// <param name="form">�e�t�H�[��</param>
        public ReviewPanel(PhotoChatForm form)
        {
            this.form = form;
            this.SuspendLayout();

            // GUI�ݒ�
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

            // �{�������̏�����
            historyList = new String[PhotoChat.HistorySize];
            historyIndex = -1;
            historyFrom = -1;
            historyEnd = -1;
        }


        /// <summary>
        /// �t�H�[�J�X�������Ȃ����Ƃ��̏���
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




        #region �ʐ^�̕ύX

        /// <summary>
        /// �\������ʐ^��ύX����B
        /// </summary>
        /// <param name="photo">�\������ʐ^</param>
        public void SetPhoto(Photo photo)
        {
            try
            {
                // �����ʐ^�Ȃ牽�����Ȃ�
                if (currentPhoto != null
                    && photo.PhotoName == currentPhoto.PhotoName) return;
                Photo temp = currentPhoto;
                currentPhoto = photo;
                if (temp != null) temp.Dispose();

                // �{�������X�V
                UpdateHistory(photo.PhotoName);

                // ���݂̃��[�h�ɉ�����������
                ResetReviewMode();

                // �������ݍĐ����[�h�ł���΍Đ�
                //if (form.IsReplayMode)
                //    new DrawingReplay().start();

                // �`��o�b�t�@�E�^�O�\���E�����\�����X�V���ĕ`��
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
        /// ���݂̃��[�h�ɉ����ď��������s���B
        /// </summary>
        private void ResetReviewMode()
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Text:
                        // �e�L�X�g���̓G���A�̃��Z�b�g
                        HideTextBox();
                        break;

                    case ReviewModes.Remove:
                        // �������݃��[�h�ɖ߂�
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
        /// ������Ԃɂ���B
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

                // �t�B�[���h�l�̏�����
                currentPhoto = null;
                historyIndex = -1;
                historyFrom = -1;
                historyEnd = -1;
                form.LeftButtonEnabled = false;
                form.RightButtonEnabled = false;

                // �\���̏�����
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


        #region �{������

        /// <summary>
        /// �{���������X�V����B
        /// </summary>
        /// <param name="photoName">�V�����\������ʐ^�̖��O</param>
        private void UpdateHistory(String photoName)
        {
            try
            {
                // �{�������ɂ��\���̏ꍇ�͍X�V���Ȃ�
                if (historyJumpFlag)
                {
                    historyJumpFlag = false;
                    return;
                }

                // �V���������̒ǉ�
                if (++historyIndex == PhotoChat.HistorySize) historyIndex = 0;
                historyList[historyIndex] = photoName;

                // �����̏I�_���X�V
                historyEnd = historyIndex;
                form.RightButtonEnabled = false;

                // �����̎n�_���X�V
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
        /// ������1�O�̎ʐ^��\������B
        /// </summary>
        public void Back()
        {
            try
            {
                // �{�������̃C���f�b�N�X��1�O�ɖ߂�
                if (--historyIndex == -1) historyIndex = PhotoChat.HistorySize - 1;
                form.RightButtonEnabled = true;

                // �����̐擪�܂Ŗ߂����Ƃ��͖߂�{�^������
                if (historyIndex == historyFrom)
                    form.LeftButtonEnabled = false;

                // �ʐ^�\��
                historyJumpFlag = true;
                form.ShowPhoto(historyList[historyIndex]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ������1��̎ʐ^��\������B
        /// </summary>
        public void Forward()
        {
            try
            {
                // �{�������̃C���f�b�N�X��1��ɐi�߂�
                if (++historyIndex == PhotoChat.HistorySize) historyIndex = 0;
                form.LeftButtonEnabled = true;

                // �����̍Ō�܂Ői�񂾂Ƃ��͐i�ރ{�^������
                if (historyIndex == historyEnd)
                    form.RightButtonEnabled = false;

                // �ʐ^�\��
                historyJumpFlag = true;
                form.ShowPhoto(historyList[historyIndex]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region �O������̍X�V�Ăяo���E�摜�擾�E���[�h�ύX

        /// <summary>
        /// �\�����̎ʐ^�Ə������݂��X�V����B
        /// </summary>
        public void UpdateImage()
        {
            try
            {
                // �X�g���[�N���E�Đ����łȂ���΃C���[�W�o�b�t�@���X�V���čĕ`��
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
        /// ���ݕ\������Ă���摜���R�s�[���ĕԂ��B
        /// </summary>
        /// <returns>���ݕ\������Ă���摜</returns>
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
        /// �T���l�C���p�̎ʐ^�����؂���摜���쐬���ĕԂ��B
        /// </summary>
        /// <returns>�T���l�C���p�摜</returns>
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
        /// �������ݐ��̊������w�肵�ď������ݍĐ��`����s���B
        /// </summary>
        /// <param name="percentage">�`�悷�鏑�����ݐ��̊����i���j</param>
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
        /// �������ݍĐ����I�����X���C�_�[�����Z�b�g����B
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
        /// �{�����[�h�ɐݒ肷��B
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
        /// �폜���[�h�ɐݒ肷��B
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
        /// ���C�u���[�h�ɐݒ肷��B
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




        #region �ʐ^�E�������ݕ\���G���A

        /// <summary>
        /// �ʐ^�E�������ݕ\���G���A������������B
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
        /// �C���[�W�̒ǉ��`����s���B
        /// </summary>
        private void imageBox_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (currentPhoto == null) return;

                // ���͒��̃X�g���[�N������Ε`��
                if (currentStroke != null)
                {
                    currentStroke.PaintOutline(e.Graphics);
                    currentStroke.Paint(e.Graphics);
                }

                // �h���b�O���̃n�C�p�[�����N�摜������Ε`��
                else if (draggingImage != null)
                {
                    e.Graphics.DrawImage(draggingImage, draggingPoint);
                }

                // �h���b�O���̉����f�[�^������Ε`��
                else if (soundDraggingPoint != Point.Empty)
                {
                    e.Graphics.DrawImage(soundImage, soundDraggingPoint);
                }

                // �e�L�X�g���͒��ł���Θg��`��
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
        /// �C���[�W���X�V����i�S�ĕ`�悷��j�B
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

                        // �ʐ^�̕`��
                        g.DrawImage(currentPhoto.Image, PhotoChat.LeftMargin, PhotoChat.TopMargin);

                        // �������݂̕`��
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
        /// �C���[�W��note��`�悵�čX�V����B
        /// note�̃f�[�^�������ŐV�łȂ���΃��C���[�̏��������������Ȃ�B
        /// �܂�Stroke�Ȃǂ̃O���[�v�����s���������݂̏ꍇ�͈��������Ȃ����̂��g���B
        /// </summary>
        /// <param name="note">�`�悷�鏑������</param>
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
        /// �C���[�W�ɍĐ����C���[�W��`�悵�Ă���ĕ`�悷��B
        /// </summary>
        /// <param name="percentage">�Đ����C���[�W�ɕ`�悷�鏑�����݂̊����i���j</param>
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


        #region �^�O�\���G���A

        /// <summary>
        /// �^�O�\���G���A������������B
        /// </summary>
        private void InitializeTagArea()
        {
            try
            {
                // �^�O�^�C�g���̔z�u
                Label label = new Label();
                label.SuspendLayout();
                label.Text = "�^�O";
                label.Bounds = new Rectangle(
                    PhotoChat.ReviewWidth, 0, PhotoChat.TagWidth, PhotoChat.TagHeight);
                label.BackColor = Color.LightGreen;
                label.ForeColor = Color.Violet;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.ResumeLayout(false);
                this.Controls.Add(label);

                // �^�O�����N�p�l���̏�����
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
        /// �^�O�p�l�������݂̎ʐ^�̃^�O�ōĐݒ肷��B
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
        /// �^�O���x���Ƀ^�O��ǉ�����B
        /// </summary>
        /// <param name="tag">�ǉ�����^�O</param>
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
        /// �^�O���x���̃����N���N���b�N�����Ƃ��̏����B
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


        #region �����f�[�^�\���G���A

        /// <summary>
        /// �����f�[�^�\���G���A������������B
        /// </summary>
        private void InitializeSoundArea()
        {
            try
            {
                soundItemList = new List<SoundItem>();

                // �����f�[�^�\�����x���̏�����
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
        /// �����f�[�^�\���G���A�̕`��
        /// </summary>
        private void soundLabel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // ���̕`��
                soundLabelPen.EndCap = LineCap.ArrowAnchor;
                int y = PhotoChat.SoundHeight / 2;
                e.Graphics.DrawLine(soundLabelPen, 10, y, soundLabel.Bounds.Right - 10, y);
                soundLabelPen.EndCap = LineCap.NoAnchor;
                int x = (soundLabel.Width / 3) + 20;
                e.Graphics.DrawLine(soundLabelPen, x, 3, x, soundLabel.Bounds.Bottom - 3);

                // �����f�[�^�z�u�I�u�W�F�N�g�̕`��
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
        /// �����f�[�^�\���G���A�����Z�b�g����B
        /// </summary>
        private void ResetSoundLabel()
        {
            try
            {
                lock (soundItemList)
                {
                    soundItemList.Clear();

                    // �B�e�����̕t�߂̉����f�[�^���������������X�g�ɒǉ�
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
        /// �����f�[�^�z�u�I�u�W�F�N�g
        /// </summary>
        public class SoundItem
        {
            private string filePath;
            private Rectangle bounds;

            /// <summary>
            /// �����f�[�^�̃t�@�C���p�X���擾����B
            /// </summary>
            public string FilePath
            {
                get { return filePath; }
            }

            /// <summary>
            /// ���̔z�u�I�u�W�F�N�g�̈ʒu�E�T�C�Y���擾����B
            /// </summary>
            public Rectangle Bounds
            {
                get { return bounds; }
            }

            /// <summary>
            /// �����f�[�^�z�u�I�u�W�F�N�g���쐬����B
            /// </summary>
            /// <param name="filePath">�����f�[�^�̃t�@�C���p�X</param>
            /// <param name="x">�z�u�ʒu��x���W</param>
            public SoundItem(string filePath, int x)
            {
                this.filePath = filePath;
                this.bounds = new Rectangle(x, 4, soundImage.Width, soundImage.Height);
            }
        }

        #endregion




        #region �X�g���[�N(Stroke)

        /// <summary>
        /// �X�g���[�N���J�n����B
        /// </summary>
        /// <param name="point">�J�n�_</param>
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
        /// �X�g���[�N��L�΂��B
        /// </summary>
        /// <param name="point">�X�g���[�N�ɒǉ�����_</param>
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
        /// �X�g���[�N���I������B
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


        #region �e�L�X�g(Text)

        // �e�L�X�g���̓G���A�̏�����
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
        /// �e�L�X�g���͂��J�n����B
        /// </summary>
        /// <param name="point">���͈ʒu�̍��W</param>
        private void StartTextInput(Point point)
        {
            try
            {
                form.KeyPreview = false;

                // �e�L�X�g���̓G���A�̕\��
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
        /// �e�L�X�g���͂��I������B
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

                //���̓G���A�㏈��
                HideTextBox();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �e�L�X�g�{�b�N�X�����Z�b�g���ĉB���B
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


        #region �n�C�p�[�����N(Hyperlink)

        /// <summary>
        /// �n�C�p�[�����N�̃h���b�O���h���b�v���󂯓����B
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
        /// �h���b�O����Ă����n�C�p�[�����N�̉摜���擾����B
        /// </summary>
        /// <param name="e"></param>
        private void HyperlinkEnter(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(typeof(string)))
                {
                    // �h���b�O�f�[�^��������ł���Ύ󂯓���
                    e.Effect = DragDropEffects.Copy;

                    // �����ʐ^�ւ̃����N�̏ꍇ�͋���
                    draggingPhotoName = (string)e.Data.GetData(typeof(string));
                    if (draggingPhotoName == currentPhoto.PhotoName)
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }

                    // �摜�̎擾
                    draggingImage = Thumbnail.GetImage(
                        draggingPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                    draggingPoint = this.PointToClient(new Point(e.X, e.Y));
                    imageBox.Refresh();
                }
                else
                {
                    // �h���b�O�f�[�^��������łȂ���Ύ󂯕t���Ȃ�
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �n�C�p�[�����N�h���b�O�̏�����������B
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
        /// �h���b�O�ړ��ɍ��킹�ăh���b�O�ʒu�̍X�V�ƍĕ`����s���B
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


        #region ���������N(Sound)

        /// <summary>
        /// �����f�[�^�h���b�v�̏����B
        /// </summary>
        /// <param name="point">�h���b�v�|�C���g</param>
        private void SoundDragDrop(Point point)
        {
            try
            {
                string fileName = form.Client.UserName + Path.GetFileName(soundDraggingItem.FilePath);
                // �����t�@�C���̕ۑ��E���M
                FileInfo file = new FileInfo(soundDraggingItem.FilePath);
                byte[] soundData = new byte[(int)file.Length];
                using (FileStream fs = file.OpenRead())
                {
                    fs.Read(soundData, 0, soundData.Length);
                }
                form.NewData(new SharedFile(SharedFile.TypeSoundFile,
                    form.Client.ID, form.Client.GetNewSerialNumber(), fileName, soundData));

                // ���������N�̍쐬
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
        /// �����f�[�^�h���b�O�̏����B
        /// </summary>
        /// <param name="point">�h���b�O�|�C���g</param>
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
        /// �����f�[�^�h���b�O�̏����N���A����B
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
        /// �����f�[�^�\���G���A��ł�Point���C���[�W�G���A��ł�Point�ɕϊ�����B
        /// </summary>
        /// <param name="soundPoint">�����f�[�^�\���G���A��ł�Point</param>
        /// <returns>�C���[�W�G���A��ł�Point</returns>
        private Point SoundPointToImagePoint(Point soundPoint)
        {
            soundPoint.Y += PhotoChat.ReviewHeight;
            return soundPoint;
        }


        /// <summary>
        /// ���������N�̉������Đ�����B
        /// </summary>
        /// <param name="sound">���������N</param>
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


        #region �폜(Removal)

        /// <summary>
        /// �������݂��폜����B
        /// </summary>
        /// <param name="note">�폜���鏑������</param>
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




        #region �}�E�X�C�x���g:Panel

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            try
            {
                switch (reviewMode)
                {
                    case ReviewModes.Hyperlink:
                        // �n�C�p�[�����N���\��
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
                        // �n�C�p�[�����N�h���b�O�ړ�
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
                        // �n�C�p�[�����N���[�h�ɓ���
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
                        // �ʏ�{�����[�h�ɖ߂�
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


        #region �}�E�X�C�x���g:imageBox

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
                            // �����N�������ݏ�ŉ����ꂽ�ꍇ�̓����N�����̏���
                            reviewMode = ReviewModes.Link;
                        }
                        else if (e.Button == MouseButtons.Left)
                        {
                            // ���{�^���̏ꍇ�̓X�g���[�N�J�n
                            StartStroke(e.Location);
                            reviewMode = ReviewModes.Stroke;
                        }
                        break;

                    case ReviewModes.Remove:
                        // �폜�J�n
                        reviewMode = ReviewModes.Removing;
                        break;

                    case ReviewModes.Replay:
                        // �Đ����~
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
                            // �E�N���b�N�����Ƃ��̓e�L�X�g���͊J�n
                            StartTextInput(e.Location);
                            reviewMode = ReviewModes.Text;
                        }
                        break;

                    case ReviewModes.Text:
                        if (e.Button == MouseButtons.Left)
                        {
                            // �e�L�X�g���͒��̍��N���b�N�͓��͊���
                            FinishTextInput();
                            reviewMode = ReviewModes.Review;
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            // �e�L�X�g���͒��̉E�N���b�N�̓L�����Z��
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
                        // �X�g���[�N����
                        reviewMode = ReviewModes.Review;
                        FinishStroke();
                        break;

                    case ReviewModes.Removing:
                        // �폜�h���b�O�I��
                        reviewMode = ReviewModes.Remove;
                        break;

                    case ReviewModes.Link:
                        // �����N����
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
                        // �X�g���[�N��L�΂�
                        ExtendStroke(e.Location);
                        break;

                    case ReviewModes.Removing:
                        // �������ݍ폜
                        RemoveNote(currentPhoto.GetPointedMyNote(
                            e.Location, form.Client.UserName));
                        break;

                    case ReviewModes.Link:
                        // �����N�������L�����Z��
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


        #region �}�E�X�C�x���g:soundLabel

        private void soundLabel_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // �����A�C�e����ŉ����ꂽ�Ƃ��̂ݏ���
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
                        // �����h���b�O���J�n����Ă���ꍇ
                        Point point = SoundPointToImagePoint(e.Location);
                        if (imageBox.Bounds.Contains(point))
                            SoundDragDrop(point);
                        SoundDragReset();
                    }
                    else if (soundDraggingItem.Bounds.Contains(e.Location))
                    {
                        // ���������A�C�e����Ń{�^���������ꂽ�特���Đ�
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
                        // �����h���b�O�ړ�
                        SoundDragOver(SoundPointToImagePoint(e.Location));
                    }
                    else if (!soundDraggingItem.Bounds.Contains(e.Location))
                    {
                        // �����h���b�O�J�n
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
