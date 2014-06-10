namespace PhotoChat
{
    partial class PaletteWindow
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.colorLabel = new System.Windows.Forms.Label();
            this.strokeSizeLabel = new System.Windows.Forms.Label();
            this.fontSizeLabel = new System.Windows.Forms.Label();
            this.fontSizeComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // colorLabel
            // 
            this.colorLabel.Location = new System.Drawing.Point(10, 8);
            this.colorLabel.Name = "colorLabel";
            this.colorLabel.Size = new System.Drawing.Size(64, 211);
            this.colorLabel.TabIndex = 0;
            this.colorLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.colorLabel_MouseDown);
            // 
            // strokeSizeLabel
            // 
            this.strokeSizeLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.strokeSizeLabel.Location = new System.Drawing.Point(10, 235);
            this.strokeSizeLabel.Name = "strokeSizeLabel";
            this.strokeSizeLabel.Size = new System.Drawing.Size(64, 100);
            this.strokeSizeLabel.TabIndex = 2;
            this.strokeSizeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.strokeSizeLabel_MouseDown);
            // 
            // fontSizeLabel
            // 
            this.fontSizeLabel.AutoSize = true;
            this.fontSizeLabel.Location = new System.Drawing.Point(10, 355);
            this.fontSizeLabel.Name = "fontSizeLabel";
            this.fontSizeLabel.Size = new System.Drawing.Size(58, 12);
            this.fontSizeLabel.TabIndex = 3;
            this.fontSizeLabel.Text = "文字サイズ";
            // 
            // fontSizeComboBox
            // 
            this.fontSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontSizeComboBox.FormattingEnabled = true;
            this.fontSizeComboBox.Location = new System.Drawing.Point(10, 370);
            this.fontSizeComboBox.Name = "fontSizeComboBox";
            this.fontSizeComboBox.Size = new System.Drawing.Size(64, 20);
            this.fontSizeComboBox.TabIndex = 4;
            this.fontSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.fontSizeComboBox_SelectedIndexChanged);
            // 
            // PaletteWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.ClientSize = new System.Drawing.Size(84, 402);
            this.ControlBox = false;
            this.Controls.Add(this.fontSizeComboBox);
            this.Controls.Add(this.fontSizeLabel);
            this.Controls.Add(this.strokeSizeLabel);
            this.Controls.Add(this.colorLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(80, 100);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaletteWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "パレット";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label colorLabel;
        private System.Windows.Forms.Label strokeSizeLabel;
        private System.Windows.Forms.Label fontSizeLabel;
        private System.Windows.Forms.ComboBox fontSizeComboBox;
    }
}