using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// セッション管理を行う。
    /// </summary>
    public class SessionManager
    {
        #region フィールド・プロパティ

        private static object sessionLock = new object();
        private string currentSessionName = string.Empty;
        private string currentSessionFile = string.Empty;
        private string currentSessionID = string.Empty;
        private List<SessionInfo> sessionList;

        /// <summary>
        /// 現在のセッション名を取得する。
        /// </summary>
        public string CurrentSessionName
        {
            get { return currentSessionName; }
        }

        /// <summary>
        /// 現在のセッションIDを取得する。
        /// </summary>
        public string CurrentSessionID
        {
            get { return currentSessionID; }
        }

        /// <summary>
        /// セッションリストを取得する。
        /// </summary>
        public List<SessionInfo> SessionList
        {
            get { return sessionList; }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// セッション管理を初期化する。
        /// </summary>
        public SessionManager()
        {
            // セッション管理ファイルを読み込む
            sessionList = new List<SessionInfo>();
            LoadSessionManagerFile();
        }

        #endregion




        #region 最近のセッション情報

        /// <summary>
        /// セッション管理ファイルを読み込む。
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
        /// セッション管理ファイルを更新する。
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
        /// 最近のセッション情報リストに要素を追加する。
        /// 上限を超えた場合は古いものを削除する。
        /// </summary>
        /// <param name="sessionID">セッションID</param>
        /// <param name="sessionName">セッション名</param>
        private void AddToSessionList(string sessionID, string sessionName)
        {
            try
            {
                sessionList.Add(new SessionInfo(sessionID, sessionName, DateTime.Now));

                // 追加して上限を超えたら古いものを削除
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




        #region アップロード管理

        /// <summary>
        /// アップロードしていないセッションのリストを取得する。
        /// </summary>
        /// <returns>セッションリスト</returns>
        public List<SessionInfo> GetUploadSessions()
        {
            List<SessionInfo> uploadList = new List<SessionInfo>();
            try
            {
                using (StreamReader sr = new StreamReader(PhotoChat.UploadSessionFile))
                {
                    // 重複を除去してリスト作成
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
        /// アップロード管理ファイルからアップロードしたセッションを削除する。
        /// </summary>
        /// <param name="removeList">アップロード完了したセッションリスト</param>
        public void RemoveUploadSessions(List<SessionInfo> removeList)
        {
            try
            {
                // 削除
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

                // ファイル更新
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
        /// アップロード管理ファイルにアップロードが必要なセッションを追記する。
        /// </summary>
        /// <param name="sessionID">セッションID</param>
        /// <param name="sessionName">セッション名</param>
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
        /// 全てのセッションのリストを取得する。
        /// </summary>
        /// <returns>全てのセッションリスト</returns>
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




        #region セッション処理

        /// <summary>
        /// 指定したセッションを開き写真リストを返す。
        /// </summary>
        /// <param name="sessionName">セッション名</param>
        /// <param name="sessionID">セッションID</param>
        /// <returns>写真名配列</returns>
        public string[] OpenSession(string sessionName, string sessionID)
        {
            try
            {
                // カレントセッションの設定
                currentSessionName = sessionName;
                currentSessionID = sessionID;

                currentSessionFile = Path.Combine(
                    PhotoChat.SessionDirectory, currentSessionID + ".dat");
                AppendUploadSession(sessionID, sessionName);

                // セッションファイルを開く（または作成）
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
        /// 現在のセッションの写真リストを返す。
        /// </summary>
        /// <returns>写真名配列。</returns>
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
        /// セッションファイルを読み写真名配列を返す。
        /// </summary>
        /// <param name="filePath">セッションファイルの絶対パス</param>
        /// <returns>写真名配列</returns>
        private static string[] ReadSessionFile(string filePath)
        {
            try
            {
                string data = string.Empty;
                lock (sessionLock)
                {
                    // セッションファイルの読み込み
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        data = reader.ReadToEnd();
                    }
                }

                // 文字列配列に変換して返す
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
        /// セッション名からセッションIDを作成する。
        /// </summary>
        /// <param name="sessionName">セッション名</param>
        /// <returns>作成したセッションID</returns>
        public static string CreateNewSessionID(string sessionName)
        {
            return sessionName + DateTime.Now.Ticks.ToString();
        }


        /// <summary>
        /// 現在のセッションファイルに追記する
        /// </summary>
        /// <param name="photoName">写真名</param>
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




        #region セッション情報クラス

        /// <summary>
        /// セッション情報を保持する。
        /// </summary>
        public class SessionInfo : IComparable<SessionInfo>
        {
            #region フィールド・プロパティ

            private string id;
            private string name;
            private DateTime date;

            /// <summary>
            /// セッションIDを取得する。
            /// </summary>
            public string ID
            {
                get { return id; }
            }

            /// <summary>
            /// セッション名を取得する。
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// セッションの作成された時刻を取得する。
            /// </summary>
            public DateTime Date
            {
                get { return date; }
            }

            #endregion


            #region コンストラクタ

            /// <summary>
            /// セッション情報を格納する。
            /// </summary>
            /// <param name="id">セッションID</param>
            /// <param name="name">セッション名</param>
            /// <param name="date">セッションの作成された時刻</param>
            public SessionInfo(string id, string name, DateTime date)
            {
                this.id = id;
                this.name = name;
                this.date = date;
            }

            /// <summary>
            /// セッション情報文字列からセッション情報を再構成する。
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


            #region メソッド

            /// <summary>
            /// 日付文字列を作成して返す。
            /// </summary>
            /// <returns>日付</returns>
            public string GetDateText()
            {
                try
                {
                    StringBuilder sb = new StringBuilder(30);
                    sb.Append(date.ToString("yyyy年MM月dd日"));
                    switch (date.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            sb.Append("(月) ");
                            break;
                        case DayOfWeek.Tuesday:
                            sb.Append("(火) ");
                            break;
                        case DayOfWeek.Wednesday:
                            sb.Append("(水) ");
                            break;
                        case DayOfWeek.Thursday:
                            sb.Append("(木) ");
                            break;
                        case DayOfWeek.Friday:
                            sb.Append("(金) ");
                            break;
                        case DayOfWeek.Saturday:
                            sb.Append("(土) ");
                            break;
                        case DayOfWeek.Sunday:
                            sb.Append("(日) ");
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
            /// セッション情報を文字列にして返す。
            /// </summary>
            /// <returns>セッション情報文字列</returns>
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
            /// セッション情報を比較する。
            /// </summary>
            /// <param name="other">比較対象</param>
            /// <returns>比較結果</returns>
            public int CompareTo(SessionInfo other)
            {
                return this.date.CompareTo(other.date);
            }

            #endregion
        }

        #endregion
    }
}
