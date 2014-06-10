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
    /// TCP�ʐM���s��
    /// </summary>
    public class TcpConnection
    {
        #region �t�B�[���h�E�v���p�e�B

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
        /// �ڑ����ID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// �ڑ������ǂ���
        /// </summary>
        public bool IsConnecting
        {
            set { isConnecting = value; }
            get { return isConnecting; }
        }

        /// <summary>
        /// �ڑ���Ƃ̒[�����ԍ�
        /// </summary>
        public long TimeDifference
        {
            get { return timeDifference; }
            set { timeDifference = value; }
        }

        /// <summary>
        /// �ڑ��ҋ@���̃A�N�Z�X��
        /// </summary>
        public int WaitingCount
        {
            get { return waitingCount; }
            set { waitingCount = value; }
        }

        /// <summary>
        /// �ڑ����IP�A�h���X
        /// </summary>
        public IPAddress IP
        {
            get
            {
                return IPAddress.Parse(ip);
            }
        }

        #endregion


        #region �R���X�g���N�^

        /// <summary>
        /// �V����TCP�ڑ��̏������B
        /// </summary>
        /// <param name="manager">�ڑ������Ǘ�����ConnectionManager</param>
        /// <param name="id">�ڑ����ID</param>
        public TcpConnection(ConnectionManager manager, string id, string ip)
        {
            this.manager = manager;
            this.id = id;
            this.ip = ip;
            isConnecting = false;
            sendCallback = new WaitCallback(SendWorker);
        }

        #endregion


        #region �ڑ��E�ؒf

        /// <summary>
        /// TCP�ڑ����J�n����B
        /// ���łɐڑ����ł����false��Ԃ��ĉ������Ȃ��B
        /// </summary>
        /// <param name="ipAddress">�ڑ����IPAddress</param>
        /// <returns>�ڑ��ɐ��������Ƃ��̂�true</returns>
        public bool Connect()
        {

            // ���ɐڑ����ł����false
            if (isConnecting)
            {
                return false;
            }

            isConnecting = true;

            try
            {
                // �ڑ�����
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

                // �ڑ����s
                if (flag == 0)
                {
                    isConnecting = false;
                    socket.Close();
                    return false;
                }

                // �ڑ�����
                StartConnection();
                return true;

            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }

            // �ڑ����s
            isConnecting = false;
            return false;
        }


        /// <summary>
        /// TCP�ڑ����󂯓����B
        /// ���łɐڑ����ł����false��Ԃ��ĉ������Ȃ��B
        /// </summary>
        /// <param name="socket">�󂯓����ڑ�</param>
        /// <returns>�ڑ����󂯓��ꂽ�Ƃ��̂�true</returns>
        public bool AcceptConnection(Socket socket)
        {
            // ���ɐڑ����ł����false
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
                    //�ڑ������t���O���M
                    byte[] sendMessage = BitConverter.GetBytes(1);
                    socket.Send(sendMessage, sendMessage.Length, SocketFlags.None);
                    // �ڑ��󂯓���
                    StartConnection();
                    return true;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }

            // �󂯓��ꎸ�s
            isConnecting = false;
            return false;
        }


        /// <summary>
        /// ����M���J�n����B
        /// </summary>
        private void StartConnection()
        {
            try
            {
                // ��M�X���b�h�N��
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
        /// �ؒf����B
        /// </summary>
        public void Close()
        {
            try
            {
                if (!isConnecting) return;
                isConnecting = false;

                timeDifference = 0;

                // �\�P�b�g�����
                networkStream.Close();
                networkStream = null;
                reader.Close();
                reader = null;
                writer.Close();
                writer = null;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                // �ؒf�ʒm
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


        #region ���M

        /// <summary>
        /// �ڑ����ł���΃f�[�^��񓯊����M����B
        /// </summary>
        /// <param name="data">���M����f�[�^</param>
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
        /// �R�[���o�b�N���ꂽ��f�[�^�𑗐M����B
        /// </summary>
        /// <param name="state">���M����f�[�^���i�[����Ă���</param>
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

                    // �f�[�^�^�C�v�̑��M
                    writer.Write(sendData.Type);

                    // �o�C�g��̑��M
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


        #region ��M

        /// <summary>
        /// �f�[�^��M�X���b�h�{��
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
                    // �^�C�v�̓ǂݎ��
                    type = reader.ReadInt32();

                    // �o�C�g��̓ǂݎ��
                    length = reader.ReadInt32();
                    data = reader.ReadBytes(length);

                    // ��M�f�[�^���C���X�^���X������ConnectionManager�̎�M�L���[�֓����
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
        /// ��M�f�[�^���C���X�^���X������B
        /// �[�����ԍ�����f�[�^�����̏C�����s���B
        ///  </summary>
        /// <param name="type">�f�[�^�̃^�C�v</param>
        /// <param name="data">�f�[�^�̃o�C�g��</param>
        /// <returns>�쐬�����C���X�^���X�B�쐬�ł��Ȃ������Ƃ���null</returns>
        private ISendable CreateInstance(int type, byte[] data)
        {
            try
            {
                // �f�[�^�^�C�v�ɉ����ăC���X�^���X���i�f�[�^�����C���j
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
        /// ���ԍ����擾����
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
                    //���ݎ�����Ticks�𑗂�
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
        /// �[���Ԏ��ԍ��̎擾�̂��ߎ����𑗐M����B
        /// UDP�Ȃ̂Ő�������Ƃ͌���Ȃ��B
        /// </summary>
        /// <param name="remoteIP">���M���IP�A�h���X</param>
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
