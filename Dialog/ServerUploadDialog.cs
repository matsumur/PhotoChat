using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace PhotoChat
{
    /// <summary>
    /// PhotoChat�T�[�o�A�b�v���[�h�_�C�A���O
    /// </summary>
    public partial class ServerUploadDialog : Form
    {
        #region �t�B�[���h�E�v���p�e�B

        private List<SessionManager.SessionInfo> uploadedSessionList
            = new List<SessionManager.SessionInfo>();
        private volatile bool uploadComplete = false;

        /// <summary>
        /// �A�b�v���[�h�����Z�b�V�����̃��X�g���擾����B
        /// </summary>
        public List<SessionManager.SessionInfo> UploadedSessionList
        {
            get { return uploadedSessionList; }
        }

        /// <summary>
        /// �A�b�v���[�h�������������ǂ������擾����B
        /// </summary>
        public bool UploadComplete
        {
            get { return uploadComplete; }
        }

        #endregion


        /// <summary>
        /// �A�b�v���[�h�_�C�A���O���쐬����B
        /// </summary>
        public ServerUploadDialog()
        {
            InitializeComponent();

            // �A�J�E���g���擾
            if (File.Exists(PhotoChat.UploadAccountFile))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(PhotoChat.UploadAccountFile))
                    {
                        mailTextBox.Text = sr.ReadLine();
                        passwordTextBox.Text = sr.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// �A�b�v���[�h�Z�b�V�����p�l����ǉ�����B
        /// </summary>
        /// <param name="sessionList">�A�b�v���[�h����Z�b�V�����̃��X�g</param>
        public void AddSessionPanels(List<SessionManager.SessionInfo> sessionList)
        {
            try
            {
                sessionList.Sort();
                sessionList.Reverse();
                foreach (SessionManager.SessionInfo info in sessionList)
                {
                    UploadSessionPanel panel = new UploadSessionPanel();
                    panel.SetSession(info);
                    uploadPanel.Controls.Add(panel);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }




        #region �A�b�v���[�h

        private void ServerUpload()
        {
            try
            {
                // �i�s�󋵕\���E�B���h�E
                int count = 0;
                foreach (Control control in uploadPanel.Controls)
                    if (((UploadSessionPanel)control).DoUpload) count++;
                ProgressWindow progressWindow = new ProgressWindow(
                    count, "PhotoChat�T�[�o�ɃA�b�v���[�h���Ă��܂�");
                count = 0;
                progressWindow.Show(this);

                // �T�[�o�Ƀ��O�C��
                PhotoChatClient client = PhotoChatClient.Instance;
                Command command;
                ServerConnection connection = new ServerConnection();
                if (connection.IsConnecting)
                {
                    connection.Send(Command.CreateLoginCommand(
                        client.ID, mailTextBox.Text, passwordTextBox.Text, client.UserName));
                    command = connection.Receive();
                    if (command == null || !bool.Parse(command.UserName))
                    {
                        MessageBox.Show("���[���A�h���X�܂��̓p�X���[�h���Ԉ���Ă��܂�");
                        connection.Close();
                        progressWindow.Dispose();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("PhotoChat�T�[�o�ɐڑ��ł��܂���ł���");
                    connection.Close();
                    progressWindow.Dispose();
                    return;
                }

                // �Z�b�V�������ƂɃA�b�v���[�h
                foreach (Control control in uploadPanel.Controls)
                {
                    UploadSessionPanel panel = (UploadSessionPanel)control;
                    if (panel.DoUpload)
                    {
                        // �Z�b�V�����f�[�^�̃A�b�v���[�h�J�n
                        SessionManager.SessionInfo info = panel.SessionInfo;
                        connection.Send(Command.CreateSessionCommand(
                            client.ID, info.ID, info.Name, panel.IsPublic));
                        command = connection.Receive();
                        if (command != null && command.Type == Command.TypeSession)
                        {
                            // �[�����Ƃ̃f�[�^���A�b�v���[�h
                            foreach (string directory in Directory.GetDirectories(PhotoChat.UserIndexDirectory))
                            {
                                string filePath = Path.Combine(directory, info.ID + ".dat");
                                if (File.Exists(filePath))
                                {
                                    UploadIndexData(connection, filePath);
                                }
                            }
                            // �Z�b�V�����f�[�^�̃A�b�v���[�h����
                            uploadedSessionList.Add(info);
                        }
                        progressWindow.SetCount(++count);
                    }
                }
                // �A�b�v���[�h�I��
                connection.Send(Command.CreateDisconnectCommand(client.ID, client.ID));
                command = connection.Receive();
                if (command != null && command.Type == Command.TypeDisconnect)
                    uploadComplete = true;

                // �A�J�E���g���ۑ�
                if (savePasswordCheckBox.Checked)
                    SaveAccount();

                progressWindow.Dispose();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���[�UIndex�t�@�C���̃f�[�^���A�b�v���[�h����B
        /// </summary>
        /// <param name="connection">�T�[�o�ڑ�</param>
        /// <param name="filePath">�t�@�C���p�X</param>
        private void UploadIndexData(ServerConnection connection, string filePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        try
                        {
                            int index1 = line.IndexOf(PhotoChat.Delimiter) + 1;
                            int index2 = line.IndexOf(PhotoChat.Delimiter, index1);
                            int type = int.Parse(line.Substring(index1, index2 - index1));
                            switch (type)
                            {
                                case PhotoChatImage.TypePhoto:
                                    connection.Send(new Photo(line.Substring(index2 + 1)));
                                    break;

                                case PhotoChatNote.TypeHyperlink:
                                case PhotoChatNote.TypeRemoval:
                                case PhotoChatNote.TypeStroke:
                                case PhotoChatNote.TypeTag:
                                case PhotoChatNote.TypeText:
                                    connection.Send(PhotoChatNote.CreateInstance(
                                        type, line.Substring(index2 + 1)));
                                    break;
                            }
                        }
                        catch (UnsupportedDataException) { }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �A�J�E���g����ۑ�����B
        /// </summary>
        private void SaveAccount()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(PhotoChat.UploadAccountFile))
                {
                    sw.WriteLine(mailTextBox.Text);
                    sw.WriteLine(passwordTextBox.Text);
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �A�b�v���[�h�Z�b�V�����p�l��

        /// <summary>
        /// �A�b�v���[�h�Z�b�V�����p�l��
        /// </summary>
        public class UploadSessionPanel : Panel
        {
            private SessionManager.SessionInfo sessionInfo;
            private CheckBox uploadCheckBox;
            private CheckBox publicCheckBox;
            private CheckBox memberCheckBox;

            /// <summary>
            /// �A�b�v���[�h�Z�b�V�����p�l�����쐬����B
            /// </summary>
            public UploadSessionPanel()
            {
                this.SuspendLayout();

                this.Location = new Point(3, 3);
                this.Size = new Size(570, 40);
                
                uploadCheckBox = new CheckBox();
                uploadCheckBox.AutoSize = true;
                uploadCheckBox.Location = new Point(3, 3);
                uploadCheckBox.Checked = true;
                uploadCheckBox.CheckState = CheckState.Checked;
                this.Controls.Add(uploadCheckBox);

                publicCheckBox = new CheckBox();
                publicCheckBox.Location = new Point(300, 3);
                publicCheckBox.Size = new Size(111, 20);
                publicCheckBox.Text = "�S�̂Ɍ��J";
                this.Controls.Add(publicCheckBox);

                memberCheckBox = new CheckBox();
                memberCheckBox.Location = new Point(420, 3);
                memberCheckBox.Size = new Size(128, 20);
                memberCheckBox.Text = "�Q���҂Ɍ��J";
                memberCheckBox.Checked = true;
                memberCheckBox.CheckState = CheckState.Checked;
                memberCheckBox.Enabled = false;
                this.Controls.Add(memberCheckBox);

                this.ResumeLayout(false);
            }

            /// <summary>
            /// �Z�b�V���������擾����B
            /// </summary>
            public SessionManager.SessionInfo SessionInfo
            {
                get { return sessionInfo; }
            }

            /// <summary>
            /// �A�b�v���[�h���邩�ǂ������擾����B
            /// </summary>
            public bool DoUpload
            {
                get { return uploadCheckBox.Checked; }
            }

            /// <summary>
            /// �S�̂Ɍ��J���邩�ǂ������擾����B
            /// </summary>
            public bool IsPublic
            {
                get { return publicCheckBox.Checked; }
            }

            /// <summary>
            /// �Z�b�V������ݒ肷��B
            /// </summary>
            /// <param name="sessionInfo">�Z�b�V�������</param>
            public void SetSession(SessionManager.SessionInfo sessionInfo)
            {
                this.sessionInfo = sessionInfo;
                uploadCheckBox.Text = sessionInfo.Date.ToString("MM/dd ") + sessionInfo.Name;
            }
        }

        #endregion




        #region �ʐM

        /// <summary>
        /// �T�[�o�ʐM�N���X
        /// </summary>
        private class ServerConnection
        {
            private Socket socket;
            private BinaryReader reader = null;
            private BinaryWriter writer = null;
            private volatile bool isConnecting = false;

            /// <summary>
            /// �ڑ������ǂ������擾����B
            /// </summary>
            public bool IsConnecting
            {
                get { return isConnecting; }
            }

            /// <summary>
            /// �T�[�o�Ƃ̐ڑ����m������B
            /// </summary>
            public ServerConnection()
            {
                try
                {
                    IPEndPoint endPoint = new IPEndPoint(
                        PhotoChat.ServerUploadAddress, PhotoChat.ServerUploadPort);
                    socket = new Socket(
                        endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(endPoint);
                    if (socket.Connected)
                    {
                        reader = new BinaryReader(new NetworkStream(socket));
                        writer = new BinaryWriter(new NetworkStream(socket));
                        this.isConnecting = true;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }

            /// <summary>
            /// �T�[�o�Ƃ̒ʐM��ؒf����B
            /// </summary>
            public void Close()
            {
                if (!isConnecting) return;
                isConnecting = false;

                // �\�P�b�g�����
                try
                {
                    reader.Close();
                    reader = null;
                    writer.Close();
                    writer = null;
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }

            /// <summary>
            /// �f�[�^�𑗐M����B
            /// </summary>
            /// <param name="data">���M�f�[�^</param>
            public void Send(ISendable data)
            {
                if (data == null || !isConnecting) return;

                try
                {
                    byte[] dataBytes = data.GetDataBytes();

                    // �f�[�^�^�C�v�̑��M
                    writer.Write(data.Type);

                    // �o�C�g��̑��M
                    writer.Write(dataBytes.Length);
                    writer.Write(dataBytes);
                    writer.Flush();
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }

            /// <summary>
            /// �R�}���h����M����B
            /// </summary>
            /// <returns>��M�R�}���h</returns>
            public Command Receive()
            {
                if (isConnecting)
                {
                    try
                    {
                        // �f�[�^�^�C�v�̓ǂݎ��
                        int type = reader.ReadInt32();

                        // �o�C�g��̓ǂݎ��
                        int length = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(length);

                        // ��M�f�[�^���C���X�^���X��
                        switch (type)
                        {
                            case Command.TypeConnect:
                            case Command.TypeSession:
                            case Command.TypeDisconnect:
                                return new Command(type, data);
                        }
                    }
                    catch (Exception e)
                    {
                        PhotoChat.WriteErrorLog(e.ToString());
                    }
                }
                return null;
            }
        }

        #endregion




        #region �C�x���g

        /// <summary>
        /// ���[���A�h���X���̓e�L�X�g�{�b�N�X
        /// </summary>
        private void mailTextBox_TextChanged(object sender, EventArgs e)
        {
            if (mailTextBox.Text.Length == 0)
            {
                passwordLabel.Enabled = false;
                passwordTextBox.Enabled = false;
            }
            else
            {
                passwordLabel.Enabled = true;
                passwordTextBox.Enabled = true;
            }
        }

        /// <summary>
        /// �p�X���[�h���̓e�L�X�g�{�b�N�X
        /// </summary>
        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            if (passwordTextBox.Text.Length == 0)
            {
                uploadButton.Enabled = false;
                savePasswordCheckBox.Enabled = false;
            }
            else
            {
                uploadButton.Enabled = true;
                savePasswordCheckBox.Enabled = true;
            }
        }

        /// <summary>
        /// �A�b�v���[�h�{�^��
        /// </summary>
        private void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                uploadButton.Enabled = false;
                ServerUpload();
                uploadButton.Enabled = true;
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion
    }
}