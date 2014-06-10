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
    /// ���[�U�����̓_�C�A���O
    /// </summary>
    public partial class UserNameInputDialog : Form
    {
        /// <summary>
        /// ���[�U�����̓_�C�A���O���쐬����B
        /// </summary>
        public UserNameInputDialog()
        {
            InitializeComponent();
            if (UserName.Length == 0)
                startButton.Enabled = false;
        }


        /// <summary>
        /// ���͂��ꂽ���[�U��
        /// </summary>
        public string UserName
        {
            get { return userNameComboBox.Text; }
            set { userNameComboBox.Text = value; }
        }


        /// <summary>
        /// ���O�̃��X�g���R���{�{�b�N�X�ɒǉ�����B
        /// </summary>
        /// <param name="nameList">���O�̃��X�g</param>
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



        #region �C�x���g

        /// <summary>
        /// �R���{�{�b�N�X�̃e�L�X�g�ɕύX���������Ƃ��̏����B
        /// </summary>
        private void userNameComboBox_TextChanged(object sender, EventArgs e)
        {
            // �e�L�X�g����Ȃ�{�^���𖳌���
            if (UserName.Length == 0)
                startButton.Enabled = false;
            else
                startButton.Enabled = true;
        }


        /// <summary>
        /// �X�^�[�g�{�^��
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