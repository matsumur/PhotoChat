using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// �n�C�p�[�����N�������݂�\��
    /// </summary>
    public class Hyperlink : PhotoChatNote
    {
        #region �t�B�[���h�E�v���p�e�B

        /// <summary>
        /// �T���l�C���̊O�g��`��Pen
        /// </summary>
        private static readonly Pen outlinePen = new Pen(Color.Blue);
        private string linkedPhotoName;
        private Bitmap thumbnailImage;

        /// <summary>
        /// �\��t����ꂽ�T���l�C���̎ʐ^��
        /// </summary>
        public string LinkedPhotoName
        {
            get { return linkedPhotoName; }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// �n�C�p�[�����N���쐬����B
        /// </summary>
        /// <param name="author">�쐬�������[�U��</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="point">�n�C�p�[�����N�̈ʒu</param>
        /// <param name="photoName">�n�C�p�[�����N���̎ʐ^��</param>
        /// <param name="linkedPhotoName">�n�C�p�[�����N��̎ʐ^��</param>
        public Hyperlink(string author, long serialNumber,
            Point point, string photoName, string linkedPhotoName)
        {
            this.type = TypeHyperlink;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.linkedPhotoName = linkedPhotoName;
            this.range = new Rectangle(
                point.X, point.Y, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
            this.thumbnailImage = Thumbnail.GetImage(
                linkedPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// �f�[�^�����񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        public Hyperlink(string dataString)
        {
            this.type = TypeHyperlink;
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
                sb.Append("Point=").Append(range.X).Append(
                    SubDelimiter).Append(range.Y).Append(PhotoChat.Delimiter);
                sb.Append("PhotoName=").Append(photoName).Append(PhotoChat.Delimiter);
                sb.Append("Link=").Append(linkedPhotoName);
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
                this.linkedPhotoName = dsd["Link"];
                this.thumbnailImage = Thumbnail.GetImage(
                    linkedPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);

                // �\��t���ʒu�̓ǂݎ��
                string str = dsd["Point"];
                int index = str.IndexOf(SubDelimiter);
                int x = int.Parse(str.Substring(0, index));
                int y = int.Parse(str.Substring(index + 1));
                this.range = new Rectangle(
                    x, y, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.ToString());
            }
        }

        #endregion




        /// <summary>
        /// �\��t�����T���l�C�����ړ�����B
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
        /// �\��t����ꂽ�T���l�C����`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        public override void Paint(Graphics g)
        {
            try
            {
                if (thumbnailImage == null)
                {
                    // �T���l�C���摜���擾�ł��Ă��Ȃ��Ƃ��͍ēǂݍ���
                    thumbnailImage = Thumbnail.GetImage(
                        linkedPhotoName, PhotoChat.ThumbnailBoxWidth, PhotoChat.ThumbnailBoxHeight);
                    if (thumbnailImage != null)
                        g.DrawImage(thumbnailImage, range.X, range.Y);
                }
                else
                {
                    g.DrawImage(thumbnailImage, range.X, range.Y);
                }

                // �O�g�`��
                g.DrawRectangle(outlinePen, range);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �\��t����ꂽ�T���l�C�����g��{���ɉ����ĕ`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
        public override void Paint(Graphics g, float scaleFactor)
        {
            try
            {
                // �g��
                float x = range.X * scaleFactor;
                float y = range.Y * scaleFactor;
                float width = range.Width * scaleFactor;
                float height = range.Height * scaleFactor;

                // �T���l�C���摜�͎擾�ł��Ă���Ƃ������`��
                if (thumbnailImage != null)
                    g.DrawImage(thumbnailImage, x, y, width, height);
                g.DrawRectangle(outlinePen, x, y, width, height);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
