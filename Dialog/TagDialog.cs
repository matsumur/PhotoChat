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
    /// タグ入力ダイアログ
    /// </summary>
    public partial class TagDialog : Form
    {
        private string tagString = string.Empty;
        private PhotoChatClient client = PhotoChatClient.Instance;

        /// <summary>
        /// 入力されたタグを取得する。
        /// </summary>
        public string TagString
        {
            get { return tagString; }
        }


        /// <summary>
        /// タグ入力ダイアログを作成する。
        /// </summary>
        public TagDialog()
        {
            InitializeComponent();

            // 過去に使ったタグを表示
            tagPanel.SuspendLayout();
            foreach (string tag in client.InputTagList)
            {
                // タグごとにリンクラベルを作成して追加
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
        /// tagが固定タグかどうかを判定する。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
        /// <returns>固定タグであればtrueを返す。</returns>
        private bool CheckDefaultTag(string tag)
        {
            return (tag == attentionButton.Text) || (tag == goodButton.Text) ||
                (tag == badButton.Text) || (tag == questionButton.Text) ||
                (tag == iseeButton.Text) || (tag == interestingButton.Text);
        }




        #region イベント処理

        /// <summary>
        /// フォームアクティブ
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            inputTextBox.Focus();
            base.OnActivated(e);
        }


        /// <summary>
        /// タグボタン
        /// </summary>
        private void TagButton_Click(object sender, EventArgs e)
        {
            tagString = ((Button)sender).Text;
        }


        /// <summary>
        /// タグ入力
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
        /// タグ入力時Enter
        /// </summary>
        private void inputTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                inputButton.PerformClick();
        }


        /// <summary>
        /// 決定ボタン
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
        /// 過去に使ったタグ選択
        /// </summary>
        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            inputTextBox.Text = e.Link.LinkData as string;
        }

        #endregion
    }
}