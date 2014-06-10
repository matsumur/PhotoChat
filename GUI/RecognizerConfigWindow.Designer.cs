namespace PhotoChat
{
    partial class RecognizerConfigWindow
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
            this.myTabControl = new System.Windows.Forms.TabControl();
            this.testPage = new System.Windows.Forms.TabPage();
            this.templateImagePanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.resultLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.strokeLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.addPage = new System.Windows.Forms.TabPage();
            this.addInfoLabel = new System.Windows.Forms.Label();
            this.newTemplateLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.roughButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.newStrokeLabel = new System.Windows.Forms.Label();
            this.managePage = new System.Windows.Forms.TabPage();
            this.templatePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.deleteButton = new System.Windows.Forms.Button();
            this.myTabControl.SuspendLayout();
            this.testPage.SuspendLayout();
            this.addPage.SuspendLayout();
            this.managePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // myTabControl
            // 
            this.myTabControl.Controls.Add(this.testPage);
            this.myTabControl.Controls.Add(this.addPage);
            this.myTabControl.Controls.Add(this.managePage);
            this.myTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myTabControl.Location = new System.Drawing.Point(0, 0);
            this.myTabControl.Name = "myTabControl";
            this.myTabControl.SelectedIndex = 0;
            this.myTabControl.Size = new System.Drawing.Size(475, 486);
            this.myTabControl.TabIndex = 0;
            this.myTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.myTabControl_Selected);
            // 
            // testPage
            // 
            this.testPage.BackColor = System.Drawing.Color.LightCyan;
            this.testPage.Controls.Add(this.templateImagePanel);
            this.testPage.Controls.Add(this.label4);
            this.testPage.Controls.Add(this.resultLabel);
            this.testPage.Controls.Add(this.label3);
            this.testPage.Controls.Add(this.strokeLabel);
            this.testPage.Controls.Add(this.label1);
            this.testPage.Location = new System.Drawing.Point(4, 25);
            this.testPage.Name = "testPage";
            this.testPage.Padding = new System.Windows.Forms.Padding(3);
            this.testPage.Size = new System.Drawing.Size(467, 457);
            this.testPage.TabIndex = 0;
            this.testPage.Text = "認識テスト";
            // 
            // templateImagePanel
            // 
            this.templateImagePanel.Location = new System.Drawing.Point(245, 95);
            this.templateImagePanel.Name = "templateImagePanel";
            this.templateImagePanel.Size = new System.Drawing.Size(200, 200);
            this.templateImagePanel.TabIndex = 5;
            this.templateImagePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.templateImagePanel_Paint);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(283, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "登録テンプレート";
            // 
            // resultLabel
            // 
            this.resultLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.resultLabel.Location = new System.Drawing.Point(71, 351);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(300, 50);
            this.resultLabel.TabIndex = 3;
            this.resultLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.resultLabel_Paint);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 325);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "認識結果";
            // 
            // strokeLabel
            // 
            this.strokeLabel.BackColor = System.Drawing.Color.White;
            this.strokeLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.strokeLabel.Location = new System.Drawing.Point(22, 95);
            this.strokeLabel.Name = "strokeLabel";
            this.strokeLabel.Size = new System.Drawing.Size(200, 200);
            this.strokeLabel.TabIndex = 1;
            this.strokeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StrokeLabel_MouseDown);
            this.strokeLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.StrokeLabel_MouseMove);
            this.strokeLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.StrokeLabel_Paint);
            this.strokeLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.StrokeLabel_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "試し書きして下さい";
            // 
            // addPage
            // 
            this.addPage.BackColor = System.Drawing.Color.LightCyan;
            this.addPage.Controls.Add(this.addInfoLabel);
            this.addPage.Controls.Add(this.newTemplateLabel);
            this.addPage.Controls.Add(this.label2);
            this.addPage.Controls.Add(this.addButton);
            this.addPage.Controls.Add(this.roughButton);
            this.addPage.Controls.Add(this.clearButton);
            this.addPage.Controls.Add(this.label7);
            this.addPage.Controls.Add(this.nameTextBox);
            this.addPage.Controls.Add(this.label6);
            this.addPage.Controls.Add(this.label5);
            this.addPage.Controls.Add(this.newStrokeLabel);
            this.addPage.Location = new System.Drawing.Point(4, 25);
            this.addPage.Name = "addPage";
            this.addPage.Padding = new System.Windows.Forms.Padding(3);
            this.addPage.Size = new System.Drawing.Size(467, 457);
            this.addPage.TabIndex = 1;
            this.addPage.Text = "テンプレート追加";
            // 
            // addInfoLabel
            // 
            this.addInfoLabel.Location = new System.Drawing.Point(134, 427);
            this.addInfoLabel.Name = "addInfoLabel";
            this.addInfoLabel.Size = new System.Drawing.Size(199, 22);
            this.addInfoLabel.TabIndex = 12;
            this.addInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // newTemplateLabel
            // 
            this.newTemplateLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.newTemplateLabel.Location = new System.Drawing.Point(81, 348);
            this.newTemplateLabel.Name = "newTemplateLabel";
            this.newTemplateLabel.Size = new System.Drawing.Size(300, 50);
            this.newTemplateLabel.TabIndex = 11;
            this.newTemplateLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.newTemplateLabel_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(53, 332);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "登録するテンプレート";
            // 
            // addButton
            // 
            this.addButton.BackColor = System.Drawing.Color.PowderBlue;
            this.addButton.Enabled = false;
            this.addButton.Location = new System.Drawing.Point(134, 401);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(199, 23);
            this.addButton.TabIndex = 9;
            this.addButton.Text = "このテンプレートを登録する";
            this.addButton.UseVisualStyleBackColor = false;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // roughButton
            // 
            this.roughButton.BackColor = System.Drawing.Color.PowderBlue;
            this.roughButton.Location = new System.Drawing.Point(292, 237);
            this.roughButton.Name = "roughButton";
            this.roughButton.Size = new System.Drawing.Size(108, 23);
            this.roughButton.TabIndex = 8;
            this.roughButton.Text = "下書きにする";
            this.roughButton.UseVisualStyleBackColor = false;
            this.roughButton.Click += new System.EventHandler(this.roughButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.BackColor = System.Drawing.Color.PowderBlue;
            this.clearButton.Location = new System.Drawing.Point(292, 279);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(108, 23);
            this.clearButton.TabIndex = 7;
            this.clearButton.Text = "消去";
            this.clearButton.UseVisualStyleBackColor = false;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(53, 94);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(120, 16);
            this.label7.TabIndex = 6;
            this.label7.Text = "ストロークの入力";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(81, 59);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(266, 23);
            this.nameTextBox.TabIndex = 5;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(53, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(218, 16);
            this.label6.TabIndex = 4;
            this.label6.Text = "テンプレート名（タグとして使用）";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(168, 16);
            this.label5.TabIndex = 3;
            this.label5.Text = "認識テンプレートの追加";
            // 
            // newStrokeLabel
            // 
            this.newStrokeLabel.BackColor = System.Drawing.Color.White;
            this.newStrokeLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.newStrokeLabel.Location = new System.Drawing.Point(81, 119);
            this.newStrokeLabel.Name = "newStrokeLabel";
            this.newStrokeLabel.Size = new System.Drawing.Size(200, 200);
            this.newStrokeLabel.TabIndex = 2;
            this.newStrokeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StrokeLabel_MouseDown);
            this.newStrokeLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.StrokeLabel_MouseMove);
            this.newStrokeLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.StrokeLabel_Paint);
            this.newStrokeLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.StrokeLabel_MouseUp);
            // 
            // managePage
            // 
            this.managePage.BackColor = System.Drawing.Color.LightCyan;
            this.managePage.Controls.Add(this.deleteButton);
            this.managePage.Controls.Add(this.label8);
            this.managePage.Controls.Add(this.templatePanel);
            this.managePage.Location = new System.Drawing.Point(4, 25);
            this.managePage.Name = "managePage";
            this.managePage.Padding = new System.Windows.Forms.Padding(3);
            this.managePage.Size = new System.Drawing.Size(467, 457);
            this.managePage.TabIndex = 2;
            this.managePage.Text = "テンプレート整理";
            // 
            // templatePanel
            // 
            this.templatePanel.AutoScroll = true;
            this.templatePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.templatePanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.templatePanel.Location = new System.Drawing.Point(35, 46);
            this.templatePanel.Name = "templatePanel";
            this.templatePanel.Size = new System.Drawing.Size(300, 392);
            this.templatePanel.TabIndex = 0;
            this.templatePanel.WrapContents = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(188, 16);
            this.label8.TabIndex = 1;
            this.label8.Text = "登録されているテンプレート";
            // 
            // deleteButton
            // 
            this.deleteButton.BackColor = System.Drawing.Color.PowderBlue;
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(341, 67);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(87, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "削除する";
            this.deleteButton.UseVisualStyleBackColor = false;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // RecognizerConfigWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(475, 486);
            this.Controls.Add(this.myTabControl);
            this.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ForeColor = System.Drawing.Color.Navy;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecognizerConfigWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ストローク認識の設定";
            this.myTabControl.ResumeLayout(false);
            this.testPage.ResumeLayout(false);
            this.testPage.PerformLayout();
            this.addPage.ResumeLayout(false);
            this.addPage.PerformLayout();
            this.managePage.ResumeLayout(false);
            this.managePage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl myTabControl;
        private System.Windows.Forms.TabPage testPage;
        private System.Windows.Forms.TabPage addPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label strokeLabel;
        private System.Windows.Forms.TabPage managePage;
        private System.Windows.Forms.Label resultLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label newStrokeLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button roughButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label newTemplateLabel;
        private System.Windows.Forms.Label addInfoLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel templateImagePanel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.FlowLayoutPanel templatePanel;
        private System.Windows.Forms.Button deleteButton;
    }
}