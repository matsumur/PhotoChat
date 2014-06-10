using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FlickrNet;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// �t���b�J�[�A�b�v���[�h�_�C�A���O
    /// </summary>
    public partial class FlickrUploadDialog : Form
    {
        #region �t�B�[���h�E�v���p�e�B

        private Flickr flickr;
        private string tempFrob;

        /// <summary>
        /// Flickr�C���X�^���X���擾����B
        /// </summary>
        public Flickr Flickr
        {
            get { return flickr; }
        }

        /// <summary>
        /// �ʐ^�̐��������擾����B
        /// </summary>
        public string Description
        {
            get { return descriptionTextBox.Text; }
        }

        /// <summary>
        /// �S�̂Ɍ��J���邩�ǂ������擾����B
        /// </summary>
        public bool IsPublic
        {
            get { return publicCheckBox.Checked; }
        }

        /// <summary>
        /// �Ƒ��Ɍ��J���邩�ǂ������擾����B
        /// </summary>
        public bool IsFamily
        {
            get { return familyCheckBox.Checked; }
        }

        /// <summary>
        /// �F�l�Ɍ��J���邩�ǂ������擾����B
        /// </summary>
        public bool IsFriend
        {
            get { return friendCheckBox.Checked; }
        }

        #endregion




        /// <summary>
        /// Flickr�A�b�v���[�h�_�C�A���O���쐬����B
        /// </summary>
        public FlickrUploadDialog()
        {
            InitializeComponent();

            if (File.Exists(PhotoChat.FlickrTokenFile))
            {
                // Flickr�g�[�N���t�@�C����ǂ��Flickr�C���X�^���X�쐬
                string token;
                using (StreamReader sr = new StreamReader(PhotoChat.FlickrTokenFile))
                {
                     token = sr.ReadLine();
                }
                flickr = new Flickr(PhotoChat.FlickrApiKey, PhotoChat.FlickrSharedSecret, token);

                // �A�b�v���[�h��������
                EnableUpload();
            }
            else
            {
                // ���[�U�F�ؗv��
                uploadLabel.Text = "��Ƀ��[�U�F�؂��s���Ă�������";
                authLabel.Enabled = true;
                step1Label.Enabled = true;
                startAuthButton.Enabled = true;
            }
        }


        /// <summary>
        /// �A�b�v���[�h�֘A��L���ɂ���B
        /// </summary>
        private void EnableUpload()
        {
            publicLabel.Enabled = true;
            publicCheckBox.Enabled = true;
            familyCheckBox.Enabled = true;
            friendCheckBox.Enabled = true;
            descriptionLabel.Enabled = true;
            descriptionTextBox.Enabled = true;
            uploadButton.Enabled = true;
            authLabel.Text = "�����[�U�F�� �� �F�؍ς�";
        }


        /// <summary>
        /// �A�b�v���[�h�{�^��
        /// </summary>
        private void uploadButton_Click(object sender, EventArgs e)
        {
            uploadButton.Enabled = false;
        }


        /// <summary>
        /// �F�؊J�n�{�^��
        /// </summary>
        private void startAuthButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Frob�擾
                flickr = new Flickr(PhotoChat.FlickrApiKey, PhotoChat.FlickrSharedSecret);
                tempFrob = flickr.AuthGetFrob();
                // �u���E�U�ŔF��
                string flickrUrl = flickr.AuthCalcUrl(tempFrob, AuthLevel.Write);
                System.Diagnostics.Process.Start(flickrUrl);

                // ���̃X�e�b�v��
                step1Label.Enabled = false;
                startAuthButton.Enabled = false;
                step2Label.Enabled = true;
                completeAuthButton.Enabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Flickr�ɃA�N�Z�X�ł��܂���");
                PhotoChat.WriteErrorLog(exception.ToString());
            }
        }


        /// <summary>
        /// �F�؊����{�^��
        /// </summary>
        private void completeAuthButton_Click(object sender, EventArgs e)
        {
            if (flickr == null)
                flickr = new Flickr(PhotoChat.FlickrApiKey, PhotoChat.FlickrSharedSecret);

            try
            {
                // �F�؎擾
                Auth auth = flickr.AuthGetToken(tempFrob);
                using (StreamWriter sw = new StreamWriter(PhotoChat.FlickrTokenFile))
                {
                    // Flickr�g�[�N�����L��
                    sw.WriteLine(auth.Token);
                    sw.Flush();
                }
                flickr.AuthToken = auth.Token;

                // �A�b�v���[�h��������
                step2Label.Enabled = false;
                completeAuthButton.Enabled = false;
                authLabel.Enabled = false;
                EnableUpload();
                uploadLabel.Text = "�A�b�v���[�h�{�^���������Ďʐ^��Flickr�ɃA�b�v���[�h";
            }
            catch (FlickrException fe)
            {
                MessageBox.Show("�u���E�U�ł̔F�؂��������Ă��܂���B");
                PhotoChat.WriteErrorLog(fe.ToString());
                step1Label.Enabled = true;
                startAuthButton.Enabled = true;
                step2Label.Enabled = false;
                completeAuthButton.Enabled = false;
            }
        }
    }
}