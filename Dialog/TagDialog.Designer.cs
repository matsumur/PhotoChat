namespace PhotoChat
{
    partial class TagDialog
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
            this.infoLabel1 = new System.Windows.Forms.Label();
            this.attentionButton = new System.Windows.Forms.Button();
            this.goodButton = new System.Windows.Forms.Button();
            this.badButton = new System.Windows.Forms.Button();
            this.questionButton = new System.Windows.Forms.Button();
            this.iseeButton = new System.Windows.Forms.Button();
            this.interestingButton = new System.Windows.Forms.Button();
            this.infoLabel3 = new System.Windows.Forms.Label();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.inputButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.infoLabel2 = new System.Windows.Forms.Label();
            this.tagPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // infoLabel1
            // 
            this.infoLabel1.Location = new System.Drawing.Point(150, 9);
            this.infoLabel1.Name = "infoLabel1";
            this.infoLabel1.Size = new System.Drawing.Size(300, 16);
            this.infoLabel1.TabIndex = 0;
            this.infoLabel1.Text = "この写真に付けるタグを選んでください";
            this.infoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // attentionButton
            // 
            this.attentionButton.BackColor = System.Drawing.Color.Cyan;
            this.attentionButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.attentionButton.Location = new System.Drawing.Point(59, 37);
            this.attentionButton.Name = "attentionButton";
            this.attentionButton.Size = new System.Drawing.Size(100, 30);
            this.attentionButton.TabIndex = 1;
            this.attentionButton.Text = "注目";
            this.attentionButton.UseVisualStyleBackColor = false;
            this.attentionButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // goodButton
            // 
            this.goodButton.BackColor = System.Drawing.Color.Cyan;
            this.goodButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.goodButton.Location = new System.Drawing.Point(249, 37);
            this.goodButton.Name = "goodButton";
            this.goodButton.Size = new System.Drawing.Size(100, 30);
            this.goodButton.TabIndex = 2;
            this.goodButton.Text = "良い！";
            this.goodButton.UseVisualStyleBackColor = false;
            this.goodButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // badButton
            // 
            this.badButton.BackColor = System.Drawing.Color.Cyan;
            this.badButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.badButton.Location = new System.Drawing.Point(430, 37);
            this.badButton.Name = "badButton";
            this.badButton.Size = new System.Drawing.Size(100, 30);
            this.badButton.TabIndex = 3;
            this.badButton.Text = "あとで見る";
            this.badButton.UseVisualStyleBackColor = false;
            this.badButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // questionButton
            // 
            this.questionButton.BackColor = System.Drawing.Color.Cyan;
            this.questionButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.questionButton.Location = new System.Drawing.Point(59, 73);
            this.questionButton.Name = "questionButton";
            this.questionButton.Size = new System.Drawing.Size(100, 30);
            this.questionButton.TabIndex = 4;
            this.questionButton.Text = "質疑応答";
            this.questionButton.UseVisualStyleBackColor = false;
            this.questionButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // iseeButton
            // 
            this.iseeButton.BackColor = System.Drawing.Color.Cyan;
            this.iseeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.iseeButton.Location = new System.Drawing.Point(249, 73);
            this.iseeButton.Name = "iseeButton";
            this.iseeButton.Size = new System.Drawing.Size(100, 30);
            this.iseeButton.TabIndex = 5;
            this.iseeButton.Text = "ノウハウ";
            this.iseeButton.UseVisualStyleBackColor = false;
            this.iseeButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // interestingButton
            // 
            this.interestingButton.BackColor = System.Drawing.Color.Cyan;
            this.interestingButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.interestingButton.Location = new System.Drawing.Point(430, 73);
            this.interestingButton.Name = "interestingButton";
            this.interestingButton.Size = new System.Drawing.Size(100, 30);
            this.interestingButton.TabIndex = 6;
            this.interestingButton.Text = "ToDo";
            this.interestingButton.UseVisualStyleBackColor = false;
            this.interestingButton.Click += new System.EventHandler(this.TagButton_Click);
            // 
            // infoLabel3
            // 
            this.infoLabel3.AutoSize = true;
            this.infoLabel3.Location = new System.Drawing.Point(27, 116);
            this.infoLabel3.Name = "infoLabel3";
            this.infoLabel3.Size = new System.Drawing.Size(86, 16);
            this.infoLabel3.TabIndex = 15;
            this.infoLabel3.Text = "タグを入力";
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(59, 135);
            this.inputTextBox.MaxLength = 20;
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(216, 23);
            this.inputTextBox.TabIndex = 10;
            this.inputTextBox.TextChanged += new System.EventHandler(this.inputTextBox_TextChanged);
            this.inputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputTextBox_KeyDown);
            // 
            // inputButton
            // 
            this.inputButton.BackColor = System.Drawing.Color.Cyan;
            this.inputButton.Enabled = false;
            this.inputButton.Location = new System.Drawing.Point(281, 135);
            this.inputButton.Name = "inputButton";
            this.inputButton.Size = new System.Drawing.Size(53, 23);
            this.inputButton.TabIndex = 11;
            this.inputButton.Text = "決定";
            this.inputButton.UseVisualStyleBackColor = false;
            this.inputButton.Click += new System.EventHandler(this.inputButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Cyan;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(488, 294);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 30);
            this.cancelButton.TabIndex = 12;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // infoLabel2
            // 
            this.infoLabel2.AutoSize = true;
            this.infoLabel2.Location = new System.Drawing.Point(56, 161);
            this.infoLabel2.Name = "infoLabel2";
            this.infoLabel2.Size = new System.Drawing.Size(118, 16);
            this.infoLabel2.TabIndex = 16;
            this.infoLabel2.Text = "最近使ったタグ";
            // 
            // tagPanel
            // 
            this.tagPanel.Location = new System.Drawing.Point(75, 180);
            this.tagPanel.Name = "tagPanel";
            this.tagPanel.Size = new System.Drawing.Size(455, 108);
            this.tagPanel.TabIndex = 17;
            // 
            // TagDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Aquamarine;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(600, 336);
            this.ControlBox = false;
            this.Controls.Add(this.tagPanel);
            this.Controls.Add(this.infoLabel2);
            this.Controls.Add(this.inputButton);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.infoLabel3);
            this.Controls.Add(this.interestingButton);
            this.Controls.Add(this.iseeButton);
            this.Controls.Add(this.questionButton);
            this.Controls.Add(this.badButton);
            this.Controls.Add(this.goodButton);
            this.Controls.Add(this.attentionButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.infoLabel1);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ForeColor = System.Drawing.Color.Navy;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagDialog";
            this.Opacity = 0.9;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label infoLabel1;
        private System.Windows.Forms.Button attentionButton;
        private System.Windows.Forms.Button goodButton;
        private System.Windows.Forms.Button badButton;
        private System.Windows.Forms.Button questionButton;
        private System.Windows.Forms.Button iseeButton;
        private System.Windows.Forms.Button interestingButton;
        private System.Windows.Forms.Label infoLabel3;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button inputButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label infoLabel2;
        private System.Windows.Forms.FlowLayoutPanel tagPanel;
    }
}