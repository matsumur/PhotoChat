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
    /// �摜�ۑ��_�C�A���O
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
        /// �I�����ꂽ�摜�I�����[�h���擾����B
        /// </summary>
        public SaveImageMode SelectedSaveMode
        {
            get { return selectedSaveMode; }
        }


        /// <summary>
        /// �摜�ۑ��_�C�A���O���쐬����B
        /// </summary>
        public SaveImageDialog()
        {
            InitializeComponent();
        }


        #region �C�x���g

        /// <summary>
        /// PhotoChat�T�[�o�A�b�v���[�h�{�^��
        /// </summary>
        private void uploadServerButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.ServerUpload;
        }

        /// <summary>
        /// �\�����ʐ^�ۑ��{�^��
        /// </summary>
        private void saveCurrentButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.SaveCurrent;
        }

        /// <summary>
        /// �Z�b�V�����ʐ^�ۑ��{�^��
        /// </summary>
        private void saveAllButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.SaveAll;
        }

        /// <summary>
        /// �\�����ʐ^Flickr�A�b�v���[�h�{�^��
        /// </summary>
        private void uploadCurrentButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.FlickrUploadCurrent;
        }

        /// <summary>
        /// �Z�b�V�����ʐ^Flickr�A�b�v���[�h�{�^��
        /// </summary>
        private void uploadAllButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.FlickrUploadAll;
        }

        /// <summary>
        /// �Z�b�V�����ʐ^SmartCalendar�G�N�X�|�[�g�{�^��
        /// </summary>
        private void scExportButton_Click(object sender, EventArgs e)
        {
            selectedSaveMode = SaveImageMode.SmartCalendarExport;
        }

        #endregion
    }
}