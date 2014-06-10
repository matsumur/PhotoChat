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
    /// 画像保存ダイアログ
    /// </summary>
    public partial class SaveImageDialog : Form
    {
        public enum SaveImageMode
        {
            ServerUpload, SaveCurrent, SaveAll, FlickrUploadCurrent,
            FlickrUploadAll, SmartCalendarExport
        };
        private SaveImageMode selectedSaveMode;

        /// <summary>
        /// 選択された画像選択モードを取得する。
        /// </summary>
        public SaveImageMode SelectedSaveMode
        {
            get { return selectedSaveMode; }
        }


        /// <summary>
        /// 画像保存ダイアログを作成する。
        /// </summary>
        public SaveImageDialog()
        {
            InitializeComponent();
        }


        #region イベント

        /// <summary>
        /// PhotoChatサーバアップロードボタン
        /// </summary>
        private void uploadServerButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.ServerUpload;
        }

        /// <summary>
        /// 表示中写真保存ボタン
        /// </summary>
        private void saveCurrentButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.SaveCurrent;
        }

        /// <summary>
        /// セッション写真保存ボタン
        /// </summary>
        private void saveAllButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.SaveAll;
        }

        /// <summary>
        /// 表示中写真Flickrアップロードボタン
        /// </summary>
        private void uploadCurrentButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.FlickrUploadCurrent;
        }

        /// <summary>
        /// セッション写真Flickrアップロードボタン
        /// </summary>
        private void uploadAllButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.FlickrUploadAll;
        }

        /// <summary>
        /// セッション写真SmartCalendarエクスポートボタン
        /// </summary>
        private void scExportButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.SmartCalendarExport;
        }

        #endregion
    }
}