//接続プロトコル

//1: JOINメッセージを送る(ブロードキャスト)
//   全端末に自分が参加したことを通知する
//   この段階では他端末のIPLISTには追加されない
//2: IPLISTメッセージを送り返す(ユニキャスト)
//   この段階で全端末の持っている(他セッションも含む)のIPとユーザIDを取得できる
//3: SENDBACKメッセージを送る(ブロードキャスト)
//   JOINメッセージから5秒後に取得したIPLISTを送り返す

//3～6秒のランダム時間ごとに
//IPLISTにIPのある端末に定期的に接続を試みる

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// 通信部の管理を行う。
    /// </summary>
    public class ConnectionManager
    {
        #region フィールド・プロパティ

        public PhotoChatClient client;
        private UdpConnection udpConnection;
        private static Dictionary<string, string> ipDictionary;
        private static Dictionary<string, TcpConnection> tcpConnectionDictionary;
        private Dictionary<string, NumberManager> numberDictionary;
        private Dictionary<string, DateTime> lastSelectTimeDictionary;
        private Thread receiveThread;
        private Queue<ISendable> receiveQueue;
        private EventWaitHandle receiveWaitHandle;
        private TcpListener tcpListener;
        private Thread connectionThread;
        private Thread tcpListeningThread;

        /// <summary>
        /// 通信部が起動中であるかどうか
        /// </summary>
        public bool IsAlive
        {
            get { return isAlive; }
        }
        private volatile bool isAlive;

        #endregion


        #region コンストラクタ

        /// <summary>
        /// 通信部を初期化する。
        /// </summary>
        /// <param name="client">PhotoChatClientインスタンス</param>
        public ConnectionManager(PhotoChatClient client)
        {
            this.client = client;

            udpConnection = new UdpConnection(
                new UdpConnection.ReceiveSignalDelegate(ReceiveSignal),
                new UdpConnection.SendIPListDeligate(SendIPList),
                new UdpConnection.ADDIPListDeligate(AddIPList));

            //#acceptCallback = new AsyncCallback(AcceptCallback);

            tcpListener = new TcpListener(IPAddress.Any, PhotoChat.TcpPort);


            receiveThread = new Thread(new ThreadStart(ProcessReceivedData));
            receiveThread.IsBackground = true;

            ipDictionary = new Dictionary<string, string>();
            addIPDictionary(PhotoChatClient.MyIPAddress, client.ID);

            tcpConnectionDictionary = new Dictionary<string, TcpConnection>();
            numberDictionary = new Dictionary<string, NumberManager>();
            lastSelectTimeDictionary = new Dictionary<string, DateTime>();


            receiveQueue = new Queue<ISendable>();
            receiveWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            isAlive = true;
            connectionThread = new Thread(new ThreadStart(connect));
            connectionThread.Start();
        }

        #endregion


        #region 起動・終了

        /// <summary>
        /// 通信部を起動する。
        /// </summary>
        public void Start()
        {
            try
            {
                //TCP受付開始
                tcpListeningThread = new Thread(new ThreadStart(listen));
                tcpListeningThread.Start();

                // 受信スレッド、UDP通信起動
                receiveThread.Start();
                udpConnection.Start(client);

                // 通信起動ログ
                PhotoChat.WriteLog("ConnectionManager Start", string.Empty, string.Empty);
                client.Form.LogWindow.AddLine("ConnectionManager Starts");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 通信部を終了する。
        /// </summary>
        public void Close()
        {
            try
            {
                if (!isAlive) return;
                isAlive = false;

                // UDP通信を停止
                udpConnection.Close();

                // 全ての通信を切断
                DisconnectAll();

                // 受信データ処理スレッド停止
                receiveWaitHandle.Set();

                // 通信停止ログ
                PhotoChat.WriteLog("ConnectionManager Closed", string.Empty, string.Empty);
                client.Form.LogWindow.AddLine("ConnectionManager Closed");
                connectionThread.Abort();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

    
        /// <summary>
        /// 全ての通信を切断する。
        /// </summary>
        public void DisconnectAll()
        {
            try
            {
                List<string> keys;
                lock (tcpConnectionDictionary)
                {
                    keys = new List<string>(tcpConnectionDictionary.Keys);
                }
                TcpConnection connection;
                for (int i = 0; i < keys.Count; i++)
                {
                    if (tcpConnectionDictionary.TryGetValue(keys[i], out connection))
                        connection.Close();
                }
                tcpConnectionDictionary.Clear();
                numberDictionary.Clear();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }
        
        #endregion


        #region 管理ユーティリティ

        /// <summary>
        /// IDに対応するTCPConnectionを得る。
        /// 新しいIDであればTCPConnectionを作成・登録する。
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>IDに対応するTCPConnection</returns>
        private TcpConnection GetConnection(string id)
        {
            TcpConnection connection = null;
            try
            {
                lock (tcpConnectionDictionary)
                {
                    tcpConnectionDictionary.TryGetValue(id, out connection);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }

            return connection;
        }

        /// <summary>
        /// IDに対応するNumberManagerを返す。
        /// 新しいIDであればNumberManagerを作成して返す。
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>IDに対応するNumberManager</returns>
        private NumberManager GetNumberManager(string id)
        {
            NumberManager numberManager = null;
            try
            {
                lock (numberDictionary)
                {
                    if (!numberDictionary.TryGetValue(id, out numberManager))
                    {
                        numberManager = new NumberManager(id);
                        numberDictionary[id] = numberManager;
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }

            return numberManager;
        }


        /// <summary>
        /// 選択コマンドが新着のものかどうかタイムスタンプを見て調べる。
        /// </summary>
        private bool IsNewSelectCommand(Command selectCommand)
        {
            try
            {
                // 最後に受信した選択コマンドのタイムスタンプを取得
                string id = selectCommand.AuthorID;
                DateTime lastSelectTime;
                if (lastSelectTimeDictionary.TryGetValue(id, out lastSelectTime)
                    && lastSelectTime >= selectCommand.TimeStamp)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// TCPDictionaryに追加する
        /// </summary>
        /// <param name="id"></param>
        /// <param name="connection"></param>
        private void addTcpConnectionDictionary(string id, TcpConnection connection)
        {
            if (!tcpConnectionDictionary.ContainsKey(id)) tcpConnectionDictionary.Add(id, connection);
        }

        /// <summary>
        /// IPDictionaryに追加する
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="id">UserID</param>
        private void addIPDictionary(string ip, string id)
        {
            if (!ipDictionary.ContainsKey(ip)) ipDictionary.Add(ip, id);
        }

        #endregion


        #region 送信

        /// <summary>
        /// 全ての接続先にデータを送信する。
        /// </summary>
        /// <param name="data">送信するデータ</param>
        public void SendAll(ISendable data)
        {
            try
            {
                lock (tcpConnectionDictionary)
                {
                    foreach (TcpConnection connection in tcpConnectionDictionary.Values)
                        connection.Send(data);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// データをIDが示す端末へ送信する。
        /// </summary>
        /// <param name="id">送信先の端末ID</param>
        /// <param name="data">送信するデータ</param>
        public void SendTo(string id, ISendable data)
        {
            try
            {
                if (id != client.ID)
                {
                    GetConnection(id).Send(data);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 受信

        /// <summary>
        /// 受信キューに受信データを追加する。
        /// </summary>
        /// <param name="receivedData">受信データ</param>
        internal void PutReceiveQueue(ISendable receivedData)
        {
            try
            {
                lock (receiveQueue)
                {
                    receiveQueue.Enqueue(receivedData);
                    receiveWaitHandle.Set();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 受信データ処理スレッド本体
        /// </summary>
        private void ProcessReceivedData()
        {
            ISendable receivedData;
            while (isAlive)
            {
                try
                {
                    // 受信キューが空のときは待機
                    if (receiveQueue.Count == 0)
                    {
                        receiveWaitHandle.Reset();
                        receiveWaitHandle.WaitOne();
                    }

                    // 受信キューからデータを取り出す
                    lock (receiveQueue)
                    {
                        receivedData = receiveQueue.Dequeue();
                    }

                    switch (receivedData.Type)
                    {
                        case PhotoChatImage.TypePhoto:
                            Receive((PhotoChatImage)receivedData);
                            break;
                            
                        case PhotoChatNote.TypeStroke:
                        case PhotoChatNote.TypeText:
                        case PhotoChatNote.TypeHyperlink:
                        case PhotoChatNote.TypeRemoval:
                        case PhotoChatNote.TypeTag:
                        case PhotoChatNote.TypeSound:
                            Receive((PhotoChatNote)receivedData);
                            break;
                            
                        case Command.TypeRequest:
                        case Command.TypeInform:
                        case Command.TypeTransfer:
                        case Command.TypeSelect:
                            Receive((Command)receivedData);
                            break;
                        case Command.TypeConnect:
                            Receive((Command)receivedData);
                            break;
                        case Command.TypeDisconnect:
                            Receive((Command)receivedData);
                            break;

                       case SharedFile.TypeSoundFile:
                            Receive((SharedFile)receivedData);
                            break;
                    }
                }
                catch (InvalidOperationException) { }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// 受信したPhotoChatImageを処理する。
        /// </summary>
        /// <param name="image">受信したPhotoChatImage</param>
        private void Receive(PhotoChatImage image)
        {
            try
            {
                // 受信データの通し番号をNumberManagerに通知
                bool isLatest =
                    GetNumberManager(image.ID).PutReceivedNumber(image.SerialNumber);

                // 最新のデータかどうかも含めてclientにデータを渡し、
                // 新着データであれば接続中の端末にこのデータが必要かコマンドを送る
                if (client.NewData(image, isLatest))
                    SendAll(Command.CreateInformCommand(image, client.ID));

                // 写真受信ログ
                PhotoChat.WriteLog("Receive Photo", image.PhotoName, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 受信したPhotoChatNoteを処理する。
        /// </summary>
        /// <param name="note">受信したPhotoChatNote</param>
        private void Receive(PhotoChatNote note)
        {
            try
            {
                // 受信データの通し番号をNumberManagerに通知
                bool isLatest =
                    GetNumberManager(note.ID).PutReceivedNumber(note.SerialNumber);

                // 最新のデータかどうかも含めてclientにデータを渡し、
                // 新着データであれば接続中の端末にこのデータが必要かコマンドを送る
                if (client.NewData(note, isLatest))
                    SendAll(Command.CreateInformCommand(note, client.ID));

                // 書き込み受信ログ
                PhotoChat.WriteLog("Receive Note", note.Author + note.SerialNumber, note.PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 受信したSharedFileを処理する。
        /// </summary>
        /// <param name="sharedFile">受信したSharedFile</param>
        private void Receive(SharedFile sharedFile)
        {
            try
            {
                // 受信データの通し番号をNumberManagerに通知
                bool isLatest =
                    GetNumberManager(sharedFile.ID).PutReceivedNumber(sharedFile.SerialNumber);

                // 最新のデータかどうかも含めてclientにデータを渡し、
                // 新着データであれば接続中の端末にこのデータが必要かコマンドを送る
                if (client.NewData(sharedFile, isLatest))
                    SendAll(Command.CreateInformCommand(sharedFile, client.ID));

                // 書き込み受信ログ
                PhotoChat.WriteLog("Receive File", sharedFile.FileName, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 受信したCommandを処理する。
        /// </summary>
        /// <param name="command">受信したCommand</param>
        private void Receive(Command command)
        {
            try
            {
                // 自分が発信したコマンドは無視
                if (command.SourceID == client.ID) return;

                // コマンドのタイプに応じて処理
                switch (command.Type)
                {
                    case Command.TypeRequest:
                        ProcessRequestCommand(command);
                        break;

                    case Command.TypeInform:
                        ProcessInformCommand(command);
                        break;

                    case Command.TypeTransfer:
                        ProcessTransferCommand(command);
                        break;

                    case Command.TypeSelect:
                        ProcessSelectCommand(command);
                        break;

                    case Command.TypeConnect:
                        ProcessConnectCommand(command);
                        break;

                    case Command.TypeDisconnect:
                        ProcessDisconnectCommand(command);
                        break;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 受信コマンド処理

        /// <summary>
        /// 写真選択コマンドを処理する。
        /// </summary>
        /// <param name="command">写真選択コマンド</param>
        private void ProcessSelectCommand(Command command)
        {
            try
            {
                // タイムスタンプが新しい選択コマンドの場合のみ処理
                if (IsNewSelectCommand(command))
                {
                    // clientに写真選択通知
                    client.Form.InformSelection(
                        command.AuthorID, command.UserName, command.PhotoName);

                    // タイムスタンプDictionaryの更新とコマンドの転送
                    lastSelectTimeDictionary[command.AuthorID] = command.TimeStamp;
                    SendAll(command);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 新着データ通知コマンドを処理する。
        /// </summary>
        /// <param name="command">新着データ通知コマンド</param>
        private void ProcessInformCommand(Command command)
        {
            try
            {
                // 自分のデータについての通知は無視
                if (command.AuthorID == client.ID) return;

                // 送信要求を出していないものがあれば送信要求コマンドを送り返す
                TcpConnection connection = GetConnection(command.SourceID);
                NumberManager numberManager = GetNumberManager(command.AuthorID);
                long limit = command.SerialNumber;
                long requestNumber;
                while ((requestNumber = numberManager.NextRequestNumber(limit)) > 0)
                {
                    connection.Send(Command.CreateRequestCommand(
                        requestNumber, command.AuthorID, client.ID));
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 保持データ通知コマンドを処理する。
        /// </summary>
        /// <param name="command">保持データ通知コマンド</param>
        private void ProcessTransferCommand(Command command)
        {
            try
            {
                // 自分のデータについての通知は無視
                if (command.AuthorID == client.ID) return;

                // 送信要求を出していないものがあれば送信要求コマンドを送り返す
                TcpConnection connection = GetConnection(command.SourceID);
                NumberManager numberManager = GetNumberManager(command.AuthorID);
                numberManager.SetFloorNumber(long.Parse(command.PhotoName));
                long limit = command.SerialNumber;
                long requestNumber;
                while ((requestNumber = numberManager.NextRequestNumber(limit)) > 0)
                {
                    connection.Send(Command.CreateRequestCommand(
                        requestNumber, command.AuthorID, client.ID));
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// データ送信要求コマンドを処理する。
        /// </summary>
        /// <param name="command">データ送信要求コマンド</param>
        private void ProcessRequestCommand(Command command)
        {
            try
            {
                // 要求されたデータを送り返す
                ISendable data = client.Request(command.AuthorID, command.SerialNumber);
                SendTo(command.SourceID, data);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// 接続通知コマンドを処理する。
        /// </summary>
        /// <param name="command">接続通知コマンド</param>
        private void ProcessConnectCommand(Command command)
        {
            try
            {
                // 接続通知
                client.AddUser(command.AuthorID, command.UserName);
                client.Form.NewConnection(command.AuthorID, command.UserName, true);
                // 接続ログ
                PhotoChat.WriteLog("Connect", command.AuthorID, command.UserName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// 切断通知コマンドを処理する。
        /// </summary>
        /// <param name="command">切断通知コマンド</param>
        private void ProcessDisconnectCommand(Command command)
        {
            try
            {
                // タイムスタンプが新しい選択コマンドの場合のみ処理
                if (IsNewSelectCommand(command))
                {
                    // 選択写真表示のリセットを通知
                    client.Form.InformSelection(command.AuthorID, command.UserName, null);

                    // タイムスタンプDictionaryの更新とコマンドの転送
                    lastSelectTimeDictionary[command.AuthorID] = command.TimeStamp;
                    SendAll(command);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 接続・切断（UDP信号処理）
        /// <summary>
        /// <param name="protcol">
        /// 0: JOIN(他のマシン起動)に対してIPLISTを返す
        /// 1: IPLISTに対してIPLISTを送り返す
        /// </param>
        /// </summary>
        /// <param name="endPoint"></param>
        private void SendIPList(int protcol, IPEndPoint endPoint)
        {
            if (protcol == 0)
            {
                string sendMessage = "IPLIST";
                foreach (KeyValuePair<string, string> kvp in ipDictionary)
                {
                    sendMessage += PhotoChat.Delimiter + kvp.Key + PhotoChat.Delimiter + kvp.Value;
                }
                udpConnection.Send(sendMessage, endPoint);
            }
            else
            {
                Thread.Sleep(10000);
                string sendMessage = "SENDBACK";
                foreach (KeyValuePair<string, string> kvp in ipDictionary)
                {
                    sendMessage += PhotoChat.Delimiter + kvp.Key + PhotoChat.Delimiter + kvp.Value;
                }
                udpConnection.Broadcast(sendMessage);
            }
        }

        /// <summary>
        /// IPListを更新する
        /// </summary>
        /// <param name="ipMessage">IPListに追加するIP</param>
        private void AddIPList(string ipMessage)
        {
            string[] ipList = ipMessage.Split(PhotoChat.Delimiter);

            string message = "GIVEYOURID";
            byte[] sendData = PhotoChat.DefaultEncoding.GetBytes(message);

            //ipList[i]: ID, ipList[i+1]: IP
            for (int i = 1; i < ipList.Length - 1; i += 2)
            {
                try
                {
                    if (ipDictionary.ContainsKey(ipList[i])) continue;
                    addIPDictionary(ipList[i], ipList[i + 1]);
                    if (tcpConnectionDictionary.ContainsKey(ipList[i + 1])) continue;
                    addTcpConnectionDictionary(ipList[i + 1], new TcpConnection(this, ipList[i + 1], ipList[i]));
                    using (System.Net.Sockets.UdpClient udpclient = new UdpClient())
                    {
                        //Console.WriteLine(ipList[i]);
                        udpclient.Send(sendData, sendData.Length, new IPEndPoint(IPAddress.Parse(ipList[i]), PhotoChat.UdpPort));
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }

        #endregion


        #region 接続・切断（TCP信号処理）

        /// <summary>
        /// 定期的に接続を開始する
        /// 3～6秒のランダム期間で接続する
        /// </summary>
        private void connect()
        {
            Random rand = new Random();

            while (isAlive)
            {
                try
                {
                    string opp = "接続対象:";
                    if (client.CurrentSessionID == string.Empty) continue;

                    foreach (KeyValuePair<string, string> kvp in ipDictionary)
                    {
                        if (kvp.Key == PhotoChatClient.MyIPAddress) continue;
                        opp += "\n" + kvp.Value;

                        TcpConnection connection;
                        if (tcpConnectionDictionary.TryGetValue(kvp.Value, out connection))
                        {
                            if (connection.Connect())
                            {
                                // 接続成功時の処理
                                ProcessSuccessfulConnection(connection, IPAddress.Parse(kvp.Key));
                            }
                        }
                        else
                        {
                            connection = new TcpConnection(this, kvp.Value, kvp.Key);
                            addTcpConnectionDictionary(kvp.Value, connection);

                            if (connection.Connect())
                            {
                                // 接続成功時の処理
                                ProcessSuccessfulConnection(connection, IPAddress.Parse(kvp.Key));
                            }
                        }
                    }
                    Thread.Sleep(rand.Next(3000, 6000));
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }

        /// <summary>
        /// 接続受付
        /// </summary>
        private void listen()
        {
            tcpListener.Start();

            while (isAlive)
            {
                try
                {
                    // 少々待機
                    Thread.Sleep(100);

                    // 接続待ちがあるか？
                    if (tcpListener.Pending() == true)
                    {
                        // ソケット、IPアドレスの取得
                        Socket socket = tcpListener.AcceptSocket();
                        IPAddress remoteIP = ((IPEndPoint)socket.RemoteEndPoint).Address;

                        byte[] packLen = new byte[sizeof(Int32)];
                        socket.Receive(packLen, packLen.Length, SocketFlags.None);
                        byte[] recvMessage = new byte[BitConverter.ToInt32(packLen, 0)];
                        socket.Receive(recvMessage, recvMessage.Length, SocketFlags.None);
                        string info = PhotoChat.DefaultEncoding.GetString(recvMessage);
                        //ID\tCurrentSessionID
                        string[] id = info.Split(PhotoChat.Delimiter);
                        byte[] sendMessage;

                        addIPDictionary(remoteIP.ToString(), id[0]);

                        //違うセッションであれば切断する
                        if (id[1] != PhotoChatClient.Instance.CurrentSessionID)
                        {
                            sendMessage = BitConverter.GetBytes(0);
                            socket.Send(sendMessage, sendMessage.Length, SocketFlags.None);
                            socket.Close();
                        }

                        // 接続受け入れ
                        TcpConnection connection;
                        if (tcpConnectionDictionary.TryGetValue(id[0], out connection))
                        {
                            if (connection.AcceptConnection(socket))
                            {
                                // 接続成功時の処理
                                ProcessSuccessfulConnection(connection, remoteIP);
                                //接続成功を通知
                                
                                connection.getTimeDifference();
                                continue;
                            }
                        }
                        else
                        {
                            connection = new TcpConnection(this, id[0], remoteIP.ToString());

                            if (connection.AcceptConnection(socket))
                            {
                                addTcpConnectionDictionary(id[0], connection);
                                // 接続成功時の処理
                                ProcessSuccessfulConnection(connection, remoteIP);
                                //接続成功を通知

                                connection.getTimeDifference();
                                continue;
                            }
                        }

                        sendMessage = BitConverter.GetBytes(0);
                        socket.Send(sendMessage, sendMessage.Length, SocketFlags.None);
                        socket.Close();
                    }

                }
                catch (Exception e)
                {
                    if (isAlive)
                    {
                        PhotoChat.WriteErrorLog(e.ToString());
                    }
                }
            }

            tcpListener.Stop();
        }


        /// <summary>
        /// 受信したUDP信号を処理する。
        /// </summary>
        /// <param name="message">受信メッセージ</param>
        /// <param name="remoteIP">送信元のIPアドレス</param>
        public void ReceiveSignal(string message, IPAddress remoteIP)
        {
            try
            {
                string[] temp = message.Split(PhotoChat.Delimiter);
                // セッション追加処理
                setSession(temp[1], temp[2]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ブロードキャスト信号を受信したときの処理
        /// </summary>
        /// <param name="sessionID">セッションID</param>
        /// <param name="sessionName">セッション名</param>
        private void setSession(string sessionID, string sessionName)
        {
            try
            {
                // セッションIDが同じときは接続処理
                if (sessionID != client.CurrentSessionID)
                {
                    // セッション選択ダイアログがあれば選択ボタンを追加
                    client.AddNearbySessionButton(sessionName, sessionID);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// 接続成功時の処理や接続コマンドの送信を行う。
        /// </summary>
        /// <param name="connection">接続先とのTCPコネクション</param>
        /// <param name="remoteIP">接続先のIPアドレス</param>
        private void ProcessSuccessfulConnection(TcpConnection connection, IPAddress remoteIP)
        {
            try
            {
                // 接続通知コマンド送信
                connection.Send(Command.CreateConnectCommand(client.ID, client.UserName));

                // 自分のデータの保持データ通知コマンド送信
                connection.Send(Command.CreateTransferCommand(
                    client.FloorSerialNumber, client.CeilingSerialNumber, client.ID, client.ID));

                // IDごとの保持データ通知コマンド送信
                List<string> keyList;
                NumberManager numberManager;
                lock (numberDictionary)
                {
                    keyList = new List<string>(numberDictionary.Keys);
                }
                foreach (string authorID in keyList)
                {
                    if (numberDictionary.TryGetValue(authorID, out numberManager))
                    {
                        connection.Send(Command.CreateTransferCommand(numberManager.FloorNumber,
                            numberManager.CeilingNumber, authorID, client.ID));
                    }
                }

                // 接続ログ
                client.Form.LogWindow.AddLine("Connect:" + remoteIP.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 切断時の処理を行う。
        /// </summary>
        /// <param name="id">切断した相手の端末ID</param>
        internal void ProcessDisconnection(string id)
        {
            try
            {
                // 切断コマンド送信
                SendAll(Command.CreateDisconnectCommand(id, client.ID));

                // GUI部に切断を通知
                client.Form.NewConnection(id, null, false);
                // コネクション削除
                tcpConnectionDictionary[id].IsConnecting = false;

                lock (numberDictionary)
                {
                    numberDictionary.Remove(id);
                }

                // 切断ログ
                PhotoChat.WriteLog("Disconnect", id, string.Empty);
                client.Form.LogWindow.AddLine("Disconnect:" + id);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 端末時間差取得

        /// <summary>
        /// 端末時間差の設定と音声部へのユーザ登録をする。
        /// </summary>
        /// <param name="remoteID">接続先の端末ID</param>
        /// <param name="timeDifference">端末時間差</param>
        public void SetTimeDifference(string remoteID, long timeDifference)
        {
            try
            {
                if (remoteID == client.ID) return;

                // 端末時間差の設定
                TcpConnection connection = null;

                while (connection == null)
                {
                    Thread.Sleep(3000);
                    connection = GetConnection(remoteID);
                }

                if (!connection.IsConnecting) return;

                connection.TimeDifference = timeDifference;
                // ログ
                PhotoChat.WriteLog("SoundControl AddUser", remoteID, timeDifference.ToString());
                client.Form.LogWindow.AddLine(
                    "SoundControlAddUser:" + remoteID + "   " + timeDifference.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region NumberManaer

        /// <summary>
        /// 通し番号により個別の端末データの受信履歴を管理する。
        /// </summary>
        public class NumberManager
        {
            private long floorNumber = -1;
            private long ceilingNumber = -1;
            private LinkedList<long> unreceivedNumbers;
            private long requestedNumber = -1;

            /// <summary>
            /// 現在のセッションにおける最小の番号を取得する。
            /// </summary>
            public long FloorNumber
            {
                get { return floorNumber; }
            }

            /// <summary>
            /// 最新の受信済み番号を取得する。
            /// </summary>
            public long CeilingNumber
            {
                get { return ceilingNumber; }
            }

            /// <summary>
            /// 未受信番号リストを取得する。
            /// </summary>
            public LinkedList<long> UnreceivedNumbers
            {
                get { return unreceivedNumbers; }
            }


            /// <summary>
            /// IDに対応する端末データの受信履歴管理インスタンスを作成する。
            /// </summary>
            /// <param name="id">端末ID</param>
            public NumberManager(string id)
            {
                unreceivedNumbers = new LinkedList<long>();
                PhotoChatClient.Instance.CheckSerialNumber(
                    id, ref floorNumber, ref ceilingNumber, unreceivedNumbers);
            }


            /// <summary>
            /// serialNumberを受信済み番号として処理する。
            /// serialNumberが最新の通し番号であればtrueを返し、
            /// 飛ばされた番号があれば未受信番号リストに追加する。
            /// </summary>
            /// <param name="serialNumber">受信したデータの通し番号</param>
            /// <returns>最新の通し番号であればtrue、そうでなければfalseを返す。</returns>
            public bool PutReceivedNumber(long serialNumber)
            {
                try
                {
                    if (ceilingNumber < 0)
                    {
                        // 最初に受信したデータの場合
                        ceilingNumber = serialNumber;
                        return true;
                    }

                    lock (unreceivedNumbers)
                    {
                        // 最新の通し番号の場合
                        if (ceilingNumber < serialNumber)
                        {
                            // 飛ばされた番号があれば未受信リストに追加
                            while (++ceilingNumber != serialNumber)
                                unreceivedNumbers.AddLast(serialNumber);
                            return true;
                        }
                        // 通し番号が最新のものでない場合
                        else
                        {
                            // 未受信番号リストから削除する
                            unreceivedNumbers.Remove(serialNumber);
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return false;
                }
            }


            /// <summary>
            /// 次に送信要求を出すデータの通し番号を返す。
            /// このメソッドを利用するときは必ず送信要求コマンドを出すこと。
            /// </summary>
            /// <param name="limit">要求を出す通し番号の上限</param>
            /// <returns>次に要求するデータの通し番号。無い場合は-1を返す。</returns>
            public long NextRequestNumber(long limit)
            {
                try
                {
                    lock (unreceivedNumbers)
                    {
                        // すでにlimitまで要求済みであれば-1を返す
                        if (requestedNumber >= limit)
                            return -1;

                        // 未受信番号リストに要求を出すべきものがないか調べる
                        if (requestedNumber < ceilingNumber)
                        {
                            foreach (long number in unreceivedNumbers)
                            {
                                if (requestedNumber < number)
                                {
                                    requestedNumber = number;
                                    return number;
                                }
                            }
                            requestedNumber = ceilingNumber;
                        }

                        // limitまでは順に送信要求を出す
                        if (requestedNumber < limit)
                            return ++requestedNumber;
                        else
                            return -1;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return -1;
                }
            }


            /// <summary>
            /// この番号が最小番号より小さければ最小番号を更新する。
            /// 初期設定のときは最大番号も設定する。
            /// </summary>
            /// <param name="floorNumber">通し番号</param>
            public void SetFloorNumber(long floorNumber)
            {
                try
                {
                    if (floorNumber < 0) return;

                    if (this.floorNumber < 0)
                    {
                        this.floorNumber = floorNumber;
                        this.ceilingNumber = floorNumber - 1;
                    }
                    else if (floorNumber < this.floorNumber)
                    {
                        while (floorNumber < this.floorNumber)
                        {
                            this.floorNumber--;
                            unreceivedNumbers.AddFirst(this.floorNumber);
                        }
                        this.floorNumber = floorNumber;
                        this.requestedNumber = floorNumber - 1;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }

        #endregion
    }
}
