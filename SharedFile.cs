using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// ���L�t�@�C���]���N���X
    /// </summary>
    public class SharedFile : ISendable
    {
        #region �t�B�[���h��v���p�e�B

        public const int TypeSoundFile = 41;

        private int type;
        private string id;
        private long serialNumber;
        private string fileName;
        private byte[] data;

        /// <summary>
        /// �^�C�v���擾����B
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        /// <summary>
        /// ID���擾����B
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
        /// �t�@�C�������擾����B
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// �f�[�^���擾����B
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// ���L�t�@�C���]���C���X�^���X���쐬����B
        /// </summary>
        /// <param name="type">�f�[�^�^�C�v</param>
        /// <param name="id">�[��ID</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="fileName">�t�@�C����</param>
        /// <param name="data">�f�[�^</param>
        public SharedFile(int type, string id, long serialNumber, string fileName, byte[] data)
        {
            this.type = type;
            this.id = id;
            this.serialNumber = serialNumber;
            this.fileName = fileName;
            this.data = data;
        }


        /// <summary>
        /// �t�@�C������C���X�^���X���쐬����B
        /// </summary>
        /// <param name="type">�f�[�^�^�C�v</param>
        /// <param name="id">�[��ID</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="filePath">�t�@�C���p�X</param>
        public SharedFile(int type, string id, long serialNumber, string filePath)
        {
            this.type = type;
            this.id = id;
            this.serialNumber = serialNumber;

            // �t�@�C���ǂݍ���
            FileInfo file = new FileInfo(filePath);
            this.fileName = file.Name;
            this.data = new byte[(int)file.Length];
            using (FileStream fs = file.OpenRead())
            {
                fs.Read(data, 0, data.Length);
            }
        }


        /// <summary>
        /// �o�C�g�񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="type">�f�[�^�^�C�v</param>
        /// <param name="dataBytes">�f�[�^�̃o�C�g��</param>
        public SharedFile(int type, byte[] dataBytes)
        {
            this.type = type;

            // �f�[�^������
            int length = BitConverter.ToInt32(dataBytes, 0);
            byte[] temp = new byte[length];
            Array.Copy(dataBytes, 4, temp, 0, length);
            InterpretDataString(PhotoChat.DefaultEncoding.GetString(temp));

            // �f�[�^
            int index = 4 + length;
            length = dataBytes.Length - index;
            data = new byte[length];
            Array.Copy(dataBytes, index, data, 0, length);
        }

        #endregion




        #region ���M�o�C�g�񏈗�

        /// <summary>
        /// �f�[�^�̑��M�o�C�g���Ԃ��B
        /// </summary>
        /// <returns>���M�o�C�g��</returns>
        public byte[] GetDataBytes()
        {
            try
            {
                // �f�[�^��������o�C�g��ɕϊ�
                byte[] fileNameBytes = PhotoChat.DefaultEncoding.GetBytes(GetDataString());
                byte[] lengthBytes = BitConverter.GetBytes(fileNameBytes.Length);

                // �S�̂̃o�C�g����쐬
                int index1 = lengthBytes.Length;
                int index2 = index1 + fileNameBytes.Length;
                byte[] dataBytes = new byte[index2 + data.Length];
                lengthBytes.CopyTo(dataBytes, 0);
                fileNameBytes.CopyTo(dataBytes, index1);
                data.CopyTo(dataBytes, index2);

                return dataBytes;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }


        /// <summary>
        /// �f�[�^�������Ԃ��B
        /// </summary>
        /// <returns>�f�[�^������</returns>
        private string GetDataString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append("ID=").Append(id).Append(PhotoChat.Delimiter);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("FileName=").Append(fileName);
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
                this.id = dsd["ID"];
                this.serialNumber = long.Parse(dsd["SerialNumber"]);
                this.fileName = dsd["FileName"];
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region �ۑ�

        /// <summary>
        /// �t�@�C����ۑ�����B
        /// </summary>
        /// <param name="directory">�ۑ���f�B���N�g���̃p�X</param>
        public void Save(string directory)
        {
            try
            {
                string file = Path.Combine(directory, fileName);
                using (FileStream fs = File.Create(file))
                {
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���[�UIndex�������Ԃ��B
        /// </summary>
        /// <returns>���[�UIndex������</returns>
        public string GetIndexString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(100);
                sb.Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(fileName);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }

        #endregion
    }
}
