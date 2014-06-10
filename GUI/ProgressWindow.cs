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
    /// �i�s�󋵕\���E�B���h�E
    /// </summary>
    public partial class ProgressWindow : Form
    {
        #region �t�B�[���h�E�v���p�e�B

        private int max = 0;
        private int count = 0;

        /// <summary>
        /// ��ƑΏۃI�u�W�F�N�g�̐����擾�E�ݒ肷��B
        /// </summary>
        public int Max
        {
            get { return max; }
            set { max = value; }
        }

        /// <summary>
        /// ��Ɠ��e�̐��������擾�E�ݒ肷��B
        /// </summary>
        public string Description
        {
            get { return descriptionLabel.Text; }
            set { descriptionLabel.Text = value; }
        }

        #endregion


        /// <summary>
        /// �i�s�󋵕\���E�B���h�E���쐬����B
        /// </summary>
        public ProgressWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// �i�s�󋵕\���E�B���h�E���쐬����B
        /// </summary>
        /// <param name="max">��ƑΏۃI�u�W�F�N�g�̐�</param>
        /// <param name="description">��Ɠ��e�̐�����</param>
        public ProgressWindow(int max, string description)
        {
            this.max = max;
            InitializeComponent();
            descriptionLabel.Text = description;
            SetCount(0);
        }


        /// <summary>
        /// �i�s�󋵕\�����X�V����B
        /// </summary>
        /// <param name="count">��Ɗ��������I�u�W�F�N�g��</param>
        public void SetCount(int count)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<int>(SetCount), count);
                    return;
                }

                this.count = count;
                progressLabel.Text = count.ToString() + "/" + max.ToString()
                    + ((float)count / max).ToString(" (##0.0%)");
                this.Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }
    }
}