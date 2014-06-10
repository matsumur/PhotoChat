namespace PhotoChat
{
    partial class SaveImageDialog
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
            this.saveCurrentButton = new System.Windows.Forms.Button();
            this.flickrUploadAllButton = new System.Windows.Forms.Button();
            this.saveAllButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.flickrUploadCurrentButton = new System.Windows.Forms.Button();
            this.uploadServerButton = new System.Windows.Forms.Button();
            this.scExportButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // saveCurrentButton
            // 
            this.saveCurrentButton.BackColor = System.Drawing.Color.PaleTurquoise;
            this.saveCurrentButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.saveCurrentButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveCurrentButton.Location = new System.Drawing.Point(12, 77);
            this.saveCurrentButton.Name = "saveCurrentButton";
            this.saveCurrentButton.Size = new System.Drawing.Size(381, 50);
            this.saveCurrentButton.TabIndex = 1;
            this.saveCurrentButton.Text = "表示中の写真を保存する";
            this.saveCurrentButton.UseVisualStyleBackColor = false;
            this.saveCurrentButton.Click += new System.EventHandler(this.saveCurrentButton_Click);
            // 
            // flickrUploadAllButton
            // 
            this.flickrUploadAllButton.BackColor = System.Drawing.Color.PaleTurquoise;
            this.flickrUploadAllButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.flickrUploadAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.flickrUploadAllButton.Location = new System.Drawing.Point(12, 272);
            this.flickrUploadAllButton.Name = "flickrUploadAllButton";
            this.flickrUploadAllButton.Size = new System.Drawing.Size(381, 50);
            this.flickrUploadAllButton.TabIndex = 5;
            this.flickrUploadAllButton.Text = "現在のセッションの写真を\r\nFlickrにアップロードする";
            this.flickrUploadAllButton.UseVisualStyleBackColor = false;
            this.flickrUploadAllButton.Click += new System.EventHandler(this.uploadAllButton_Click);
            // 
            // saveAllButton
            // 
            this.saveAllButton.BackColor = System.Drawing.Color.PaleTurquoise;
            this.saveAllButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.saveAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveAllButton.Location = new System.Drawing.Point(12, 142);
            this.saveAllButton.Name = "saveAllButton";
            this.saveAllButton.Size = new System.Drawing.Size(381, 50);
            this.saveAllButton.TabIndex = 2;
            this.saveAllButton.Text = "現在のセッションの写真を一括保存する\r\n（HTMLファイルも同時に作成）";
            this.saveAllButton.UseVisualStyleBackColor = false;
            this.saveAllButton.Click += new System.EventHandler(this.saveAllButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.PowderBlue;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(147, 408);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(113, 33);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // flickrUploadCurrentButton
            // 
            this.flickrUploadCurrentButton.BackColor = System.Drawing.Color.PaleTurquoise;
            this.flickrUploadCurrentButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.flickrUploadCurrentButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.flickrUploadCurrentButton.Location = new System.Drawing.Point(12, 207);
            this.flickrUploadCurrentButton.Name = "flickrUploadCurrentButton";
            this.flickrUploadCurrentButton.Size = new System.Drawing.Size(381, 50);
            this.flickrUploadCurrentButton.TabIndex = 4;
            this.flickrUploadCurrentButton.Text = "表示中の写真を\r\nFlickrにアップロードする";
            this.flickrUploadCurrentButton.UseVisualStyleBackColor = false;
            this.flickrUploadCurrentButton.Click += new System.EventHandler(this.uploadCurrentButton_Click);
            // 
            // uploadServerButton
            // 
            this.uploadServerButton.BackColor = System.Drawing.Color.PaleTurquoise;
            this.uploadServerButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.uploadServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.uploadServerButton.Location = new System.Drawing.Point(12, 12);
            this.uploadServerButton.Name = "uploadServerButton";
            this.uploadServerButton.Size = new System.Drawing.Size(381, 50);
            this.uploadServerButton.TabIndex = 0;
            this.uploadServerButton.Text = "PhotoChatサーバにアップロードする";
            this.uploadServerButton.UseVisualStyleBackColor = false;
            this.uploadServerButton.Click += new System.EventHandler(this.uploadServerButton_Click);
            // 
            // scExportButton
            // 
            this.scExportButton.BackColor = System.Drawing.Color.PaleTurquoise;
            this.scExportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.scExportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scExportButton.Location = new System.Drawing.Point(12, 337);
            this.scExportButton.Name = "scExportButton";
            this.scExportButton.Size = new System.Drawing.Size(381, 50);
            this.scExportButton.TabIndex = 7;
            this.scExportButton.Text = "現在のセッションの写真を\r\nSmartCalendarにエクスポートする";
            this.scExportButton.UseVisualStyleBackColor = false;
            this.scExportButton.Click += new System.EventHandler(this.scExportButton_Click);
            // 
            // SaveImageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(405, 453);
            this.ControlBox = false;
            this.Controls.Add(this.scExportButton);
            this.Controls.Add(this.uploadServerButton);
            this.Controls.Add(this.flickrUploadCurrentButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveAllButton);
            this.Controls.Add(this.flickrUploadAllButton);
            this.Controls.Add(this.saveCurrentButton);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ForeColor = System.Drawing.Color.Navy;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveImageDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "画像の保存";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveCurrentButton;
        private System.Windows.Forms.Button flickrUploadAllButton;
        private System.Windows.Forms.Button saveAllButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button flickrUploadCurrentButton;
        private System.Windows.Forms.Button uploadServerButton;
        private System.Windows.Forms.Button scExportButton;
    }
}