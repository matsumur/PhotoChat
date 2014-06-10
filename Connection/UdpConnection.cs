using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

namespace PhotoChat
{
    /// <summary>
    /// UDP通信を行う。
    /// </summary>
    public class UdpConnection
    {
        #region フィールド・プロパティ

        private PhotoChatClient client;
        public delegate void ReceiveSignalDelegate(string message, IPAddress remoteIP);
        public delegate void SendIPListDeligate(int protcol, IPEndPoint endPoint);
        public delegate void ADDIPListDeligate(string IPs);
        private volatile bool isAlive;

        private Thread requireSessionThread;

        // 送信関連
        private UdpClient sendClient;
        private ADDIPListDeligate addIPListDeligate;
        private SendIPListDeligate sendIPListDeligate;
        private AsyncCallback completeSendIP;
        private AsyncCallback completeAddIP;

        // 受信関連
        private UdpClient receiveClient;
        private ReceiveSignalDelegate receiveSignal;
        private AsyncCallback receiveCallback;
        private AsyncCallback completeCallback;
        private IPEndPoint remoteEP = null;


        /// <summary>
        /// 通信が起動中であるかどうか
        /// </summary>
        public bool IsAlive
        {
            get { return isAlive; }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// UDP通信部を初期化する。
        /// </summary>
        /// <param name="receiveSignal">受信信号処理デリゲート</param>
        public UdpConnection(ReceiveSignalDelegate receiveSignal, SendIPListDeligate sendIPListDeligate, ADDIPListDeligate addIPListDeligate)
        {
            this.receiveSignal = receiveSignal;
            this.sendIPListDeligate = sendIPListDeligate;
            this.addIPListDeligate = addIPListDeligate;
            isAlive = true;

            // 送信初期化
            sendClient = new UdpClient();
            sendClient.Ttl = 3;
            completeSendIP = new AsyncCallback(SendIPComplete);

            // 受信初期化
            receiveClient = new UdpClient(new IPEndPoint(IPAddress.Any, PhotoChat.UdpPort));
            receiveCallback = new AsyncCallback(ReceiveCallback);
            completeCallback = new AsyncCallback(ReceiveComplete);
            completeAddIP = new AsyncCallback(CompleteAddIP);
        }

        #endregion


        #region 起動・停止
        /// <summary>
        /// 通信を開始する。
        /// </summary>
        /// <param name="client">PhotoChatClientインスタンス</param>
        public void Start(PhotoChatClient client)
        {
            try
            {
                this.client = client;

                //ネットワーク上のホスト探索のためにメッセージを送る
                Broadcast("JOIN" + PhotoChat.Delimiter + client.ID);

                // 受信開始
                receiveClient.BeginReceive(receiveCallback, null);

                requireSessionThread = new Thread(new ThreadStart(requireSession));
                requireSessionThread.Start();

            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 通信を終了し、リソースを開放する。
        /// </summary>
        public void Close()
        {
            if (!isAlive) return;
            isAlive = false;

            try
            {
                sendClient.Close();
                receiveClient.Close();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 送信

        /// <summary>
        /// 端末IDのブロードキャストを行う。
        /// </summary>
        public void Broadcast(string sendMessage)
        {
            try
            {
                byte[] broadcastMessage = PhotoChat.DefaultEncoding.GetBytes(sendMessage);
                sendClient.Send(broadcastMessage, broadcastMessage.Length, "255.255.255.255", PhotoChat.UdpPort);
            }
            catch (SocketException) { }
            catch (Exception e)
            {
                if (isAlive)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }

        public void Send(string sendMessage, IPEndPoint remoteEP)
        {
            byte[] message = PhotoChat.DefaultEncoding.GetBytes(sendMessage);
            sendClient.Send(message, message.Length, remoteEP);
        }

        private void SendIPComplete(IAsyncResult asyncResult)
        {
            sendIPListDeligate.EndInvoke(asyncResult);
        }
        #endregion


        #region 受信

        /// <summary>
        /// UDP信号を受信したときにコールバックされる。
        /// </summary>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                // 受信データを読み取る
                byte[] message = receiveClient.EndReceive(asyncResult, ref remoteEP);
                string str = PhotoChat.DefaultEncoding.GetString(message);

                if (str.StartsWith(PhotoChat.TimeHeader))
                {
                    // 端末時間差取得処理
                    receiveTimeSignal(str, remoteEP, DateTime.Now.Ticks);
                }
                else if (str.StartsWith(PhotoChat.TimeDiffHeader)){
                    setTimeDiff(str);
                }
                else
                {
                    remoteEP = new IPEndPoint(remoteEP.Address, PhotoChat.UdpPort);

                    if (str.StartsWith("JOIN"))
                    {
                        sendIPListDeligate.BeginInvoke(0, remoteEP, completeSendIP, null);
                        sendIPListDeligate.BeginInvoke(1, remoteEP, completeSendIP, null);
                    }
                    else if (str.StartsWith("IPLIST"))
                    {
                        addIPListDeligate.BeginInvoke(str, completeAddIP, null);
                    }
                    else if (str.StartsWith("SENDBACK"))
                    {
                        addIPListDeligate.BeginInvoke(str, completeAddIP, null);
                    }
                    else if (str.StartsWith("REQUIRESESSION"))
                    {
                        Send("SESSION" + PhotoChat.Delimiter + client.CurrentSessionID + PhotoChat.Delimiter + client.CurrentSessionName, remoteEP);
                    }
                    else if (str.StartsWith("SESSION"))
                    {
                        // 受信データの非同期処理
                        receiveSignal.BeginInvoke(str, remoteEP.Address, completeCallback, null);
                    }
                    else if (str.StartsWith("GIVEYOURID"))
                    {
                        string sendMessage = "MYIPID" + PhotoChat.Delimiter + PhotoChatClient.MyIPAddress + PhotoChat.Delimiter + PhotoChatClient.Instance.ID;
                        Send(sendMessage, remoteEP);
                    }
                }
            }
            catch (Exception e)
            {
                if (isAlive)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }

            try
            {
                // 続けて非同期受信
                if (isAlive)
                    receiveClient.BeginReceive(receiveCallback, null);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 受信データ処理が完了したときにコールバックされる。
        /// </summary>
        private void ReceiveComplete(IAsyncResult asyncResult)
        {
            receiveSignal.EndInvoke(asyncResult);
        }

        /// <summary>
        /// IP追加が終了したときにコールバックされる
        /// </summary>
        /// <param name="asyncResult"></param>
        private void CompleteAddIP(IAsyncResult asyncResult)
        {
            addIPListDeligate.EndInvoke(asyncResult);
        }

        #endregion


        #region 端末時間差取得


        /// <summary>
        /// 受信した端末時間差取得のための信号を処理する。
        /// </summary>
        /// <param name="message">受信メッセージ</param>
        /// <param name="remoteIP">送信元のIPEndPoint</param>
        /// <param name="ticks">受信時の時刻</param>
        private void receiveTimeSignal(string message, IPEndPoint remoteEP, long ticks)
        {
            try
            {
                string[] values = message.Split(new Char[] { PhotoChat.Delimiter }, StringSplitOptions.RemoveEmptyEntries);

                // 往路時間差を計算して応答信号を送信
                long timeDifference = ticks - long.Parse(values[1]);
                string sendMessage =
                    PhotoChat.TimeHeader + PhotoChat.Delimiter + timeDifference.ToString()
                    + PhotoChat.Delimiter + DateTime.Now.Ticks.ToString();
                //これの受信部分はTcpConnectionのgetTimeDiff
                Send(sendMessage, remoteEP);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        private void setTimeDiff(string message)
        {
            try
            {
                string[] values = message.Split(PhotoChat.Delimiter);
                PhotoChatClient.Instance.ConnectionManager.SetTimeDifference(values[1], -long.Parse(values[2]));
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        private void requireSession()
        {
            while (client.CurrentSessionName == string.Empty)
            {
                Broadcast("REQUIRESESSION");
                Thread.Sleep(3000);
            }
        }
        #endregion
    }
}
