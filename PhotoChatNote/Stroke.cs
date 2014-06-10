using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PhotoChat
{
    /// <summary>
    /// �X�g���[�N�i�菑���j�������݂�\��
    /// </summary>
    public class Stroke : PhotoChatNote
    {
        #region �t�B�[���h�E�v���p�e�B

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
        /// �X�g���[�N�̓_����擾����B
        /// </summary>
        public Point[] Points
        {
            get { return points.ToArray(); }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// �X�g���[�N�C���X�^���X�����������A�X�g���[�N���J�n����B
        /// </summary>
        /// <param name="author">�������݂��s�������[�U��</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="photoName">�������ݑΏۂ̎ʐ^��</param>
        /// <param name="color">�y���̐F</param>
        /// <param name="strokeWidth">�y���̑���</param>
        /// <param name="p">�X�g���[�N�̊J�n�_</param>
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
        /// �f�[�^�����񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        public Stroke(string dataString)
        {
            this.type = TypeStroke;
            InterpretDataString(dataString);
        }

        #endregion




        #region �f�[�^�����񏈗�

        /// <summary>
        /// �e�l�̏������f�[�^�������Ԃ��B
        /// </summary>
        /// <returns>�f�[�^������</returns>
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
        /// �f�[�^�������ǂݎ��e�l��ݒ肷��B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
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

                // �_�f�f�[�^�̓ǂݎ��
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

                // �X�g���[�N���Œ肵�͈͂��v�Z����
                Fix();
                FigureRange();
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region �y���ݒ�

        /// <summary>
        /// 2�̃y����ݒ肷��B
        /// </summary>
        private void SetPen()
        {
            try
            {
                // ����̃y��
                this.mainPen = new Pen(color, strokeWidth);
                mainPen.StartCap = LineCap.Round;
                mainPen.EndCap = LineCap.Round;

                // �A�E�g���C���̃y��
                this.outlinePen = new Pen(OutlineColor, strokeWidth + OutlineWidth);
                outlinePen.StartCap = LineCap.Round;
                outlinePen.EndCap = LineCap.Round;

                // �y�����̔����̒������L��
                this.penWidthHalf = (strokeWidth + OutlineWidth) / 2;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �X�g���[�N����

        /// <summary>
        /// �X�g���[�N�̓_��ǉ�����B
        /// </summary>
        /// <param name="p">�ǉ�����_�̍��W</param>
        public void AddPoint(Point p)
        {
            try
            {
                points.Add(p);

                // �������ɔ͈͂��L����
                if (p.X - penWidthHalf < range.Left)
                {
                    range.Width += range.X - p.X + penWidthHalf;
                    range.X = p.X - penWidthHalf;
                }
                else if (range.Right < p.X + penWidthHalf)
                {
                    range.Width = p.X + penWidthHalf - range.X;
                }

                // �c�����ɔ͈͂��L����
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
        /// �X�g���[�N���Œ肷��B
        /// </summary>
        public void Fix()
        {
            try
            {
                // 1�_�����Ȃ��Ƃ��͂���1�_�ǉ�
                if (points.Count == 1)
                {
                    points.Add(new Point(points[0].X + 1, points[0].Y));
                }

                // �͈͌v�Z�iAddPoint�Ŕ͈͌v�Z����Ȃ�K�v�Ȃ��j
                //FigureRange();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �X�g���[�N�͈̔͂��v�Z����B
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
        /// �X�g���[�N�������݂��ړ�����B
        /// </summary>
        /// <param name="dx">�E�����Ɉړ����鋗��</param>
        /// <param name="dy">�������Ɉړ����鋗��</param>
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




        #region �`��

        /// <summary>
        /// �X�g���[�N��`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
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
        /// �X�g���[�N���g��{���ɉ����ĕ`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
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
        /// �X�g���[�N�̉�����`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
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
        /// �X�g���[�N�̉������g��{���ɉ����ĕ`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
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
