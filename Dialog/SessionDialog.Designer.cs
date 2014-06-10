namespace PhotoChat
{
    partial class SessionDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.inputButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.nearbySessionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.pastSessionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(440, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "新しいセッション名を入力するかセッションを選択してください";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(115, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "新しいセッション名";
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(150, 73);
            this.inputTextBox.MaxLength = 20;
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(220, 23);
            this.inputTextBox.TabIndex = 2;
            this.inputTextBox.TextChanged += new System.EventHandler(this.inputTextBox_TextChanged);
            this.inputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputTextBox_KeyDown);
            // 
            // inputButton
            // 
            this.inputButton.BackColor = System.Drawing.Color.Aquamarine;
            this.inputButton.Enabled = false;
            this.inputButton.Location = new System.Drawing.Point(376, 73);
            this.inputButton.Name = "inputButton";
            this.inputButton.Size = new System.Drawing.Size(51, 23);
            this.inputButton.TabIndex = 3;
            this.inputButton.Text = "決定";
            this.inputButton.UseVisualStyleBackColor = false;
            this.inputButton.Click += new System.EventHandler(this.inputButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(58, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(171, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "近くのセッションを選択";
            // 
            // nearbySessionPanel
            // 
            this.nearbySessionPanel.AutoScroll = true;
            this.nearbySessionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nearbySessionPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.nearbySessionPanel.Location = new System.Drawing.Point(12, 140);
            this.nearbySessionPanel.Name = "nearbySessionPanel";
            this.nearbySessionPanel.Size = new System.Drawing.Size(260, 278);
            this.nearbySessionPanel.TabIndex = 5;
            this.nearbySessionPanel.WrapContents = false;
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Aquamarine;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(240, 424);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(95, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // pastSessionPanel
            // 
            this.pastSessionPanel.AutoScroll = true;
            this.pastSessionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pastSessionPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pastSessionPanel.Location = new System.Drawing.Point(298, 140);
            this.pastSessionPanel.Name = "pastSessionPanel";
            this.pastSessionPanel.Size = new System.Drawing.Size(260, 278);
            this.pastSessionPanel.TabIndex = 7;
            this.pastSessionPanel.WrapContents = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(341, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(178, 16);
            this.label4.TabIndex = 8;
            this.label4.Text = "過去のセッションを選択";
            // 
            // SessionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.ClientSize = new System.Drawing.Size(570, 460);
            this.ControlBox = false;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pastSessionPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.nearbySessionPanel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.inputButton);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ForeColor = System.Drawing.Color.Navy;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SessionDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "セッション選択";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button inputButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel nearbySessionPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel pastSessionPanel;
        private System.Windows.Forms.Label label4;
    }
}