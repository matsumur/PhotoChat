using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// TCP通信を行う
    /// </summary>
    public class TcpConnection
    {
        #region フィールド・プロパティ

        public delegate void SendDelegate(int type, byte[] data);
        private ConnectionManager manager;
        private Socket socket;
        private NetworkStream networkStream = null;
        private BinaryReader reader = null;
        private BinaryWriter writer = null;
        private string id;
        private volatile bool isConnecting;
        private long timeDifference = 0;
        private int waitingCount = 0;
        private WaitCallback sendCallback;
        private Thread receiveThread;
        private string ip;

        /// <summary>
        /// 接続先のID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// 接続中かどうか
        /// </summary>
        public bool IsConnecting
        {
            set { isConnecting = value; }
            get { return isConnecting; }
        }

        /// <summary>
        /// 接続先との端末時間差
        /// </summary>
        public long TimeDifference
        {
            get { return timeDifference; }
            set { timeDifference = value; }
        }

        /// <summary>
        /// 接続待機中のアクセス回数
        /// </summary>
        public int WaitingCount
        {
            get { return waitingCount; }
            set { waitingCount = value; }
        }

        /// <summary>
        /// 接続先のIPアドレス
        /// </summary>
        public IPAddress IP
        {
            get
            {
                return IPAddress.Parse(ip);
            }
        }

        #endregion


        #region コンストラクタ

        /// <summary>
        /// 新しいTCP接続の初期化。
        /// </summary>
        /// <param name="manager">接続部を管理するConnectionManager</param>
        /// <param name="id">接続先のID</param>
        public TcpConnection(ConnectionManager manager, string id, string ip)
        {
            this.manager = manager;
            this.id = id;
            this.ip = ip;
            isConnecting = false;
            sendCallback = new WaitCallback(SendWorker);
        }

        #endregion


        #region 接続・切断

        /// <summary>
        /// TCP接続を開始する。
        /// すでに接続中であればfalseを返して何もしない。
        /// </summary>
        /// <param name="ipAddress">接続先のIPAddress</param>
        /// <returns>接続に成功したときのみtrue</returns>
        public bool Connect()
        {

            // 既に接続中であればfalse
            if (isConnecting)
            {
                return false;
            }

            isConnecting = true;

            try
            {
                // 接続処理
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), PhotoChat.TcpPort);

                this.socket = new Socket(
                    endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Connect(endPoint);

                byte[] sendMessage = PhotoChat.DefaultEncoding.GetBytes(
                    PhotoChatClient.Instance.ID + PhotoChat.Delimiter
                    + PhotoChatClient.Instance.CurrentSessionID);
                byte[] sendLen = BitConverter.GetBytes(sendMessage.Length);
                byte[] recvMessage = BitConverter.GetBytes(1);

                socket.Send(sendLen, sendLen.Length, SocketFlags.None);
                socket.Send(sendMessage, sendMessage.Length, SocketFlags.None);
                socket.Receive(recvMessage, recvMessage.Length, SocketFlags.None);

                int flag = BitConverter.ToInt32(recvMessage, 0);

                // 接続失敗
                if (flag == 0)
                {
                    isConnecting = false;
                    socket.Close();
                    return false;
                }

                // 接続成功
                StartConnection();
                return true;

            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }

            // 接続失敗
            isConnecting = false;
            return false;
        }


        /// <summary>
        /// TCP接続を受け入れる。
        /// すでに接続中であればfalseを返して何もしない。
        /// </summary>
        /// <param name="socket">受け入れる接続</param>
        /// <returns>接続を受け入れたときのみtrue</returns>
        public bool AcceptConnection(Socket socket)
        {
            // 既に接続中であればfalse
            if (isConnecting)
            {
                return false;
            }

            isConnecting = true;

            try
            {
                this.socket = socket;
                if (this.socket.Connected)
                {
                    //接続完了フラグ送信
                    byte[] sendMessage = BitConverter.GetBytes(1);
                    socket.Send(sendMessage, sendMessage.Length, SocketFlags.None);
                    // 接続受け入れ
                    StartConnection();
                    return true;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }

            // 受け入れ失敗
            isConnecting = false;
            return false;
        }


        /// <summary>
        /// 送受信を開始する。
        /// </summary>
        private void StartConnection()
        {
            try
            {
                // 受信スレッド起動
                receiveThread = new Thread(new ThreadStart(Receive));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 切断する。
        /// </summary>
        public void Close()
        {
            try
            {
                if (!isConnecting) return;
                isConnecting = false;

                timeDifference = 0;

                // ソケットを閉じる
                networkStream.Close();
                networkStream = null;
                reader.Close();
                reader = null;
                writer.Close();
                writer = null;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                // 切断通知
                manager.ProcessDisconnection(id);
                receiveThread.Abort();

            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            finally
            {
                isConnecting = false;
            }
        }

        #endregion


        #region 送信

        /// <summary>
        /// 接続中であればデータを非同期送信する。
        /// </summary>
        /// <param name="data">送信するデータ</param>
        public void Send(ISendable data)
        {
            try
            {
                if (data != null && isConnecting)
                    ThreadPool.QueueUserWorkItem(sendCallback, data);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// コールバックされたらデータを送信する。
        /// </summary>
        /// <param name="state">送信するデータが格納されている</param>
        private void SendWorker(object state)
        {
            if (!isConnecting) return;
            if (ip == PhotoChatClient.MyIPAddress) return;

            try
            {
                if (writer == null)
                {
                    NetworkStream networkStream = new NetworkStream(socket);
                    writer = new BinaryWriter(networkStream);
                }
                lock (writer)
                {
                    ISendable sendData = (ISendable)state;
                    byte[] dataBytes = sendData.GetDataBytes();

                    // データタイプの送信
                    writer.Write(sendData.Type);

                    // バイト列の送信
                    writer.Write(dataBytes.Length);
                    writer.Write(dataBytes);
                    writer.Flush();
                }
            }
            catch (SocketException)
            {
                Close();
            }
            catch (IOException)
            {
                Close();
            }
            catch (Exception e)
            {
                if (isConnecting)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }

        #endregion


        #region 受信

        /// <summary>
        /// データ受信スレッド本体
        /// </summary>
        private void Receive()
        {
            if (networkStream == null)
            {
                networkStream = new NetworkStream(socket);
            }
            if (reader == null)
            {
                reader = new BinaryReader(networkStream);
            }

            int type, length;
            byte[] data;
            ISendable instance;

            length = 0;

            while (isConnecting)
            {
                try
                {
                    // タイプの読み取り
                    type = reader.ReadInt32();

                    // バイト列の読み取り
                    length = reader.ReadInt32();
                    data = reader.ReadBytes(length);

                    // 受信データをインスタンス化してConnectionManagerの受信キューへ入れる
                    instance = CreateInstance(type, data);
                    if (instance != null)
                        manager.PutReceiveQueue(instance);

                }
                catch (SocketException)
                {
                    Close();
                }
                catch (IOException)
                {
                    Close();
                }
                catch (Exception e)
                {
                    if (isConnecting)
                    {
                        PhotoChat.WriteErrorLog("outof" + length.ToString() + "\n" + e.ToString());
                    }
                }
            }
        }


        /// <summary>
        /// 受信データをインスタンス化する。
        /// 端末時間差からデータ時刻の修正も行う。
        ///  </summary>
        /// <param name="type">データのタイプ</param>
        /// <param name="data">データのバイト列</param>
        /// <returns>作成したインスタンス。作成できなかったときはnull</returns>
        private ISendable CreateInstance(int type, byte[] data)
        {
            try
            {
                // データタイプに応じてインスタンス化（データ時刻修正）
                switch (type)
                {
                    case PhotoChatImage.TypePhoto:
                        PhotoChatImage image = PhotoChatImage.CreateInstance(type, data);
                        if (image != null)
                            image.Date = image.Date.AddTicks(-timeDifference);
                        return image;

                    case PhotoChatNote.TypeHyperlink:
                    case PhotoChatNote.TypeRemoval:
                    case PhotoChatNote.TypeStroke:
                    case PhotoChatNote.TypeTag:
                    case PhotoChatNote.TypeText:
                    case PhotoChatNote.TypeSound:
                        PhotoChatNote note = PhotoChatNote.CreateInstance(type, data);
                        if (note != null)
                            note.Date = note.Date.AddTicks(-timeDifference);
                        return note;

                    case Command.TypeRequest:
                    case Command.TypeInform:
                    case Command.TypeTransfer:
                    case Command.TypeSelect:
                    case Command.TypeConnect:
                    case Command.TypeDisconnect:
                        return new Command(type, data);

                    case SharedFile.TypeSoundFile:
                        return new SharedFile(type, data);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }

        /// <summary>
        /// 時間差を取得する
        /// </summary>
        public void getTimeDifference()
        {
            Thread th = new Thread(new ThreadStart(startGetTimeDiff));
            th.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void startGetTimeDiff()
        {
            using (UdpClient udp = new UdpClient())
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), PhotoChat.UdpPort);
                IPEndPoint remoteEP = null;
                string sendMessage;
                string recvMessage;
                byte[] sendByte;
                byte[] recvByte;
                long[] diff = new long[3];

                for (int i = 0; i < 3; i++)
                {
                    //現在時刻のTicksを送る
                    sendMessage = PhotoChat.TimeHeader + PhotoChat.Delimiter + DateTime.Now.Ticks.ToString();
                    sendByte = PhotoChat.DefaultEncoding.GetBytes(sendMessage);
                    udp.Send(sendByte, sendByte.Length, endPoint);

                    recvByte = udp.Receive(ref remoteEP);
                    long localTime = DateTime.Now.Ticks;
                    recvMessage = PhotoChat.DefaultEncoding.GetString(recvByte);
                    string[] args = recvMessage.Split(PhotoChat.Delimiter);
                    long diff1 = long.Parse(args[1]);
                    long remoteTime = long.Parse(args[2]);
                    long diff2 = remoteTime - localTime;
                    diff[i] = (diff1 + diff2) / 2;
                }

                Array.Sort(diff);
                manager.SetTimeDifference(this.id, diff[1]);
                string sendBackMessage = 
                    PhotoChat.TimeDiffHeader + PhotoChat.Delimiter
                    + PhotoChatClient.Instance.ID + PhotoChat.Delimiter + diff[1].ToString();
                byte[] notifyTimeDiffMessage = PhotoChat.DefaultEncoding.GetBytes(sendBackMessage);
                udp.Send(notifyTimeDiffMessage, notifyTimeDiffMessage.Length, endPoint);
                udp.Close();
            }
        }

        /// <summary>
        /// 端末間時間差の取得のため時刻を送信する。
        /// UDPなので成功するとは限らない。
        /// </summary>
        /// <param name="remoteIP">送信先のIPアドレス</param>
        private void CheckTimeDifference(object remoteIP)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint((IPAddress)remoteIP, PhotoChat.UdpPort);

            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
