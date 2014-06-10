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
    /// ログ表示用ウィンドウ
    /// </summary>
    public partial class LogWindow : Form
    {
        private delegate void AddLineDelegate(string text);


        /// <summary>
        /// ログウィンドウを初期化する。
        /// </summary>
        public LogWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 1行文字列を追加する。
        /// </summary>
        /// <param name="text">追加する文字列</param>
        public void AddLine(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    // 別スレッドからの呼び出しの場合
                    Invoke(new AddLineDelegate(AddLine), text);
                    return;
                }

                if (this.Visible)
                {
                    textBox.AppendText(text + Environment.NewLine);
                    Refresh();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }




        #region キーイベント

        /// <summary>
        /// キー入力があったら親フォームのキーイベントを呼び出す。
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            PhotoChatClient.Instance.Form.DoKeyDown(e);
            base.OnKeyDown(e);
        }

        #endregion
    }
}