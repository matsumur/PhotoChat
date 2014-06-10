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
    /// PhotoChatサーバアップロードダイアログ
    /// </summary>
    public partial class ServerUploadDialog : Form
    {
        #region フィールド・プロパティ

        private List<SessionManager.SessionInfo> uploadedSessionList
            = new List<SessionManager.SessionInfo>();
        private volatile bool uploadComplete = false;

        /// <summary>
        /// アップロードしたセッションのリストを取得する。
        /// </summary>
        public List<SessionManager.SessionInfo> UploadedSessionList
        {
            get { return uploadedSessionList; }
        }

        /// <summary>
        /// アップロードが成功したかどうかを取得する。
        /// </summary>
        public bool UploadComplete
        {
            get { return uploadComplete; }
        }

        #endregion


        /// <summary>
        /// アップロードダイアログを作成する。
        /// </summary>
        public ServerUploadDialog()
        {
            InitializeComponent();

            // アカウント情報取得
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
        /// アップロードセッションパネルを追加する。
        /// </summary>
        /// <param name="sessionList">アップロードするセッションのリスト</param>
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




        #region アップロード

        private void ServerUpload()
        {
            try
            {
                // 進行状況表示ウィンドウ
                int count = 0;
                foreach (Control control in uploadPanel.Controls)
                    if (((UploadSessionPanel)control).DoUpload) count++;
                ProgressWindow progressWindow = new ProgressWindow(
                    count, "PhotoChatサーバにアップロードしています");
                count = 0;
                progressWindow.Show(this);

                // サーバにログイン
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
                        MessageBox.Show("メールアドレスまたはパスワードが間違っています");
                        connection.Close();
                        progressWindow.Dispose();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("PhotoChatサーバに接続できませんでした");
                    connection.Close();
                    progressWindow.Dispose();
                    return;
                }

                // セッションごとにアップロード
                foreach (Control control in uploadPanel.Controls)
                {
                    UploadSessionPanel panel = (UploadSessionPanel)control;
                    if (panel.DoUpload)
                    {
                        // セッションデータのアップロード開始
                        SessionManager.SessionInfo info = panel.SessionInfo;
                        connection.Send(Command.CreateSessionCommand(
                            client.ID, info.ID, info.Name, panel.IsPublic));
                        command = connection.Receive();
                        if (command != null && command.Type == Command.TypeSession)
                        {
                            // 端末ごとのデータをアップロード
                            foreach (string directory in Directory.GetDirectories(PhotoChat.UserIndexDirectory))
                            {
                                string filePath = Path.Combine(directory, info.ID + ".dat");
                                if (File.Exists(filePath))
                                {
                                    UploadIndexData(connection, filePath);
                                }
                            }
                            // セッションデータのアップロード完了
                            uploadedSessionList.Add(info);
                        }
                        progressWindow.SetCount(++count);
                    }
                }
                // アップロード終了
                connection.Send(Command.CreateDisconnectCommand(client.ID, client.ID));
                command = connection.Receive();
                if (command != null && command.Type == Command.TypeDisconnect)
                    uploadComplete = true;

                // アカウント情報保存
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
        /// ユーザIndexファイルのデータをアップロードする。
        /// </summary>
        /// <param name="connection">サーバ接続</param>
        /// <param name="filePath">ファイルパス</param>
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
        /// アカウント情報を保存する。
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




        #region アップロードセッションパネル

        /// <summary>
        /// アップロードセッションパネル
        /// </summary>
        public class UploadSessionPanel : Panel
        {
            private SessionManager.SessionInfo sessionInfo;
            private CheckBox uploadCheckBox;
            private CheckBox publicCheckBox;
            private CheckBox memberCheckBox;

            /// <summary>
            /// アップロードセッションパネルを作成する。
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
                publicCheckBox.Text = "全体に公開";
                this.Controls.Add(publicCheckBox);

                memberCheckBox = new CheckBox();
                memberCheckBox.Location = new Point(420, 3);
                memberCheckBox.Size = new Size(128, 20);
                memberCheckBox.Text = "参加者に公開";
                memberCheckBox.Checked = true;
                memberCheckBox.CheckState = CheckState.Checked;
                memberCheckBox.Enabled = false;
                this.Controls.Add(memberCheckBox);

                this.ResumeLayout(false);
            }

            /// <summary>
            /// セッション情報を取得する。
            /// </summary>
            public SessionManager.SessionInfo SessionInfo
            {
                get { return sessionInfo; }
            }

            /// <summary>
            /// アップロードするかどうかを取得する。
            /// </summary>
            public bool DoUpload
            {
                get { return uploadCheckBox.Checked; }
            }

            /// <summary>
            /// 全体に公開するかどうかを取得する。
            /// </summary>
            public bool IsPublic
            {
                get { return publicCheckBox.Checked; }
            }

            /// <summary>
            /// セッションを設定する。
            /// </summary>
            /// <param name="sessionInfo">セッション情報</param>
            public void SetSession(SessionManager.SessionInfo sessionInfo)
            {
                this.sessionInfo = sessionInfo;
                uploadCheckBox.Text = sessionInfo.Date.ToString("MM/dd ") + sessionInfo.Name;
            }
        }

        #endregion




        #region 通信

        /// <summary>
        /// サーバ通信クラス
        /// </summary>
        private class ServerConnection
        {
            private Socket socket;
            private BinaryReader reader = null;
            private BinaryWriter writer = null;
            private volatile bool isConnecting = false;

            /// <summary>
            /// 接続中かどうかを取得する。
            /// </summary>
            public bool IsConnecting
            {
                get { return isConnecting; }
            }

            /// <summary>
            /// サーバとの接続を確立する。
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
            /// サーバとの通信を切断する。
            /// </summary>
            public void Close()
            {
                if (!isConnecting) return;
                isConnecting = false;

                // ソケットを閉じる
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
            /// データを送信する。
            /// </summary>
            /// <param name="data">送信データ</param>
            public void Send(ISendable data)
            {
                if (data == null || !isConnecting) return;

                try
                {
                    byte[] dataBytes = data.GetDataBytes();

                    // データタイプの送信
                    writer.Write(data.Type);

                    // バイト列の送信
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
            /// コマンドを受信する。
            /// </summary>
            /// <returns>受信コマンド</returns>
            public Command Receive()
            {
                if (isConnecting)
                {
                    try
                    {
                        // データタイプの読み取り
                        int type = reader.ReadInt32();

                        // バイト列の読み取り
                        int length = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(length);

                        // 受信データをインスタンス化
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




        #region イベント

        /// <summary>
        /// メールアドレス入力テキストボックス
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
        /// パスワード入力テキストボックス
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
        /// アップロードボタン
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