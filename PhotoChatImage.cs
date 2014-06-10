using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PhotoChat
{
    /// <summary>
    /// �摜�f�[�^�̒��ۊ�{�N���X
    /// </summary>
    public abstract class PhotoChatImage : IComparable, IComparable<PhotoChatImage>, ISendable
    {
        public const int TypePhoto = 11;


        #region �t�B�[���h�E�v���p�e�B

        protected int type;
        protected string author;
        protected string id;
        protected long serialNumber;
        protected DateTime date;
        protected string photoName;
        protected string infoText;
        protected Bitmap image;


        /// <summary>
        /// �摜�f�[�^�̃^�C�v���擾����B
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        /// <summary>
        /// �B�e�Җ����擾����B
        /// </summary>
        public string Author
        {
            get { return author; }
            set {
                this.author = value;
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
        }

        /// <summary>
        /// �B�e�����[��ID���擾����B
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// �ʂ��ԍ����擾����B
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }

        /// <summary>
        /// �B�e�������擾�E�ݒ肷��B
        /// </summary>
        public DateTime Date
        {
            get { return date; }
            set {
                this.date = value;
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
        }

        /// <summary>
        /// �ʐ^���i�[��ID_�ʂ��ԍ��j���擾����B
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }

        /// <summary>
        /// �摜�̊ȈՏ�񕶎�����擾����B
        /// </summary>
        public string InfoText
        {
            get { return infoText; }
        }

        /// <summary>
        /// �摜���擾����B
        /// </summary>
        public Bitmap Image
        {
            get { return image; }
        }

        #endregion




        #region �f�X�g���N�^

        /// <summary>
        /// ���\�[�X���������B
        /// </summary>
        public virtual void Dispose()
        {
            try
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region Set/Get

        /// <summary>
        /// ID�E�ʂ��ԍ�����t�@�C������ݒ肷��B
        /// </summary>
        protected void SetPhotoName()
        {
            try
            {
                StringBuilder sb = new StringBuilder(id, 50);
                sb.Append('_').Append(serialNumber);
                photoName = sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �e�l�̏������f�[�^�������Ԃ��B
        /// </summary>
        /// <returns>�f�[�^������</returns>
        public abstract string GetDataString();


        /// <summary>
        /// �f�[�^�̃o�C�g��i�f�[�^������Ɖ摜�f�[�^�̘A���j��Ԃ��B
        /// �f�[�^������̒�����\��4�o�C�g�{�f�[�^������{�摜�f�[�^
        /// </summary>
        /// <returns>�f�[�^�̃o�C�g��</returns>
        public abstract byte[] GetDataBytes();

        #endregion




        #region static���\�b�h

        /// <summary>
        /// �f�[�^�̃o�C�g�񂩂�摜�f�[�^�̃C���X�^���X���쐬����B
        /// �f�[�^���s�K�؂Ȃ��̂ł������ꍇ��null��Ԃ��B
        /// </summary>
        /// <param name="type">�摜�f�[�^�̃^�C�v</param>
        /// <param name="dataBytes">�f�[�^�̃o�C�g��</param>
        /// <returns>�쐬�����C���X�^���X�B�쐬�ł��Ȃ������Ƃ���null��Ԃ��B</returns>
        public static PhotoChatImage CreateInstance(int type, byte[] dataBytes)
        {
            try
            {
                string dataString;
                byte[] imageBytes;
                InterpretDataBytes(dataBytes, out dataString, out imageBytes);

                PhotoChatImage image = null;
                switch (type)
                {
                    case TypePhoto:
                        image = new Photo(dataString, imageBytes);
                        break;
                }
                return image;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �f�[�^�̃o�C�g����f�[�^������Ɖ摜�̃o�C�i���ɕ�������B
        /// </summary>
        /// <param name="dataBytes">�f�[�^�̃o�C�g��</param>
        /// <param name="dataString">���������f�[�^��������i�[����string</param>
        /// <param name="imageBytes">���������摜�̃o�C�i�����i�[����byte[]</param>
        private static void InterpretDataBytes(
            byte[] dataBytes, out string dataString, out byte[] imageBytes)
        {
            try
            {
                // �f�[�^������̃R�s�[
                int length = BitConverter.ToInt32(dataBytes, 0);
                dataString = PhotoChat.DefaultEncoding.GetString(dataBytes, sizeof(int), length);

                // �摜�f�[�^�̃R�s�[
                int index = sizeof(int) + length;
                length = dataBytes.Length - index;
                imageBytes = new byte[length];
                Array.Copy(dataBytes, index, imageBytes, 0, length);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                dataString = string.Empty;
                imageBytes = new byte[0];
            }
        }


        /// <summary>
        /// �͈͓��Ɏ��܂�悤�ɏk�������摜���쐬����B
        /// ���Ƃ��Ɣ͈͓��Ɏ��܂��Ă��Ă��V���ȃC���X�^���X���쐬����B
        /// </summary>
        /// <param name="original">�k������摜</param>
        /// <param name="maxWidth">���̏��</param>
        /// <param name="maxHeight">�����̏��</param>
        /// <returns>�k�������摜</returns>
        public static Bitmap ResizeImage(Image original, int maxWidth, int maxHeight)
        {
            if (original.Width > maxWidth || original.Height > maxHeight)
            {
                try
                {
                    // �c������ێ������k���T�C�Y���v�Z
                    float widthScale = (float)maxWidth / (float)original.Width;
                    float heightScale = (float)maxHeight / (float)original.Height;
                    float scaleFactor = Math.Min(widthScale, heightScale);
                    int width = (int)(original.Width * scaleFactor);
                    int height = (int)(original.Height * scaleFactor);

                    // �k���摜�̍쐬
                    return new Bitmap(original, width, height);
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
            return new Bitmap(original);
        }

        #endregion




        #region IComparable �����o

        /// <summary>
        /// ���݂̃C���X�^���X�𓯂��^�̕ʂ̃I�u�W�F�N�g�Ɣ�r����B
        /// </summary>
        /// <param name="obj">���̃C���X�^���X�Ɣ�r����I�u�W�F�N�g</param>
        /// <returns>��r����</returns>
        public int CompareTo(object obj)
        {
            // �^�𒲂ׂ�
            PhotoChatImage another = obj as PhotoChatImage;
            if (another == null)
            {
                throw new ArgumentException();
            }

            // ��r
            return CompareTo(another);
        }


        /// <summary>
        /// ���݂�PhotoChatImage��ʂ�PhotoChatImage�Ɣ�r����B
        /// </summary>
        /// <param name="another">��r����PhotoChatImage</param>
        /// <returns>��r����</returns>
        public int CompareTo(PhotoChatImage another)
        {
            int result = date.CompareTo(another.Date);
            if (result == 0)
                result = id.CompareTo(another.ID);
            return result;
        }

        #endregion
    }
}
