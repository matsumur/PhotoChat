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
    /// ユーザ名入力ダイアログ
    /// </summary>
    public partial class UserNameInputDialog : Form
    {
        /// <summary>
        /// ユーザ名入力ダイアログを作成する。
        /// </summary>
        public UserNameInputDialog()
        {
            InitializeComponent();
            if (UserName.Length == 0)
                startButton.Enabled = false;
        }


        /// <summary>
        /// 入力されたユーザ名
        /// </summary>
        public string UserName
        {
            get { return userNameComboBox.Text; }
            set { userNameComboBox.Text = value; }
        }


        /// <summary>
        /// 名前のリストをコンボボックスに追加する。
        /// </summary>
        /// <param name="nameList">名前のリスト</param>
        public void AddNameList(string[] nameList)
        {
            try
            {
                userNameComboBox.Items.AddRange(nameList);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }



        #region イベント

        /// <summary>
        /// コンボボックスのテキストに変更があったときの処理。
        /// </summary>
        private void userNameComboBox_TextChanged(object sender, EventArgs e)
        {
            // テキストが空ならボタンを無効化
            if (UserName.Length == 0)
                startButton.Enabled = false;
            else
                startButton.Enabled = true;
        }


        /// <summary>
        /// スタートボタン
        /// </summary>
        private void startButton_Click(object sender, EventArgs e)
        {
            if (PhotoChat.ContainsInvalidChars(userNameComboBox.Text))
            {
                MessageBox.Show(PhotoChat.InvalidCharsMessage);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        #endregion
    }
}