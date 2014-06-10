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
    /// �p���b�g�c�[���E�B���h�E
    /// </summary>
    public partial class PaletteWindow : Form
    {
        #region �t�B�[���h

        private PhotoChatForm form;
        private int colorIndex = 0;
        private int strokeSizeIndex = 1;
        private Pen pen = new Pen(Color.Black);
        private const int ColorCellSize = 20;  // �F�̐����`�̃T�C�Y
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




        #region �R���X�g���N�^�E�f�X�g���N�^

        /// <summary>
        /// �p���b�g�c�[���E�B���h�E���쐬����B
        /// </summary>
        public PaletteWindow(PhotoChatForm form)
        {
            this.form = form;
            InitializeComponent();

            // �����F�������_���ɐݒ�
            if (form.ReviewPanel != null)
            {
                Random random = new Random();
                colorIndex = random.Next(DefaultColorList.Length);
                form.ReviewPanel.NoteColor = DefaultColorList[colorIndex];
                form.PenColor = DefaultColorList[colorIndex];
            }

            // �F�I�����x���̏�����
            this.colorLabel.Image = CreateColorLabelImage();

            // �����I�����x���̏�����
            this.strokeSizeLabel.Image = CreateStrokeSizeLabelImage();

            // �����T�C�Y�{�b�N�X�̏�����
            this.fontSizeComboBox.Items.AddRange(FontSizeList);
            this.fontSizeComboBox.Text = PhotoChat.DefaultTextFontSize.ToString();
        }


        /// <summary>
        /// �g�p���̃��\�[�X�����ׂăN���[���A�b�v���܂��B
        /// </summary>
        /// <param name="disposing">�}�l�[�W ���\�[�X���j�������ꍇ true�A�j������Ȃ��ꍇ�� false �ł��B</param>
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




        #region �F�I��

        /// <summary>
        /// �F�I�����x���̔w�i�C���[�W���쐬����B
        /// </summary>
        /// <returns>�쐬�����C���[�W</returns>
        private Bitmap CreateColorLabelImage()
        {
            Bitmap image = new Bitmap(64, 211);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(this.BackColor);

                // �e�F�̐����`��`��
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

                // �����F�̑I��g��`��
                pen.Color = SelectedRectColor;
                rect.X = (colorIndex % 3) * (ColorCellSize + 1);
                rect.Y = (colorIndex / 3) * (ColorCellSize + 1);
                rect.Size = new Size(21, 21);
                g.DrawRectangle(pen, rect);
            }
            return image;
        }


        /// <summary>
        /// �F�I�����x���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void colorLabel_MouseDown(object sender, MouseEventArgs e)
        {
            // �I��F�̗�ƍs���Z�o
            int column = (e.X - 1) / (ColorCellSize + 1);
            int row = (e.Y - 1) / (ColorCellSize + 1);

            using (Graphics g = Graphics.FromImage(colorLabel.Image))
            {
                // ����܂ł̑I��g������
                pen.Color = this.BackColor;
                int x = (colorIndex % 3) * (ColorCellSize + 1);
                int y = (colorIndex / 3) * (ColorCellSize + 1);
                Rectangle rect = new Rectangle(x, y, 21, 21);
                g.DrawRectangle(pen, rect);

                // �I��g�̕`��
                pen.Color = SelectedRectColor;
                rect.X = column * (ColorCellSize + 1);
                rect.Y = row * (ColorCellSize + 1);
                g.DrawRectangle(pen, rect);
            }

            // �F�ݒ�
            colorIndex = row * 3 + column;
            form.ReviewPanel.NoteColor = DefaultColorList[colorIndex];
            form.PenColor = DefaultColorList[colorIndex];
            form.SetPenMode();
            colorLabel.Invalidate();
        }

        #endregion




        #region �����I��

        /// <summary>
        /// �����I�����x���̔w�i�C���[�W���쐬����B
        /// </summary>
        /// <returns>�쐬�����C���[�W</returns>
        private Bitmap CreateStrokeSizeLabelImage()
        {
            Bitmap image = new Bitmap(64, 100);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(this.BackColor);

                // �����I�����ڂ̕`��
                pen.Color = Color.MidnightBlue;
                for (int i = 0; i < StrokeSizeList.Length; i++)
                {
                    pen.Width = StrokeSizeList[i];
                    int y = (i * 19) + 11;
                    g.DrawLine(pen, 7, y, 57, y);
                }

                // �����I��g�̕`��
                pen.Color = SelectedRectColor;
                pen.Width = 1;
                g.DrawRectangle(pen, 4, strokeSizeIndex * 19 + 2, 55, 18);
            }
            return image;
        }


        /// <summary>
        /// �����I�����x���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void strokeSizeLabel_MouseDown(object sender, MouseEventArgs e)
        {
            using (Graphics g = Graphics.FromImage(strokeSizeLabel.Image))
            {
                // ����܂ł̑I��g������
                pen.Color = this.BackColor;
                g.DrawRectangle(pen, 4, strokeSizeIndex * 19 + 2, 55, 18);

                // �I�����ꂽ�����̎Z�o
                strokeSizeIndex = e.Y / 20;

                // �I��g�̕`��
                pen.Color = SelectedRectColor;
                g.DrawRectangle(pen, 4, strokeSizeIndex * 19 + 2, 55, 18);
            }

            // �����ݒ�
            form.ReviewPanel.StrokeSize = StrokeSizeList[strokeSizeIndex];
            form.SetPenMode();
            strokeSizeLabel.Invalidate();
        }

        #endregion




        #region �����T�C�Y

        /// <summary>
        /// �����T�C�Y�̐ݒ�ɕύX���������Ƃ��̏����B
        /// </summary>
        private void fontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            form.ReviewPanel.TextFontSize = int.Parse(fontSizeComboBox.Text);
            form.SetPenMode();
        }

        #endregion




        #region �L�[�C�x���g

        /// <summary>
        /// �L�[���͂���������e�t�H�[���̃L�[�C�x���g���Ăяo���B
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            form.DoKeyDown(e);
            base.OnKeyDown(e);
        }

        #endregion
    }
}