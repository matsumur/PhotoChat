using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConversationFieldDetector
{
    /// <summary>
    /// �����f�[�^�i�[�N���X
    /// </summary>
    public class SoundData:ConversationFieldDetector.ISendable
    {
        #region �t�B�[���h�E�v���p�e�B

        public const int TypeSoundData = 111;
        private long time;
        private double[] data;
        private string id;


        /// <summary>
        /// �f�[�^�^�C�v���擾����B
        /// </summary>
        public int Type
        {
            get { return TypeSoundData; }
        }

        /// <summary>
        /// �f�[�^�������擾����B
        /// </summary>
        public long Time
        {
            get { return time; }
            set { time = value; }
        }

        /// <summary>
        /// �����f�[�^�̃o�C�g����擾����B
        /// </summary>
        public double[] Data
        {
            get { return data; }
        }

        /// <summary>
        /// �[��ID���擾����B
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        #endregion


        #region �R���X�g���N�^�E�C���X�^���X�č\��

        /// <summary>
        /// ���M�p�����f�[�^���쐬����B
        /// </summary>
        /// <param name="time">�f�[�^�̘^���J�n����</param>
        /// <param name="data">�����f�[�^</param>
        /// <param name="id">�[��ID</param>
        public SoundData(long time, double[] data, string id)
        {
            this.time = time;
            this.id = id;
            this.data = new double[data.Length];
            data.CopyTo(this.data, 0);
        }


        /// <summary>
        /// �o�C�g�񂩂�C���X�^���X���č\������B
        /// </summary>
        /// <param name="dataBytes">�����f�[�^�̃o�C�g��</param>
        /// <returns>�č\�������C���X�^���X</returns>
        public static SoundData CreateInstance(byte[] dataBytes)
        {
            try
            {
                // �����̓ǂݎ��
                long time = BitConverter.ToInt64(dataBytes, 0);
                int index = sizeof(long);

                // �[��ID�̓ǂݎ��
                int idLength = BitConverter.ToInt32(dataBytes, index);
                index += sizeof(int);
                string id = Encoding.UTF8.GetString(dataBytes, index, idLength);
                index += idLength;

                // �����f�[�^�̓ǂݎ��
                int dataLength = (dataBytes.Length - index) / sizeof(double);
                double[] data = new double[dataLength];
                for (int i = 0; i < dataLength; i++)
                {
                    data[i] = BitConverter.ToDouble(dataBytes, index);
                    index += sizeof(double);
                }

                return new SoundData(time, data, id);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return null;
            }
        }

        #endregion


        /// <summary>
        /// �ʐM�ő���f�[�^�ɕϊ�
        /// </summary>
        /// <returns>���M�o�C�g��</returns>
        public byte[] GetDataBytes()
        {
            try
            {
                // �����ƒ[��ID���o�C�g��ɕϊ�
                byte[] timeBytes = BitConverter.GetBytes(time);
                byte[] idBytes = Encoding.UTF8.GetBytes(id);
                byte[] idLengthBytes = BitConverter.GetBytes(idBytes.Length);

                // �S�̂̃o�C�g����쐬
                int totalLength = timeBytes.Length + idLengthBytes.Length
                    + idBytes.Length + data.Length * sizeof(double);
                byte[] dataBytes = new byte[totalLength];

                // �����ƒ[��ID���R�s�[
                int index = 0;
                timeBytes.CopyTo(dataBytes, index);
                index += timeBytes.Length;
                idLengthBytes.CopyTo(dataBytes, index);
                index += idLengthBytes.Length;
                idBytes.CopyTo(dataBytes, index);
                index += idBytes.Length;

                // �����f�[�^���R�s�[
                for (int i = 0; i < data.Length; i++)
                {
                    BitConverter.GetBytes(data[i]).CopyTo(dataBytes, index);
                    index += sizeof(double);
                }

                return dataBytes;
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }
    }
}