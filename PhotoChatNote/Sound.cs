using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// ���������N��\���B
    /// </summary>
    public class Sound : PhotoChatNote
    {
        #region �t�B�[���h�E�v���p�e�B
        
        private static readonly Bitmap soundImage = Properties.Resources.sound;
        private string soundFileName;

        /// <summary>
        /// �����t�@�C�������擾����B
        /// </summary>
        public string SoundFileName
        {
            get { return soundFileName; }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// ���������N���쐬����B
        /// </summary>
        /// <param name="author">�����f�[�^��\��t�������[�U��</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <param name="photoName">�\��t�����ʐ^��</param>
        /// <param name="point">�\��t�����ʒu</param>
        /// <param name="soundFileName">�����f�[�^�̃t�@�C����</param>
        public Sound(string author, long serialNumber,
            string photoName, Point point, string soundFileName)
        {
            this.type = TypeSound;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.range = new Rectangle(point, soundImage.Size);
            this.soundFileName = soundFileName;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// �f�[�^�����񂩂�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        public Sound(string dataString)
        {
            this.type = TypeSound;
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
                sb.Append("Point=").Append(range.X).Append(
                    SubDelimiter).Append(range.Y).Append(PhotoChat.Delimiter);
                sb.Append("SoundFileName=").Append(soundFileName);
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
                this.soundFileName = dsd["SoundFileName"];
                this.infoText = author + date.ToString(PhotoChat.DateFormat);

                // �ʒu�̓ǂݎ��
                string str = dsd["Point"];
                int index = str.IndexOf(SubDelimiter);
                int x = int.Parse(str.Substring(0, index));
                int y = int.Parse(str.Substring(index + 1));
                this.range = new Rectangle(new Point(x, y), soundImage.Size);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        /// <summary>
        /// ���������N���ړ�����B
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
        /// �����A�C�R����`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        public override void Paint(Graphics g)
        {
            try
            {
                g.DrawImage(soundImage, range.Location);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �����A�C�R�����g��{���ɉ����ĕ`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
        public override void Paint(Graphics g, float scaleFactor)
        {
            try
            {
                // �g��`��
                float x = range.X * scaleFactor;
                float y = range.Y * scaleFactor;
                float width = range.Width * scaleFactor;
                float height = range.Height * scaleFactor;
                g.DrawImage(soundImage, x, y, width, height);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
