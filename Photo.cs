using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Tuat.Hands;

namespace PhotoChat
{
    /// <summary>
    /// �ʐ^�I�u�W�F�N�g
    /// </summary>
    public class Photo : PhotoChatImage
    {
        #region �t�B�[���h�E�v���p�e�B

        private const string Properties = "PROPERTIES";
        private static Brush infoBrush = new SolidBrush(PhotoChat.InfoColor);

        private LinkedList<PhotoChatNote> noteList = new LinkedList<PhotoChatNote>();
        private LinkedList<NoteGroup> groupList = new LinkedList<NoteGroup>();
        private List<string> tagList = new List<string>();
        private string proximity = string.Empty;
        private double latitude = 200;
        private double longitude = 200;
        private int noteCount;

        /// <summary>
        /// �\������鏑�����݂̐����擾
        /// </summary>
        public int NoteCount
        {
            get { return noteCount; }
        }

        /// <summary>
        /// �������݃��X�g�i�S�Ă̏������݁j�̗v�f�����擾
        /// </summary>
        public int NoteListSize
        {
            get { return noteList.Count; }
        }

        /// <summary>
        /// �^�O�z����擾
        /// </summary>
        public string[] TagArray
        {
            get { return tagList.ToArray(); }
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




        #region �R���X�g���N�^

        /// <summary>
        /// �B�e�����ʐ^�̃C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�B�e�҂̃��[�U��</param>
        /// <param name="serialNumber">�f�[�^�ɕt����ʂ��ԍ�</param>
        /// <param name="image">�摜�f�[�^</param>
        /// <param name="proximityList">�ߐڊ֌W���</param>
        public Photo(string author, long serialNumber, Bitmap image, string[] proximityList)
            : this(author, serialNumber, image, proximityList, 200, 200) { }


        /// <summary>
        /// �B�e�����ʐ^�̃C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�B�e�҂̃��[�U��</param>
        /// <param name="serialNumber">�f�[�^�ɕt����ʂ��ԍ�</param>
        /// <param name="image">�摜�f�[�^</param>
        /// <param name="proximityList">�ߐڊ֌W���</param>
        /// <param name="latitude">�B�e�n�_�̈ܓx</param>
        /// <param name="longitude">�B�e�n�_�o�x</param>
        public Photo(string author, long serialNumber, Bitmap image,
            string[] proximityList, double latitude, double longitude)
        {
            this.type = TypePhoto;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.proximity = PhotoChat.ArrayToString(proximityList);
            this.latitude = latitude;
            this.longitude = longitude;
            SetPhotoName();

            // �摜��JPEG�`���ŕۑ�
            image.Save(GetImageFilePath(photoName), ImageFormat.Jpeg);

            // �\���T�C�Y�ɕϊ�
            this.image =
                ResizeImage(image, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);

            // �������݃t�@�C���̍쐬
            CreateNotesFile();
        }


        /// <summary>
        /// ��荞�񂾃t�@�C������ʐ^�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�B�e�҂̃��[�U��</param>
        /// <param name="serialNumber">�f�[�^�ɕt����ʂ��ԍ�</param>
        /// <param name="originalFile">�摜�t�@�C���̃p�X</param>
        /// <param name="proximityList">�ߐڊ֌W���</param>
        public Photo(string author, long serialNumber, string originalFile, string[] proximityList)
            : this(author, serialNumber, originalFile, proximityList, 200, 200) { }


        /// <summary>
        /// ��荞�񂾃t�@�C������ʐ^�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="author">�B�e�҂̃��[�U��</param>
        /// <param name="serialNumber">�f�[�^�ɕt����ʂ��ԍ�</param>
        /// <param name="originalFile">�摜�t�@�C���̃p�X</param>
        /// <param name="proximityList">�ߐڊ֌W���</param>
        /// <param name="latitude">��荞�񂾏ꏊ�̈ܓx</param>
        /// <param name="longitude">��荞�񂾏ꏊ�̌o�x</param>
        public Photo(string author, long serialNumber, string originalFile,
            string[] proximityList, double latitude, double longitude)
        {
            this.type = TypePhoto;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.proximity = PhotoChat.ArrayToString(proximityList);
            this.latitude = latitude;
            this.longitude = longitude;
            SetPhotoName();

            // �t�@�C���̉摜�f�[�^��ǂݍ���JPEG�`���ŕۑ�����
            Bitmap original = new Bitmap(originalFile);
            original.Save(GetImageFilePath(photoName), ImageFormat.Jpeg);

            // �\���T�C�Y�ɕϊ�
            this.image =
                ResizeImage(original, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
            original.Dispose();

            // �������݃t�@�C���̍쐬
            CreateNotesFile();
        }


        /// <summary>
        /// �摜�E�������݃t�@�C����ǂݍ��ݎʐ^�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        public Photo(string photoName) : this(photoName, false) { }


        /// <summary>
        /// �I���W�i���T�C�Y��Bitmap��ێ�����Photo���쐬����B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <param name="isOriginalSize">�I���W�i���T�C�Y�ɂ���Ȃ�true</param>
        public Photo(string photoName, bool isOriginalSize)
        {
            this.type = TypePhoto;

            // �摜�t�@�C����ǂݍ���
            string filePath = GetImageFilePath(photoName);
            if (!File.Exists(filePath))
                throw new UnsupportedDataException("�ʐ^�t�@�C����������܂���B");
            Bitmap temp = new Bitmap(filePath);
            if (isOriginalSize)
                this.image = new Bitmap(temp);
            else
                this.image =
                    ResizeImage(temp, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
            temp.Dispose();

            // �������݃t�@�C����ǂݍ���
            ReadNotesFile(photoName);
        }


        /// <summary>
        /// �f�[�^������Ɖ摜�f�[�^����ʐ^�C���X�^���X���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        /// <param name="imageBytes">�摜�f�[�^�̃o�C�g��</param>
        public Photo(string dataString, byte[] imageBytes)
        {
            this.type = TypePhoto;
            InterpretDataString(dataString);

            // �摜��ۑ�����
            string filePath = GetImageFilePath(photoName);
            if (File.Exists(filePath))
                throw new UnsupportedDataException("�d����M�F" + photoName);
            try
            {
                using (FileStream fs = File.Create(filePath))
                {
                    fs.Write(imageBytes, 0, imageBytes.Length);
                    fs.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return;
            }

            // �摜�t�@�C����ǂݍ���
            Bitmap original = new Bitmap(filePath);
            this.image =
                ResizeImage(original, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
            original.Dispose();

            // ��Ɏ�M�����������݃f�[�^������Γǂݍ���
            ReadEarlyNotes();

            // �������݃t�@�C���̍쐬
            CreateNotesFile();
        }

        #endregion




        #region �t�@�C���p�X�擾�E�������݃t�@�C�����o��

        /// <summary>
        /// �摜�t�@�C���̃p�X��Ԃ�
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�摜�t�@�C���̃p�X</returns>
        public static string GetImageFilePath(string photoName)
        {
            try
            {
                return Path.Combine(PhotoChat.PhotoDirectory, photoName + ".jpg");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// �������݃t�@�C�����쐬����B
        /// </summary>
        private void CreateNotesFile()
        {
            try
            {
                string filePath = PhotoChatNote.GetNotesFilePath(photoName);
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    // �v���p�e�B�̕ۑ�
                    sw.WriteLine(Properties);
                    sw.WriteLine(GetDataString());

                    // �������݃f�[�^�̕ۑ�
                    foreach (PhotoChatNote note in noteList)
                    {
                        sw.WriteLine(note.GetSaveString());
                    }
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������݃t�@�C����ǂݍ��ށB
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        private void ReadNotesFile(string photoName)
        {
            try
            {
                string filePath = PhotoChatNote.GetNotesFilePath(photoName);
                using (StreamReader sr = new StreamReader(filePath))
                {
                    // �v���p�e�B�̓ǂݍ���
                    string line = sr.ReadLine();
                    if (line == null || line != Properties)
                        throw new UnsupportedDataException("�s���ȏ������݃t�@�C��");
                    if ((line = sr.ReadLine()) != null)
                        InterpretDataString(line);

                    // �������݃f�[�^�̓ǂݍ���
                    ReadNotes(sr);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                throw new UnsupportedDataException(e.Message);
            }
        }


        /// <summary>
        /// �ʐ^����Ɏ�M�����������݃f�[�^������Γǂݍ��ށB
        /// </summary>
        private void ReadEarlyNotes()
        {
            try
            {
                // �������݃t�@�C�����쐬����Ă��Ȃ���Ή������Ȃ�
                string filePath = PhotoChatNote.GetNotesFilePath(photoName);
                if (!File.Exists(filePath)) return;

                // ��s�����������݃f�[�^��ǂݍ���
                using (StreamReader sr = new StreamReader(filePath))
                {
                    ReadNotes(sr);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������݃f�[�^��ǂݍ���
        /// </summary>
        /// <param name="sr"></param>
        private void ReadNotes(StreamReader sr)
        {
            try
            {
                string line, dataString;
                int type, index;
                while ((line = sr.ReadLine()) != null)
                {
                    index = line.IndexOf(PhotoChat.Delimiter);
                    type = int.Parse(line.Substring(0, index));
                    dataString = line.Substring(index + 1);
                    AddNote(PhotoChatNote.CreateInstance(type, dataString));
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �f�[�^�����񏈗�

        /// <summary>
        /// �f�[�^�̃o�C�g��i�f�[�^������Ɖ摜�f�[�^�̘A���j��Ԃ��B
        /// �f�[�^������̒�����\��4�o�C�g�{�f�[�^������{�摜�f�[�^
        /// </summary>
        /// <returns>�f�[�^�̃o�C�g��</returns>
        public override byte[] GetDataBytes()
        {
            try
            {
                // �f�[�^��������o�C�g��ɕϊ�
                byte[] dataStringBytes = PhotoChat.DefaultEncoding.GetBytes(GetDataString());
                byte[] lengthBytes = BitConverter.GetBytes(dataStringBytes.Length);

                // �S�̂̃o�C�g����쐬
                FileInfo file = new FileInfo(GetImageFilePath(photoName));
                int fileLength = (int)file.Length;
                int totalLength = lengthBytes.Length + dataStringBytes.Length + fileLength;
                byte[] dataBytes = new byte[totalLength];

                // �f�[�^�����񕪂��R�s�[
                int index1 = lengthBytes.Length;
                int index2 = index1 + dataStringBytes.Length;
                lengthBytes.CopyTo(dataBytes, 0);
                dataStringBytes.CopyTo(dataBytes, index1);

                // �摜�f�[�^���o�C�g��ɒǉ�
                try
                {
                    using (FileStream fs = file.OpenRead())
                    {
                        fs.Read(dataBytes, index2, fileLength);
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return null;
                }

                return dataBytes;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }


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
                sb.Append("Proximity=").Append(proximity).Append(PhotoChat.Delimiter);
                sb.Append("Geo=").Append(latitude).Append(',').Append(longitude);
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
                this.proximity = dsd.GetValue("Proximity", string.Empty);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
                SetPhotoName();

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


        /// <summary>
        /// ���[�UIndex�������Ԃ��B
        /// </summary>
        /// <returns>���[�UIndex������</returns>
        public string GetIndexString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(50);
                sb.Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(photoName);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }

        #endregion




        #region �������ݒǉ�

        /// <summary>
        /// �������݃��X�g�ւ̒ǉ�
        /// </summary>
        /// <param name="note">�ǉ����鏑�����݃I�u�W�F�N�g</param>
        public void AddNote(PhotoChatNote note)
        {
            try
            {
                if (note == null) return;

                lock (noteList)
                {
                    // noteList�ւ̒ǉ�
                    InsertInSortedList(note, noteList);
                }

                // �������݂̎�ނɉ���������
                switch (note.Type)
                {
                    case PhotoChatNote.TypeRemoval:
                        Removal removal = (Removal)note;
                        RemoveNote(removal.TargetID, removal.TargetSerialNumber);
                        break;

                    case PhotoChatNote.TypeTag:
                        AddTag((Tag)note);
                        break;

                    default:
                        noteCount++;
                        Grouping(note);
                        break;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������݃O���[�v�i�\���p�j����폜����B
        /// </summary>
        /// <param name="id">�폜�f�[�^�̒[��ID</param>
        /// <param name="serialNumber">�폜�f�[�^�̒ʂ��ԍ�</param>
        private void RemoveNote(string id, long serialNumber)
        {
            try
            {
                lock (groupList)
                {
                    foreach (NoteGroup group in groupList)
                    {
                        if (group.RemoveNote(id, serialNumber))
                        {
                            // �O���[�v�̗v�f����ɂȂ�����O���[�v���폜
                            if (group.First == null)
                                groupList.Remove(group);

                            noteCount--;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �^�O��ǉ�����B
        /// </summary>
        /// <param name="tag">�^�O��������</param>
        private void AddTag(Tag tag)
        {
            try
            {
                if (!tagList.Contains(tag.TagString))
                    tagList.Add(tag.TagString);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������݂��O���[�v������B
        /// </summary>
        /// <param name="note">�V������������</param>
        private void Grouping(PhotoChatNote note)
        {
            try
            {
                if (note.Type == PhotoChatNote.TypeStroke)
                {
                    // �X�g���[�N�̏ꍇ�̓O���[�v���ł�����̂��Ȃ������ׂ�
                    foreach (NoteGroup group in groupList)
                    {
                        // �������[�U���̏������݃O���[�v�𒲂ׂ�
                        if (!group.IsFixed && group.First.Author == note.Author)
                        {
                            if (group.AddNote(note))
                                return;
                            else
                                break;
                        }
                    }
                }
                // �O���[�v���ł��Ȃ����̂͐V�����������݃O���[�v
                InsertInGroupList(note, groupList);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// note��list��������ۂ悤�ɑ}������B
        /// </summary>
        /// <param name="note">�}������PhotoChatNote</param>
        /// <param name="list">PhotoChatNote�������Ɋi�[����Ă���LinkedList</param>
        private static void InsertInSortedList(PhotoChatNote note, LinkedList<PhotoChatNote> list)
        {
            try
            {
                // ��납��}���ӏ���T��
                LinkedListNode<PhotoChatNote> node;
                for (node = list.Last; node != null; node = node.Previous)
                    if (note.CompareTo(node.Value) > 0) break;

                // �}��
                if (node == null)
                    list.AddFirst(note);
                else
                    list.AddAfter(node, note);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �V����NoteGroup���쐬����list��������ۂ悤�ɑ}������B
        /// </summary>
        /// <param name="note">�V����PhotoChatNote</param>
        /// <param name="list">NoteGroup�������Ɋi�[����Ă���LinkedList</param>
        private static void InsertInGroupList(PhotoChatNote note,
                                              LinkedList<NoteGroup> list)
        {
            try
            {
                // ��납��}���ӏ���T��
                LinkedListNode<NoteGroup> node;
                for (node = list.Last; node != null; node = node.Previous)
                    if (note.CompareTo(node.Value.First) > 0) break;

                // �}��
                if (node == null)
                    list.AddFirst(new NoteGroup(note));
                else
                    list.AddAfter(node, new NoteGroup(note));
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �������݌���

        /// <summary>
        /// �^�O���t�����Ă��邩�ǂ�����Ԃ��B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        /// <returns>���̃^�O���t�����Ă����true��Ԃ��B</returns>
        public bool ContainsTag(string tag)
        {
            return tagList.Contains(tag);
        }


        /// <summary>
        /// point�ɂ���ŏ��Ɍ��������������݂�Ԃ��B
        /// </summary>
        /// <param name="point">���WPoint</param>
        /// <returns>point�ɂ��鏑�����݁B������Ȃ��ꍇ��null�B</returns>
        public PhotoChatNote GetPointedNote(Point point)
        {
            try
            {
                lock (groupList)
                {
                    // �O���[�v�P�ʂŌ���
                    foreach (NoteGroup group in groupList)
                    {
                        if (group.Contains(point))
                        {
                            // �O���[�v���ōŏ��Ɍ��������������݂�Ԃ�
                            return group.GetPointedNote(point);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }


        /// <summary>
        /// point�ɂ���ŏ��Ɍ������������̏������݂�Ԃ��B
        /// </summary>
        /// <param name="point">���WPoint</param>
        /// <param name="userName">���݂̃��[�U��</param>
        /// <returns>point�ɂ��鎩���̏������݁B������Ȃ��ꍇ��null�B</returns>
        public PhotoChatNote GetPointedMyNote(Point point, string userName)
        {
            try
            {
                lock (groupList)
                {
                    // �O���[�v�P�ʂŌ���
                    foreach (NoteGroup group in groupList)
                    {
                        if (group.First.ID == PhotoChatClient.Instance.ID
                            && group.First.Author == userName
                            && group.Contains(point))
                        {
                            // �O���[�v���ōŏ��Ɍ��������������݂�Ԃ�
                            return group.GetPointedNote(point);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }


        /// <summary>
        /// point�ɂ���ŏ��Ɍ������������N�������݂�Ԃ��B
        /// </summary>
        /// <param name="point">���WPoint</param>
        /// <returns>point�ɂ��郊���N�������݁B������Ȃ��ꍇ��null�B</returns>
        public PhotoChatNote GetPointedLink(Point point)
        {
            try
            {
                lock (groupList)
                {
                    PhotoChatNote note;
                    foreach (NoteGroup group in groupList)
                    {
                        note = group.First;
                        if (note.Type == PhotoChatNote.TypeHyperlink
                            || note.Type == PhotoChatNote.TypeSound)
                        {
                            if (note.Contains(point))
                                return note;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }

        #endregion




        #region �����F��

        /// <summary>
        /// �ʐ^��̏������݂𕶎��F�����Č��ʂ��^�O�ɂ���B
        /// </summary>
        /// <param name="form">PhotoChatForm</param>
        public void Recognize(PhotoChatForm form)
        {

            //// �X�g���[�N�̏������݃O���[�v�̂ݏ���
            //foreach (NoteGroup group in groupList)
            //{
            //    if (group.First.Type == PhotoChatNote.TypeStroke)
            //    {
            //        // �O���[�v���̊e�X�g���[�N�����
            //        InkRecognizer.Clear();
            //        foreach (PhotoChatNote note in group.NoteList)
            //        {
            //            InkRecognizer.AddStroke();
            //            foreach (Point point in ((Stroke)note).Points)
            //            {
            //                InkRecognizer.AddPoint(point.X, point.Y);
            //            }
            //        }

            //        // �F��
            //        string result = InkRecognizer.Recognize().Trim();
            //        if (!PhotoChat.ContainsInvalidChars(result))
            //        {
            //            // �`�ԑf��͂��炢���ׂ��H
            //            // �Ƃ肠�����t�@�C���Ɏg���Ȃ�����������10��������
            //            // �����ƃ}�V�ȃR�[�h�ɂ���ׂ�
            //            int index;
            //            while ((index = result.IndexOfAny(Path.GetInvalidFileNameChars())) >= 0)
            //            {
            //                result.Remove(index, 1);
            //            }
            //            while ((index = result.IndexOf('?')) >= 0)
            //            {
            //                result.Remove(index, 1);
            //            }
            //            while ((index = result.IndexOf('*')) >= 0)
            //            {
            //                result.Remove(index, 1);
            //            }
            //            if (result.Length > 10)
            //                result = result.Substring(0, 10);
            //            form.NewData(new Tag(form.Client.UserName,
            //                form.Client.GetNewSerialNumber(), photoName, result));
            //        }
            //    }
            //}
        }

        #endregion




        #region �������ݕ`��

        /// <summary>
        /// �S�Ă̏������݁i�\���p�j��`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        public void PaintNotes(Graphics g)
        {
            try
            {
                lock (groupList)
                {
                    // �O���[�v���Ƃɏ������݂�`��
                    foreach (NoteGroup group in groupList)
                    {
                        group.Paint(g);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������݂��w�肵�������̐������`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="end">�`�悷�鏑�����݂̊����i���j</param>
        public void PaintNotes(Graphics g, int percentage)
        {
            try
            {
                lock (groupList)
                {
                    // ��������`�搔���Z�o�����̐������`��
                    int count = (noteCount * percentage) / 100;
                    foreach (NoteGroup group in groupList)
                    {
                        if (count == 0) break;
                        group.Paint(g, ref count);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �S�Ă̏������݁i�\���p�j���g�債�ĕ`�悷��B
        /// </summary>
        /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
        /// <param name="scaleFactor">�g��{��</param>
        public void PaintNotes(Graphics g, float scaleFactor)
        {
            try
            {
                lock (groupList)
                {
                    foreach (NoteGroup group in groupList)
                    {
                        // �O���[�v���Ƃɏ������݂��g�債�ĕ`��i�������ݏ���t����j
                        group.Paint(g, scaleFactor);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region NoteGroup�N���X

        /// <summary>
        /// �������݂̂܂Ƃ܂�
        /// </summary>
        public class NoteGroup
        {
            #region �t�B�[���h�E�v���p�e�B

            private LinkedList<PhotoChatNote> myNoteList;
            private Rectangle range;
            private long lastTime;
            private bool isFixed;

            /// <summary>
            /// �O���[�v�쐬���̍ŏ��̗v�f
            /// </summary>
            public PhotoChatNote First
            {
                get
                {
                    if (myNoteList.First != null)
                        return myNoteList.First.Value;
                    else return null;
                }
            }

            /// <summary>
            /// ���̃O���[�v���Œ�i�����v�f�̒ǉ����ł��Ȃ��j���ꂽ���ǂ���
            /// </summary>
            public bool IsFixed
            {
                get { return isFixed; }
                set { IsFixed = value; }
            }

            /// <summary>
            /// ���̃O���[�v�̏������݃I�u�W�F�N�g�̃��X�g��Ԃ��B
            /// </summary>
            public LinkedList<PhotoChatNote> NoteList
            {
                get { return myNoteList; }
            }

            #endregion


            #region �R���X�g���N�^

            /// <summary>
            /// �V���ȏ������݃O���[�v���쐬����B
            /// </summary>
            /// <param name="note">�ŏ��̏�������</param>
            public NoteGroup(PhotoChatNote note)
            {
                this.range = note.Range;
                this.lastTime = note.Date.Ticks;
                this.myNoteList = new LinkedList<PhotoChatNote>();
                myNoteList.AddFirst(note);

                // �������݂��X�g���[�N�ȊO�ł���ΌŒ�
                if (note.Type == PhotoChatNote.TypeStroke)
                    isFixed = false;
                else
                    isFixed = true;
            }

            #endregion


            #region �O���[�v����

            /// <summary>
            /// �������݂��O���[�v�ɒǉ�����B
            /// �ǉ����ׂ����̊m�F���s���A�ǉ����Ȃ��ꍇ�̓O���[�v���Œ肷��B
            /// </summary>
            /// <param name="note">�ǉ����鏑������</param>
            /// <returns>�ǉ������ꍇ��true</returns>
            public bool AddNote(PhotoChatNote note)
            {
                try
                {
                    // �O��̏������݂���̎��ԊԊu���m�F
                    long span = note.Date.Ticks - lastTime;
                    if (span > TimeSpan.TicksPerSecond * PhotoChat.GroupingSpan)
                    {
                        isFixed = true;
                        return false;
                    }

                    // �O���[�v�̏������ݗ̈�Ƃ̈ʒu�Ԋu���m�F
                    int space = PhotoChat.GroupingSpace;
                    Rectangle inflatedRange = Rectangle.Inflate(range, space, space);
                    if (!inflatedRange.IntersectsWith(note.Range))
                    {
                        isFixed = true;
                        return false;
                    }

                    // �������݂��O���[�v�ɒǉ�
                    InsertInSortedList(note, myNoteList);
                    lastTime = note.Date.Ticks;
                    range = Rectangle.Union(range, note.Range);
                    return true;
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return false;
                }
            }


            /// <summary>
            /// �O���[�v���珑�����݂��폜����B
            /// </summary>
            /// <param name="id">�폜���鏑�����݂̒[��ID</param>
            /// <param name="serialNumber">�폜���鏑�����݂̒ʂ��ԍ�</param>
            /// <returns>�폜������true�A�������݂��Ȃ����false</returns>
            public bool RemoveNote(string id, long serialNumber)
            {
                try
                {
                    // �[��ID���m�F
                    if (id == myNoteList.First.Value.ID)
                    {
                        // �O���[�v���Œʂ��ԍ����������̂�T��
                        foreach (PhotoChatNote note in myNoteList)
                        {
                            if (serialNumber == note.SerialNumber)
                            {
                                // �폜
                                myNoteList.Remove(note);
                                if (myNoteList.Count != 0)
                                {
                                    // �������ݗ̈�̍Čv�Z
                                    range = myNoteList.First.Value.Range;
                                    foreach (PhotoChatNote temp in myNoteList)
                                    {
                                        range = Rectangle.Union(range, temp.Range);
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
                return false;
            }


            /// <summary>
            /// point�����̃O���[�v�̏������ݗ̈���ɂ��邩�ǂ�����Ԃ��B
            /// </summary>
            /// <param name="point">�_�̍��W</param>
            /// <returns>point���̈���ɂ����true</returns>
            public bool Contains(Point point)
            {
                return range.Contains(point);
            }


            /// <summary>
            /// point�ɂ���ŏ��Ɍ��������������݂�Ԃ��B
            /// </summary>
            /// <param name="point">���WPoint</param>
            /// <returns>point�ɂ��鏑�����݁B������Ȃ��ꍇ��null�B</returns>
            public PhotoChatNote GetPointedNote(Point point)
            {
                try
                {
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        if (note.Contains(point)) return note;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
                return null;
            }

            #endregion


            #region �O���[�v�`��

            /// <summary>
            /// ���̃O���[�v��`�悷��B
            /// </summary>
            /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
            public void Paint(Graphics g)
            {
                try
                {
                    // �������񂾃��[�U���̕`��
                    if (range.Width > PhotoChat.TrivialSize
                        || range.Height > PhotoChat.TrivialSize)
                    {
                        g.DrawString(First.Author, PhotoChat.RegularFont, infoBrush,
                            range.X, range.Y - PhotoChat.InfoFontSize);
                    }

                    // �X�g���[�N�ł���Η֊s�̕`��
                    if (First.Type == PhotoChatNote.TypeStroke)
                    {
                        foreach (PhotoChatNote note in myNoteList)
                            note.PaintOutline(g);
                    }

                    // �������݂̕`��
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        note.Paint(g);
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }


            /// <summary>
            /// ���̃O���[�v���w�萔�����`�悷��B
            /// </summary>
            /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
            /// <param name="count">�c��̕`�悷�鏑�����ݐ�</param>
            public void Paint(Graphics g, ref int count)
            {
                try
                {
                    // �������񂾃��[�U���̕`��
                    if (range.Width > PhotoChat.TrivialSize
                        || range.Height > PhotoChat.TrivialSize)
                    {
                        g.DrawString(First.Author, PhotoChat.RegularFont, infoBrush,
                            range.X, range.Y - PhotoChat.InfoFontSize);
                    }

                    // �X�g���[�N�ł���Η֊s�̕`��
                    if (First.Type == PhotoChatNote.TypeStroke)
                    {
                        int i = count;
                        foreach (PhotoChatNote note in myNoteList)
                        {
                            note.PaintOutline(g);
                            if (--i == 0) break;
                        }
                    }

                    // �������݂̕`��
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        note.Paint(g);
                        if (--count == 0) break;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }


            /// <summary>
            /// ���̃O���[�v���g�債�ĕ`�悷��B
            /// </summary>
            /// <param name="g">�O���t�B�b�N�R���e�L�X�g</param>
            /// <param name="scaleFactor">�g��{��</param>
            public void Paint(Graphics g, float scaleFactor)
            {
                try
                {
                    // �������ݏ��̕`��
                    if (range.Width > PhotoChat.TrivialSize
                        || range.Height > PhotoChat.TrivialSize)
                    {
                        float x = range.X * scaleFactor;
                        float y = range.Y * scaleFactor - PhotoChat.InfoFontSize;
                        g.DrawString(First.InfoText, PhotoChat.RegularFont, infoBrush, x, y);
                    }

                    // �X�g���[�N�ł���Η֊s�̕`��
                    if (First.Type == PhotoChatNote.TypeStroke)
                    {
                        foreach (PhotoChatNote note in myNoteList)
                            note.PaintOutline(g, scaleFactor);
                    }

                    // �������݂̕`��
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        note.Paint(g, scaleFactor);
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }

            #endregion
        }

        #endregion
    }
}
