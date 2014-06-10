using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// �������݃f�[�^�̒��ۊ�{�N���X
    /// </summary>
    public abstract class PhotoChatNote : IComparable, IComparable<PhotoChatNote>, ISendable
    {
        #region �t�B�[���h�E�v���p�e�B

        public const int TypeStroke = 21;
        public const int TypeText = 22;
        public const int TypeHyperlink = 23;
        public const int TypeRemoval = 24;
        public const int TypeTag = 25;
        public const int TypeSound = 26;
        protected const char SubDelimiter = ',';
        protected static Brush infoBrush = new SolidBrush(PhotoChat.InfoColor);

        protected int type;
        protected string author;
        protected string id;
        protected long serialNumber;
        protected DateTime date;
        protected string photoName;
        protected string infoText;
        protected Rectangle range;


        /// <summary>
        /// �������݂̃^�C�v
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        /// <summary>
        /// �������݂��s�������[�U��
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
        /// �������݂��s��ꂽ�[����ID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// �ʂ��ԍ�
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }

        /// <summary>
        /// �������݂��s��ꂽ����
        /// </summary>
        public DateTime Date
        {
            get { return date; }
            set
            {
                this.date = value;
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
            }
        }

        /// <summary>
        /// �������݂��s��ꂽ�ʐ^��
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }

        /// <summary>
        /// �������݂Ɋւ����񕶎���
        /// </summary>
        public string InfoText
        {
            get { return infoText; }
        }

        /// <summary>
        /// �������݂͈̔�
        /// </summary>
        public virtual Rectangle Range
        {
            get { return range; }
        }

        #endregion




        #region Set/Get

        /// <summary>
        /// �e�l�̏������f�[�^�������Ԃ��B
        /// </summary>
        /// <returns>�f�[�^������</returns>
        public abstract string GetDataString();


        /// <summary>
        /// �������݃f�[�^�ۑ��p�������Ԃ��B
        /// </summary>
        /// <returns>�������݃f�[�^�ۑ��p������</returns>
        public string GetSaveString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(300);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(GetDataString());
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
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
                StringBuilder sb = new StringBuilder(300);
                sb.Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(GetDataString());
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// �f�[�^�̃o�C�g��i�f�[�^��������o�C�g��ɕϊ��������́j��Ԃ��B
        /// </summary>
        /// <returns>�f�[�^�̃o�C�g��</returns>
        public byte[] GetDataBytes()
        {
            return PhotoChat.DefaultEncoding.GetBytes(GetDataString());
        }

        #endregion




        #region static���\�b�h

        /// <summary>
        /// �f�[�^�̃o�C�g�񂩂珑�����݃f�[�^�̃C���X�^���X���쐬����B
        /// �f�[�^���s�K�؂Ȃ��̂ł������Ƃ���null��Ԃ��B
        /// </summary>
        /// <param name="type">�������݂̃^�C�v</param>
        /// <param name="dataBytes">�f�[�^�̃o�C�g��</param>
        /// <returns>�쐬�����C���X�^���X�B�쐬�ł��Ȃ������Ƃ���null��Ԃ��B</returns>
        public static PhotoChatNote CreateInstance(int type, byte[] dataBytes)
        {
            try
            {
                string dataString = PhotoChat.DefaultEncoding.GetString(dataBytes);
                return CreateInstance(type, dataString);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �f�[�^�����񂩂珑�����݃f�[�^�̃C���X�^���X���쐬����B
        /// �f�[�^���s�K�؂Ȃ��̂ł������Ƃ���null��Ԃ��B
        /// </summary>
        /// <param name="type">�������݂̃^�C�v</param>
        /// <param name="dataString">�f�[�^������</param>
        /// <returns>�쐬�����C���X�^���X�B�쐬�ł��Ȃ������Ƃ���null��Ԃ��B</returns>
        public static PhotoChatNote CreateInstance(int type, string dataString)
        {
            try
            {
                PhotoChatNote note = null;
                switch (type)
                {
                    case TypeStroke:
                        note = new Stroke(dataString);
                        break;

                    case TypeRemoval:
                        note = new Removal(dataString);
                        break;

                    case TypeHyperlink:
                        note = new Hyperlink(dataString);
                        break;

                    case TypeTag:
                        note = new Tag(dataString);
                        break;

                    case TypeText:
                        note = new Text(dataString);
                        break;

                    case TypeSound:
                        note = new Sound(dataString);
                        break;
                }
                return note;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �������݃t�@�C���̃p�X��Ԃ��B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�������݃t�@�C���̃p�X</returns>
        public static string GetNotesFilePath(string photoName)
        {
            return Path.Combine(PhotoChat.NotesDirectory, photoName + ".dat");
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
            PhotoChatNote another = obj as PhotoChatNote;
            if(another == null)
            {
                throw new ArgumentException();
            }

            // ��r
            return CompareTo(another);
        }


        /// <summary>
        /// ���݂�PhotoChatNote��ʂ�PhotoChatNote�Ɣ�r����B
        /// </summary>
        /// <param name="another">��r����PhotoChatNote</param>
        /// <returns>��r����</returns>
        public int CompareTo(PhotoChatNote another)
        {
            int result = date.CompareTo(another.Date);
            if (result == 0)
                result = id.CompareTo(another.ID);
            return result;
        }

        #endregion




        /// <summary>
        /// �������݃f�[�^���t�@�C���ɏ������ށB
        /// </summary>
        public void Save()
        {
            try
            {
                string filePath = GetNotesFilePath(photoName);
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine(GetSaveString());
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �_���������݂͈͓̔��ɂ��邩�ǂ����𔻒肷��B
        /// </summary>
        /// <param name="point">�_�̍��W</param>
        /// <returns>�������݂͈͓̔��ɂ����true</returns>
        public virtual bool Contains(Point point)
        {
            return range.Contains(point);
        }


        /// <summary>
        /// �������݂��ړ�����B
        /// �T�u�N���X�Ŏ�������Ȃ��ꍇ�͉������Ȃ��B
        /// </summary>
        /// <param name="dx">�E�����Ɉړ����鋗��</param>
        /// <param name="dy">�������Ɉړ����鋗��</param>
        public virtual void Move(int dx, int dy) { }




        #region �`��

        /// <summary>
        /// �������݂�`�悷��B
        /// �T�u�N���X�Ŏ�������Ȃ��ꍇ�͉������Ȃ��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        public virtual void Paint(Graphics g) { }


        /// <summary>
        /// �������݂��g��{���ɉ����ĕ`�悷��B
        /// �T�u�N���X�Ŏ�������Ȃ��ꍇ�͉������Ȃ��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
        public virtual void Paint(Graphics g, float scaleFactor) { }


        /// <summary>
        /// �������݂̗֊s��`�悷��B
        /// �T�u�N���X�Ŏ�������Ȃ��ꍇ�͉������Ȃ��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        public virtual void PaintOutline(Graphics g) { }


        /// <summary>
        /// �������݂̗֊s���g��{���ɉ����ĕ`�悷��B
        /// �T�u�N���X�Ŏ�������Ȃ��ꍇ�͉������Ȃ��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
        public virtual void PaintOutline(Graphics g, float scaleFactor) { }


        /// <summary>
        /// �������݂Ɋւ����񕶎����`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        public void PaintInfoText(Graphics g)
        {
            try
            {
                g.DrawString(infoText, PhotoChat.RegularFont, infoBrush,
                    range.X, range.Y + range.Height + PhotoChat.InfoFontSize);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
