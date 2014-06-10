using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PhotoChat
{
    /// <summary>
    /// セッション選択ダイアログ
    /// </summary>
    public partial class SessionDialog : Form
    {
        #region フィールド・プロパティ

        private string sessionName;
        private string sessionID;

        /// <summary>
        /// 入力されたセッション名を取得する。
        /// </summary>
        public string SessionName
        {
            get { return sessionName; }
        }

        /// <summary>
        /// 選択されたセッションのIDを取得する。
        /// </summary>
        public string SessionID
        {
            get { return sessionID; }
        }

        /// <summary>
        /// キャンセルボタンが有効かどうかを取得・設定する。
        /// </summary>
        public bool CancelButtonEnabled
        {
            get { return cancelButton.Enabled; }
            set { cancelButton.Enabled = value; }
        }

        #endregion




        /// <summary>
        /// セッション選択ダイアログを作成する。
        /// </summary>
        /// <param name="firstTime"></param>
        public SessionDialog()
        {
            InitializeComponent();
        }




        #region セッション選択ボタン

        /// <summary>
        /// 近くのセッション選択ボタンを追加する。
        /// </summary>
        /// <param name="sessionName">セッション名</param>
        /// <param name="sessionID">セッションID</param>
        public void AddNearbyButton(string sessionName, string sessionID)
        {
            try
            {
                lock (nearbySessionPanel)
                {
                    // すでに追加されていないか調べる
                    foreach (Control control in nearbySessionPanel.Controls)
                    {
                        if (sessionID == ((SessionButton)control).SessionID)
                            return;
                    }

                    // ボタン作成
                    SessionButton sessionButton = new SessionButton();
                    sessionButton.SessionName = sessionName;
                    sessionButton.SessionID = sessionID;
                    sessionButton.Text = sessionName;
                    sessionButton.Size = new Size(nearbySessionPanel.Width - 30, 40);
                    sessionButton.Click += new EventHandler(SessionButton_Click);
                    AddNearbyButton(sessionButton);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 近くのセッション選択エリアにボタンを追加する。
        /// </summary>
        /// <param name="button">追加するボタン</param>
        private void AddNearbyButton(SessionButton button)
        {
            try
            {
                if (InvokeRequired)
                {
                    // 別スレッドからの呼び出しの場合
                    Invoke(new Action<SessionButton>(AddNearbyButton), button);
                    return;
                }
                nearbySessionPanel.Controls.Add(button);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// セッション選択ボタンを追加する。
        /// </summary>
        /// <param name="sessionName">セッション名</param>
        /// <param name="sessionID">セッションID</param>
        /// <param name="info">ボタン文字列の先頭につける文字列</param>
        public void AddPastButton(string sessionName, string sessionID, string info)
        {
            try
            {
                lock (pastSessionPanel)
                {
                    // すでに追加されていないか調べる
                    foreach (Control control in pastSessionPanel.Controls)
                    {
                        if (sessionID == ((SessionButton)control).SessionID)
                            return;
                    }

                    // ボタン作成
                    SessionButton sessionButton = new SessionButton();
                    sessionButton.SessionName = sessionName;
                    sessionButton.SessionID = sessionID;
                    sessionButton.Text = info + Environment.NewLine + sessionName;
                    sessionButton.Size = new Size(pastSessionPanel.Width - 30, 40);
                    sessionButton.Click += new EventHandler(SessionButton_Click);
                    AddPastButton(sessionButton);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 過去のセッション選択エリアにボタンを追加する。
        /// </summary>
        /// <param name="button">追加するボタン</param>
        private void AddPastButton(SessionButton button)
        {
            try
            {
                if (InvokeRequired)
                {
                    // 別スレッドからの呼び出しの場合
                    Invoke(new Action<SessionButton>(AddPastButton), button);
                    return;
                }
                pastSessionPanel.Controls.Add(button);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// セッション選択ボタン
        /// </summary>
        public class SessionButton : Button
        {
            private string sessionName;
            private string sessionID;

            /// <summary>
            /// セッション選択ボタンを作成する。
            /// </summary>
            public SessionButton()
            {
                this.DialogResult = DialogResult.OK;
                this.BackColor = Color.Aquamarine;
                this.TextAlign = ContentAlignment.MiddleCenter;
            }

            /// <summary>
            /// このボタンのセッション名を取得する。
            /// </summary>
            public string SessionName
            {
                get { return sessionName; }
                set { sessionName = value; }
            }

            /// <summary>
            /// このボタンのセッションIDを取得する。
            /// </summary>
            public string SessionID
            {
                get { return sessionID; }
                set { sessionID = value; }
            }
        }

        #endregion




        #region イベント

        /// <summary>
        /// フォームアクティブ
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            inputTextBox.Focus();
            base.OnActivated(e);
        }


        /// <summary>
        /// セッション名入力
        /// </summary>
        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            // テキストボックスが空のときは決定ボタン無効化
            if (inputTextBox.Text.Length == 0)
                inputButton.Enabled = false;
            else
                inputButton.Enabled = true;
        }


        /// <summary>
        /// セッション名入力時Enter
        /// </summary>
        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                inputButton.PerformClick();
        }


        /// <summary>
        /// 決定ボタン
        /// </summary>
        private void inputButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (PhotoChat.ContainsInvalidChars(inputTextBox.Text))
                {
                    MessageBox.Show(PhotoChat.InvalidCharsMessage);
                }
                else
                {
                    sessionName = inputTextBox.Text;
                    sessionID = SessionManager.CreateNewSessionID(sessionName);
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// セッションボタン
        /// </summary>
        private void SessionButton_Click(object sender, EventArgs e)
        {
            try
            {
                SessionButton sessionButton = (SessionButton)sender;
                sessionName = sessionButton.SessionName;
                sessionID = sessionButton.SessionID;
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion
    }
}