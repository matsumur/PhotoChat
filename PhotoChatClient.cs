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
    /// データの管理と橋渡しをするクラス
    /// </summary>
    public class PhotoChatClient
    {
        #region フィールド・プロパティ

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
        /// GUIフォームを取得する。
        /// </summary>
        public PhotoChatForm Form
        {
            get { return form; }
        }

        /// <summary>
        /// 通信管理部を取得する。
        /// </summary>
        public ConnectionManager ConnectionManager
        {
            get { return connectionManager; }
        }

        /// <summary>
        /// ストローク認識クラスを取得する。
        /// </summary>
        public StrokeRecognizer StrokeRecognizer
        {
            get { return strokeRecognizer; }
        }

        /// <summary>
        /// 端末IDを取得する。
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// ユーザ名を取得する。
        /// </summary>
        public string UserName
        {
            get { return userName; }
        }

        /// <summary>
        /// 現在のセッションにおける最小の通し番号を取得する。
        /// </summary>
        public long FloorSerialNumber
        {
            get { return floorSerialNumber; }
        }

        /// <summary>
        /// この端末における最新のデータ通し番号を取得する。
        /// </summary>
        public long CeilingSerialNumber
        {
            get { return ceilingSerialNumber; }
        }

        /// <summary>
        /// 現在のセッション名を取得する。
        /// </summary>
        public string CurrentSessionName
        {
            get { return sessionManager.CurrentSessionName; }
        }

        /// <summary>
        /// 現在のセッションのIDを取得する。
        /// </summary>
        public string CurrentSessionID
        {
            get { return sessionManager.CurrentSessionID; }
        }

        /// <summary>
        /// 最近入力したタグのリストを取得する。
        /// </summary>
        public LinkedList<string> InputTagList
        {
            get { return inputTagList; }
        }

        /// <summary>
        /// 現在のセッションで使用されているタグのリストを取得する。
        /// </summary>
        public List<string> SessionTagList
        {
            get { return sessionTagList; }
        }

        /// <summary>
        /// Instanceプロパティ。
        /// Clientのインスタンスは１つだけに制限し、ここから取得。
        /// setは定義しない（禁止）。
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


        #region コンストラクタ・初期化・終了処理

        /// <summary>
        /// 外部からクラスを作成する唯一のメソッド。
        /// privateなコンストラクタを呼び出し、ただ1つのインスタンスを作成。
        /// </summary>
        /// <returns>ただ一つのPhotoChatClientインスタンス</returns>
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
        /// privateなコンストラクタ。
        /// アプリケーションの初期化を行う。
        /// Clientのインスタンスはただ１つに制限する。
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

            // スプラッシュウィンドウの表示
            SplashWindow splashWindow = new SplashWindow();
            splashWindow.Show();

            // GUI初期化
            form = new PhotoChatForm(this);

            // セッション管理初期化
            sessionManager = new SessionManager();

            // データ読み込み
            LoadConfigFile();
            SetCeilingSerialNumber();
            LoadTagHistory();
            strokeRecognizer = new StrokeRecognizer();

            // 通信部初期化
            connectionManager = new ConnectionManager(this);

            // スプラッシュウィンドウを閉じる
            splashWindow.Close();

            // ユーザ名の入力
            form.Show();
            InputUserName();
        }

        private void SendAll(ISendable data)
        {
            connectionManager.SendAll((ISendable)data);
        }

        /// <summary>
        /// ユーザ名の入力をユーザに求める。
        /// </summary>
        private void InputUserName()
        {
            // ユーザ名入力ダイアログの表示
            UserNameInputDialog inputDialog = new UserNameInputDialog();
            inputDialog.AddNameList(userNameList.ToArray());
            if (userNameList.Count == 0)
                inputDialog.UserName = Environment.UserName;
            else
                inputDialog.UserName = userNameList[0];
            if (inputDialog.ShowDialog(form) == DialogResult.OK)
                userName = inputDialog.UserName;
            else
                throw new Exception("起動中に終了を選択しました。");
            inputDialog.Dispose();

            // IDの初期設定:ユーザ名+時刻
            if (id == string.Empty)
                id = userName + DateTime.Now.Ticks.ToString();

            // 入力されたユーザ名を名前リストの先頭に移動（追加）
            userNameList.Remove(userName);
            userNameList.Insert(0, userName);

            // 名前リストが上限を超えていたら古いものを消去
            if (userNameList.Count > PhotoChat.MaxUserNameListSize)
                userNameList.RemoveAt(userNameList.Count - 1);

            // 設定ファイルの更新
            UpdateConfigFile();
        }


        /// <summary>
        /// 各部を起動する。
        /// </summary>
        public void Start()
        {
            // 起動
            connectionManager.Start();

            // セッションダイアログ
            SelectSession(true);

            PhotoChat.WriteLog("Start", "端末ID:" + id, "ユーザ名:" + userName);
        }


        /// <summary>
        /// PhotoChatを終了する（GUI部から呼び出される）
        /// </summary>
        public void Close()
        {
            try
            {
                // 通信部終了
                if (connectionManager != null)
                    connectionManager.Close();

                // ユーザIndex状態ファイル保存
                SaveIndexStatusFile(true);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 設定ファイル処理・拡張子確認

        /// <summary>
        /// 設定ファイルを更新する
        /// </summary>
        internal void UpdateConfigFile()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(PhotoChat.ConfigFile, false))
                {
                    // IDの書き込み
                    sw.WriteLine("ID=" + id);

                    // ユーザ名入力履歴の書き込み
                    StringBuilder sb = new StringBuilder("UserNames=", 50);
                    foreach (string name in userNameList)
                    {
                        sb.Append(name).Append(PhotoChat.Delimiter);
                    }
                    sw.WriteLine(sb.ToString());

                    // GUI設定の書き込み
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
        /// 設定ファイルを読み込み設定値を取得する
        /// </summary>
        private void LoadConfigFile()
        {
            try
            {
                // 設定ファイルを読み込む
                if (File.Exists(PhotoChat.ConfigFile))
                {
                    // ファイルの読み込み
                    Dictionary<string, string> configDictionary
                        = PhotoChat.GetDictionaryFromFile(PhotoChat.ConfigFile);

                    // IDの取得
                    configDictionary.TryGetValue("ID", out id);

                    // ユーザ名入力履歴の取得
                    string userNames;
                    if (configDictionary.TryGetValue("UserNames", out userNames))
                    {
                        string[] temp = userNames.Split(new Char[] { PhotoChat.Delimiter },
                                                        StringSplitOptions.RemoveEmptyEntries);
                        userNameList.AddRange(temp);
                    }

                    // GUI設定の取得
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
        /// ファイル名の拡張子がサポートする画像形式か確認する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>サポートする画像形式であればtrue</returns>
        public bool CheckImageFileExtension(string fileName)
        {
            try
            {
                // 拡張子を切り出す
                int index = fileName.LastIndexOf('.');
                string extension = fileName.Substring(index + 1).ToLower();

                // サポートするものか確認
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




        #region シリアル番号処理

        /// <summary>
        /// 新しい通し番号を返す。
        /// </summary>
        /// <returns>新しい通し番号</returns>
        public long GetNewSerialNumber()
        {
            return Interlocked.Increment(ref ceilingSerialNumber);
        }


        /// <summary>
        /// 最新の通し番号を設定する。
        /// </summary>
        private void SetCeilingSerialNumber()
        {
            if (id == null) return;

            try
            {
                // ユーザIndexディレクトリがあるか調べる
                string directoryPath = Path.Combine(PhotoChat.UserIndexDirectory, id);
                if (Directory.Exists(directoryPath))
                {
                    // ユーザIndex状態ファイルを読む
                    string lastSession = string.Empty;
                    if (!ReadIndexStatusFile(directoryPath, ref lastSession))
                    {
                        // 前回正常終了していない場合は最新通し番号を調べる
                        if (lastSession != string.Empty)
                        {
                            // 前回セッションのユーザIndexファイルを調べる
                            string filePath = Path.Combine(directoryPath, lastSession + ".dat");
                            if (File.Exists(filePath))
                            {
                                using (StreamReader sr = new StreamReader(filePath))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        // より大きな番号があれば更新
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
        /// ユーザIndexの状態ファイルを更新する。
        /// </summary>
        /// <param name="closing">正常終了保存時であればtrue、読み込み直後であればfalse</param>
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
                    // 正常終了フラグと最新通し番号の書き込み
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
        /// ユーザIndex状態ファイルを読む。
        /// </summary>
        /// <param name="directoryPath">ディレクトリパス</param>
        /// <param name="lastSession">最後に開いたセッション</param>
        /// <returns>前回正常終了していればtrue</returns>
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
        /// 現在のセッションにおける最小の通し番号を設定する。
        /// </summary>
        private void SetFloorSerialNumber()
        {
            if (id == null) return;

            try
            {
                // 少なくとも現在の最新番号以下
                floorSerialNumber = ceilingSerialNumber;

                string directory = Path.Combine(PhotoChat.UserIndexDirectory, id);
                string filePath = Path.Combine(directory, CurrentSessionID + ".dat");
                if (File.Exists(filePath))
                {
                    // 現在のセッションを過去に開いたことがある場合
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            // より小さな番号があれば更新
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
        /// 端末IDに対応するユーザIndexを調べ未受信データと最小・最大番号を設定する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="floorNumber">最小番号</param>
        /// <param name="ceilingNumber">最大番号</param>
        /// <param name="unreceivedNumbers">未受信番号リスト</param>
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
                    // 現在のセッションのデータを調べる
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            int index = line.IndexOf(PhotoChat.Delimiter);
                            long number = long.Parse(line.Substring(0, index));

                            // 最小番号
                            if (floorNumber < 0)
                                floorNumber = number;
                            else if (floorNumber > number)
                            {
                                while (--floorNumber > number)
                                    unreceivedNumbers.AddFirst(floorNumber);
                                floorNumber = number;
                            }

                            // 最大番号
                            if (ceilingNumber < 0)
                                ceilingNumber = number;
                            else if (ceilingNumber < number)
                            {
                                while (++ceilingNumber < number)
                                    unreceivedNumbers.AddLast(ceilingNumber);
                                ceilingNumber = number;
                            }

                            // 未受信番号
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




        #region セッションダイアログ

        /// <summary>
        /// セッション選択ダイアログを表示して選択されたセッションを開く。
        /// </summary>
        /// <param name="firstTime">起動時かどうか</param>
        public void SelectSession(bool firstTime)
        {
            try
            {
                sessionDialog = new SessionDialog();

                // 起動時はキャンセルボタンを無効化
                if (firstTime)
                {
                    sessionDialog.CancelButtonEnabled = false;
                }
                // 過去のセッション選択ボタンを追加
                AddPastSessionButton();

                // ダイアログ表示
                if (sessionDialog.ShowDialog(form) == DialogResult.OK)
                {
                    // 接続を切る
                    connectionManager.DisconnectAll();

                    // サムネイル一覧をクリア
                    if (!firstTime)
                    {
                        form.ClearThumbnailList();
                        form.ClearReviewPanel();
                    }

                    // 選択されたセッションを開く
                    string[] photoArray = sessionManager.OpenSession(
                        sessionDialog.SessionName, sessionDialog.SessionID);
                    SaveIndexStatusFile(false);
                    SetFloorSerialNumber();
                    LoadSessionTag();

                    // サムネイル一覧へサムネイルを一括挿入
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
        /// 過去のセッション選択ボタンを追加する。
        /// </summary>
        private void AddPastSessionButton()
        {
            try
            {
                // 過去のセッションを最近のものから順に並べ替え
                List<SessionManager.SessionInfo> sessionList = sessionManager.SessionList;
                sessionList.Sort();
                sessionList.Reverse();

                // ボタン追加
                foreach (SessionManager.SessionInfo info in sessionList)
                {
                    if (info.ID == CurrentSessionID)
                        sessionDialog.AddPastButton(info.Name, info.ID, "現在のセッション");
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
        /// セッション選択ダイアログに近くのセッション選択ボタンを追加する。
        /// </summary>
        /// <param name="sessionName">セッション名</param>
        /// <param name="sessionID">セッションID</param>
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




        #region 新規データ処理

        /// <summary>
        /// 新しいPhotoChatImageを登録・保存する。
        /// </summary>
        /// <param name="image">新しいPhotoChatImage</param>
        /// <param name="isLatest">最新のものであることが保障される場合にtrueとする。</param>
        /// <returns>登録に成功したらtrue、新しいデータでない場合はfalseを返す。</returns>
        public bool NewData(PhotoChatImage image, bool isLatest)
        {
            try
            {
                Photo photo = image as Photo;
                if (photo != null)
                {
                    // ユーザIndexファイルへの書き込み（重複を省く）
                    if (!AppendIndexData(photo.ID, photo.SerialNumber, photo.GetIndexString(), isLatest))
                        return false;

                    // Thumbnail画像の作成・登録
                    sessionManager.AppendPhoto(photo.PhotoName);
                    Thumbnail.SaveImage(photo.PhotoName, photo.Image);
                    form.AddThumbnail(new Thumbnail(photo));

                    // 新着自動選択
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
        /// 新しいPhotoChatNoteを登録・保存する。
        /// </summary>
        /// <param name="note">新しいPhotoChatNote</param>
        /// <param name="isLatest">最新のものであることが保障される場合にtrueとする。</param>
        /// <returns>登録に成功したらtrue、新しいデータでない場合はfalseを返す。</returns>
        public bool NewData(PhotoChatNote note, bool isLatest)
        {
            try
            {
                // ユーザIndexファイルへの書き込み（重複を省く）
                if (!AppendIndexData(note.ID, note.SerialNumber, note.GetIndexString(), isLatest))
                    return false;
                note.Save();

                // 表示中のPhotoであれば書き込みを追加
                Photo photo = form.GetCurrentPhoto(note.PhotoName);
                if (photo != null)
                {
                    photo.AddNote(note);
                    form.UpdateReviewPanel(note);
                }

                // タグ書き込みであればタグファイルに追記
                AppendTag(note);

                // サムネイルの書き込みカウントを増減
                form.CountThumbnailNote(note);

                // 新着自動選択
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
        /// 新しいSharedFileを登録・保存する。
        /// </summary>
        /// <param name="sharedFile">新しいSharedFile</param>
        /// <param name="isLatest">最新のものであることが保障される場合にtrueとする。</param>
        /// <returns>登録に成功したらtrue、新しいデータでない場合はfalseを返す。</returns>
        public bool NewData(SharedFile sharedFile, bool isLatest)
        {
            try
            {
                // ユーザIndexファイルへの書き込み（重複を省く）
                if (!AppendIndexData(sharedFile.ID, sharedFile.SerialNumber, sharedFile.GetIndexString(), isLatest))
                    return false;

                // 保存
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
        /// 新着データのあった写真の自動選択。
        /// ただし前回の変更から2秒以内は変更しない。
        /// </summary>
        /// <param name="photoName">写真名</param>
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




        #region インデックスファイル追記

        /// <summary>
        /// ユーザIndexファイルに追記する
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="serialNumber">追記データの通し番号</param>
        /// <param name="dataString">追記する文字列</param>
        /// <param name="isLatest">最新の書き込みであることが保障されている場合はtrue</param>
        /// <returns>追記に成功した場合はtrue、すでにデータがあった場合はfalseを返す。</returns>
        private bool AppendIndexData(string id, long serialNumber, string dataString, bool isLatest)
        {
            try
            {
                lock (writeIndexLock)
                {
                    string filePath = GetIndexFilePath(id, CurrentSessionID);

                    // 重複しないか確認
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

                    // ファイルに追記
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
        /// 端末IDとセッションIDに対応するユーザIndexファイルパスを返す。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="sessionID">セッションID</param>
        /// <returns>対応するユーザIndexファイル名</returns>
        private string GetIndexFilePath(string id, string sessionID)
        {
            try
            {
                // 端末IDに対応するディレクトリパスの取得（なければ作成）
                string directoryPath = Path.Combine(PhotoChat.UserIndexDirectory, id);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // Indexファイルパスを返す
                return Path.Combine(directoryPath, sessionID + ".dat");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }

        #endregion




        #region データ要求

        /// <summary>
        /// 端末IDと通し番号に対応するデータを返す。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="serialNumber">通し番号</param>
        /// <returns>要求されたデータ。なければnullを返す。</returns>
        public ISendable Request(string id, long serialNumber)
        {
            try
            {
                // ユーザIndexファイルからデータを探す
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

                    // インスタンスの作成
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




        #region タグ処理

        /// <summary>
        /// noteがタグであればタグファイルおよびタグリストに追加する。
        /// </summary>
        /// <param name="note">新しいPhotoChatNote</param>
        private void AppendTag(PhotoChatNote note)
        {
            // タグ書き込みのみ処理
            if (note.Type != PhotoChatNote.TypeTag) return;

            try
            {
                Tag tag = (Tag)note;

                // タグファイルに追記
                string filePath = GetTagFilePath(tag.TagString);
                lock (tagFileLock)
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.WriteLine(tag.PhotoName);
                        sw.Flush();
                    }
                }

                // セッションタグリストに追加
                AddSessionTag(tag.TagString);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 重複しないようセッションタグリストに追加しファイルに追記する。
        /// </summary>
        /// <param name="tag">追加するタグ</param>
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
        /// 現在のセッションで使用されたタグリストをファイルからロードする。
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
        /// 重複しないよう最近入力したタグリストに追加しファイルを更新する。
        /// </summary>
        /// <param name="tag">追加するタグ</param>
        public void UpdateInputTags(string tag)
        {
            if (!inputTagList.Contains(tag))
            {
                // リストの先頭に追加し上限から溢れたものを削除
                inputTagList.AddFirst(tag);
                if (inputTagList.Count > PhotoChat.InputTagListSize)
                    inputTagList.RemoveLast();
            }
            else
            {
                if (tag == inputTagList.First.Value)
                    return;

                // 入力されたタグを先頭に移動
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
        /// 最近入力したタグのリストをファイルからロードする。
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
        /// タグファイルのパスを取得する。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
        /// <returns>タグファイルのパス</returns>
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
        /// 指定したタグの付いた写真名の配列を返す。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
        /// <returns>タグの付いた写真名の配列</returns>
        public static string[] SearchTaggedPhoto(string tag)
        {
            try
            {
                string data = string.Empty;
                lock (tagFileLock)
                {
                    // タグファイルの読み込み
                    string filePath = GetTagFilePath(tag);
                    if (File.Exists(filePath))
                    {
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            data = reader.ReadToEnd();
                        }
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
        /// 指定したタグの付いた写真名を返す。
        /// </summary>
        /// <param name="tagArray">タグ配列</param>
        /// <param name="andSearch">AND検索ならtrue、OR検索ならfalse</param>
        /// <returns>タグの付いた写真名の配列</returns>
        public static string[] SearchTaggedPhoto(string[] tagArray, bool andSearch)
        {
            try
            {
                // 各タグの付いた写真名の配列を取得
                string[][] photoArrays = new string[tagArray.Length][];
                for (int i = 0; i < tagArray.Length; i++)
                    photoArrays[i] = SearchTaggedPhoto(tagArray[i]);

                // 写真名配列をAND/OR結合して返す
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


        #region 近接関係

        /// <summary>
        /// 近くにいるユーザ名の配列を返す。
        /// </summary>
        /// <returns>近くにいるユーザ名配列</returns>
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
        /// ユーザ名変換辞書にユーザを追加する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="userName">ユーザ名</param>
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


        #region アップロード・セッション画像一括保存（HTML化）

        /// <summary>
        /// PhotoChatサーバにアップロードする。
        /// </summary>
        public void UploadToServer()
        {
            try
            {
                ServerUploadDialog uploadDialog = new ServerUploadDialog();

                // アップロードするセッションの取得
                uploadDialog.AddSessionPanels(sessionManager.GetUploadSessions());
                // 全てを再アップロードする場合はこちら
                //uploadDialog.AddSessionPanels(sessionManager.GetAllSession());

                if (uploadDialog.ShowDialog(form) == DialogResult.OK)
                {
                    // アップロード管理ファイルの更新
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
        /// 現在のセッションの写真と書き込みをオリジナルサイズで一括保存する。
        /// HTMLファイルも作成する。
        /// </summary>
        public void SaveSessionImages()
        {
            try
            {
                // 保存先ディレクトリ入力
                string saveDirectory = InputSaveDirectory();
                if (saveDirectory == null) return;

                // 現在のセッションファイルを読み取る
                string[] photoNameArray = sessionManager.GetCurrentSessionPhotoArray();
                if (photoNameArray == null) return;

                // 進行状況表示ウィンドウ
                ProgressWindow progressWindow = new ProgressWindow(
                    photoNameArray.Length, "写真をフォルダに保存しています");
                progressWindow.Show(form);

                // セッションの写真を１枚ずつ作成
                int count = 0;
                string directoryPath = string.Empty;
                foreach (string photoName in photoNameArray)
                {
                    // 画像を保存するディレクトリを一定枚数ごとに変更
                    if ((count % PhotoChat.SaveFolderSize) == 0)
                    {
                        // 保存先ディレクトリを変更
                        string name = "Photo" + (count + 1) + "_" + (count + PhotoChat.SaveFolderSize);
                        directoryPath = Path.Combine(saveDirectory, name);
                        Directory.CreateDirectory(directoryPath);
                    }

                    // 画像を作成して保存
                    Bitmap image = GetOriginalSizePhoto(photoName);
                    if (image != null)
                    {
                        string filePath = Path.Combine(directoryPath, photoName + ".jpg");
                        image.Save(filePath, ImageFormat.Jpeg);
                        image.Dispose();
                    }
                    progressWindow.SetCount(++count);
                }

                // HTMLファイル作成
                CreateHtmlFiles(photoNameArray, saveDirectory);
                progressWindow.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 現在のセッションの写真と書き込みをオリジナルサイズでFlickrにアップロードする。
        /// </summary>
        public void UploadSessionImagesToFlickr()
        {
            try
            {
                FlickrUploadDialog uploadDialog = new FlickrUploadDialog();
                if (uploadDialog.ShowDialog(form) == DialogResult.OK)
                {
                    // アップロード設定読み取り
                    FlickrNet.Flickr flickr = uploadDialog.Flickr;
                    string description = uploadDialog.Description;
                    bool isPublic = uploadDialog.IsPublic;
                    bool isFamily = uploadDialog.IsFamily;
                    bool isFriend = uploadDialog.IsFamily;

                    // 現在のセッションファイルを読み取る
                    string[] photoNameArray = sessionManager.GetCurrentSessionPhotoArray();
                    if (photoNameArray != null)
                    {
                        // 進行状況表示ウィンドウ
                        ProgressWindow progressWindow = new ProgressWindow(
                            photoNameArray.Length, "写真をFlickrにアップロードしています");
                        progressWindow.Show(form);

                        // セッションの写真を１枚ずつ作成してアップロード
                        int count = 0;
                        string title, tags;
                        string tempFile = "temp_FlickrUpload.jpg";
                        foreach (string photoName in photoNameArray)
                        {
                            Bitmap image = GetOriginalSizePhoto(photoName);
                            if (image != null)
                            {
                                // 一時ファイルに画像を保存
                                image.Save(tempFile, ImageFormat.Jpeg);
                                image.Dispose();

                                // タイトル作成
                                count++;
                                title = CurrentSessionName + count.ToString();

                                // タグ取得
                                Thumbnail thumbnail = form.GetThumbnail(photoName);
                                if (thumbnail != null)
                                    tags = thumbnail.Tags;
                                else
                                    tags = Thumbnail.GetTags(photoName);

                                // アップロード
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
                MessageBox.Show("Flickrにアクセスできませんでした");
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 現在のセッションのデータをSmartCalendarにエクスポートする。
        /// </summary>
        public void ExportToSmartCalendar()
        {
            try
            {
                // 保存先ディレクトリ入力
                string saveDirectory = string.Empty;
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "エクスポート先のフォルダを指定してください。";
                if (folderDialog.ShowDialog(form) == DialogResult.OK)
                    saveDirectory = folderDialog.SelectedPath;
                folderDialog.Dispose();
                if (saveDirectory == string.Empty) return;

                // 現在のセッションファイルを読み取る
                string[] photoNameArray = sessionManager.GetCurrentSessionPhotoArray();
                if (photoNameArray != null)
                {
                    // 進行状況ウィンドウ
                    ProgressWindow progressWindow = new ProgressWindow(
                        photoNameArray.Length, "SmartCalendarにエクスポートしています");
                    progressWindow.Show(form);

                    // ディレクトリ作成
                    DateTime date = form.GetThumbnail(photoNameArray[0]).Date;
                    string year = date.Year.ToString("0000");
                    string month = date.Month.ToString("00");
                    string day = date.Day.ToString("00");
                    string directoryPath = Path.Combine(saveDirectory, year);
                    directoryPath = Path.Combine(directoryPath, year + month);
                    directoryPath = Path.Combine(directoryPath, year + "_" + month + day);
                    Directory.CreateDirectory(directoryPath);

                    // セッションの写真を1枚ずつ作成してエクスポート
                    int count = 0;
                    string tags = string.Empty;
                    foreach (string photoName in photoNameArray)
                    {
                        Bitmap image = GetOriginalSizePhoto(photoName);
                        if (image != null)
                        {
                            // 撮影時刻
                            Thumbnail thumbnail = form.GetThumbnail(photoName);
                            PropertyItem propItem = Properties.Resources.forExif.PropertyItems[0];
                            propItem.Id = 0x9003;
                            propItem.Type = 2;
                            propItem.Value = Encoding.ASCII.GetBytes(
                                thumbnail.Date.ToString("yyyy:MM:dd HH:mm:ss"));
                            propItem.Len = propItem.Value.Length;
                            image.SetPropertyItem(propItem);

                            // タグ情報
                            if (thumbnail != null)
                                tags = thumbnail.Tags;
                            else
                                tags = Thumbnail.GetTags(photoName);
                            propItem.Id = 0x9C9C;
                            propItem.Type = 7;
                            propItem.Value = Encoding.GetEncoding("UNICODE").GetBytes(tags + "\0");
                            propItem.Len = propItem.Value.Length;
                            image.SetPropertyItem(propItem);

                            // 保存
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
        /// 写真と書き込みをオリジナルサイズで描画した画像ファイルを作成する。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>作成した画像</returns>
        public static Bitmap GetOriginalSizePhoto(string photoName)
        {
            try
            {
                // オリジナルサイズPhotoの作成
                Photo photo = new Photo(photoName, true);

                // 拡大倍率算出
                float scale = 1;
                if (photo.Image.Width > PhotoChat.PhotoWidth
                    || photo.Image.Height > PhotoChat.PhotoHeight)
                {
                    float widthScale = (float)photo.Image.Width / (float)PhotoChat.PhotoWidth;
                    float heightScale = (float)photo.Image.Height / (float)PhotoChat.PhotoHeight;
                    scale = Math.Max(widthScale, heightScale);
                }

                // ビットマップ作成
                Bitmap image = new Bitmap(
                    (int)(PhotoChat.ReviewWidth * scale), (int)(PhotoChat.ReviewHeight * scale));
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.Clear(Color.White);

                    // 写真の描画
                    g.DrawImage(
                        photo.Image, PhotoChat.LeftMargin * scale, PhotoChat.TopMargin * scale);

                    // 書き込みの描画
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
        /// ユーザに保存先ディレクトリを入力させる。
        /// </summary>
        /// <returns>保存先ディレクトリのパス</returns>
        private string InputSaveDirectory()
        {
            string directory = null;

            // 保存先ディレクトリ入力ダイアログ
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "保存先のフォルダを指定してください。"
                + Environment.NewLine + "新たにフォルダを作成して保存します。";
            if (folderDialog.ShowDialog(form) == DialogResult.OK)
            {
                // 入力されたディレクトリにセッション名と同じ名前のディレクトリがあれば警告
                string temp = folderDialog.SelectedPath;
                directory = Path.Combine(temp, sessionManager.CurrentSessionName);
                if (Directory.Exists(directory))
                {
                    string text = "保存先フォルダ内にセッション名と同じ名前のフォルダがあります。"
                        + Environment.NewLine
                        + "そのフォルダに同名ファイルがあれば上書きしてしまいますがいいですか？";
                    string caption = "確認：ファイル上書きの許可";
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
        /// 各フォルダに写真一覧HTMLファイルを作成する。
        /// </summary>
        /// <param name="photoNameArray">写真名配列</param>
        /// <param name="saveDirectory">保存先ディレクトリ</param>
        private void CreateHtmlFiles(string[] photoNameArray, string saveDirectory)
        {
            if (photoNameArray.Length == 0) return;

            // 写真を保存したフォルダに一覧HTMLファイル作成
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

                    // ヘッダ
                    sb.Append("<html lang=\"ja\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=Shift_JIS\">\n<title>");
                    sb.Append(Path.GetFileName(saveDirectory));
                    sb.Append(count / PhotoChat.SaveFolderSize + 1);
                    sb.Append("</title>\n</head>\n<body bgcolor=\"#F0F8FF\">\n");

                    // ボディ
                    do
                    {
                        sb.Append(CreatePhotoHtml(photoNameArray[count]));
                        if (++count == photoNameArray.Length) break;
                    } while ((count % PhotoChat.SaveFolderSize) != 0);

                    // フッタ
                    sb.Append("</body>\n</html>\n");

                    // ファイル保存
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

            // インデックスファイル作成
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html lang=\"ja\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=Shift_JIS\">\n<title>");
                sb.Append(Path.GetFileName(saveDirectory)).Append("　目次");
                sb.Append("</title>\n</head>\n<body bgcolor=\"#F0F8FF\">\n<h2>");
                sb.Append(Path.GetFileName(saveDirectory)).Append("　目次");
                sb.Append("</h2>\n<p>\n<ul>\n");
                count = 0;
                foreach (string fileName in fileList)
                {
                    sb.Append("<li><a href=\"").Append(fileName).Append("\">写真");
                    sb.Append(count + 1).Append("から");
                    sb.Append(count + PhotoChat.SaveFolderSize).Append("</a>\n");
                    count += PhotoChat.SaveFolderSize;
                }
                sb.Append("</ul>\n</p>\n</body>\n</html>\n");

                // ファイル保存
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
        /// 写真についてのHTMLを作成する。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>作成したHTML</returns>
        private string CreatePhotoHtml(string photoName)
        {
            StringBuilder sb = new StringBuilder(100);
            try
            {
                // 写真のサムネイル取得
                Thumbnail thumbnail = form.GetThumbnail(photoName);
                if (thumbnail == null)
                    thumbnail = new Thumbnail(photoName);

                sb.Append("<div>").Append(thumbnail.Date.ToString("yyyy/MM/dd HH:mm:ss"));
                sb.Append("　　撮影者：").Append(thumbnail.Author).Append("</div>\n");
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
