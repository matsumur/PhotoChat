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
    /// フリッカーアップロードダイアログ
    /// </summary>
    public partial class FlickrUploadDialog : Form
    {
        #region フィールド・プロパティ

        private Flickr flickr;
        private string tempFrob;

        /// <summary>
        /// Flickrインスタンスを取得する。
        /// </summary>
        public Flickr Flickr
        {
            get { return flickr; }
        }

        /// <summary>
        /// 写真の説明文を取得する。
        /// </summary>
        public string Description
        {
            get { return descriptionTextBox.Text; }
        }

        /// <summary>
        /// 全体に公開するかどうかを取得する。
        /// </summary>
        public bool IsPublic
        {
            get { return publicCheckBox.Checked; }
        }

        /// <summary>
        /// 家族に公開するかどうかを取得する。
        /// </summary>
        public bool IsFamily
        {
            get { return familyCheckBox.Checked; }
        }

        /// <summary>
        /// 友人に公開するかどうかを取得する。
        /// </summary>
        public bool IsFriend
        {
            get { return friendCheckBox.Checked; }
        }

        #endregion




        /// <summary>
        /// Flickrアップロードダイアログを作成する。
        /// </summary>
        public FlickrUploadDialog()
        {
            InitializeComponent();

            if (File.Exists(PhotoChat.FlickrTokenFile))
            {
                // Flickrトークンファイルを読んでFlickrインスタンス作成
                string token;
                using (StreamReader sr = new StreamReader(PhotoChat.FlickrTokenFile))
                {
                     token = sr.ReadLine();
                }
                flickr = new Flickr(PhotoChat.FlickrApiKey, PhotoChat.FlickrSharedSecret, token);

                // アップロード準備完了
                EnableUpload();
            }
            else
            {
                // ユーザ認証要求
                uploadLabel.Text = "先にユーザ認証を行ってください";
                authLabel.Enabled = true;
                step1Label.Enabled = true;
                startAuthButton.Enabled = true;
            }
        }


        /// <summary>
        /// アップロード関連を有効にする。
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
            authLabel.Text = "●ユーザ認証 ← 認証済み";
        }


        /// <summary>
        /// アップロードボタン
        /// </summary>
        private void uploadButton_Click(object sender, EventArgs e)
        {
            uploadButton.Enabled = false;
        }


        /// <summary>
        /// 認証開始ボタン
        /// </summary>
        private void startAuthButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Frob取得
                flickr = new Flickr(PhotoChat.FlickrApiKey, PhotoChat.FlickrSharedSecret);
                tempFrob = flickr.AuthGetFrob();
                // ブラウザで認証
                string flickrUrl = flickr.AuthCalcUrl(tempFrob, AuthLevel.Write);
                System.Diagnostics.Process.Start(flickrUrl);

                // 次のステップへ
                step1Label.Enabled = false;
                startAuthButton.Enabled = false;
                step2Label.Enabled = true;
                completeAuthButton.Enabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Flickrにアクセスできません");
                PhotoChat.WriteErrorLog(exception.ToString());
            }
        }


        /// <summary>
        /// 認証完了ボタン
        /// </summary>
        private void completeAuthButton_Click(object sender, EventArgs e)
        {
            if (flickr == null)
                flickr = new Flickr(PhotoChat.FlickrApiKey, PhotoChat.FlickrSharedSecret);

            try
            {
                // 認証取得
                Auth auth = flickr.AuthGetToken(tempFrob);
                using (StreamWriter sw = new StreamWriter(PhotoChat.FlickrTokenFile))
                {
                    // Flickrトークンを記憶
                    sw.WriteLine(auth.Token);
                    sw.Flush();
                }
                flickr.AuthToken = auth.Token;

                // アップロード準備完了
                step2Label.Enabled = false;
                completeAuthButton.Enabled = false;
                authLabel.Enabled = false;
                EnableUpload();
                uploadLabel.Text = "アップロードボタンを押して写真をFlickrにアップロード";
            }
            catch (FlickrException fe)
            {
                MessageBox.Show("ブラウザでの認証が完了していません。");
                PhotoChat.WriteErrorLog(fe.ToString());
                step1Label.Enabled = true;
                startAuthButton.Enabled = true;
                step2Label.Enabled = false;
                completeAuthButton.Enabled = false;
            }
        }
    }
}