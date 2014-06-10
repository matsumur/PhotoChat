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
    /// 設定ダイアログ
    /// </summary>
    public partial class ConfigDialog : Form
    {
        private PhotoChatForm form;

        /// <summary>
        /// 設定ダイアログを作成する。
        /// </summary>
        /// <param name="form">親フォーム</param>
        public ConfigDialog(PhotoChatForm form)
        {
            this.form = form;
            InitializeComponent();

            // 現在の設定を反映
            autoScrollCheckBox.Checked = form.IsAutoScrollMode;
            logCheckBox.Checked = form.LogWindow.Visible;
            //gpsCheckBox.Checked = form.GpsReceiver.IsActive;
        }




        #region イベント

        /// <summary>
        /// オートスクロールモードの設定
        /// </summary>
        private void autoScrollCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            form.IsAutoScrollMode = autoScrollCheckBox.Checked;
        }

        /// <summary>
        /// ログウィンドウの表示・非表示
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
        /// GPS機能のON/OFF
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
        /// ストローク認識機能設定ウィンドウの表示
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