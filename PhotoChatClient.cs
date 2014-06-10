using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;

namespace PhotoChat
{
    /// <summary>
    /// �f�[�^�̊Ǘ��Ƌ��n��������N���X
    /// </summary>
    public class PhotoChatClient
    {
        #region �t�B�[���h�E�v���p�e�B

        private static PhotoChatClient instance;
        private PhotoChatForm form;
        private ConnectionManager connectionManager;
        private SessionManager sessionManager;
        private SessionDialog sessionDialog;
        private StrokeRecognizer strokeRecognizer;
        private string id = string.Empty;
        private string userName = string.Empty;
        private long floorSerialNumber = -1;
        private long ceilingSerialNumber = -1;
        private List<string> userNameList = new List<string>();
        private Dictionary<string, string> userNameDictionary = new Dictionary<string, string>();
        private LinkedList<string> inputTagList = new LinkedList<string>();
        private List<string> sessionTagList = new List<string>();
        private long preTime;
        private string currentPhotoName;
        private object writeIndexLock = new object();
        private static object tagFileLock = new object();
        private static string myIPAddress;
        //public Capture gCapture = null;

        /// <summary>
        /// GUI�t�H�[�����擾����B
        /// </summary>
        public PhotoChatForm Form
        {
            get { return form; }
        }

        /// <summary>
        /// �ʐM�Ǘ������擾����B
        /// </summary>
        public ConnectionManager ConnectionManager
        {
            get { return connectionManager; }
        }

        /// <summary>
        /// �X�g���[�N�F���N���X���擾����B
        /// </summary>
        public StrokeRecognizer StrokeRecognizer
        {
            get { return strokeRecognizer; }
        }

        /// <summary>
        /// �[��ID���擾����B
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// ���[�U�����擾����B
        /// </summary>
        public string UserName
        {
            get { return userName; }
        }

        /// <summary>
        /// ���݂̃Z�b�V�����ɂ�����ŏ��̒ʂ��ԍ����擾����B
        /// </summary>
        public long FloorSerialNumber
        {
            get { return floorSerialNumber; }
        }

        /// <summary>
        /// ���̒[���ɂ�����ŐV�̃f�[�^�ʂ��ԍ����擾����B
        /// </summary>
        public long CeilingSerialNumber
        {
            get { return ceilingSerialNumber; }
        }

        /// <summary>
        /// ���݂̃Z�b�V���������擾����B
        /// </summary>
        public string CurrentSessionName
        {
            get { return sessionManager.CurrentSessionName; }
        }

        /// <summary>
        /// ���݂̃Z�b�V������ID���擾����B
        /// </summary>
        public string CurrentSessionID
        {
            get { return sessionManager.CurrentSessionID; }
        }

        /// <summary>
        /// �ŋߓ��͂����^�O�̃��X�g���擾����B
        /// </summary>
        public LinkedList<string> InputTagList
        {
            get { return inputTagList; }
        }

        /// <summary>
        /// ���݂̃Z�b�V�����Ŏg�p����Ă���^�O�̃��X�g���擾����B
        /// </summary>
        public List<string> SessionTagList
        {
            get { return sessionTagList; }
        }

        /// <summary>
        /// Instance�v���p�e�B�B
        /// Client�̃C���X�^���X�͂P�����ɐ������A��������擾�B
        /// set�͒�`���Ȃ��i�֎~�j�B
        /// </summary>
        public static PhotoChatClient Instance
        {
            get
            {
                if (instance == null)
                    return CreateInstance();
                else
                    return instance;
            }
        }

        public static string MyIPAddress
        {
            get { return myIPAddress; }
        }

        #endregion


        #region �R���X�g���N�^�E�������E�I������

        /// <summary>
        /// �O������N���X���쐬����B��̃��\�b�h�B
        /// private�ȃR���X�g���N�^���Ăяo���A����1�̃C���X�^���X���쐬�B
        /// </summary>
        /// <returns>�������PhotoChatClient�C���X�^���X</returns>
        public static PhotoChatClient CreateInstance()
        {
            if (instance == null)
            {
                try
                {
                    instance = new PhotoChatClient();
                    instance.Start();
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
            return instance;
        }


        /// <summary>
        /// private�ȃR���X�g���N�^�B
        /// �A�v���P�[�V�����̏��������s���B
        /// Client�̃C���X�^���X�͂����P�ɐ�������B
        /// </summary>
        private PhotoChatClient()
        {
            string CPName = Dns.GetHostName();
            IPHostEntry ipentry = Dns.GetHostEntry(CPName);

            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    myIPAddress = ip.ToString();
                    break;
                }
            }

            Console.WriteLine(myIPAddress);

            // �X�v���b�V���E�B���h�E�̕\��
            SplashWindow splashWindow = new SplashWindow();
            splashWindow.Show();

            // GUI������
            form = new PhotoChatForm(this);

            // �Z�b�V�����Ǘ�������
            sessionManager = new SessionManager();

            // �f�[�^�ǂݍ���
            LoadConfigFile();
            SetCeilingSerialNumber();
            LoadTagHistory();
            strokeRecognizer = new StrokeRecognizer();

            // �ʐM��������
            connectionManager = new ConnectionManager(this);

            // �X�v���b�V���E�B���h�E�����
            splashWindow.Close();

            // ���[�U���̓���
            form.Show();
            InputUserName();
        }

        private void SendAll(ISendable data)
        {
            connectionManager.SendAll((ISendable)data);
        }

        /// <summary>
        /// ���[�U���̓��͂����[�U�ɋ��߂�B
        /// </summary>
        private void InputUserName()
        {
            // ���[�U�����̓_�C�A���O�̕\��
            UserNameInputDialog inputDialog = new UserNameInputDialog();
            inputDialog.AddNameList(userNameList.ToArray());
            if (userNameList.Count == 0)
                inputDialog.UserName = Environment.UserName;
            else
                inputDialog.UserName = userNameList[0];
            if (inputDialog.ShowDialog(form) == DialogResult.OK)
                userName = inputDialog.UserName;
            else
                throw new Exception("�N�����ɏI����I�����܂����B");
            inputDialog.Dispose();

            // ID�̏����ݒ�:���[�U��+����
            if (id == string.Empty)
                id = userName + DateTime.Now.Ticks.ToString();

            // ���͂��ꂽ���[�U���𖼑O���X�g�̐擪�Ɉړ��i�ǉ��j
            userNameList.Remove(userName);
            userNameList.Insert(0, userName);

            // ���O���X�g������𒴂��Ă�����Â����̂�����
            if (userNameList.Count > PhotoChat.MaxUserNameListSize)
                userNameList.RemoveAt(userNameList.Count - 1);

            // �ݒ�t�@�C���̍X�V
            UpdateConfigFile();
        }


        /// <summary>
        /// �e�����N������B
        /// </summary>
        public void Start()
        {
            // �N��
            connectionManager.Start();

            // �Z�b�V�����_�C�A���O
            SelectSession(true);

            PhotoChat.WriteLog("Start", "�[��ID:" + id, "���[�U��:" + userName);
        }


        /// <summary>
        /// PhotoChat���I������iGUI������Ăяo�����j
        /// </summary>
        public void Close()
        {
            try
            {
                // �ʐM���I��
                if (connectionManager != null)
                    connectionManager.Close();

                // ���[�UIndex��ԃt�@�C���ۑ�
                SaveIndexStatusFile(true);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �ݒ�t�@�C�������E�g���q�m�F

        /// <summary>
        /// �ݒ�t�@�C�����X�V����
        /// </summary>
        internal void UpdateConfigFile()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(PhotoChat.ConfigFile, false))
                {
                    // ID�̏�������
                    sw.WriteLine("ID=" + id);

                    // ���[�U�����͗����̏�������
                    StringBuilder sb = new StringBuilder("UserNames=", 50);
                    foreach (string name in userNameList)
                    {
                        sb.Append(name).Append(PhotoChat.Delimiter);
                    }
                    sw.WriteLine(sb.ToString());

                    // GUI�ݒ�̏�������
                    sw.WriteLine("AutoScroll=" + form.IsAutoScrollMode.ToString());
                    sw.WriteLine("ReplayMode=" + form.IsReplayMode.ToString());
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ݒ�t�@�C����ǂݍ��ݐݒ�l���擾����
        /// </summary>
        private void LoadConfigFile()
        {
            try
            {
                // �ݒ�t�@�C����ǂݍ���
                if (File.Exists(PhotoChat.ConfigFile))
                {
                    // �t�@�C���̓ǂݍ���
                    Dictionary<string, string> configDictionary
                        = PhotoChat.GetDictionaryFromFile(PhotoChat.ConfigFile);

                    // ID�̎擾
                    configDictionary.TryGetValue("ID", out id);

                    // ���[�U�����͗����̎擾
                    string userNames;
                    if (configDictionary.TryGetValue("UserNames", out userNames))
                    {
                        string[] temp = userNames.Split(new Char[] { PhotoChat.Delimiter },
                                                        StringSplitOptions.RemoveEmptyEntries);
                        userNameList.AddRange(temp);
                    }

                    // GUI�ݒ�̎擾
                    string value;
                    if (configDictionary.TryGetValue("AutoScroll", out value))
                        form.IsAutoScrollMode = bool.Parse(value);
                    if (configDictionary.TryGetValue("ReplayMode", out value))
                        form.IsReplayMode = bool.Parse(value);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                throw new Exception();
            }
        }


        /// <summary>
        /// �t�@�C�����̊g���q���T�|�[�g����摜�`�����m�F����
        /// </summary>
        /// <param name="fileName">�t�@�C����</param>
        /// <returns>�T�|�[�g����摜�`���ł����true</returns>
        public bool CheckImageFileExtension(string fileName)
        {
            try
            {
                // �g���q��؂�o��
                int index = fileName.LastIndexOf('.');
                string extension = fileName.Substring(index + 1).ToLower();

                // �T�|�[�g������̂��m�F
                if (extension == "jpg" || extension == "jpeg"
                    || extension == "png" || extension == "gif"
                    || extension == "bmp" || extension == "wbmp")
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return false;
        }

        #endregion




        #region �V���A���ԍ�����

        /// <summary>
        /// �V�����ʂ��ԍ���Ԃ��B
        /// </summary>
        /// <returns>�V�����ʂ��ԍ�</returns>
        public long GetNewSerialNumber()
        {
            return Interlocked.Increment(ref ceilingSerialNumber);
        }


        /// <summary>
        /// �ŐV�̒ʂ��ԍ���ݒ肷��B
        /// </summary>
        private void SetCeilingSerialNumber()
        {
            if (id == null) return;

            try
            {
                // ���[�UIndex�f�B���N�g�������邩���ׂ�
                string directoryPath = Path.Combine(PhotoChat.UserIndexDirectory, id);
                if (Directory.Exists(directoryPath))
                {
                    // ���[�UIndex��ԃt�@�C����ǂ�
                    string lastSession = string.Empty;
                    if (!ReadIndexStatusFile(directoryPath, ref lastSession))
                    {
                        // �O�񐳏�I�����Ă��Ȃ��ꍇ�͍ŐV�ʂ��ԍ��𒲂ׂ�
                        if (lastSession != string.Empty)
                        {
                            // �O��Z�b�V�����̃��[�UIndex�t�@�C���𒲂ׂ�
                            string filePath = Path.Combine(directoryPath, lastSession + ".dat");
                            if (File.Exists(filePath))
                            {
                                using (StreamReader sr = new StreamReader(filePath))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        // ���傫�Ȕԍ�������΍X�V
                                        int index = line.IndexOf(PhotoChat.Delimiter);
                                        long number = long.Parse(line.Substring(0, index));
                                        if (ceilingSerialNumber < number) ceilingSerialNumber = number;
                                    }
                                }
                            }
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
        /// ���[�UIndex�̏�ԃt�@�C�����X�V����B
        /// </summary>
        /// <param name="closing">����I���ۑ����ł����true�A�ǂݍ��ݒ���ł����false</param>
        private void SaveIndexStatusFile(bool closing)
        {
            if (id == null) return;
            try
            {
                string directoryPath = Path.Combine(PhotoChat.UserIndexDirectory, id);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                string filePath = Path.Combine(directoryPath, PhotoChat.StatusFile);
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    // ����I���t���O�ƍŐV�ʂ��ԍ��̏�������
                    sw.WriteLine("NormalEnd=" + closing.ToString());
                    sw.WriteLine("LastSession=" + CurrentSessionID);
                    sw.WriteLine("CeilingNumber=" + ceilingSerialNumber.ToString());
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���[�UIndex��ԃt�@�C����ǂށB
        /// </summary>
        /// <param name="directoryPath">�f�B���N�g���p�X</param>
        /// <param name="lastSession">�Ō�ɊJ�����Z�b�V����</param>
        /// <returns>�O�񐳏�I�����Ă����true</returns>
        private bool ReadIndexStatusFile(string directoryPath, ref string lastSession)
        {
            try
            {
                string filePath = Path.Combine(directoryPath, PhotoChat.StatusFile);
                if (!File.Exists(filePath)) return false;
                Dictionary<string, string> statusDictionary
                    = PhotoChat.GetDictionaryFromFile(filePath);
                bool isNomalEnd = bool.Parse(statusDictionary["NormalEnd"]);
                lastSession = statusDictionary["LastSession"];
                ceilingSerialNumber = long.Parse(statusDictionary["CeilingNumber"]);

                return isNomalEnd;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return false;
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����ɂ�����ŏ��̒ʂ��ԍ���ݒ肷��B
        /// </summary>
        private void SetFloorSerialNumber()
        {
            if (id == null) return;

            try
            {
                // ���Ȃ��Ƃ����݂̍ŐV�ԍ��ȉ�
                floorSerialNumber = ceilingSerialNumber;

                string directory = Path.Combine(PhotoChat.UserIndexDirectory, id);
                string filePath = Path.Combine(directory, CurrentSessionID + ".dat");
                if (File.Exists(filePath))
                {
                    // ���݂̃Z�b�V�������ߋ��ɊJ�������Ƃ�����ꍇ
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            // ��菬���Ȕԍ�������΍X�V
                            int index = line.IndexOf(PhotoChat.Delimiter);
                            long number = long.Parse(line.Substring(0, index));
                            if (floorSerialNumber > number) floorSerialNumber = number;
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
        /// �[��ID�ɑΉ����郆�[�UIndex�𒲂ז���M�f�[�^�ƍŏ��E�ő�ԍ���ݒ肷��B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="floorNumber">�ŏ��ԍ�</param>
        /// <param name="ceilingNumber">�ő�ԍ�</param>
        /// <param name="unreceivedNumbers">����M�ԍ����X�g</param>
        public void CheckSerialNumber(string id, ref long floorNumber,
            ref long ceilingNumber, LinkedList<long> unreceivedNumbers)
        {
            if (id == null) return;

            try
            {
                string directoryPath = Path.Combine(PhotoChat.UserIndexDirectory, id);
                string filePath = Path.Combine(directoryPath, CurrentSessionID + ".dat");
                if (File.Exists(filePath))
                {
                    // ���݂̃Z�b�V�����̃f�[�^�𒲂ׂ�
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            int index = line.IndexOf(PhotoChat.Delimiter);
                            long number = long.Parse(line.Substring(0, index));

                            // �ŏ��ԍ�
                            if (floorNumber < 0)
                                floorNumber = number;
                            else if (floorNumber > number)
                            {
                                while (--floorNumber > number)
                                    unreceivedNumbers.AddFirst(floorNumber);
                                floorNumber = number;
                            }

                            // �ő�ԍ�
                            if (ceilingNumber < 0)
                                ceilingNumber = number;
                            else if (ceilingNumber < number)
                            {
                                while (++ceilingNumber < number)
                                    unreceivedNumbers.AddLast(ceilingNumber);
                                ceilingNumber = number;
                            }

                            // ����M�ԍ�
                            unreceivedNumbers.Remove(number);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �Z�b�V�����_�C�A���O

        /// <summary>
        /// �Z�b�V�����I���_�C�A���O��\�����đI�����ꂽ�Z�b�V�������J���B
        /// </summary>
        /// <param name="firstTime">�N�������ǂ���</param>
        public void SelectSession(bool firstTime)
        {
            try
            {
                sessionDialog = new SessionDialog();

                // �N�����̓L�����Z���{�^���𖳌���
                if (firstTime)
                {
                    sessionDialog.CancelButtonEnabled = false;
                }
                // �ߋ��̃Z�b�V�����I���{�^����ǉ�
                AddPastSessionButton();

                // �_�C�A���O�\��
                if (sessionDialog.ShowDialog(form) == DialogResult.OK)
                {
                    // �ڑ���؂�
                    connectionManager.DisconnectAll();

                    // �T���l�C���ꗗ���N���A
                    if (!firstTime)
                    {
                        form.ClearThumbnailList();
                        form.ClearReviewPanel();
                    }

                    // �I�����ꂽ�Z�b�V�������J��
                    string[] photoArray = sessionManager.OpenSession(
                        sessionDialog.SessionName, sessionDialog.SessionID);
                    SaveIndexStatusFile(false);
                    SetFloorSerialNumber();
                    LoadSessionTag();

                    // �T���l�C���ꗗ�փT���l�C�����ꊇ�}��
                    if (photoArray != null)
                    {
                        List<Thumbnail> thumbnailList = new List<Thumbnail>(photoArray.Length);
                        foreach (string photoName in photoArray)
                        {
                            try
                            {
                                thumbnailList.Add(new Thumbnail(photoName));
                            }
                            catch (UnsupportedDataException ude)
                            {
                                PhotoChat.WriteErrorLog(ude.ToString());
                            }
                        }
                        form.AddThumbnailList(thumbnailList);
                    }
                }
                sessionDialog.Dispose();
                sessionDialog = null;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ߋ��̃Z�b�V�����I���{�^����ǉ�����B
        /// </summary>
        private void AddPastSessionButton()
        {
            try
            {
                // �ߋ��̃Z�b�V�������ŋ߂̂��̂��珇�ɕ��בւ�
                List<SessionManager.SessionInfo> sessionList = sessionManager.SessionList;
                sessionList.Sort();
                sessionList.Reverse();

                // �{�^���ǉ�
                foreach (SessionManager.SessionInfo info in sessionList)
                {
                    if (info.ID == CurrentSessionID)
                        sessionDialog.AddPastButton(info.Name, info.ID, "���݂̃Z�b�V����");
                    else
                        sessionDialog.AddPastButton(info.Name, info.ID, info.GetDateText());
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �Z�b�V�����I���_�C�A���O�ɋ߂��̃Z�b�V�����I���{�^����ǉ�����B
        /// </summary>
        /// <param name="sessionName">�Z�b�V������</param>
        /// <param name="sessionID">�Z�b�V����ID</param>
        public void AddNearbySessionButton(string sessionName, string sessionID)
        {
            try
            {
                if (sessionDialog != null)
                {
                    sessionDialog.AddNearbyButton(sessionName, sessionID);
                    sessionDialog.Refresh();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �V�K�f�[�^����

        /// <summary>
        /// �V����PhotoChatImage��o�^�E�ۑ�����B
        /// </summary>
        /// <param name="image">�V����PhotoChatImage</param>
        /// <param name="isLatest">�ŐV�̂��̂ł��邱�Ƃ��ۏႳ���ꍇ��true�Ƃ���B</param>
        /// <returns>�o�^�ɐ���������true�A�V�����f�[�^�łȂ��ꍇ��false��Ԃ��B</returns>
        public bool NewData(PhotoChatImage image, bool isLatest)
        {
            try
            {
                Photo photo = image as Photo;
                if (photo != null)
                {
                    // ���[�UIndex�t�@�C���ւ̏������݁i�d�����Ȃ��j
                    if (!AppendIndexData(photo.ID, photo.SerialNumber, photo.GetIndexString(), isLatest))
                        return false;

                    // Thumbnail�摜�̍쐬�E�o�^
                    sessionManager.AppendPhoto(photo.PhotoName);
                    Thumbnail.SaveImage(photo.PhotoName, photo.Image);
                    form.AddThumbnail(new Thumbnail(photo));

                    // �V�������I��
                    if (form.IsLiveMode) AutoSelect(photo.PhotoName);

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
        /// �V����PhotoChatNote��o�^�E�ۑ�����B
        /// </summary>
        /// <param name="note">�V����PhotoChatNote</param>
        /// <param name="isLatest">�ŐV�̂��̂ł��邱�Ƃ��ۏႳ���ꍇ��true�Ƃ���B</param>
        /// <returns>�o�^�ɐ���������true�A�V�����f�[�^�łȂ��ꍇ��false��Ԃ��B</returns>
        public bool NewData(PhotoChatNote note, bool isLatest)
        {
            try
            {
                // ���[�UIndex�t�@�C���ւ̏������݁i�d�����Ȃ��j
                if (!AppendIndexData(note.ID, note.SerialNumber, note.GetIndexString(), isLatest))
                    return false;
                note.Save();

                // �\������Photo�ł���Ώ������݂�ǉ�
                Photo photo = form.GetCurrentPhoto(note.PhotoName);
                if (photo != null)
                {
                    photo.AddNote(note);
                    form.UpdateReviewPanel(note);
                }

                // �^�O�������݂ł���΃^�O�t�@�C���ɒǋL
                AppendTag(note);

                // �T���l�C���̏������݃J�E���g�𑝌�
                form.CountThumbnailNote(note);

                // �V�������I��
                if (form.IsLiveMode) AutoSelect(note.PhotoName);
                return true;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return false;
        }


        /// <summary>
        /// �V����SharedFile��o�^�E�ۑ�����B
        /// </summary>
        /// <param name="sharedFile">�V����SharedFile</param>
        /// <param name="isLatest">�ŐV�̂��̂ł��邱�Ƃ��ۏႳ���ꍇ��true�Ƃ���B</param>
        /// <returns>�o�^�ɐ���������true�A�V�����f�[�^�łȂ��ꍇ��false��Ԃ��B</returns>
        public bool NewData(SharedFile sharedFile, bool isLatest)
        {
            try
            {
                // ���[�UIndex�t�@�C���ւ̏������݁i�d�����Ȃ��j
                if (!AppendIndexData(sharedFile.ID, sharedFile.SerialNumber, sharedFile.GetIndexString(), isLatest))
                    return false;

                // �ۑ�
                switch (sharedFile.Type)
                {
                    case SharedFile.TypeSoundFile:
                        sharedFile.Save(PhotoChat.SoundNoteDirectory);
                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return false;
        }


        /// <summary>
        /// �V���f�[�^�̂������ʐ^�̎����I���B
        /// �������O��̕ύX����2�b�ȓ��͕ύX���Ȃ��B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        private void AutoSelect(string photoName)
        {
            long span = DateTime.Now.Ticks - preTime;
            if (photoName != currentPhotoName
                && span > TimeSpan.TicksPerSecond * PhotoChat.AutoSelectSpan)
            {
                form.ShowPhoto(new Photo(photoName));
                currentPhotoName = photoName;
                preTime = DateTime.Now.Ticks;
            }
        }

        #endregion




        #region �C���f�b�N�X�t�@�C���ǋL

        /// <summary>
        /// ���[�UIndex�t�@�C���ɒǋL����
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="serialNumber">�ǋL�f�[�^�̒ʂ��ԍ�</param>
        /// <param name="dataString">�ǋL���镶����</param>
        /// <param name="isLatest">�ŐV�̏������݂ł��邱�Ƃ��ۏႳ��Ă���ꍇ��true</param>
        /// <returns>�ǋL�ɐ��������ꍇ��true�A���łɃf�[�^���������ꍇ��false��Ԃ��B</returns>
        private bool AppendIndexData(string id, long serialNumber, string dataString, bool isLatest)
        {
            try
            {
                lock (writeIndexLock)
                {
                    string filePath = GetIndexFilePath(id, CurrentSessionID);

                    // �d�����Ȃ����m�F
                    if (!isLatest)
                    {
                        string line, number = serialNumber.ToString();
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.StartsWith(number)) return false;
                            }
                        }
                    }

                    // �t�@�C���ɒǋL
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.WriteLine(dataString);
                        sw.Flush();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return false;
            }
        }


        /// <summary>
        /// �[��ID�ƃZ�b�V����ID�ɑΉ����郆�[�UIndex�t�@�C���p�X��Ԃ��B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <returns>�Ή����郆�[�UIndex�t�@�C����</returns>
        private string GetIndexFilePath(string id, string sessionID)
        {
            try
            {
                // �[��ID�ɑΉ�����f�B���N�g���p�X�̎擾�i�Ȃ���΍쐬�j
                string directoryPath = Path.Combine(PhotoChat.UserIndexDirectory, id);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // Index�t�@�C���p�X��Ԃ�
                return Path.Combine(directoryPath, sessionID + ".dat");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }

        #endregion




        #region �f�[�^�v��

        /// <summary>
        /// �[��ID�ƒʂ��ԍ��ɑΉ�����f�[�^��Ԃ��B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="serialNumber">�ʂ��ԍ�</param>
        /// <returns>�v�����ꂽ�f�[�^�B�Ȃ����null��Ԃ��B</returns>
        public ISendable Request(string id, long serialNumber)
        {
            try
            {
                // ���[�UIndex�t�@�C������f�[�^��T��
                string filePath = GetIndexFilePath(id, CurrentSessionID);
                if (File.Exists(filePath))
                {
                    string line, number = serialNumber.ToString();
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith(number)) break;
                        }
                    }
                    if (line == null) return null;

                    // �C���X�^���X�̍쐬
                    int index1 = number.Length + 1;
                    int index2 = line.IndexOf(PhotoChat.Delimiter, index1);
                    int type = int.Parse(line.Substring(index1, index2 - index1));
                    switch (type)
                    {
                        case PhotoChatImage.TypePhoto:
                            return new Photo(line.Substring(index2 + 1));

                        case PhotoChatNote.TypeHyperlink:
                        case PhotoChatNote.TypeRemoval:
                        case PhotoChatNote.TypeStroke:
                        case PhotoChatNote.TypeTag:
                        case PhotoChatNote.TypeText:
                        case PhotoChatNote.TypeSound:
                            return PhotoChatNote.CreateInstance(type, line.Substring(index2 + 1));

                        case SharedFile.TypeSoundFile:
                            string path = Path.Combine(
                                PhotoChat.SoundNoteDirectory, line.Substring(index2 + 1));
                            return new SharedFile(type, id, serialNumber, path);
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




        #region �^�O����

        /// <summary>
        /// note���^�O�ł���΃^�O�t�@�C������у^�O���X�g�ɒǉ�����B
        /// </summary>
        /// <param name="note">�V����PhotoChatNote</param>
        private void AppendTag(PhotoChatNote note)
        {
            // �^�O�������݂̂ݏ���
            if (note.Type != PhotoChatNote.TypeTag) return;

            try
            {
                Tag tag = (Tag)note;

                // �^�O�t�@�C���ɒǋL
                string filePath = GetTagFilePath(tag.TagString);
                lock (tagFileLock)
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.WriteLine(tag.PhotoName);
                        sw.Flush();
                    }
                }

                // �Z�b�V�����^�O���X�g�ɒǉ�
                AddSessionTag(tag.TagString);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �d�����Ȃ��悤�Z�b�V�����^�O���X�g�ɒǉ����t�@�C���ɒǋL����B
        /// </summary>
        /// <param name="tag">�ǉ�����^�O</param>
        private void AddSessionTag(string tag)
        {
            if (!sessionTagList.Contains(tag))
            {
                sessionTagList.Add(tag);

                string filePath
                    = Path.Combine(PhotoChat.TagListDirectory, CurrentSessionID + ".dat");
                lock (tagFileLock)
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.WriteLine(tag);
                        sw.Flush();
                    }
                }
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����Ŏg�p���ꂽ�^�O���X�g���t�@�C�����烍�[�h����B
        /// </summary>
        private void LoadSessionTag()
        {
            try
            {
                sessionTagList.Clear();
                string filePath
                    = Path.Combine(PhotoChat.TagListDirectory, CurrentSessionID + ".dat");
                if (File.Exists(filePath))
                {
                    lock (tagFileLock)
                    {
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                                sessionTagList.Add(line);
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
        /// �d�����Ȃ��悤�ŋߓ��͂����^�O���X�g�ɒǉ����t�@�C�����X�V����B
        /// </summary>
        /// <param name="tag">�ǉ�����^�O</param>
        public void UpdateInputTags(string tag)
        {
            if (!inputTagList.Contains(tag))
            {
                // ���X�g�̐擪�ɒǉ�����������ꂽ���̂��폜
                inputTagList.AddFirst(tag);
                if (inputTagList.Count > PhotoChat.InputTagListSize)
                    inputTagList.RemoveLast();
            }
            else
            {
                if (tag == inputTagList.First.Value)
                    return;

                // ���͂��ꂽ�^�O��擪�Ɉړ�
                inputTagList.Remove(tag);
                inputTagList.AddFirst(tag);
            }

            lock (tagFileLock)
            {
                using (StreamWriter sw = new StreamWriter(PhotoChat.InputTagsFile))
                {
                    foreach (string temp in InputTagList)
                        sw.WriteLine(temp);
                    sw.Flush();
                }
            }
        }


        /// <summary>
        /// �ŋߓ��͂����^�O�̃��X�g���t�@�C�����烍�[�h����B
        /// </summary>
        private void LoadTagHistory()
        {
            try
            {
                inputTagList.Clear();
                if (File.Exists(PhotoChat.InputTagsFile))
                {
                    lock (tagFileLock)
                    {
                        using (StreamReader sr = new StreamReader(PhotoChat.InputTagsFile))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                                inputTagList.AddLast(line);
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
        /// �^�O�t�@�C���̃p�X���擾����B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        /// <returns>�^�O�t�@�C���̃p�X</returns>
        public static string GetTagFilePath(string tag)
        {
            try
            {
                return Path.Combine(PhotoChat.TagDirectory, tag + ".dat");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// �w�肵���^�O�̕t�����ʐ^���̔z���Ԃ��B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        /// <returns>�^�O�̕t�����ʐ^���̔z��</returns>
        public static string[] SearchTaggedPhoto(string tag)
        {
            try
            {
                string data = string.Empty;
                lock (tagFileLock)
                {
                    // �^�O�t�@�C���̓ǂݍ���
                    string filePath = GetTagFilePath(tag);
                    if (File.Exists(filePath))
                    {
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            data = reader.ReadToEnd();
                        }
                    }
                }

                // ������z��ɕϊ����ĕԂ�
                return data.Split(
                    new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new string[0];
            }
        }


        /// <summary>
        /// �w�肵���^�O�̕t�����ʐ^����Ԃ��B
        /// </summary>
        /// <param name="tagArray">�^�O�z��</param>
        /// <param name="andSearch">AND�����Ȃ�true�AOR�����Ȃ�false</param>
        /// <returns>�^�O�̕t�����ʐ^���̔z��</returns>
        public static string[] SearchTaggedPhoto(string[] tagArray, bool andSearch)
        {
            try
            {
                // �e�^�O�̕t�����ʐ^���̔z����擾
                string[][] photoArrays = new string[tagArray.Length][];
                for (int i = 0; i < tagArray.Length; i++)
                    photoArrays[i] = SearchTaggedPhoto(tagArray[i]);

                // �ʐ^���z���AND/OR�������ĕԂ�
                if (andSearch)
                    return PhotoChat.Intersect(photoArrays);
                else
                    return PhotoChat.Union(photoArrays);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new string[0];
            }
        }

        #endregion


        #region �ߐڊ֌W

        /// <summary>
        /// �߂��ɂ��郆�[�U���̔z���Ԃ��B
        /// </summary>
        /// <returns>�߂��ɂ��郆�[�U���z��</returns>
        public string[] GetNearUsers()
        {
            try
            {
                return new string[0];
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new string[0];
            }
        }


        /// <summary>
        /// ���[�U���ϊ������Ƀ��[�U��ǉ�����B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="userName">���[�U��</param>
        public void AddUser(string id, string userName)
        {
            try
            {
                userNameDictionary[id] = userName;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region �A�b�v���[�h�E�Z�b�V�����摜�ꊇ�ۑ��iHTML���j

        /// <summary>
        /// PhotoChat�T�[�o�ɃA�b�v���[�h����B
        /// </summary>
        public void UploadToServer()
        {
            try
            {
                ServerUploadDialog uploadDialog = new ServerUploadDialog();

                // �A�b�v���[�h����Z�b�V�����̎擾
                uploadDialog.AddSessionPanels(sessionManager.GetUploadSessions());
                // �S�Ă��ăA�b�v���[�h����ꍇ�͂�����
                //uploadDialog.AddSessionPanels(sessionManager.GetAllSession());

                if (uploadDialog.ShowDialog(form) == DialogResult.OK)
                {
                    // �A�b�v���[�h�Ǘ��t�@�C���̍X�V
                    if (uploadDialog.UploadComplete)
                        sessionManager.RemoveUploadSessions(uploadDialog.UploadedSessionList);
                }
                uploadDialog.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����̎ʐ^�Ə������݂��I���W�i���T�C�Y�ňꊇ�ۑ�����B
        /// HTML�t�@�C�����쐬����B
        /// </summary>
        public void SaveSessionImages()
        {
            try
            {
                // �ۑ���f�B���N�g������
                string saveDirectory = InputSaveDirectory();
                if (saveDirectory == null) return;

                // ���݂̃Z�b�V�����t�@�C����ǂݎ��
                string[] photoNameArray = sessionManager.GetCurrentSessionPhotoArray();
                if (photoNameArray == null) return;

                // �i�s�󋵕\���E�B���h�E
                ProgressWindow progressWindow = new ProgressWindow(
                    photoNameArray.Length, "�ʐ^���t�H���_�ɕۑ����Ă��܂�");
                progressWindow.Show(form);

                // �Z�b�V�����̎ʐ^���P�����쐬
                int count = 0;
                string directoryPath = string.Empty;
                foreach (string photoName in photoNameArray)
                {
                    // �摜��ۑ�����f�B���N�g������薇�����ƂɕύX
                    if ((count % PhotoChat.SaveFolderSize) == 0)
                    {
                        // �ۑ���f�B���N�g����ύX
                        string name = "Photo" + (count + 1) + "_" + (count + PhotoChat.SaveFolderSize);
                        directoryPath = Path.Combine(saveDirectory, name);
                        Directory.CreateDirectory(directoryPath);
                    }

                    // �摜���쐬���ĕۑ�
                    Bitmap image = GetOriginalSizePhoto(photoName);
                    if (image != null)
                    {
                        string filePath = Path.Combine(directoryPath, photoName + ".jpg");
                        image.Save(filePath, ImageFormat.Jpeg);
                        image.Dispose();
                    }
                    progressWindow.SetCount(++count);
                }

                // HTML�t�@�C���쐬
                CreateHtmlFiles(photoNameArray, saveDirectory);
                progressWindow.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����̎ʐ^�Ə������݂��I���W�i���T�C�Y��Flickr�ɃA�b�v���[�h����B
        /// </summary>
        public void UploadSessionImagesToFlickr()
        {
            try
            {
                FlickrUploadDialog uploadDialog = new FlickrUploadDialog();
                if (uploadDialog.ShowDialog(form) == DialogResult.OK)
                {
                    // �A�b�v���[�h�ݒ�ǂݎ��
                    FlickrNet.Flickr flickr = uploadDialog.Flickr;
                    string description = uploadDialog.Description;
                    bool isPublic = uploadDialog.IsPublic;
                    bool isFamily = uploadDialog.IsFamily;
                    bool isFriend = uploadDialog.IsFamily;

                    // ���݂̃Z�b�V�����t�@�C����ǂݎ��
                    string[] photoNameArray = sessionManager.GetCurrentSessionPhotoArray();
                    if (photoNameArray != null)
                    {
                        // �i�s�󋵕\���E�B���h�E
                        ProgressWindow progressWindow = new ProgressWindow(
                            photoNameArray.Length, "�ʐ^��Flickr�ɃA�b�v���[�h���Ă��܂�");
                        progressWindow.Show(form);

                        // �Z�b�V�����̎ʐ^���P�����쐬���ăA�b�v���[�h
                        int count = 0;
                        string title, tags;
                        string tempFile = "temp_FlickrUpload.jpg";
                        foreach (string photoName in photoNameArray)
                        {
                            Bitmap image = GetOriginalSizePhoto(photoName);
                            if (image != null)
                            {
                                // �ꎞ�t�@�C���ɉ摜��ۑ�
                                image.Save(tempFile, ImageFormat.Jpeg);
                                image.Dispose();

                                // �^�C�g���쐬
                                count++;
                                title = CurrentSessionName + count.ToString();

                                // �^�O�擾
                                Thumbnail thumbnail = form.GetThumbnail(photoName);
                                if (thumbnail != null)
                                    tags = thumbnail.Tags;
                                else
                                    tags = Thumbnail.GetTags(photoName);

                                // �A�b�v���[�h
                                flickr.UploadPicture(tempFile, title,
                                    description, tags, isPublic, isFamily, isFriend);
                                progressWindow.SetCount(count);
                            }
                        }
                        File.Delete(tempFile);
                        progressWindow.Dispose();
                    }
                }
                uploadDialog.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show("Flickr�ɃA�N�Z�X�ł��܂���ł���");
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����̃f�[�^��SmartCalendar�ɃG�N�X�|�[�g����B
        /// </summary>
        public void ExportToSmartCalendar()
        {
            try
            {
                // �ۑ���f�B���N�g������
                string saveDirectory = string.Empty;
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "�G�N�X�|�[�g��̃t�H���_���w�肵�Ă��������B";
                if (folderDialog.ShowDialog(form) == DialogResult.OK)
                    saveDirectory = folderDialog.SelectedPath;
                folderDialog.Dispose();
                if (saveDirectory == string.Empty) return;

                // ���݂̃Z�b�V�����t�@�C����ǂݎ��
                string[] photoNameArray = sessionManager.GetCurrentSessionPhotoArray();
                if (photoNameArray != null)
                {
                    // �i�s�󋵃E�B���h�E
                    ProgressWindow progressWindow = new ProgressWindow(
                        photoNameArray.Length, "SmartCalendar�ɃG�N�X�|�[�g���Ă��܂�");
                    progressWindow.Show(form);

                    // �f�B���N�g���쐬
                    DateTime date = form.GetThumbnail(photoNameArray[0]).Date;
                    string year = date.Year.ToString("0000");
                    string month = date.Month.ToString("00");
                    string day = date.Day.ToString("00");
                    string directoryPath = Path.Combine(saveDirectory, year);
                    directoryPath = Path.Combine(directoryPath, year + month);
                    directoryPath = Path.Combine(directoryPath, year + "_" + month + day);
                    Directory.CreateDirectory(directoryPath);

                    // �Z�b�V�����̎ʐ^��1�����쐬���ăG�N�X�|�[�g
                    int count = 0;
                    string tags = string.Empty;
                    foreach (string photoName in photoNameArray)
                    {
                        Bitmap image = GetOriginalSizePhoto(photoName);
                        if (image != null)
                        {
                            // �B�e����
                            Thumbnail thumbnail = form.GetThumbnail(photoName);
                            PropertyItem propItem = Properties.Resources.forExif.PropertyItems[0];
                            propItem.Id = 0x9003;
                            propItem.Type = 2;
                            propItem.Value = Encoding.ASCII.GetBytes(
                                thumbnail.Date.ToString("yyyy:MM:dd HH:mm:ss"));
                            propItem.Len = propItem.Value.Length;
                            image.SetPropertyItem(propItem);

                            // �^�O���
                            if (thumbnail != null)
                                tags = thumbnail.Tags;
                            else
                                tags = Thumbnail.GetTags(photoName);
                            propItem.Id = 0x9C9C;
                            propItem.Type = 7;
                            propItem.Value = Encoding.GetEncoding("UNICODE").GetBytes(tags + "\0");
                            propItem.Len = propItem.Value.Length;
                            image.SetPropertyItem(propItem);

                            // �ۑ�
                            string filePath = Path.Combine(directoryPath, photoName + ".jpg");
                            image.Save(filePath, ImageFormat.Jpeg);
                            image.Dispose();
                        }
                        progressWindow.SetCount(++count);
                    }
                    progressWindow.Dispose();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ʐ^�Ə������݂��I���W�i���T�C�Y�ŕ`�悵���摜�t�@�C�����쐬����B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�쐬�����摜</returns>
        public static Bitmap GetOriginalSizePhoto(string photoName)
        {
            try
            {
                // �I���W�i���T�C�YPhoto�̍쐬
                Photo photo = new Photo(photoName, true);

                // �g��{���Z�o
                float scale = 1;
                if (photo.Image.Width > PhotoChat.PhotoWidth
                    || photo.Image.Height > PhotoChat.PhotoHeight)
                {
                    float widthScale = (float)photo.Image.Width / (float)PhotoChat.PhotoWidth;
                    float heightScale = (float)photo.Image.Height / (float)PhotoChat.PhotoHeight;
                    scale = Math.Max(widthScale, heightScale);
                }

                // �r�b�g�}�b�v�쐬
                Bitmap image = new Bitmap(
                    (int)(PhotoChat.ReviewWidth * scale), (int)(PhotoChat.ReviewHeight * scale));
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.Clear(Color.White);

                    // �ʐ^�̕`��
                    g.DrawImage(
                        photo.Image, PhotoChat.LeftMargin * scale, PhotoChat.TopMargin * scale);

                    // �������݂̕`��
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    photo.PaintNotes(g, scale);
                }
                photo.Dispose();

                return image;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// ���[�U�ɕۑ���f�B���N�g������͂�����B
        /// </summary>
        /// <returns>�ۑ���f�B���N�g���̃p�X</returns>
        private string InputSaveDirectory()
        {
            string directory = null;

            // �ۑ���f�B���N�g�����̓_�C�A���O
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "�ۑ���̃t�H���_���w�肵�Ă��������B"
                + Environment.NewLine + "�V���Ƀt�H���_���쐬���ĕۑ����܂��B";
            if (folderDialog.ShowDialog(form) == DialogResult.OK)
            {
                // ���͂��ꂽ�f�B���N�g���ɃZ�b�V�������Ɠ������O�̃f�B���N�g��������Όx��
                string temp = folderDialog.SelectedPath;
                directory = Path.Combine(temp, sessionManager.CurrentSessionName);
                if (Directory.Exists(directory))
                {
                    string text = "�ۑ���t�H���_���ɃZ�b�V�������Ɠ������O�̃t�H���_������܂��B"
                        + Environment.NewLine
                        + "���̃t�H���_�ɓ����t�@�C��������Ώ㏑�����Ă��܂��܂��������ł����H";
                    string caption = "�m�F�F�t�@�C���㏑���̋���";
                    if (MessageBox.Show(form, text, caption, MessageBoxButtons.OKCancel)
                        == DialogResult.Cancel)
                    {
                        directory = null;
                    }
                }
            }
            folderDialog.Dispose();
            return directory;
        }


        /// <summary>
        /// �e�t�H���_�Ɏʐ^�ꗗHTML�t�@�C�����쐬����B
        /// </summary>
        /// <param name="photoNameArray">�ʐ^���z��</param>
        /// <param name="saveDirectory">�ۑ���f�B���N�g��</param>
        private void CreateHtmlFiles(string[] photoNameArray, string saveDirectory)
        {
            if (photoNameArray.Length == 0) return;

            // �ʐ^��ۑ������t�H���_�ɈꗗHTML�t�@�C���쐬
            List<string> fileList = new List<string>();
            int count = 0;
            do
            {
                try
                {
                    string name = "Photo" + (count + 1) + "_" + (count + PhotoChat.SaveFolderSize);
                    string directoryPath = Path.Combine(saveDirectory, name);
                    string filePath = Path.Combine(directoryPath, "index.html");
                    StringBuilder sb = new StringBuilder(100 * PhotoChat.SaveFolderSize);

                    // �w�b�_
                    sb.Append("<html lang=\"ja\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=Shift_JIS\">\n<title>");
                    sb.Append(Path.GetFileName(saveDirectory));
                    sb.Append(count / PhotoChat.SaveFolderSize + 1);
                    sb.Append("</title>\n</head>\n<body bgcolor=\"#F0F8FF\">\n");

                    // �{�f�B
                    do
                    {
                        sb.Append(CreatePhotoHtml(photoNameArray[count]));
                        if (++count == photoNameArray.Length) break;
                    } while ((count % PhotoChat.SaveFolderSize) != 0);

                    // �t�b�^
                    sb.Append("</body>\n</html>\n");

                    // �t�@�C���ۑ�
                    using (StreamWriter sw = new StreamWriter(filePath, false, PhotoChat.FileEncoding))
                    {
                        sw.Write(sb.ToString());
                        sw.Flush();
                    }
                    fileList.Add(Path.Combine(name, "index.html"));
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            } while (count < photoNameArray.Length);

            // �C���f�b�N�X�t�@�C���쐬
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html lang=\"ja\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=Shift_JIS\">\n<title>");
                sb.Append(Path.GetFileName(saveDirectory)).Append("�@�ڎ�");
                sb.Append("</title>\n</head>\n<body bgcolor=\"#F0F8FF\">\n<h2>");
                sb.Append(Path.GetFileName(saveDirectory)).Append("�@�ڎ�");
                sb.Append("</h2>\n<p>\n<ul>\n");
                count = 0;
                foreach (string fileName in fileList)
                {
                    sb.Append("<li><a href=\"").Append(fileName).Append("\">�ʐ^");
                    sb.Append(count + 1).Append("����");
                    sb.Append(count + PhotoChat.SaveFolderSize).Append("</a>\n");
                    count += PhotoChat.SaveFolderSize;
                }
                sb.Append("</ul>\n</p>\n</body>\n</html>\n");

                // �t�@�C���ۑ�
                string filePath = Path.Combine(saveDirectory, "index.html");
                using (StreamWriter sw = new StreamWriter(filePath, false, PhotoChat.FileEncoding))
                {
                    sw.Write(sb.ToString());
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ʐ^�ɂ��Ă�HTML���쐬����B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�쐬����HTML</returns>
        private string CreatePhotoHtml(string photoName)
        {
            StringBuilder sb = new StringBuilder(100);
            try
            {
                // �ʐ^�̃T���l�C���擾
                Thumbnail thumbnail = form.GetThumbnail(photoName);
                if (thumbnail == null)
                    thumbnail = new Thumbnail(photoName);

                sb.Append("<div>").Append(thumbnail.Date.ToString("yyyy/MM/dd HH:mm:ss"));
                sb.Append("�@�@�B�e�ҁF").Append(thumbnail.Author).Append("</div>\n");
                sb.Append("<div><img src=\"").Append(photoName).Append(".jpg\"><BR><BR></div>\n");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return sb.ToString();
        }

        #endregion
    }
}
