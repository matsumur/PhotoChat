namespace PhotoChat
{
    partial class FlickrUploadDialog
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.startAuthButton = new System.Windows.Forms.Button();
            this.completeAuthButton = new System.Windows.Forms.Button();
            this.step1Label = new System.Windows.Forms.Label();
            this.step2Label = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.uploadButton = new System.Windows.Forms.Button();
            this.uploadLabel = new System.Windows.Forms.Label();
            this.authLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.publicCheckBox = new System.Windows.Forms.CheckBox();
            this.publicLabel = new System.Windows.Forms.Label();
            this.familyCheckBox = new System.Windows.Forms.CheckBox();
            this.friendCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // startAuthButton
            // 
            this.startAuthButton.BackColor = System.Drawing.Color.Aquamarine;
            this.startAuthButton.Enabled = false;
            this.startAuthButton.Location = new System.Drawing.Point(508, 281);
            this.startAuthButton.Name = "startAuthButton";
            this.startAuthButton.Size = new System.Drawing.Size(101, 23);
            this.startAuthButton.TabIndex = 0;
            this.startAuthButton.Text = "認証開始";
            this.startAuthButton.UseVisualStyleBackColor = false;
            this.startAuthButton.Click += new System.EventHandler(this.startAuthButton_Click);
            // 
            // completeAuthButton
            // 
            this.completeAuthButton.BackColor = System.Drawing.Color.Aquamarine;
            this.completeAuthButton.Enabled = false;
            this.completeAuthButton.Location = new System.Drawing.Point(349, 314);
            this.completeAuthButton.Name = "completeAuthButton";
            this.completeAuthButton.Size = new System.Drawing.Size(101, 23);
            this.completeAuthButton.TabIndex = 1;
            this.completeAuthButton.Text = "認証完了";
            this.completeAuthButton.UseVisualStyleBackColor = false;
            this.completeAuthButton.Click += new System.EventHandler(this.completeAuthButton_Click);
            // 
            // step1Label
            // 
            this.step1Label.AutoSize = true;
            this.step1Label.Enabled = false;
            this.step1Label.Location = new System.Drawing.Point(32, 284);
            this.step1Label.Name = "step1Label";
            this.step1Label.Size = new System.Drawing.Size(470, 16);
            this.step1Label.TabIndex = 2;
            this.step1Label.Text = "１：開始ボタンを押してブラウザでPhotoChatを認証してください。";
            // 
            // step2Label
            // 
            this.step2Label.AutoSize = true;
            this.step2Label.Enabled = false;
            this.step2Label.Location = new System.Drawing.Point(32, 317);
            this.step2Label.Name = "step2Label";
            this.step2Label.Size = new System.Drawing.Size(311, 16);
            this.step2Label.TabIndex = 3;
            this.step2Label.Text = "２：認証したら完了ボタンを押してください。";
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Aquamarine;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(524, 347);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(101, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // uploadButton
            // 
            this.uploadButton.BackColor = System.Drawing.Color.Aquamarine;
            this.uploadButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.uploadButton.Enabled = false;
            this.uploadButton.Location = new System.Drawing.Point(31, 195);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(120, 23);
            this.uploadButton.TabIndex = 5;
            this.uploadButton.Text = "アップロード";
            this.uploadButton.UseVisualStyleBackColor = false;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // uploadLabel
            // 
            this.uploadLabel.AutoSize = true;
            this.uploadLabel.Location = new System.Drawing.Point(12, 9);
            this.uploadLabel.Name = "uploadLabel";
            this.uploadLabel.Size = new System.Drawing.Size(391, 16);
            this.uploadLabel.TabIndex = 6;
            this.uploadLabel.Text = "アップロードボタンを押して写真をFlickrにアップロード";
            // 
            // authLabel
            // 
            this.authLabel.AutoSize = true;
            this.authLabel.Enabled = false;
            this.authLabel.Location = new System.Drawing.Point(12, 254);
            this.authLabel.Name = "authLabel";
            this.authLabel.Size = new System.Drawing.Size(108, 16);
            this.authLabel.TabIndex = 7;
            this.authLabel.Text = "●ユーザ認証";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Enabled = false;
            this.descriptionLabel.Location = new System.Drawing.Point(32, 71);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(119, 16);
            this.descriptionLabel.TabIndex = 8;
            this.descriptionLabel.Text = "写真の説明文：";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.AcceptsReturn = true;
            this.descriptionTextBox.Enabled = false;
            this.descriptionTextBox.Location = new System.Drawing.Point(60, 90);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(520, 91);
            this.descriptionTextBox.TabIndex = 9;
            this.descriptionTextBox.Text = "PhotoChatで撮影";
            // 
            // publicCheckBox
            // 
            this.publicCheckBox.AutoSize = true;
            this.publicCheckBox.Enabled = false;
            this.publicCheckBox.Location = new System.Drawing.Point(123, 39);
            this.publicCheckBox.Name = "publicCheckBox";
            this.publicCheckBox.Size = new System.Drawing.Size(111, 20);
            this.publicCheckBox.TabIndex = 10;
            this.publicCheckBox.Text = "全体に公開";
            this.publicCheckBox.UseVisualStyleBackColor = true;
            // 
            // publicLabel
            // 
            this.publicLabel.AutoSize = true;
            this.publicLabel.Enabled = false;
            this.publicLabel.Location = new System.Drawing.Point(32, 40);
            this.publicLabel.Name = "publicLabel";
            this.publicLabel.Size = new System.Drawing.Size(85, 16);
            this.publicLabel.TabIndex = 11;
            this.publicLabel.Text = "公開設定：";
            // 
            // familyCheckBox
            // 
            this.familyCheckBox.AutoSize = true;
            this.familyCheckBox.Checked = true;
            this.familyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.familyCheckBox.Enabled = false;
            this.familyCheckBox.Location = new System.Drawing.Point(257, 39);
            this.familyCheckBox.Name = "familyCheckBox";
            this.familyCheckBox.Size = new System.Drawing.Size(111, 20);
            this.familyCheckBox.TabIndex = 12;
            this.familyCheckBox.Text = "家族に公開";
            this.familyCheckBox.UseVisualStyleBackColor = true;
            // 
            // friendCheckBox
            // 
            this.friendCheckBox.AutoSize = true;
            this.friendCheckBox.Checked = true;
            this.friendCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.friendCheckBox.Enabled = false;
            this.friendCheckBox.Location = new System.Drawing.Point(391, 39);
            this.friendCheckBox.Name = "friendCheckBox";
            this.friendCheckBox.Size = new System.Drawing.Size(111, 20);
            this.friendCheckBox.TabIndex = 13;
            this.friendCheckBox.Text = "友人に公開";
            this.friendCheckBox.UseVisualStyleBackColor = true;
            // 
            // FlickrUploadDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.ClientSize = new System.Drawing.Size(637, 382);
            this.ControlBox = false;
            this.Controls.Add(this.friendCheckBox);
            this.Controls.Add(this.familyCheckBox);
            this.Controls.Add(this.publicLabel);
            this.Controls.Add(this.publicCheckBox);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.authLabel);
            this.Controls.Add(this.uploadLabel);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.step2Label);
            this.Controls.Add(this.step1Label);
            this.Controls.Add(this.completeAuthButton);
            this.Controls.Add(this.startAuthButton);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ForeColor = System.Drawing.Color.Navy;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FlickrUploadDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Flickrにアップロード";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startAuthButton;
        private System.Windows.Forms.Button completeAuthButton;
        private System.Windows.Forms.Label step1Label;
        private System.Windows.Forms.Label step2Label;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Label uploadLabel;
        private System.Windows.Forms.Label authLabel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.CheckBox publicCheckBox;
        private System.Windows.Forms.Label publicLabel;
        private System.Windows.Forms.CheckBox familyCheckBox;
        private System.Windows.Forms.CheckBox friendCheckBox;
    }
}