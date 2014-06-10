namespace PhotoChat
{
    partial class ConfigDialog
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
            this.autoScrollCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.logCheckBox = new System.Windows.Forms.CheckBox();
            this.gpsCheckBox = new System.Windows.Forms.CheckBox();
            this.portComboBox = new System.Windows.Forms.ComboBox();
            this.rateComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.configRecognizerButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // autoScrollCheckBox
            // 
            this.autoScrollCheckBox.AutoSize = true;
            this.autoScrollCheckBox.Location = new System.Drawing.Point(12, 12);
            this.autoScrollCheckBox.Name = "autoScrollCheckBox";
            this.autoScrollCheckBox.Size = new System.Drawing.Size(404, 52);
            this.autoScrollCheckBox.TabIndex = 0;
            this.autoScrollCheckBox.Text = "自動スクロール：\r\n　サムネイル一覧の表示領域を新着写真があったときに\r\n　自動スクロールする。";
            this.autoScrollCheckBox.UseVisualStyleBackColor = true;
            this.autoScrollCheckBox.CheckedChanged += new System.EventHandler(this.autoScrollCheckBox_CheckedChanged);
            // 
            // okButton
            // 
            this.okButton.BackColor = System.Drawing.Color.PowderBlue;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(367, 267);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "ＯＫ";
            this.okButton.UseVisualStyleBackColor = false;
            // 
            // logCheckBox
            // 
            this.logCheckBox.AutoSize = true;
            this.logCheckBox.Location = new System.Drawing.Point(12, 80);
            this.logCheckBox.Name = "logCheckBox";
            this.logCheckBox.Size = new System.Drawing.Size(239, 20);
            this.logCheckBox.TabIndex = 2;
            this.logCheckBox.Text = "ログ表示ウィンドウを表示する。";
            this.logCheckBox.UseVisualStyleBackColor = true;
            this.logCheckBox.CheckedChanged += new System.EventHandler(this.logCheckBox_CheckedChanged);
            // 
            // gpsCheckBox
            // 
            this.gpsCheckBox.AutoSize = true;
            this.gpsCheckBox.Location = new System.Drawing.Point(12, 125);
            this.gpsCheckBox.Name = "gpsCheckBox";
            this.gpsCheckBox.Size = new System.Drawing.Size(144, 20);
            this.gpsCheckBox.TabIndex = 3;
            this.gpsCheckBox.Text = "GPSを使用する。";
            this.gpsCheckBox.UseVisualStyleBackColor = true;
            this.gpsCheckBox.CheckedChanged += new System.EventHandler(this.gpsCheckBox_CheckedChanged);
            // 
            // portComboBox
            // 
            this.portComboBox.FormattingEnabled = true;
            this.portComboBox.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "COM10",
            "COM20",
            "COM30",
            "COM40",
            "COM50"});
            this.portComboBox.Location = new System.Drawing.Point(177, 148);
            this.portComboBox.Name = "portComboBox";
            this.portComboBox.Size = new System.Drawing.Size(104, 24);
            this.portComboBox.TabIndex = 4;
            this.portComboBox.Text = "COM40";
            // 
            // rateComboBox
            // 
            this.rateComboBox.FormattingEnabled = true;
            this.rateComboBox.Items.AddRange(new object[] {
            "2400",
            "4800",
            "9600"});
            this.rateComboBox.Location = new System.Drawing.Point(177, 178);
            this.rateComboBox.Name = "rateComboBox";
            this.rateComboBox.Size = new System.Drawing.Size(104, 24);
            this.rateComboBox.TabIndex = 5;
            this.rateComboBox.Text = "4800";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "シリアルポート：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(86, 181);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "ボーレート：";
            // 
            // configRecognizerButton
            // 
            this.configRecognizerButton.BackColor = System.Drawing.Color.PowderBlue;
            this.configRecognizerButton.Location = new System.Drawing.Point(12, 233);
            this.configRecognizerButton.Name = "configRecognizerButton";
            this.configRecognizerButton.Size = new System.Drawing.Size(217, 23);
            this.configRecognizerButton.TabIndex = 8;
            this.configRecognizerButton.Text = "ストローク認識の設定とテスト";
            this.configRecognizerButton.UseVisualStyleBackColor = false;
            this.configRecognizerButton.Click += new System.EventHandler(this.configRecognizerButton_Click);
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.ClientSize = new System.Drawing.Size(479, 302);
            this.ControlBox = false;
            this.Controls.Add(this.configRecognizerButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rateComboBox);
            this.Controls.Add(this.portComboBox);
            this.Controls.Add(this.gpsCheckBox);
            this.Controls.Add(this.logCheckBox);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.autoScrollCheckBox);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "設定";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox autoScrollCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.CheckBox logCheckBox;
        private System.Windows.Forms.CheckBox gpsCheckBox;
        private System.Windows.Forms.ComboBox portComboBox;
        private System.Windows.Forms.ComboBox rateComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button configRecognizerButton;
    }
}