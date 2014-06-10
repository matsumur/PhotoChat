using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// �������ݍ폜��\��
    /// </summary>
    public class Removal : PhotoChatNote
    {
        #region �t�B�[���h�E�v���p�e�B

        /// <summary>
        /// �폜�Ώۂ�ID
        /// </summary>
        public string TargetID
        {
            get { return targetID; }
        }
        private string targetID;

        /// <summary>
        /// �폜�Ώۂ̒ʂ��ԍ�
        /// </summary>
        public long TargetSerialNumber
        {
            get { return targetSerialNumber; }
        }
        private long targetSerialNumber;

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
        /// �������ݍ폜�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�폜���s�������[�U��</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="photoName">�������݂̂���ʐ^��</param>
        /// <param name="targetID">�폜�Ώۂ�ID</param>
        /// <param name="targetSerialNumber">�폜�Ώۂ̒ʂ��ԍ�</param>
        public Removal(
            string author,
            long serialNumber,
            string photoName,
            string targetID,
            long targetSerialNumber)
        {
            this.type = TypeRemoval;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.targetID = targetID;
            this.targetSerialNumber = targetSerialNumber;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// �f�[�^�����񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="textData">�f�[�^������</param>
        public Removal(string dataString)
        {
            this.type = TypeRemoval;
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
                sb.Append("TargetID=").Append(targetID).Append(PhotoChat.Delimiter);
                sb.Append("TargetNumber=").Append(targetSerialNumber);
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
                this.targetID = dsd["TargetID"];
                this.targetSerialNumber = long.Parse(dsd["TargetNumber"]);
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
