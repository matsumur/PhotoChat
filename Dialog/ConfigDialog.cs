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
    /// �ݒ�_�C�A���O
    /// </summary>
    public partial class ConfigDialog : Form
    {
        private PhotoChatForm form;

        /// <summary>
        /// �ݒ�_�C�A���O���쐬����B
        /// </summary>
        /// <param name="form">�e�t�H�[��</param>
        public ConfigDialog(PhotoChatForm form)
        {
            this.form = form;
            InitializeComponent();

            // ���݂̐ݒ�𔽉f
            autoScrollCheckBox.Checked = form.IsAutoScrollMode;
            logCheckBox.Checked = form.LogWindow.Visible;
            //gpsCheckBox.Checked = form.GpsReceiver.IsActive;
        }




        #region �C�x���g

        /// <summary>
        /// �I�[�g�X�N���[�����[�h�̐ݒ�
        /// </summary>
        private void autoScrollCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            form.IsAutoScrollMode = autoScrollCheckBox.Checked;
        }

        /// <summary>
        /// ���O�E�B���h�E�̕\���E��\��
        /// </summary>
        private void logCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (logCheckBox.Checked)
                    form.LogWindow.Show();
                else
                    form.LogWindow.Hide();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// GPS�@�\��ON/OFF
        /// </summary>
        private void gpsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                /*
                if (gpsCheckBox.Checked)
                    form.GpsReceiver.Start(portComboBox.Text, int.Parse(rateComboBox.Text));
                else
                    form.GpsReceiver.Close();
                 */ 
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// �X�g���[�N�F���@�\�ݒ�E�B���h�E�̕\��
        /// </summary>
        private void configRecognizerButton_Click(object sender, EventArgs e)
        {
            try
            {
                RecognizerConfigWindow recognizerWindow =
                    new RecognizerConfigWindow(form.Client.StrokeRecognizer);
                recognizerWindow.Show(form);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion
    }
}