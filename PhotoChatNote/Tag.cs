using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// �^�O�t���������݂�\��
    /// </summary>
    public class Tag : PhotoChatNote
    {
        #region �t�B�[���h�E�v���p�e�B

        /// <summary>
        /// �^�O������
        /// </summary>
        public string TagString
        {
            get { return tagString; }
        }
        private string tagString;

        /// <summary>
        /// �������݂͈̔́i���0�͈́j
        /// </summary>
        public override Rectangle Range
        {
            get { return new Rectangle(0, 0, 0, 0); }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// �^�O�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�^�O�t���������[�U��</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="photoName">�^�O�t�������ʐ^��</param>
        /// <param name="tagString">�^�O������</param>
        public Tag(string author, long serialNumber, string photoName, string tagString)
        {
            this.type = TypeTag;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.tagString = tagString;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// �f�[�^�����񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        public Tag(string dataString)
        {
            this.type = TypeTag;
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
                sb.Append("Tag=").Append(tagString);
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
                this.tagString = dsd["Tag"];
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        /// <summary>
        /// �_���������݂͈͓̔��ɂ��邩�ǂ����𔻒肷��B
        /// </summary>
        /// <param name="point">�_�̍��W</param>
        /// <returns>���false</returns>
        public override bool Contains(Point point)
        {
            return false;
        }
    }
}
