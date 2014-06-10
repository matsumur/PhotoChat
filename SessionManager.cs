using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// �Z�b�V�����Ǘ����s���B
    /// </summary>
    public class SessionManager
    {
        #region �t�B�[���h�E�v���p�e�B

        private static object sessionLock = new object();
        private string currentSessionName = string.Empty;
        private string currentSessionFile = string.Empty;
        private string currentSessionID = string.Empty;
        private List<SessionInfo> sessionList;

        /// <summary>
        /// ���݂̃Z�b�V���������擾����B
        /// </summary>
        public string CurrentSessionName
        {
            get { return currentSessionName; }
        }

        /// <summary>
        /// ���݂̃Z�b�V����ID���擾����B
        /// </summary>
        public string CurrentSessionID
        {
            get { return currentSessionID; }
        }

        /// <summary>
        /// �Z�b�V�������X�g���擾����B
        /// </summary>
        public List<SessionInfo> SessionList
        {
            get { return sessionList; }
        }

        #endregion




        #region �R���X�g���N�^

        /// <summary>
        /// �Z�b�V�����Ǘ�������������B
        /// </summary>
        public SessionManager()
        {
            // �Z�b�V�����Ǘ��t�@�C����ǂݍ���
            sessionList = new List<SessionInfo>();
            LoadSessionManagerFile();
        }

        #endregion




        #region �ŋ߂̃Z�b�V�������

        /// <summary>
        /// �Z�b�V�����Ǘ��t�@�C����ǂݍ��ށB
        /// </summary>
        private void LoadSessionManagerFile()
        {
            if (File.Exists(PhotoChat.SessionManagerFile))
            {
                try
                {
                    lock (sessionList)
                    {
                        using (StreamReader sr = new StreamReader(PhotoChat.SessionManagerFile))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                                sessionList.Add(new SessionInfo(line));
                        }
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }

        
        /// <summary>
        /// �Z�b�V�����Ǘ��t�@�C�����X�V����B
        /// </summary>
        private void SaveSessionManagerFile()
        {
            try
            {
                lock (sessionList)
                {
                    using (StreamWriter sw = new StreamWriter(PhotoChat.SessionManagerFile))
                    {
                        foreach (SessionInfo sessionInfo in sessionList)
                            sw.WriteLine(sessionInfo.ToString());
                        sw.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ŋ߂̃Z�b�V������񃊃X�g�ɗv�f��ǉ�����B
        /// ����𒴂����ꍇ�͌Â����̂��폜����B
        /// </summary>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <param name="sessionName">�Z�b�V������</param>
        private void AddToSessionList(string sessionID, string sessionName)
        {
            try
            {
                sessionList.Add(new SessionInfo(sessionID, sessionName, DateTime.Now));

                // �ǉ����ď���𒴂�����Â����̂��폜
                if (sessionList.Count > PhotoChat.SessionListSize)
                {
                    sessionList.Sort();
                    sessionList.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �A�b�v���[�h�Ǘ�

        /// <summary>
        /// �A�b�v���[�h���Ă��Ȃ��Z�b�V�����̃��X�g���擾����B
        /// </summary>
        /// <returns>�Z�b�V�������X�g</returns>
        public List<SessionInfo> GetUploadSessions()
        {
            List<SessionInfo> uploadList = new List<SessionInfo>();
            try
            {
                using (StreamReader sr = new StreamReader(PhotoChat.UploadSessionFile))
                {
                    // �d�����������ă��X�g�쐬
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        SessionInfo sessionInfo = new SessionInfo(line);
                        bool found = false;
                        for (int i = 0; i < uploadList.Count; i++)
                        {
                            if (uploadList[i].ID == sessionInfo.ID)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            uploadList.Add(sessionInfo);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return uploadList;
        }


        /// <summary>
        /// �A�b�v���[�h�Ǘ��t�@�C������A�b�v���[�h�����Z�b�V�������폜����B
        /// </summary>
        /// <param name="removeList">�A�b�v���[�h���������Z�b�V�������X�g</param>
        public void RemoveUploadSessions(List<SessionInfo> removeList)
        {
            try
            {
                // �폜
                List<SessionInfo> uploadList = GetUploadSessions();
                foreach (SessionInfo info in removeList)
                {
                    for (int i = 0; i < uploadList.Count; i++)
                    {
                        if (uploadList[i].ID == info.ID)
                        {
                            uploadList.RemoveAt(i);
                            break;
                        }
                    }
                }

                // �t�@�C���X�V
                using (StreamWriter sw = new StreamWriter(PhotoChat.UploadSessionFile))
                {
                    foreach (SessionInfo info in uploadList)
                        sw.WriteLine(info.ToString());
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �A�b�v���[�h�Ǘ��t�@�C���ɃA�b�v���[�h���K�v�ȃZ�b�V������ǋL����B
        /// </summary>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <param name="sessionName">�Z�b�V������</param>
        private void AppendUploadSession(string sessionID, string sessionName)
        {
            try
            {
                DateTime date = new DateTime(long.Parse(sessionID.Substring(sessionName.Length)));
                SessionInfo sessionInfo = new SessionInfo(sessionID, sessionName, date);
                using (StreamWriter sw = new StreamWriter(PhotoChat.UploadSessionFile, true))
                {
                    sw.WriteLine(sessionInfo.ToString());
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �S�ẴZ�b�V�����̃��X�g���擾����B
        /// </summary>
        /// <returns>�S�ẴZ�b�V�������X�g</returns>
        public List<SessionInfo> GetAllSession()
        {
            List<SessionInfo> list = new List<SessionInfo>();
            try
            {
                foreach (string filePath in Directory.GetFiles(PhotoChat.SessionDirectory))
                {
                    if (filePath != PhotoChat.SessionManagerFile
                        && filePath != PhotoChat.UploadSessionFile
                        && filePath != PhotoChat.UploadAccountFile)
                    {
                        string id = Path.GetFileNameWithoutExtension(filePath);
                        string name = id.Substring(0, id.Length - 18);
                        DateTime date = new DateTime(long.Parse(id.Substring(id.Length - 18)));
                        list.Add(new SessionInfo(id, name, date));
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return list;
        }

        #endregion




        #region �Z�b�V��������

        /// <summary>
        /// �w�肵���Z�b�V�������J���ʐ^���X�g��Ԃ��B
        /// </summary>
        /// <param name="sessionName">�Z�b�V������</param>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <returns>�ʐ^���z��</returns>
        public string[] OpenSession(string sessionName, string sessionID)
        {
            try
            {
                // �J�����g�Z�b�V�����̐ݒ�
                currentSessionName = sessionName;
                currentSessionID = sessionID;

                currentSessionFile = Path.Combine(
                    PhotoChat.SessionDirectory, currentSessionID + ".dat");
                AppendUploadSession(sessionID, sessionName);

                // �Z�b�V�����t�@�C�����J���i�܂��͍쐬�j
                if (File.Exists(currentSessionFile))
                    return ReadSessionFile(currentSessionFile);
                else
                {
                    using (FileStream fs = File.Create(currentSessionFile)) { }
                    AddToSessionList(sessionID, sessionName);
                    SaveSessionManagerFile();
                    return null;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����̎ʐ^���X�g��Ԃ��B
        /// </summary>
        /// <returns>�ʐ^���z��B</returns>
        public string[] GetCurrentSessionPhotoArray()
        {
            try
            {
                if (File.Exists(currentSessionFile))
                    return ReadSessionFile(currentSessionFile);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }


        /// <summary>
        /// �Z�b�V�����t�@�C����ǂݎʐ^���z���Ԃ��B
        /// </summary>
        /// <param name="filePath">�Z�b�V�����t�@�C���̐�΃p�X</param>
        /// <returns>�ʐ^���z��</returns>
        private static string[] ReadSessionFile(string filePath)
        {
            try
            {
                string data = string.Empty;
                lock (sessionLock)
                {
                    // �Z�b�V�����t�@�C���̓ǂݍ���
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        data = reader.ReadToEnd();
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
        /// �Z�b�V����������Z�b�V����ID���쐬����B
        /// </summary>
        /// <param name="sessionName">�Z�b�V������</param>
        /// <returns>�쐬�����Z�b�V����ID</returns>
        public static string CreateNewSessionID(string sessionName)
        {
            return sessionName + DateTime.Now.Ticks.ToString();
        }


        /// <summary>
        /// ���݂̃Z�b�V�����t�@�C���ɒǋL����
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        public void AppendPhoto(string photoName)
        {
            try
            {
                lock (sessionLock)
                {
                    using (StreamWriter sw = new StreamWriter(currentSessionFile, true))
                    {
                        sw.WriteLine(photoName);
                        sw.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �Z�b�V�������N���X

        /// <summary>
        /// �Z�b�V��������ێ�����B
        /// </summary>
        public class SessionInfo : IComparable<SessionInfo>
        {
            #region �t�B�[���h�E�v���p�e�B

            private string id;
            private string name;
            private DateTime date;

            /// <summary>
            /// �Z�b�V����ID���擾����B
            /// </summary>
            public string ID
            {
                get { return id; }
            }

            /// <summary>
            /// �Z�b�V���������擾����B
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// �Z�b�V�����̍쐬���ꂽ�������擾����B
            /// </summary>
            public DateTime Date
            {
                get { return date; }
            }

            #endregion


            #region �R���X�g���N�^

            /// <summary>
            /// �Z�b�V���������i�[����B
            /// </summary>
            /// <param name="id">�Z�b�V����ID</param>
            /// <param name="name">�Z�b�V������</param>
            /// <param name="date">�Z�b�V�����̍쐬���ꂽ����</param>
            public SessionInfo(string id, string name, DateTime date)
            {
                this.id = id;
                this.name = name;
                this.date = date;
            }

            /// <summary>
            /// �Z�b�V������񕶎��񂩂�Z�b�V���������č\������B
            /// </summary>
            /// <param name="sessionString"></param>
            public SessionInfo(string sessionString)
            {
                string[] temp = sessionString.Split(
                    new Char[] { PhotoChat.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                this.id = temp[0];
                this.name = temp[1];
                this.date = new DateTime(long.Parse(temp[2]));
            }

            #endregion


            #region ���\�b�h

            /// <summary>
            /// ���t��������쐬���ĕԂ��B
            /// </summary>
            /// <returns>���t</returns>
            public string GetDateText()
            {
                try
                {
                    StringBuilder sb = new StringBuilder(30);
                    sb.Append(date.ToString("yyyy�NMM��dd��"));
                    switch (date.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            sb.Append("(��) ");
                            break;
                        case DayOfWeek.Tuesday:
                            sb.Append("(��) ");
                            break;
                        case DayOfWeek.Wednesday:
                            sb.Append("(��) ");
                            break;
                        case DayOfWeek.Thursday:
                            sb.Append("(��) ");
                            break;
                        case DayOfWeek.Friday:
                            sb.Append("(��) ");
                            break;
                        case DayOfWeek.Saturday:
                            sb.Append("(�y) ");
                            break;
                        case DayOfWeek.Sunday:
                            sb.Append("(��) ");
                            break;
                    }
                    sb.Append(date.ToString("HH:mm"));

                    return sb.ToString();
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return string.Empty;
                }
            }


            /// <summary>
            /// �Z�b�V�������𕶎���ɂ��ĕԂ��B
            /// </summary>
            /// <returns>�Z�b�V������񕶎���</returns>
            public override string ToString()
            {
                try
                {
                    StringBuilder sb = new StringBuilder(50);
                    sb.Append(id).Append(PhotoChat.Delimiter);
                    sb.Append(name).Append(PhotoChat.Delimiter);
                    sb.Append(date.Ticks.ToString());
                    return sb.ToString();
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return string.Empty;
                }
            }


            /// <summary>
            /// �Z�b�V���������r����B
            /// </summary>
            /// <param name="other">��r�Ώ�</param>
            /// <returns>��r����</returns>
            public int CompareTo(SessionInfo other)
            {
                return this.date.CompareTo(other.date);
            }

            #endregion
        }

        #endregion
    }
}
