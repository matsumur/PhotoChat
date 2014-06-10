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
    /// �Z�b�V�����I���_�C�A���O
    /// </summary>
    public partial class SessionDialog : Form
    {
        #region �t�B�[���h�E�v���p�e�B

        private string sessionName;
        private string sessionID;

        /// <summary>
        /// ���͂��ꂽ�Z�b�V���������擾����B
        /// </summary>
        public string SessionName
        {
            get { return sessionName; }
        }

        /// <summary>
        /// �I�����ꂽ�Z�b�V������ID���擾����B
        /// </summary>
        public string SessionID
        {
            get { return sessionID; }
        }

        /// <summary>
        /// �L�����Z���{�^�����L�����ǂ������擾�E�ݒ肷��B
        /// </summary>
        public bool CancelButtonEnabled
        {
            get { return cancelButton.Enabled; }
            set { cancelButton.Enabled = value; }
        }

        #endregion




        /// <summary>
        /// �Z�b�V�����I���_�C�A���O���쐬����B
        /// </summary>
        /// <param name="firstTime"></param>
        public SessionDialog()
        {
            InitializeComponent();
        }




        #region �Z�b�V�����I���{�^��

        /// <summary>
        /// �߂��̃Z�b�V�����I���{�^����ǉ�����B
        /// </summary>
        /// <param name="sessionName">�Z�b�V������</param>
        /// <param name="sessionID">�Z�b�V����ID</param>
        public void AddNearbyButton(string sessionName, string sessionID)
        {
            try
            {
                lock (nearbySessionPanel)
                {
                    // ���łɒǉ�����Ă��Ȃ������ׂ�
                    foreach (Control control in nearbySessionPanel.Controls)
                    {
                        if (sessionID == ((SessionButton)control).SessionID)
                            return;
                    }

                    // �{�^���쐬
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
        /// �߂��̃Z�b�V�����I���G���A�Ƀ{�^����ǉ�����B
        /// </summary>
        /// <param name="button">�ǉ�����{�^��</param>
        private void AddNearbyButton(SessionButton button)
        {
            try
            {
                if (InvokeRequired)
                {
                    // �ʃX���b�h����̌Ăяo���̏ꍇ
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
        /// �Z�b�V�����I���{�^����ǉ�����B
        /// </summary>
        /// <param name="sessionName">�Z�b�V������</param>
        /// <param name="sessionID">�Z�b�V����ID</param>
        /// <param name="info">�{�^��������̐擪�ɂ��镶����</param>
        public void AddPastButton(string sessionName, string sessionID, string info)
        {
            try
            {
                lock (pastSessionPanel)
                {
                    // ���łɒǉ�����Ă��Ȃ������ׂ�
                    foreach (Control control in pastSessionPanel.Controls)
                    {
                        if (sessionID == ((SessionButton)control).SessionID)
                            return;
                    }

                    // �{�^���쐬
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
        /// �ߋ��̃Z�b�V�����I���G���A�Ƀ{�^����ǉ�����B
        /// </summary>
        /// <param name="button">�ǉ�����{�^��</param>
        private void AddPastButton(SessionButton button)
        {
            try
            {
                if (InvokeRequired)
                {
                    // �ʃX���b�h����̌Ăяo���̏ꍇ
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
        /// �Z�b�V�����I���{�^��
        /// </summary>
        public class SessionButton : Button
        {
            private string sessionName;
            private string sessionID;

            /// <summary>
            /// �Z�b�V�����I���{�^�����쐬����B
            /// </summary>
            public SessionButton()
            {
                this.DialogResult = DialogResult.OK;
                this.BackColor = Color.Aquamarine;
                this.TextAlign = ContentAlignment.MiddleCenter;
            }

            /// <summary>
            /// ���̃{�^���̃Z�b�V���������擾����B
            /// </summary>
            public string SessionName
            {
                get { return sessionName; }
                set { sessionName = value; }
            }

            /// <summary>
            /// ���̃{�^���̃Z�b�V����ID���擾����B
            /// </summary>
            public string SessionID
            {
                get { return sessionID; }
                set { sessionID = value; }
            }
        }

        #endregion




        #region �C�x���g

        /// <summary>
        /// �t�H�[���A�N�e�B�u
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            inputTextBox.Focus();
            base.OnActivated(e);
        }


        /// <summary>
        /// �Z�b�V����������
        /// </summary>
        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            // �e�L�X�g�{�b�N�X����̂Ƃ��͌���{�^��������
            if (inputTextBox.Text.Length == 0)
                inputButton.Enabled = false;
            else
                inputButton.Enabled = true;
        }


        /// <summary>
        /// �Z�b�V���������͎�Enter
        /// </summary>
        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                inputButton.PerformClick();
        }


        /// <summary>
        /// ����{�^��
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
        /// �Z�b�V�����{�^��
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