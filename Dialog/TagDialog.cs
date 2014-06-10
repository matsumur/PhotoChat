using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// �^�O���̓_�C�A���O
    /// </summary>
    public partial class TagDialog : Form
    {
        private string tagString = string.Empty;
        private PhotoChatClient client = PhotoChatClient.Instance;

        /// <summary>
        /// ���͂��ꂽ�^�O���擾����B
        /// </summary>
        public string TagString
        {
            get { return tagString; }
        }


        /// <summary>
        /// �^�O���̓_�C�A���O���쐬����B
        /// </summary>
        public TagDialog()
        {
            InitializeComponent();

            // �ߋ��Ɏg�����^�O��\��
            tagPanel.SuspendLayout();
            foreach (string tag in client.InputTagList)
            {
                // �^�O���ƂɃ����N���x�����쐬���Ēǉ�
                LinkLabel label = new LinkLabel();
                label.AutoSize = true;
                label.LinkBehavior = LinkBehavior.NeverUnderline;
                label.Text = tag;
                label.Links.Add(0, tag.Length, tag);
                label.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
                label.ResumeLayout(false);
                tagPanel.Controls.Add(label);
            }
            tagPanel.ResumeLayout(true);
        }


        /// <summary>
        /// tag���Œ�^�O���ǂ����𔻒肷��B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        /// <returns>�Œ�^�O�ł����true��Ԃ��B</returns>
        private bool CheckDefaultTag(string tag)
        {
            return (tag == attentionButton.Text) || (tag == goodButton.Text) ||
                (tag == badButton.Text) || (tag == questionButton.Text) ||
                (tag == iseeButton.Text) || (tag == interestingButton.Text);
        }




        #region �C�x���g����

        /// <summary>
        /// �t�H�[���A�N�e�B�u
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            inputTextBox.Focus();
            base.OnActivated(e);
        }


        /// <summary>
        /// �^�O�{�^��
        /// </summary>
        private void TagButton_Click(object sender, EventArgs e)
        {
            tagString = ((Button)sender).Text;
        }


        /// <summary>
        /// �^�O����
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
        /// �^�O���͎�Enter
        /// </summary>
        private void inputTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                inputButton.PerformClick();
        }


        /// <summary>
        /// ����{�^��
        /// </summary>
        private void inputButton_Click(object sender, EventArgs e)
        {
            if (PhotoChat.ContainsInvalidChars(inputTextBox.Text))
            {
                MessageBox.Show(PhotoChat.InvalidCharsMessage);
            }
            else
            {
                tagString = inputTextBox.Text;
                client.UpdateInputTags(tagString);
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// �ߋ��Ɏg�����^�O�I��
        /// </summary>
        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            inputTextBox.Text = e.Link.LinkData as string;
        }

        #endregion
    }
}