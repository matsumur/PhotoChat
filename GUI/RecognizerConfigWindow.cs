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
    /// �X�g���[�N�F���@�\�ݒ�E�B���h�E
    /// </summary>
    public partial class RecognizerConfigWindow : Form
    {
        #region �t�B�[���h

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


        #region �R���X�g���N�^�E�^�u�I��

        /// <summary>
        /// �X�g���[�N�F���@�\�ݒ�E�B���h�E���쐬����B
        /// </summary>
        /// <param name="strokeRecognizer">�X�g���[�N�F���N���X</param>
        public RecognizerConfigWindow(StrokeRecognizer strokeRecognizer)
        {
            this.strokeRecognizer = strokeRecognizer;
            InitializeComponent();

            // �o�^�ς݃e���v���[�g�摜�̍X�V
            UpdateTemplateImagePanel();

            // �y���E�u���V�̏�����
            currentPen = new Pen(Color.Red, 3);
            currentPen.StartCap = LineCap.Round;
            currentPen.EndCap = LineCap.Round;
            roughPen = new Pen(Color.Pink, 3);
            roughPen.StartCap = LineCap.Round;
            roughPen.EndCap = LineCap.Round;
            foreBrush = new SolidBrush(this.ForeColor);
        }

        /// <summary>
        /// �^�u�I���C�x���g
        /// </summary>
        private void myTabControl_Selected(object sender, TabControlEventArgs e)
        {
            // ���ʂ̏�����
            currentStroke = null;
            roughStroke = null;
            resultName = string.Empty;

            // �I�����ꂽ�^�u�ɉ�����������
            if (myTabControl.SelectedTab == testPage)
            {
                // �o�^�ς݃e���v���[�g�摜�̍X�V
                UpdateTemplateImagePanel();
            }
            else if (myTabControl.SelectedTab == addPage)
            {
                // �o�^�{�^���𖳌���
                addInfoLabel.Text = string.Empty;
                addInfoLabel.Refresh();
                addButton.Enabled = false;
            }
            else if (myTabControl.SelectedTab == managePage)
            {
                // �e���v���[�g���X�g�̍X�V
                UpdateTemplateList();
                selectedLabel = null;
                deleteButton.Enabled = false;
            }
        }

        #endregion


        #region �X�g���[�N����

        /// <summary>
        /// �X�g���[�N���͊J�n
        /// </summary>
        private void StrokeLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isStroking)
            {
                // ���̓X�g���[�N�����������ĊJ�n�_��ǉ�
                isStroking = true;
                currentStroke = new List<PointF>();
                currentStroke.Add(e.Location);
                ((Control)sender).Refresh();
            }
        }

        /// <summary>
        /// �X�g���[�N���͒�
        /// </summary>
        private void StrokeLabel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isStroking)
            {
                // ���̓X�g���[�N�ɓ_��ǉ�
                currentStroke.Add(e.Location);
                ((Control)sender).Refresh();
            }
        }

        /// <summary>
        /// �X�g���[�N���͏I��
        /// </summary>
        private void StrokeLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isStroking)
            {
                // �X�g���[�N���͏I�����̏���
                if (myTabControl.SelectedTab == testPage)
                {
                    // �F���e�X�g�̏ꍇ�͔F�����s
                    Recognize();
                }
                else if (myTabControl.SelectedTab == addPage)
                {
                    // �e���v���[�g�ǉ��̏ꍇ�͓o�^�������ł��Ă���΃v���r���[�\��
                    if (nameTextBox.Text.Length != 0)
                        UpdateNewTemplate();
                }
                ((Control)sender).Refresh();
                isStroking = false;
            }
        }

        /// <summary>
        /// ���̓X�g���[�N�̕`��
        /// </summary>
        private void StrokeLabel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // �������X�g���[�N�̕`��
                if (roughStroke != null)
                {
                    for (int i = 1; i < roughStroke.Count; i++)
                        e.Graphics.DrawLine(roughPen, roughStroke[i - 1], roughStroke[i]);
                }

                // ���̓X�g���[�N�̕`��
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


        #region �F���e�X�g

        /// <summary>
        /// �o�^�ς݃e���v���[�g�摜���X�V����B
        /// </summary>
        private void UpdateTemplateImagePanel()
        {
            // �e���v���[�g�摜�̃��X�g���X�V�i�ő�X���j
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
        /// �o�^�ς݃e���v���[�g�摜��`�悷��B
        /// </summary>
        private void templateImagePanel_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < templateImages.Count; i++)
                e.Graphics.DrawImage(templateImages[i], (i % 3) * 75, (i / 3) * 75);
        }

        /// <summary>
        /// �F�����ʂ�`�悷��B
        /// </summary>
        private void resultLabel_Paint(object sender, PaintEventArgs e)
        {
            if (resultName != string.Empty)
            {
                e.Graphics.DrawImage(resultImage, 0, 0);
                e.Graphics.DrawString(
                    "�e���v���[�g���F" + resultName, this.Font, foreBrush, 60, 7);
                e.Graphics.DrawString(
                    "�X�R�A�F" + resultScore.ToString(), this.Font, foreBrush, 60, 30);
            }
        }

        /// <summary>
        /// �X�g���[�N�̔F���e�X�g�����s����B
        /// </summary>
        private void Recognize()
        {
            StrokeRecognizer.Result result =
                strokeRecognizer.Recognize(currentStroke.ToArray());

            // �F�����ʂ̕\��
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


        #region �e���v���[�g�o�^

        /// <summary>
        /// �e���v���[�g�ǉ� > �e���v���[�g�����̓{�b�N�X
        /// </summary>
        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            // �e���v���[�g���ƃX�g���[�N�����͂���Ă���΃v���r���[�\��
            if (nameTextBox.Text.Length == 0)
                addButton.Enabled = false;
            else if (currentStroke != null)
                UpdateNewTemplate();
        }

        /// <summary>
        /// �e���v���[�g�ǉ� > �e���v���[�g�o�^�{�^��
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

                // �e���v���[�g�o�^
                strokeRecognizer.AddTemplate(nameTextBox.Text, currentStroke.ToArray());
                string filePath = Path.Combine(
                    PhotoChat.StrokeRecognizerDirectory, resultName + ".jpg");
                resultImage.Save(filePath, ImageFormat.Jpeg);

                // �o�^�������b�Z�[�W
                addInfoLabel.Text = "�o�^���܂���";
                addInfoLabel.Refresh();

                // �X�g���[�N����
                currentStroke = null;
                roughStroke = null;
                nameTextBox.Clear();
                newStrokeLabel.Refresh();
            }
        }

        /// <summary>
        /// �e���v���[�g�ǉ� > �������{�^��
        /// </summary>
        private void roughButton_Click(object sender, EventArgs e)
        {
            // ���͂����X�g���[�N���������Ƃ��ĕ\�����A���̓X�g���[�N�͏���
            roughStroke = currentStroke;
            currentStroke = null;
            addButton.Enabled = false;
            newStrokeLabel.Refresh();
        }

        /// <summary>
        /// �e���v���[�g�ǉ� > �����{�^��
        /// </summary>
        private void clearButton_Click(object sender, EventArgs e)
        {
            // �������E���̓X�g���[�N�Ƃ��ɏ���
            currentStroke = null;
            roughStroke = null;
            addButton.Enabled = false;
            newStrokeLabel.Refresh();
        }

        /// <summary>
        /// �o�^����e���v���[�g�̃v���r���[���X�V����B
        /// </summary>
        private void UpdateNewTemplate()
        {
            // �o�^����e���v���[�g�̃v���r���[�쐬�E�\��
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

            // �o�^�{�^����L����
            addInfoLabel.Text = string.Empty;
            addInfoLabel.Refresh();
            addButton.Enabled = true;
        }

        /// <summary>
        /// �o�^����e���v���[�g�̃v���r���[��\������B
        /// </summary>
        private void newTemplateLabel_Paint(object sender, PaintEventArgs e)
        {
            if (resultName != string.Empty)
            {
                e.Graphics.DrawImage(resultImage, 0, 0);
                e.Graphics.DrawString(
                    "�e���v���[�g���F" + resultName, this.Font, foreBrush, 60, 10);
            }
        }

        #endregion


        #region �e���v���[�g����

        /// <summary>
        /// �e���v���[�g���X�g���X�V����B
        /// </summary>
        private void UpdateTemplateList()
        {
            // ���X�g�X�V
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
        /// �e���v���[�g�̉摜�Ɩ��O��\�����郉�x��
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
            /// ���x���̕`��
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
            /// ���x�����I������Ă��邩�ǂ�����ݒ肷��B
            /// </summary>
            public bool Selected
            {
                get { return selected; }
                set { selected = value; }
            }
        }

        /// <summary>
        /// ���x���̑I��
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
        /// �e���v���[�g���� > �폜�{�^��
        /// </summary>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            // �I������Ă���e���v���[�g���폜
            deleteButton.Enabled = false;
            strokeRecognizer.RemoveTemplate(selectedLabel.Name);
            selectedLabel = null;

            // �e���v���[�g���X�g�X�V
            UpdateTemplateList();
        }

        #endregion
    }
}