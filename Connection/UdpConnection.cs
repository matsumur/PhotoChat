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
    /// UDP�ʐM���s���B
    /// </summary>
    public class UdpConnection
    {
        #region �t�B�[���h�E�v���p�e�B

        private PhotoChatClient client;
        public delegate void ReceiveSignalDelegate(string message, IPAddress remoteIP);
        public delegate void SendIPListDeligate(int protcol, IPEndPoint endPoint);
        public delegate void ADDIPListDeligate(string IPs);
        private volatile bool isAlive;

        private Thread requireSessionThread;

        // ���M�֘A
        private UdpClient sendClient;
        private ADDIPListDeligate addIPListDeligate;
        private SendIPListDeligate sendIPListDeligate;
        private AsyncCallback completeSendIP;
        private AsyncCallback completeAddIP;

        // ��M�֘A
        private UdpClient receiveClient;
        private ReceiveSignalDelegate receiveSignal;
        private AsyncCallback receiveCallback;
        private AsyncCallback completeCallback;
        private IPEndPoint remoteEP = null;


        /// <summary>
        /// �ʐM���N�����ł��邩�ǂ���
        /// </summary>
        public bool IsAlive
        {
            get { return isAlive; }
        }

        #endregion

        #region �R���X�g���N�^

        /// <summary>
        /// UDP�ʐM��������������B
        /// </summary>
        /// <param name="receiveSignal">��M�M�������f���Q�[�g</param>
        public UdpConnection(ReceiveSignalDelegate receiveSignal, SendIPListDeligate sendIPListDeligate, ADDIPListDeligate addIPListDeligate)
        {
            this.receiveSignal = receiveSignal;
            this.sendIPListDeligate = sendIPListDeligate;
            this.addIPListDeligate = addIPListDeligate;
            isAlive = true;

            // ���M������
            sendClient = new UdpClient();
            sendClient.Ttl = 3;
            completeSendIP = new AsyncCallback(SendIPComplete);

            // ��M������
            receiveClient = new UdpClient(new IPEndPoint(IPAddress.Any, PhotoChat.UdpPort));
            receiveCallback = new AsyncCallback(ReceiveCallback);
            completeCallback = new AsyncCallback(ReceiveComplete);
            completeAddIP = new AsyncCallback(CompleteAddIP);
        }

        #endregion


        #region �N���E��~
        /// <summary>
        /// �ʐM���J�n����B
        /// </summary>
        /// <param name="client">PhotoChatClient�C���X�^���X</param>
        public void Start(PhotoChatClient client)
        {
            try
            {
                this.client = client;

                //�l�b�g���[�N��̃z�X�g�T���̂��߂Ƀ��b�Z�[�W�𑗂�
                Broadcast("JOIN" + PhotoChat.Delimiter + client.ID);

                // ��M�J�n
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
        /// �ʐM���I�����A���\�[�X���J������B
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


        #region ���M

        /// <summary>
        /// �[��ID�̃u���[�h�L���X�g���s���B
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


        #region ��M

        /// <summary>
        /// UDP�M������M�����Ƃ��ɃR�[���o�b�N�����B
        /// </summary>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                // ��M�f�[�^��ǂݎ��
                byte[] message = receiveClient.EndReceive(asyncResult, ref remoteEP);
                string str = PhotoChat.DefaultEncoding.GetString(message);

                if (str.StartsWith(PhotoChat.TimeHeader))
                {
                    // �[�����ԍ��擾����
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
                        // ��M�f�[�^�̔񓯊�����
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
                // �����Ĕ񓯊���M
                if (isAlive)
                    receiveClient.BeginReceive(receiveCallback, null);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ��M�f�[�^���������������Ƃ��ɃR�[���o�b�N�����B
        /// </summary>
        private void ReceiveComplete(IAsyncResult asyncResult)
        {
            receiveSignal.EndInvoke(asyncResult);
        }

        /// <summary>
        /// IP�ǉ����I�������Ƃ��ɃR�[���o�b�N�����
        /// </summary>
        /// <param name="asyncResult"></param>
        private void CompleteAddIP(IAsyncResult asyncResult)
        {
            addIPListDeligate.EndInvoke(asyncResult);
        }

        #endregion


        #region �[�����ԍ��擾


        /// <summary>
        /// ��M�����[�����ԍ��擾�̂��߂̐M������������B
        /// </summary>
        /// <param name="message">��M���b�Z�[�W</param>
        /// <param name="remoteIP">���M����IPEndPoint</param>
        /// <param name="ticks">��M���̎���</param>
        private void receiveTimeSignal(string message, IPEndPoint remoteEP, long ticks)
        {
            try
            {
                string[] values = message.Split(new Char[] { PhotoChat.Delimiter }, StringSplitOptions.RemoveEmptyEntries);

                // ���H���ԍ����v�Z���ĉ����M���𑗐M
                long timeDifference = ticks - long.Parse(values[1]);
                string sendMessage =
                    PhotoChat.TimeHeader + PhotoChat.Delimiter + timeDifference.ToString()
                    + PhotoChat.Delimiter + DateTime.Now.Ticks.ToString();
                //����̎�M������TcpConnection��getTimeDiff
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
