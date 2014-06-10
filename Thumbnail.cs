using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;

namespace PhotoChat
{
    /// <summary>
    /// �T���l�C���I�u�W�F�N�g
    /// </summary>
    public class Thumbnail : IComparable, IComparable<Thumbnail>
    {
        #region �t�B�[���h�E�v���p�e�B

        // �T���l�C���ꗗ�\���p
        private static Bitmap imageBuffer;
        private static volatile bool needBufferResize = true;
        private const int Margin = 6;
        private const int TopMargin = 15;
        private const int HeightLimit = 255 - TopMargin - Margin;
        private static int imageWidth = 1;
        private static int imageHeight = 1;
        private static readonly SolidBrush brush = new SolidBrush(Color.Black);
        private const int InfoOffsetX = 1;
        private const int InfoOffsetY = 0;
        private const int ImageOffsetX = Margin;
        private const int ImageOffsetY = TopMargin;
        private static readonly Bitmap unreadImage = Properties.Resources.unread;
        private static int unreadOffsetX = 0;
        private static int unreadOffsetY = 0;
        private static readonly Bitmap updatedImage = Properties.Resources.updated;
        private static int updatedOffsetX = 0;
        private static int updatedOffsetY = 0;
        private static readonly Bitmap markerImage = Properties.Resources.marker;
        private static int markerOffsetX = 0;
        private const int MarkerOffsetY = TopMargin;
        private static readonly Bitmap attentionImage = Properties.Resources.attention;
        private static int attentionOffsetX = 0;
        private const int AttentionOffsetY = TopMargin;
        private static readonly SolidBrush nameBrush = new SolidBrush(Color.Blue);
        private const float NameOffsetX = Margin + 2;
        private static float nameOffsetY = 0;
        private static readonly Pen showingPen = new Pen(Color.Blue, 2);

        // ��{���
        private string photoName;
        private string author;
        private string id;
        private long serialNumber;
        private DateTime date;
        private List<string> tagList = new List<string>();
        private string proximity = string.Empty;
        private double latitude = 200;
        private double longitude = 200;

        // �`��p���
        private Bitmap image;
        private string infoText;
        private Dictionary<string, string> nameDictionary;
        private int noteCount;
        private bool unread;
        private bool updated;
        private bool marked;
        private bool attentionFlag;
        private bool showing = false;
        private int needImageUpdate = 0;
        private int needDataSave = 0;


        /// <summary>
        /// �T���l�C���ɑΉ�����ʐ^�����擾����B
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }

        /// <summary>
        /// �B�e�҂̃��[�U�����擾����B
        /// </summary>
        public string Author
        {
            get { return author; }
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
        /// �ʐ^�̎B�e�������擾����B
        /// </summary>
        public DateTime Date
        {
            get { return date; }
        }

        /// <summary>
        /// �R���}��؂�̃^�O���擾����B
        /// </summary>
        public string Tags
        {
            get { return PhotoChat.ArrayToString(tagList.ToArray()); }
        }

        /// <summary>
        /// �ߐڊ֌W�����擾����B
        /// </summary>
        public string Proximity
        {
            get { return proximity; }
        }

        /// <summary>
        /// �ܓx���擾����B
        /// </summary>
        public double Latitude
        {
            get { return latitude; }
        }

        /// <summary>
        /// �o�x���擾����Bs
        /// </summary>
        public double Longitude
        {
            get { return longitude; }
        }

        #endregion




        #region �R���X�g���N�^�E�f�X�g���N�^

        /// <summary>
        /// Photo�C���X�^���X����Thumbnail���쐬����B
        /// </summary>
        /// <param name="photo">Photo�C���X�^���X</param>
        public Thumbnail(Photo photo)
        {
            // Photo����f�[�^�̎擾
            this.photoName = photo.PhotoName;
            this.author = photo.Author;
            this.id = photo.ID;
            this.serialNumber = photo.SerialNumber;
            this.date = photo.Date;
            this.proximity = photo.Proximity;
            this.latitude = photo.Latitude;
            this.longitude = photo.Longitude;
            this.noteCount = photo.NoteCount;
            AddTag(photo.TagArray);

            // ���̑��̃t�B�[���h�̏�����
            this.image = GetImage(photoName, imageWidth, imageHeight);
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.nameDictionary = new Dictionary<string, string>();
            this.unread = true;
            this.updated = false;
            this.marked = false;
            this.attentionFlag = false;

            // �T���l�C���f�[�^�ۑ�
            Interlocked.Increment(ref needDataSave);
            SaveData();
        }


        /// <summary>
        /// Thumbnail�t�@�C����ǂݍ��݃C���X�^���X���쐬����B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        public Thumbnail(string photoName)
        {
            // �T���l�C���摜��ǂݍ���
            this.photoName = photoName;
            this.image = GetImage(photoName, imageWidth, imageHeight);

            // �t�@�C������f�[�^��ǂ�
            ReadDataFile(photoName);

            // ���̑��̃t�B�[���h�̏�����
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.nameDictionary = new Dictionary<string, string>();
        }


        /// <summary>
        /// ���ۑ��f�[�^��ۑ������\�[�X���������B
        /// </summary>
        public void Dispose()
        {
            try
            {
                // ���ۑ��̃f�[�^������Εۑ�
                SaveData();

                // ���\�[�X���
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




        #region �t�@�C�����o��

        /// <summary>
        /// �T���l�C���摜��ۑ�����B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <param name="newImage">�T���l�C��������摜</param>
        public static void SaveImage(string photoName, Bitmap newImage)
        {
            try
            {
                Bitmap image = PhotoChatImage.ResizeImage(
                    newImage, PhotoChat.ThumbnailWidth, PhotoChat.ThumbnailHeight);
                image.Save(GetImageFilePath(photoName), ImageFormat.Jpeg);
                image.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���f�[�^��ۑ�����B
        /// </summary>
        public void SaveData()
        {
            if (Interlocked.Exchange(ref needDataSave, 0) != 0)
            {
                try
                {
                    string filePath = GetDataFilePath(photoName);
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        // �T���l�C���f�[�^�̏�������
                        sw.WriteLine(GetDataString());
                        sw.Flush();
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// �T���l�C���f�[�^�t�@�C����ǂݍ��ށB
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        private void ReadDataFile(string photoName)
        {
            try
            {
                string filePath = GetDataFilePath(photoName);
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line = sr.ReadLine();
                    InterpretDataString(line);
                }
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }


        /// <summary>
        /// �T���l�C���摜�t�@�C���̃p�X���擾����B�B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�T���l�C���摜�t�@�C���̃p�X</returns>
        public static string GetImageFilePath(string photoName)
        {
            return Path.Combine(PhotoChat.ThumbnailDirectory, photoName + ".jpg");
        }


        /// <summary>
        /// �T���l�C���f�[�^�t�@�C���̃p�X���擾����B�B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�T���l�C���t�@�C���̃p�X</returns>
        public static string GetDataFilePath(string photoName)
        {
            return Path.Combine(PhotoChat.ThumbnailDirectory, photoName + ".dat");
        }


        /// <summary>
        /// �w�肵���T�C�Y�ɃT���l�C���摜���k�����ĕԂ��B
        /// �������w��T�C�Y���摜�t�@�C���ȏ�̏ꍇ�̓T�C�Y�ϊ����Ȃ��B
        /// �T���l�C���摜���܂������ꍇ��null��Ԃ��B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <param name="width">�T���l�C���̕�</param>
        /// <param name="height">�T���l�C���̍���</param>
        /// <returns>�w�肵���T�C�Y�̃T���l�C���摜�B�摜���Ȃ��ꍇ��null�B</returns>
        public static Bitmap GetImage(string photoName, int width, int height)
        {
            try
            {
                // �摜�t�@�C�������݂��邩�m�F
                string filePath = GetImageFilePath(photoName);
                if (!File.Exists(filePath))
                    return null;

                // �摜���w��T�C�Y�ɕϊ����ĕԂ�
                Bitmap original = new Bitmap(filePath);
                Bitmap image = PhotoChatImage.ResizeImage(original, width, height);
                original.Dispose();
                return image;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// �ʐ^�ɕt����ꂽ�^�O���R���}��؂�Ŏ擾����B
        /// �C���X�^���X�����݂���ꍇ��Tags�v���p�e�B����擾�����ق��������B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�ʐ^�ɕt����ꂽ�^�O�̃R���}��؂蕶����</returns>
        public static string GetTags(string photoName)
        {
            try
            {
                string filePath = GetDataFilePath(photoName);
                if (File.Exists(filePath))
                {
                    string line;
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        line = sr.ReadLine();
                    }
                    DataStringDictionary dsd = new DataStringDictionary(line);
                    return dsd.GetValue("TagList", string.Empty);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return string.Empty;
        }

        #endregion




        #region �f�[�^�����񏈗�

        /// <summary>
        /// �e�t�B�[���h�̏������f�[�^�������Ԃ��B
        /// </summary>
        /// <returns>�f�[�^������</returns>
        private string GetDataString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(300);
                sb.Append("Author=").Append(author).Append(PhotoChat.Delimiter);
                sb.Append("ID=").Append(id).Append(PhotoChat.Delimiter);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("Ticks=").Append(date.Ticks).Append(PhotoChat.Delimiter);
                sb.Append("TagList=").Append(PhotoChat.ArrayToString(tagList.ToArray())).Append(PhotoChat.Delimiter);
                sb.Append("Proximity=").Append(proximity).Append(PhotoChat.Delimiter);
                sb.Append("Geo=").Append(latitude).Append(',').Append(longitude).Append(PhotoChat.Delimiter);
                sb.Append("NoteCount=").Append(noteCount).Append(PhotoChat.Delimiter);
                sb.Append("Unread=").Append(unread).Append(PhotoChat.Delimiter);
                sb.Append("Updated=").Append(updated).Append(PhotoChat.Delimiter);
                sb.Append("Marked=").Append(marked).Append(PhotoChat.Delimiter);
                sb.Append("AttentionFlag=").Append(attentionFlag).Append(PhotoChat.Delimiter);
                sb.Append("NeedImageUpdate=").Append(needImageUpdate);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// �f�[�^�������ǂݎ��e�t�B�[���h��ݒ肷��B
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
                AddTag(PhotoChat.StringToArray(dsd.GetValue("TagList", string.Empty)));
                this.proximity = dsd.GetValue("Proximity", string.Empty);
                this.noteCount = int.Parse(dsd.GetValue("NoteCount", "0"));
                this.unread = bool.Parse(dsd.GetValue("Unread", "false"));
                this.updated = bool.Parse(dsd.GetValue("Updated", "false"));
                this.marked = bool.Parse(dsd.GetValue("Marked", "false"));
                this.attentionFlag = bool.Parse(dsd.GetValue("AttentionFlag", "false"));
                this.needImageUpdate = int.Parse(dsd.GetValue("NeedImageUpdate", "0"));

                // GPS���W�̓ǂݎ��
                string str = dsd.GetValue("Geo", "200,200");
                int index = str.IndexOf(',');
                this.latitude = double.Parse(str.Substring(0, index));
                this.longitude = double.Parse(str.Substring(index + 1));
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region �T���l�C���X�V�E�T�C�Y�ύX

        /// <summary>
        /// �T���l�C���摜���ēǂݍ��݂���B
        /// </summary>
        public void ReloadImage()
        {
            try
            {
                Image temp = image;
                image = GetImage(photoName, imageWidth, imageHeight);
                if (temp != null) temp.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���摜���X�V����B
        /// </summary>
        /// <param name="newImage">�X�V</param>
        public void UpdateImage(Bitmap newImage)
        {
            try
            {
                if (Interlocked.Exchange(ref needImageUpdate, 0) != 0)
                {
                    SaveImage(photoName, newImage);
                    Image temp = image;
                    this.image = PhotoChatImage.ResizeImage(newImage, imageWidth, imageHeight);
                    if (temp != null) temp.Dispose();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���ꗗ�ɕ\�����鍀�ڂ̃T�C�Y���v�Z�E�ݒ肵�A���ڂ̍�����Ԃ��B
        /// </summary>
        /// <param name="width">�T���l�C���ꗗ�̕�</param>
        /// <returns>�Z�o�����T���l�C�����ڂ̍���</returns>
        public static int Resize(int width)
        {
            // �T���l�C���摜�̃T�C�Y���v�Z
            imageWidth = width - Margin - Margin;
            imageHeight = (int)(imageWidth * 0.75);
            if (imageHeight > HeightLimit) imageHeight = HeightLimit;

            // �I�t�Z�b�g�l���v�Z
            unreadOffsetX = imageWidth + Margin - unreadImage.Width;
            unreadOffsetY = imageHeight + TopMargin - unreadImage.Height;
            updatedOffsetX = imageWidth + Margin - updatedImage.Width;
            updatedOffsetY = imageHeight + TopMargin - updatedImage.Height;
            markerOffsetX = imageWidth + Margin - markerImage.Width;
            attentionOffsetX = imageWidth + Margin - attentionImage.Width;

            // �o�b�t�@�X�V�t���O�𗧂āA�T���l�C�����ڂ̍�����Ԃ�
            needBufferResize = true;
            return imageHeight + TopMargin + Margin;
        }

        #endregion




        #region �T���l�C���f�[�^����

        /// <summary>
        /// �������ݐ���1���₷�B
        /// </summary>
        public void IncrementNoteCount()
        {
            try
            {
                noteCount++;
                if (!showing)
                    updated = true;
                Interlocked.Increment(ref needDataSave);
                Interlocked.Increment(ref needImageUpdate);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������ݐ���1���炷�B
        /// </summary>
        public void DecrementNoteCount()
        {
            try
            {
                noteCount--;
                Interlocked.Increment(ref needDataSave);
                Interlocked.Increment(ref needImageUpdate);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �^�O��ǉ�����B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        public void AddTag(string tag)
        {
            try
            {
                if (!tagList.Contains(tag))
                    tagList.Add(tag);
                Interlocked.Increment(ref needDataSave);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �^�O��ǉ�����B
        /// </summary>
        /// <param name="tagArray">�^�O�z��</param>
        public void AddTag(string[] tagArray)
        {
            try
            {
                for (int i = 0; i < tagArray.Length; i++)
                    AddTag(tagArray[i]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���ɖڈ��t���邩�ǂ����ݒ肷��B
        /// </summary>
        /// <param name="marked">�ڈ��t����Ȃ�true�B</param>
        public void SetMarker(bool marked)
        {
            try
            {
                this.marked = marked;
                Interlocked.Increment(ref needDataSave);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���ɒ��ڃ}�[�N��t���邩�ǂ����ݒ肷��B
        /// </summary>
        /// <param name="attentionFlag">���ڃ}�[�N��t����Ȃ�true�B</param>
        public void SetAttention(bool attentionFlag)
        {
            try
            {
                this.attentionFlag = attentionFlag;
                Interlocked.Increment(ref needDataSave);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���̃T���l�C����\����Ԃɂ���B
        /// </summary>
        /// <returns>���߂ĕ\�������Ƃ���true�A�����łȂ��Ƃ���false��Ԃ��B</returns>
        public bool Visit()
        {
            try
            {
                showing = true;
                if (unread || updated)
                {
                    unread = false;
                    updated = false;
                    Interlocked.Increment(ref needDataSave);
                    return true;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return false;
        }


        /// <summary>
        /// ���̃T���l�C�����\����Ԃɂ���B
        /// </summary>
        public void Leave()
        {
            showing = false;
        }

        #endregion




        #region ���[�U���ǉ��E�폜

        /// <summary>
        /// ���̎ʐ^��I�𒆂̃��[�U����ǉ�����B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="userName">���[�U��</param>
        public void AddUserName(string id, string userName)
        {
            try
            {
                nameDictionary[id] = userName;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �I�𒆂̃��[�U�����폜����B
        /// </summary>
        /// <param name="id">�[��ID</param>
        public void RemoveUserName(string id)
        {
            try
            {
                nameDictionary.Remove(id);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �T���l�C���ꗗ�̍��ڂ̕`��

        /// <summary>
        /// ���̃T���l�C�����ڂ�`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="bounds">�`��͈�</param>
        public void Paint(Graphics g, Rectangle bounds)
        {
            try
            {
                // �K�v�ł���Ε`��o�b�t�@�̃T�C�Y�ύX
                if (needBufferResize)
                {
                    if (imageBuffer != null)
                        imageBuffer.Dispose();
                    imageBuffer = new Bitmap(bounds.Width, bounds.Height);
                }

                // �o�b�t�@�ւ̕`��
                using (Graphics bg = Graphics.FromImage(imageBuffer))
                {
                    // �������ݐ���\���w�i�̕`��
                    int power = noteCount * 2;
                    if (power > 255) power = 255;
                    bg.Clear(Color.FromArgb(255, 255 - power, 255 - power));

                    // �B�e�҂Ǝ����̕`��
                    bg.DrawString(infoText, PhotoChat.RegularFont, brush, InfoOffsetX, InfoOffsetY);

                    // �T���l�C���摜�̕`��
                    bg.DrawImage(image, ImageOffsetX, ImageOffsetY, imageWidth, imageHeight);

                    // �V���E�X�V�}�[�N�̕`��
                    if (unread)
                        bg.DrawImage(unreadImage, unreadOffsetX, unreadOffsetY);
                    else
                    {
                        if (updated)
                            bg.DrawImage(updatedImage, updatedOffsetX, updatedOffsetY);
                    }

                    // �^�O�}�[�N�̕`��
                    if (marked)
                        bg.DrawImage(markerImage, markerOffsetX, MarkerOffsetY);
                    if (attentionFlag)
                        bg.DrawImage(attentionImage, attentionOffsetX, AttentionOffsetY);

                    // �ʐ^�I�𒆃��[�U���̕`��
                    nameOffsetY = ImageOffsetY;
                    foreach (string userName in nameDictionary.Values)
                    {
                        bg.DrawString(userName, PhotoChat.RegularFont, nameBrush, NameOffsetX, nameOffsetY);
                        nameOffsetY += PhotoChat.RegularFont.Size;
                    }

                    // �\�����g�̕`��
                    if (showing)
                        bg.DrawRectangle(showingPen, 1, 1, bounds.Width - 2, bounds.Height - 2);
                }

                // �o�b�t�@�̕`��
                g.DrawImage(imageBuffer, bounds.X, bounds.Y);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
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
            Thumbnail other = obj as Thumbnail;
            if (other == null)
            {
                throw new ArgumentException();
            }

            // ��r
            return CompareTo(other);
        }


        /// <summary>
        /// ���݂�Thumbnail��ʂ�Thumbnail�Ɣ�r����B
        /// </summary>
        /// <param name="another">��r����Thumbnail</param>
        /// <returns>��r����</returns>
        public int CompareTo(Thumbnail other)
        {
            int result = date.CompareTo(other.Date);
            if (result == 0)
                result = photoName.CompareTo(other.PhotoName);
            return result;
        }

        #endregion
    }
}
