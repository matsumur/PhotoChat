//�ڑ��v���g�R��

//1: JOIN���b�Z�[�W�𑗂�(�u���[�h�L���X�g)
//   �S�[���Ɏ������Q���������Ƃ�ʒm����
//   ���̒i�K�ł͑��[����IPLIST�ɂ͒ǉ�����Ȃ�
//2: IPLIST���b�Z�[�W�𑗂�Ԃ�(���j�L���X�g)
//   ���̒i�K�őS�[���̎����Ă���(���Z�b�V�������܂�)��IP�ƃ��[�UID���擾�ł���
//3: SENDBACK���b�Z�[�W�𑗂�(�u���[�h�L���X�g)
//   JOIN���b�Z�[�W����5�b��Ɏ擾����IPLIST�𑗂�Ԃ�

//3�`6�b�̃����_�����Ԃ��Ƃ�
//IPLIST��IP�̂���[���ɒ���I�ɐڑ������݂�

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
    /// �ʐM���̊Ǘ����s���B
    /// </summary>
    public class ConnectionManager
    {
        #region �t�B�[���h�E�v���p�e�B

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
        /// �ʐM�����N�����ł��邩�ǂ���
        /// </summary>
        public bool IsAlive
        {
            get { return isAlive; }
        }
        private volatile bool isAlive;

        #endregion


        #region �R���X�g���N�^

        /// <summary>
        /// �ʐM��������������B
        /// </summary>
        /// <param name="client">PhotoChatClient�C���X�^���X</param>
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


        #region �N���E�I��

        /// <summary>
        /// �ʐM�����N������B
        /// </summary>
        public void Start()
        {
            try
            {
                //TCP��t�J�n
                tcpListeningThread = new Thread(new ThreadStart(listen));
                tcpListeningThread.Start();

                // ��M�X���b�h�AUDP�ʐM�N��
                receiveThread.Start();
                udpConnection.Start(client);

                // �ʐM�N�����O
                PhotoChat.WriteLog("ConnectionManager Start", string.Empty, string.Empty);
                client.Form.LogWindow.AddLine("ConnectionManager Starts");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ʐM�����I������B
        /// </summary>
        public void Close()
        {
            try
            {
                if (!isAlive) return;
                isAlive = false;

                // UDP�ʐM���~
                udpConnection.Close();

                // �S�Ă̒ʐM��ؒf
                DisconnectAll();

                // ��M�f�[�^�����X���b�h��~
                receiveWaitHandle.Set();

                // �ʐM��~���O
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
        /// �S�Ă̒ʐM��ؒf����B
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


        #region �Ǘ����[�e�B���e�B

        /// <summary>
        /// ID�ɑΉ�����TCPConnection�𓾂�B
        /// �V����ID�ł����TCPConnection���쐬�E�o�^����B
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>ID�ɑΉ�����TCPConnection</returns>
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
        /// ID�ɑΉ�����NumberManager��Ԃ��B
        /// �V����ID�ł����NumberManager���쐬���ĕԂ��B
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>ID�ɑΉ�����NumberManager</returns>
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
        /// �I���R�}���h���V���̂��̂��ǂ����^�C���X�^���v�����Ē��ׂ�B
        /// </summary>
        private bool IsNewSelectCommand(Command selectCommand)
        {
            try
            {
                // �Ō�Ɏ�M�����I���R�}���h�̃^�C���X�^���v���擾
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
        /// TCPDictionary�ɒǉ�����
        /// </summary>
        /// <param name="id"></param>
        /// <param name="connection"></param>
        private void addTcpConnectionDictionary(string id, TcpConnection connection)
        {
            if (!tcpConnectionDictionary.ContainsKey(id)) tcpConnectionDictionary.Add(id, connection);
        }

        /// <summary>
        /// IPDictionary�ɒǉ�����
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="id">UserID</param>
        private void addIPDictionary(string ip, string id)
        {
            if (!ipDictionary.ContainsKey(ip)) ipDictionary.Add(ip, id);
        }

        #endregion


        #region ���M

        /// <summary>
        /// �S�Ă̐ڑ���Ƀf�[�^�𑗐M����B
        /// </summary>
        /// <param name="data">���M����f�[�^</param>
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
        /// �f�[�^��ID�������[���֑��M����B
        /// </summary>
        /// <param name="id">���M��̒[��ID</param>
        /// <param name="data">���M����f�[�^</param>
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


        #region ��M

        /// <summary>
        /// ��M�L���[�Ɏ�M�f�[�^��ǉ�����B
        /// </summary>
        /// <param name="receivedData">��M�f�[�^</param>
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
        /// ��M�f�[�^�����X���b�h�{��
        /// </summary>
        private void ProcessReceivedData()
        {
            ISendable receivedData;
            while (isAlive)
            {
                try
                {
                    // ��M�L���[����̂Ƃ��͑ҋ@
                    if (receiveQueue.Count == 0)
                    {
                        receiveWaitHandle.Reset();
                        receiveWaitHandle.WaitOne();
                    }

                    // ��M�L���[����f�[�^�����o��
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
        /// ��M����PhotoChatImage����������B
        /// </summary>
        /// <param name="image">��M����PhotoChatImage</param>
        private void Receive(PhotoChatImage image)
        {
            try
            {
                // ��M�f�[�^�̒ʂ��ԍ���NumberManager�ɒʒm
                bool isLatest =
                    GetNumberManager(image.ID).PutReceivedNumber(image.SerialNumber);

                // �ŐV�̃f�[�^���ǂ������܂߂�client�Ƀf�[�^��n���A
                // �V���f�[�^�ł���ΐڑ����̒[���ɂ��̃f�[�^���K�v���R�}���h�𑗂�
                if (client.NewData(image, isLatest))
                    SendAll(Command.CreateInformCommand(image, client.ID));

                // �ʐ^��M���O
                PhotoChat.WriteLog("Receive Photo", image.PhotoName, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ��M����PhotoChatNote����������B
        /// </summary>
        /// <param name="note">��M����PhotoChatNote</param>
        private void Receive(PhotoChatNote note)
        {
            try
            {
                // ��M�f�[�^�̒ʂ��ԍ���NumberManager�ɒʒm
                bool isLatest =
                    GetNumberManager(note.ID).PutReceivedNumber(note.SerialNumber);

                // �ŐV�̃f�[�^���ǂ������܂߂�client�Ƀf�[�^��n���A
                // �V���f�[�^�ł���ΐڑ����̒[���ɂ��̃f�[�^���K�v���R�}���h�𑗂�
                if (client.NewData(note, isLatest))
                    SendAll(Command.CreateInformCommand(note, client.ID));

                // �������ݎ�M���O
                PhotoChat.WriteLog("Receive Note", note.Author + note.SerialNumber, note.PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ��M����SharedFile����������B
        /// </summary>
        /// <param name="sharedFile">��M����SharedFile</param>
        private void Receive(SharedFile sharedFile)
        {
            try
            {
                // ��M�f�[�^�̒ʂ��ԍ���NumberManager�ɒʒm
                bool isLatest =
                    GetNumberManager(sharedFile.ID).PutReceivedNumber(sharedFile.SerialNumber);

                // �ŐV�̃f�[�^���ǂ������܂߂�client�Ƀf�[�^��n���A
                // �V���f�[�^�ł���ΐڑ����̒[���ɂ��̃f�[�^���K�v���R�}���h�𑗂�
                if (client.NewData(sharedFile, isLatest))
                    SendAll(Command.CreateInformCommand(sharedFile, client.ID));

                // �������ݎ�M���O
                PhotoChat.WriteLog("Receive File", sharedFile.FileName, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ��M����Command����������B
        /// </summary>
        /// <param name="command">��M����Command</param>
        private void Receive(Command command)
        {
            try
            {
                // ���������M�����R�}���h�͖���
                if (command.SourceID == client.ID) return;

                // �R�}���h�̃^�C�v�ɉ����ď���
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


        #region ��M�R�}���h����

        /// <summary>
        /// �ʐ^�I���R�}���h����������B
        /// </summary>
        /// <param name="command">�ʐ^�I���R�}���h</param>
        private void ProcessSelectCommand(Command command)
        {
            try
            {
                // �^�C���X�^���v���V�����I���R�}���h�̏ꍇ�̂ݏ���
                if (IsNewSelectCommand(command))
                {
                    // client�Ɏʐ^�I��ʒm
                    client.Form.InformSelection(
                        command.AuthorID, command.UserName, command.PhotoName);

                    // �^�C���X�^���vDictionary�̍X�V�ƃR�}���h�̓]��
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
        /// �V���f�[�^�ʒm�R�}���h����������B
        /// </summary>
        /// <param name="command">�V���f�[�^�ʒm�R�}���h</param>
        private void ProcessInformCommand(Command command)
        {
            try
            {
                // �����̃f�[�^�ɂ��Ă̒ʒm�͖���
                if (command.AuthorID == client.ID) return;

                // ���M�v�����o���Ă��Ȃ����̂�����Α��M�v���R�}���h�𑗂�Ԃ�
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
        /// �ێ��f�[�^�ʒm�R�}���h����������B
        /// </summary>
        /// <param name="command">�ێ��f�[�^�ʒm�R�}���h</param>
        private void ProcessTransferCommand(Command command)
        {
            try
            {
                // �����̃f�[�^�ɂ��Ă̒ʒm�͖���
                if (command.AuthorID == client.ID) return;

                // ���M�v�����o���Ă��Ȃ����̂�����Α��M�v���R�}���h�𑗂�Ԃ�
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
        /// �f�[�^���M�v���R�}���h����������B
        /// </summary>
        /// <param name="command">�f�[�^���M�v���R�}���h</param>
        private void ProcessRequestCommand(Command command)
        {
            try
            {
                // �v�����ꂽ�f�[�^�𑗂�Ԃ�
                ISendable data = client.Request(command.AuthorID, command.SerialNumber);
                SendTo(command.SourceID, data);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// �ڑ��ʒm�R�}���h����������B
        /// </summary>
        /// <param name="command">�ڑ��ʒm�R�}���h</param>
        private void ProcessConnectCommand(Command command)
        {
            try
            {
                // �ڑ��ʒm
                client.AddUser(command.AuthorID, command.UserName);
                client.Form.NewConnection(command.AuthorID, command.UserName, true);
                // �ڑ����O
                PhotoChat.WriteLog("Connect", command.AuthorID, command.UserName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// �ؒf�ʒm�R�}���h����������B
        /// </summary>
        /// <param name="command">�ؒf�ʒm�R�}���h</param>
        private void ProcessDisconnectCommand(Command command)
        {
            try
            {
                // �^�C���X�^���v���V�����I���R�}���h�̏ꍇ�̂ݏ���
                if (IsNewSelectCommand(command))
                {
                    // �I���ʐ^�\���̃��Z�b�g��ʒm
                    client.Form.InformSelection(command.AuthorID, command.UserName, null);

                    // �^�C���X�^���vDictionary�̍X�V�ƃR�}���h�̓]��
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


        #region �ڑ��E�ؒf�iUDP�M�������j
        /// <summary>
        /// <param name="protcol">
        /// 0: JOIN(���̃}�V���N��)�ɑ΂���IPLIST��Ԃ�
        /// 1: IPLIST�ɑ΂���IPLIST�𑗂�Ԃ�
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
        /// IPList���X�V����
        /// </summary>
        /// <param name="ipMessage">IPList�ɒǉ�����IP</param>
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


        #region �ڑ��E�ؒf�iTCP�M�������j

        /// <summary>
        /// ����I�ɐڑ����J�n����
        /// 3�`6�b�̃����_�����ԂŐڑ�����
        /// </summary>
        private void connect()
        {
            Random rand = new Random();

            while (isAlive)
            {
                try
                {
                    string opp = "�ڑ��Ώ�:";
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
                                // �ڑ��������̏���
                                ProcessSuccessfulConnection(connection, IPAddress.Parse(kvp.Key));
                            }
                        }
                        else
                        {
                            connection = new TcpConnection(this, kvp.Value, kvp.Key);
                            addTcpConnectionDictionary(kvp.Value, connection);

                            if (connection.Connect())
                            {
                                // �ڑ��������̏���
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
        /// �ڑ���t
        /// </summary>
        private void listen()
        {
            tcpListener.Start();

            while (isAlive)
            {
                try
                {
                    // ���X�ҋ@
                    Thread.Sleep(100);

                    // �ڑ��҂������邩�H
                    if (tcpListener.Pending() == true)
                    {
                        // �\�P�b�g�AIP�A�h���X�̎擾
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

                        //�Ⴄ�Z�b�V�����ł���ΐؒf����
                        if (id[1] != PhotoChatClient.Instance.CurrentSessionID)
                        {
                            sendMessage = BitConverter.GetBytes(0);
                            socket.Send(sendMessage, sendMessage.Length, SocketFlags.None);
                            socket.Close();
                        }

                        // �ڑ��󂯓���
                        TcpConnection connection;
                        if (tcpConnectionDictionary.TryGetValue(id[0], out connection))
                        {
                            if (connection.AcceptConnection(socket))
                            {
                                // �ڑ��������̏���
                                ProcessSuccessfulConnection(connection, remoteIP);
                                //�ڑ�������ʒm
                                
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
                                // �ڑ��������̏���
                                ProcessSuccessfulConnection(connection, remoteIP);
                                //�ڑ�������ʒm

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
        /// ��M����UDP�M������������B
        /// </summary>
        /// <param name="message">��M���b�Z�[�W</param>
        /// <param name="remoteIP">���M����IP�A�h���X</param>
        public void ReceiveSignal(string message, IPAddress remoteIP)
        {
            try
            {
                string[] temp = message.Split(PhotoChat.Delimiter);
                // �Z�b�V�����ǉ�����
                setSession(temp[1], temp[2]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �u���[�h�L���X�g�M������M�����Ƃ��̏���
        /// </summary>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <param name="sessionName">�Z�b�V������</param>
        private void setSession(string sessionID, string sessionName)
        {
            try
            {
                // �Z�b�V����ID�������Ƃ��͐ڑ�����
                if (sessionID != client.CurrentSessionID)
                {
                    // �Z�b�V�����I���_�C�A���O������ΑI���{�^����ǉ�
                    client.AddNearbySessionButton(sessionName, sessionID);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// �ڑ��������̏�����ڑ��R�}���h�̑��M���s���B
        /// </summary>
        /// <param name="connection">�ڑ���Ƃ�TCP�R�l�N�V����</param>
        /// <param name="remoteIP">�ڑ����IP�A�h���X</param>
        private void ProcessSuccessfulConnection(TcpConnection connection, IPAddress remoteIP)
        {
            try
            {
                // �ڑ��ʒm�R�}���h���M
                connection.Send(Command.CreateConnectCommand(client.ID, client.UserName));

                // �����̃f�[�^�̕ێ��f�[�^�ʒm�R�}���h���M
                connection.Send(Command.CreateTransferCommand(
                    client.FloorSerialNumber, client.CeilingSerialNumber, client.ID, client.ID));

                // ID���Ƃ̕ێ��f�[�^�ʒm�R�}���h���M
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

                // �ڑ����O
                client.Form.LogWindow.AddLine("Connect:" + remoteIP.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ؒf���̏������s���B
        /// </summary>
        /// <param name="id">�ؒf��������̒[��ID</param>
        internal void ProcessDisconnection(string id)
        {
            try
            {
                // �ؒf�R�}���h���M
                SendAll(Command.CreateDisconnectCommand(id, client.ID));

                // GUI���ɐؒf��ʒm
                client.Form.NewConnection(id, null, false);
                // �R�l�N�V�����폜
                tcpConnectionDictionary[id].IsConnecting = false;

                lock (numberDictionary)
                {
                    numberDictionary.Remove(id);
                }

                // �ؒf���O
                PhotoChat.WriteLog("Disconnect", id, string.Empty);
                client.Form.LogWindow.AddLine("Disconnect:" + id);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region �[�����ԍ��擾

        /// <summary>
        /// �[�����ԍ��̐ݒ�Ɖ������ւ̃��[�U�o�^������B
        /// </summary>
        /// <param name="remoteID">�ڑ���̒[��ID</param>
        /// <param name="timeDifference">�[�����ԍ�</param>
        public void SetTimeDifference(string remoteID, long timeDifference)
        {
            try
            {
                if (remoteID == client.ID) return;

                // �[�����ԍ��̐ݒ�
                TcpConnection connection = null;

                while (connection == null)
                {
                    Thread.Sleep(3000);
                    connection = GetConnection(remoteID);
                }

                if (!connection.IsConnecting) return;

                connection.TimeDifference = timeDifference;
                // ���O
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
        /// �ʂ��ԍ��ɂ��ʂ̒[���f�[�^�̎�M�������Ǘ�����B
        /// </summary>
        public class NumberManager
        {
            private long floorNumber = -1;
            private long ceilingNumber = -1;
            private LinkedList<long> unreceivedNumbers;
            private long requestedNumber = -1;

            /// <summary>
            /// ���݂̃Z�b�V�����ɂ�����ŏ��̔ԍ����擾����B
            /// </summary>
            public long FloorNumber
            {
                get { return floorNumber; }
            }

            /// <summary>
            /// �ŐV�̎�M�ςݔԍ����擾����B
            /// </summary>
            public long CeilingNumber
            {
                get { return ceilingNumber; }
            }

            /// <summary>
            /// ����M�ԍ����X�g���擾����B
            /// </summary>
            public LinkedList<long> UnreceivedNumbers
            {
                get { return unreceivedNumbers; }
            }


            /// <summary>
            /// ID�ɑΉ�����[���f�[�^�̎�M�����Ǘ��C���X�^���X���쐬����B
            /// </summary>
            /// <param name="id">�[��ID</param>
            public NumberManager(string id)
            {
                unreceivedNumbers = new LinkedList<long>();
                PhotoChatClient.Instance.CheckSerialNumber(
                    id, ref floorNumber, ref ceilingNumber, unreceivedNumbers);
            }


            /// <summary>
            /// serialNumber����M�ςݔԍ��Ƃ��ď�������B
            /// serialNumber���ŐV�̒ʂ��ԍ��ł����true��Ԃ��A
            /// ��΂��ꂽ�ԍ�������Ζ���M�ԍ����X�g�ɒǉ�����B
            /// </summary>
            /// <param name="serialNumber">��M�����f�[�^�̒ʂ��ԍ�</param>
            /// <returns>�ŐV�̒ʂ��ԍ��ł����true�A�����łȂ����false��Ԃ��B</returns>
            public bool PutReceivedNumber(long serialNumber)
            {
                try
                {
                    if (ceilingNumber < 0)
                    {
                        // �ŏ��Ɏ�M�����f�[�^�̏ꍇ
                        ceilingNumber = serialNumber;
                        return true;
                    }

                    lock (unreceivedNumbers)
                    {
                        // �ŐV�̒ʂ��ԍ��̏ꍇ
                        if (ceilingNumber < serialNumber)
                        {
                            // ��΂��ꂽ�ԍ�������Ζ���M���X�g�ɒǉ�
                            while (++ceilingNumber != serialNumber)
                                unreceivedNumbers.AddLast(serialNumber);
                            return true;
                        }
                        // �ʂ��ԍ����ŐV�̂��̂łȂ��ꍇ
                        else
                        {
                            // ����M�ԍ����X�g����폜����
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
            /// ���ɑ��M�v�����o���f�[�^�̒ʂ��ԍ���Ԃ��B
            /// ���̃��\�b�h�𗘗p����Ƃ��͕K�����M�v���R�}���h���o�����ƁB
            /// </summary>
            /// <param name="limit">�v�����o���ʂ��ԍ��̏��</param>
            /// <returns>���ɗv������f�[�^�̒ʂ��ԍ��B�����ꍇ��-1��Ԃ��B</returns>
            public long NextRequestNumber(long limit)
            {
                try
                {
                    lock (unreceivedNumbers)
                    {
                        // ���ł�limit�܂ŗv���ς݂ł����-1��Ԃ�
                        if (requestedNumber >= limit)
                            return -1;

                        // ����M�ԍ����X�g�ɗv�����o���ׂ����̂��Ȃ������ׂ�
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

                        // limit�܂ł͏��ɑ��M�v�����o��
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
            /// ���̔ԍ����ŏ��ԍ���菬������΍ŏ��ԍ����X�V����B
            /// �����ݒ�̂Ƃ��͍ő�ԍ����ݒ肷��B
            /// </summary>
            /// <param name="floorNumber">�ʂ��ԍ�</param>
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
