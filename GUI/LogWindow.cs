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
    /// ���O�\���p�E�B���h�E
    /// </summary>
    public partial class LogWindow : Form
    {
        private delegate void AddLineDelegate(string text);


        /// <summary>
        /// ���O�E�B���h�E������������B
        /// </summary>
        public LogWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 1�s�������ǉ�����B
        /// </summary>
        /// <param name="text">�ǉ����镶����</param>
        public void AddLine(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    // �ʃX���b�h����̌Ăяo���̏ꍇ
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




        #region �L�[�C�x���g

        /// <summary>
        /// �L�[���͂���������e�t�H�[���̃L�[�C�x���g���Ăяo���B
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            PhotoChatClient.Instance.Form.DoKeyDown(e);
            base.OnKeyDown(e);
        }

        #endregion
    }
}