using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// �����R�}���h���J�v�Z����
    /// </summary>
    public class Command : ISendable
    {
        #region �t�B�[���h�E�v���p�e�B

        public const int TypeRequest = 31;
        public const int TypeInform = 32;
        public const int TypeTransfer = 33;
        public const int TypeSelect = 34;
        public const int TypeConnect = 35;
        public const int TypeDisconnect = 36;
        public const int TypeLogin = 37;
        public const int TypeSession = 38;


        /// <summary>
        /// �R�}���h�̃^�C�v
        /// </summary>
        public int Type
        {
            get { return type; }
        }
        private int type;

        /// <summary>
        /// �ʂ��ԍ�
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }
        private long serialNumber;

        /// <summary>
        /// �^�C���X�^���v
        /// </summary>
        public DateTime TimeStamp
        {
            get { return timeStamp; }
        }
        private DateTime timeStamp;

        /// <summary>
        /// �f�[�^�쐬�҂̒[��ID
        /// </summary>
        public string AuthorID
        {
            get { return authorID; }
        }
        private string authorID;

        /// <summary>
        /// �R�}���h���M���̒[��ID
        /// </summary>
        public string SourceID
        {
            get { return sourceID; }
        }
        private string sourceID;

        /// <summary>
        /// ���[�U��
        /// </summary>
        public string UserName
        {
            get { return userName; }
        }
        private string userName;

        /// <summary>
        /// �ʐ^��
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }
        private string photoName;

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// �R�}���h���쐬����B
        /// �C���X�^���X�쐬���\�b�h����Ăяo�����B
        /// </summary>
        /// <param name="type">�R�}���h�̃^�C�v</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="timeStamp">�^�C���X�^���v</param>
        /// <param name="authorID">�f�[�^�쐬�҂̒[��ID</param>
        /// <param name="sourceID">�R�}���h���M���̒[��ID</param>
        /// <param name="userName">���[�U��</param>
        /// <param name="photoName">�ʐ^��</param>
        private Command(int type, long serialNumber, DateTime timeStamp,
            string authorID, string sourceID, string userName, string photoName)
        {
            this.type = type;
            this.serialNumber = serialNumber;
            this.timeStamp = timeStamp;
            this.authorID = authorID;
            this.sourceID = sourceID;
            this.userName = userName;
            this.photoName = photoName;
        }


        /// <summary>
        /// �f�[�^�̃o�C�g�񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="type">�R�}���h�̃^�C�v</param>
        /// <param name="dataBytes">�f�[�^�̃o�C�g��</param>
        public Command(int type, byte[] dataBytes)
        {
            this.type = type;
            InterpretDataBytes(dataBytes);
        }

        #endregion




        #region �f�[�^�����񏈗�

        /// <summary>
        /// �e�l�̏������f�[�^�̃o�C�g���Ԃ��B
        /// </summary>
        /// <returns>�f�[�^�̃o�C�g��</returns>
        public byte[] GetDataBytes()
        {
            try
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("TimeStamp=").Append(timeStamp.Ticks).Append(PhotoChat.Delimiter);
                sb.Append("AuthorID=").Append(authorID).Append(PhotoChat.Delimiter);
                sb.Append("SourceID=").Append(sourceID).Append(PhotoChat.Delimiter);
                sb.Append("UserName=").Append(userName).Append(PhotoChat.Delimiter);
                sb.Append("PhotoName=").Append(photoName);
                return PhotoChat.DefaultEncoding.GetBytes(sb.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }


        /// <summary>
        /// �f�[�^�̃o�C�g���ǂݎ��e�l��ݒ肷��B
        /// </summary>
        /// <param name="dataBytes">�f�[�^�̃o�C�g��</param>
        private void InterpretDataBytes(byte[] dataBytes)
        {
            try
            {
                string dataString = PhotoChat.DefaultEncoding.GetString(dataBytes);
                DataStringDictionary dsd = new DataStringDictionary(dataString);
                this.serialNumber = long.Parse(dsd["SerialNumber"]);
                this.timeStamp = new DateTime(long.Parse(dsd["TimeStamp"]));
                this.authorID = dsd["AuthorID"];
                this.sourceID = dsd["SourceID"];
                this.userName = dsd["UserName"];
                this.photoName = dsd["PhotoName"];
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region �R�}���h�쐬

        /// <summary>
        /// �f�[�^���M�v���R�}���h���쐬����B
        /// </summary>
        /// <param name="serialNumber">�v������f�[�^�̒ʂ��ԍ�</param>
        /// <param name="authorID">�v������f�[�^�̍쐬��ID</param>
        /// <param name="sourceID">�R�}���h���M����ID</param>
        /// <returns>�f�[�^���M�v���R�}���h</returns>
        public static Command CreateRequestCommand(
            long serialNumber, string authorID, string sourceID)
        {
            try
            {
                return new Command(
                    TypeRequest, serialNumber, DateTime.Now, authorID, sourceID, "null", "null");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �V���f�[�^�ʒm�R�}���h���쐬����B
        /// </summary>
        /// <param name="image">�V���摜�f�[�^</param>
        /// <param name="sourceID">�R�}���h���M����ID</param>
        /// <returns>�V���f�[�^�ʒm�R�}���h</returns>
        public static Command CreateInformCommand(PhotoChatImage image, string sourceID)
        {
            try
            {
                return new Command(TypeInform, image.SerialNumber,
                    DateTime.Now, image.ID, sourceID, image.Author, image.PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �V���f�[�^�ʒm�R�}���h���R�}���h���쐬����B
        /// </summary>
        /// <param name="note">�V���������݃f�[�^</param>
        /// <param name="sourceID">�R�}���h���M����ID</param>
        /// <returns>�V���f�[�^�ʒm�R�}���h</returns>
        public static Command CreateInformCommand(PhotoChatNote note, string sourceID)
        {
            try
            {
                return new Command(TypeInform, note.SerialNumber,
                    DateTime.Now, note.ID, sourceID, note.Author, note.PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �V���f�[�^�ʒm�R�}���h���R�}���h���쐬����B
        /// </summary>
        /// <param name="sharedFile">�V�����L�t�@�C��</param>
        /// <param name="sourceID">�R�}���h���M����ID</param>
        /// <returns>�V���f�[�^�ʒm�R�}���h</returns>
        public static Command CreateInformCommand(SharedFile sharedFile, string sourceID)
        {
            try
            {
                return new Command(TypeInform, sharedFile.SerialNumber,
                    DateTime.Now, sharedFile.ID, sourceID, "null", "null");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// ID�ɑΉ�����ێ��f�[�^�ʒm�R�}���h���쐬����B
        /// </summary>
        /// <param name="floorNumber">�ێ��f�[�^�̍ŏ��ԍ�</param>
        /// <param name="ceilingNumber">�ێ��f�[�^�̍ő�ԍ�</param>
        /// <param name="authorID">�Ώۂ�ID</param>
        /// <param name="sourceID">�R�}���h���M����ID</param>
        /// <returns>�ێ��f�[�^�ʒm�R�}���h</returns>
        public static Command CreateTransferCommand(
            long floorNumber, long ceilingNumber, string authorID, string sourceID)
        {
            try
            {
                return new Command(TypeTransfer, ceilingNumber,
                    DateTime.Now, authorID, sourceID, "null", floorNumber.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �ʐ^�I���R�}���h���쐬����B
        /// </summary>
        /// <param name="id">�I�����s�������[�U�̒[��ID</param>
        /// <param name="userName">�I�����s�������[�U��</param>
        /// <param name="photoName">�I�������ʐ^��</param>
        /// <returns>�ʐ^�I���R�}���h</returns>
        public static Command CreateSelectCommand(string id, string userName, string photoName)
        {
            try
            {
                return new Command(TypeSelect, 0, DateTime.Now, id, id, userName, photoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �ڑ��ʒm�R�}���h���쐬����B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="userName">���[�U��</param>
        /// <returns>�ڑ��ʒm�R�}���h</returns>
        public static Command CreateConnectCommand(string id, string userName)
        {
            try
            {
                return new Command(TypeConnect, 0, DateTime.Now, id, id, userName, "null");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �ؒf�ʒm�R�}���h���쐬����B
        /// </summary>
        /// <param name="authorID">�ؒf�����[��ID</param>
        /// <param name="sourceID">�R�}���h���M���̒[��ID</param>
        /// <returns>�ؒf�ʒm�R�}���h</returns>
        public static Command CreateDisconnectCommand(string authorID, string sourceID)
        {
            try
            {
                return new Command(TypeDisconnect, 0,
                    DateTime.Now, authorID, sourceID, "null", "Disconnect");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// ���O�C���R�}���h���쐬����B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="mailAddress">���[���A�h���X</param>
        /// <param name="password">�p�X���[�h</param>
        /// <param name="userName">���[�U��</param>
        /// <returns>���O�C���R�}���h</returns>
        public static Command CreateLoginCommand(
            string id, string mailAddress, string password, string userName)
        {
            try
            {
                return new Command(TypeLogin, 0, DateTime.Now, id, password, userName, mailAddress);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �Z�b�V�����A�b�v���[�h�ʒm�R�}���h���쐬����B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <param name="sessionName">�Z�b�V������</param>
        /// <param name="isPublic">�S�̌��J���邩�ǂ���</param>
        /// <returns>�Z�b�V�����A�b�v���[�h�ʒm�R�}���h</returns>
        public static Command CreateSessionCommand(
            string id, string sessionID, string sessionName, bool isPublic)
        {
            try
            {
                if (isPublic)
                    return new Command(TypeSession, 1, DateTime.Now, id, id, sessionID, sessionName);
                else
                    return new Command(TypeSession, 0, DateTime.Now, id, id, sessionID, sessionName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }

        #endregion
    }
}
