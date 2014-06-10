using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// �L�[�{�[�h���͂����������݂�\��
    /// </summary>
    public class Text : PhotoChatNote
    {
        private SolidBrush brush;
        private Font font;
        private string bodyText;




        #region �R���X�g���N�^�E�f�X�g���N�^

        /// <summary>
        /// �e�L�X�g�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�^�O�t���������[�U��</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="photoName">�^�O�t�������ʐ^��</param>
        /// <param name="color">�e�L�X�g�̐F</param>
        /// <param name="textFontSize">�t�H���g�T�C�Y</param>
        /// <param name="point">�e�L�X�g�̈ʒu</param>
        /// <param name="bodyText">�������񂾕�����</param>
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
        /// �f�[�^�����񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        public Text(string dataString)
        {
            this.type = TypeText;
            InterpretDataString(dataString);
        }


        /// <summary>
        /// �f�X�g���N�^
        /// </summary>
        ~Text()
        {
            brush.Dispose();
            font.Dispose();
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
                Color color = Color.FromArgb(int.Parse(dsd["Color"]));
                this.brush = new SolidBrush(color);
                int fontSize = int.Parse(dsd["Size"]);
                this.font = new Font(PhotoChat.FontName, fontSize, FontStyle.Bold);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);

                // �e�L�X�g�̈ʒu�̓ǂݎ��
                string str = dsd["Point"];
                int index = str.IndexOf(SubDelimiter);
                int x = int.Parse(str.Substring(0, index));
                int y = int.Parse(str.Substring(index + 1));
                Point point = new Point(x, y);

                // �e�L�X�g�̓ǂݎ��
                str = dsd["Body"];
                this.bodyText = str.Replace("< BR >", Environment.NewLine);

                // �������ݗ̈�����߂�
                this.range = GetRange(point);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        /// <summary>
        /// �������ݗ̈���v�Z����B
        /// </summary>
        /// <param name="point">�e�L�X�g�̍���̍��W</param>
        /// <returns>�������ݗ̈�</returns>
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
        /// �e�L�X�g���ړ�����B
        /// </summary>
        /// <param name="dx">�E�����Ɉړ����鋗��</param>
        /// <param name="dy">�������Ɉړ����鋗��</param>
        public override void Move(int dx, int dy)
        {
            range.X += dx;
            range.Y += dy;
        }




        #region �`��

        /// <summary>
        /// �e�L�X�g��`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
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
        /// �e�L�X�g���g��{���ɉ����ĕ`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
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
