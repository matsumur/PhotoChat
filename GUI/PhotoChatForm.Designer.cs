namespace PhotoChat
{
    partial class PhotoChatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PhotoChatForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.thumbnailListBox = new MyListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.toolBarPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.sessionButton = new System.Windows.Forms.Button();
            this.cameraButton = new System.Windows.Forms.Button();
            this.noteButton = new System.Windows.Forms.Button();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.leftButton = new System.Windows.Forms.Button();
            this.rightButton = new System.Windows.Forms.Button();
            this.penCheckBox = new System.Windows.Forms.CheckBox();
            this.paletteCheckBox = new System.Windows.Forms.CheckBox();
            this.eraserCheckBox = new System.Windows.Forms.CheckBox();
            this.tagButton = new System.Windows.Forms.Button();
            this.historySlider = new System.Windows.Forms.TrackBar();
            this.liveCheckBox = new System.Windows.Forms.CheckBox();
            this.configButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.connectionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.proximityStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.SuspendLayout();
            this.toolBarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historySlider)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 53);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(1);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.thumbnailListBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1305, 634);
            this.splitContainer1.SplitterDistance = 194;
            this.splitContainer1.TabIndex = 3;
            // 
            // thumbnailListBox
            // 
            this.thumbnailListBox.AllowDrop = true;
            this.thumbnailListBox.BackColor = System.Drawing.Color.AliceBlue;
            this.thumbnailListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.thumbnailListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thumbnailListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.thumbnailListBox.IntegralHeight = false;
            this.thumbnailListBox.Location = new System.Drawing.Point(0, 0);
            this.thumbnailListBox.Margin = new System.Windows.Forms.Padding(4);
            this.thumbnailListBox.Name = "thumbnailListBox";
            this.thumbnailListBox.ScrollAlwaysVisible = true;
            this.thumbnailListBox.Size = new System.Drawing.Size(194, 634);
            this.thumbnailListBox.TabIndex = 0;
            this.thumbnailListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.thumbnailListBox_DrawItem);
            this.thumbnailListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.thumbnailListBox_DragDrop);
            this.thumbnailListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.thumbnailListBox_DragEnter);
            this.thumbnailListBox.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.thumbnailListBox_QueryContinueDrag);
            this.thumbnailListBox.Layout += new System.Windows.Forms.LayoutEventHandler(this.thumbnailListBox_Layout);
            this.thumbnailListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.thumbnailListBox_MouseDown);
            this.thumbnailListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.thumbnailListBox_MouseMove);
            this.thumbnailListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.thumbnailListBox_MouseUp);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Panel1MinSize = 640;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.splitContainer2.Panel2.Click += new System.EventHandler(this.splitContainer2_Panel2_Click);
            this.splitContainer2.Panel2MinSize = 100;
            this.splitContainer2.Size = new System.Drawing.Size(1107, 634);
            this.splitContainer2.SplitterDistance = 996;
            this.splitContainer2.TabIndex = 0;
            // 
            // toolBarPanel
            // 
            this.toolBarPanel.BackColor = System.Drawing.Color.SkyBlue;
            this.toolBarPanel.Controls.Add(this.sessionButton);
            this.toolBarPanel.Controls.Add(this.cameraButton);
            this.toolBarPanel.Controls.Add(this.noteButton);
            this.toolBarPanel.Controls.Add(this.splitter2);
            this.toolBarPanel.Controls.Add(this.leftButton);
            this.toolBarPanel.Controls.Add(this.rightButton);
            this.toolBarPanel.Controls.Add(this.penCheckBox);
            this.toolBarPanel.Controls.Add(this.paletteCheckBox);
            this.toolBarPanel.Controls.Add(this.eraserCheckBox);
            this.toolBarPanel.Controls.Add(this.tagButton);
            this.toolBarPanel.Controls.Add(this.historySlider);
            this.toolBarPanel.Controls.Add(this.liveCheckBox);
            this.toolBarPanel.Controls.Add(this.configButton);
            this.toolBarPanel.Controls.Add(this.saveButton);
            this.toolBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolBarPanel.Location = new System.Drawing.Point(0, 0);
            this.toolBarPanel.Margin = new System.Windows.Forms.Padding(4);
            this.toolBarPanel.Name = "toolBarPanel";
            this.toolBarPanel.Size = new System.Drawing.Size(1305, 53);
            this.toolBarPanel.TabIndex = 7;
            // 
            // sessionButton
            // 
            this.sessionButton.BackColor = System.Drawing.Color.LightCyan;
            this.sessionButton.Image = ((System.Drawing.Image)(resources.GetObject("sessionButton.Image")));
            this.sessionButton.Location = new System.Drawing.Point(1, 1);
            this.sessionButton.Margin = new System.Windows.Forms.Padding(1);
            this.sessionButton.Name = "sessionButton";
            this.sessionButton.Size = new System.Drawing.Size(67, 51);
            this.sessionButton.TabIndex = 0;
            this.sessionButton.TabStop = false;
            this.sessionButton.UseVisualStyleBackColor = true;
            this.sessionButton.Click += new System.EventHandler(this.sessionButton_Click);
            // 
            // cameraButton
            // 
            this.cameraButton.BackColor = System.Drawing.Color.LightCyan;
            this.cameraButton.Image = ((System.Drawing.Image)(resources.GetObject("cameraButton.Image")));
            this.cameraButton.Location = new System.Drawing.Point(70, 1);
            this.cameraButton.Margin = new System.Windows.Forms.Padding(1);
            this.cameraButton.Name = "cameraButton";
            this.cameraButton.Size = new System.Drawing.Size(67, 51);
            this.cameraButton.TabIndex = 1;
            this.cameraButton.TabStop = false;
            this.cameraButton.UseVisualStyleBackColor = true;
            this.cameraButton.Click += new System.EventHandler(this.cameraButton_Click);
            // 
            // noteButton
            // 
            this.noteButton.BackColor = System.Drawing.Color.LightCyan;
            this.noteButton.Image = ((System.Drawing.Image)(resources.GetObject("noteButton.Image")));
            this.noteButton.Location = new System.Drawing.Point(139, 1);
            this.noteButton.Margin = new System.Windows.Forms.Padding(1);
            this.noteButton.Name = "noteButton";
            this.noteButton.Size = new System.Drawing.Size(67, 51);
            this.noteButton.TabIndex = 2;
            this.noteButton.TabStop = false;
            this.noteButton.UseVisualStyleBackColor = true;
            this.noteButton.Click += new System.EventHandler(this.noteButton_Click);
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(211, 4);
            this.splitter2.Margin = new System.Windows.Forms.Padding(4);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(4, 45);
            this.splitter2.TabIndex = 7;
            this.splitter2.TabStop = false;
            // 
            // leftButton
            // 
            this.leftButton.BackColor = System.Drawing.Color.LightCyan;
            this.leftButton.Image = ((System.Drawing.Image)(resources.GetObject("leftButton.Image")));
            this.leftButton.Location = new System.Drawing.Point(220, 1);
            this.leftButton.Margin = new System.Windows.Forms.Padding(1);
            this.leftButton.Name = "leftButton";
            this.leftButton.Size = new System.Drawing.Size(56, 51);
            this.leftButton.TabIndex = 3;
            this.leftButton.TabStop = false;
            this.leftButton.UseVisualStyleBackColor = true;
            this.leftButton.Click += new System.EventHandler(this.leftButton_Click);
            // 
            // rightButton
            // 
            this.rightButton.BackColor = System.Drawing.Color.LightCyan;
            this.rightButton.Image = ((System.Drawing.Image)(resources.GetObject("rightButton.Image")));
            this.rightButton.Location = new System.Drawing.Point(278, 1);
            this.rightButton.Margin = new System.Windows.Forms.Padding(1);
            this.rightButton.Name = "rightButton";
            this.rightButton.Size = new System.Drawing.Size(56, 51);
            this.rightButton.TabIndex = 4;
            this.rightButton.TabStop = false;
            this.rightButton.UseVisualStyleBackColor = true;
            this.rightButton.Click += new System.EventHandler(this.rightButton_Click);
            // 
            // penCheckBox
            // 
            this.penCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.penCheckBox.AutoCheck = false;
            this.penCheckBox.BackColor = System.Drawing.Color.LightCyan;
            this.penCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("penCheckBox.Image")));
            this.penCheckBox.Location = new System.Drawing.Point(336, 1);
            this.penCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.penCheckBox.Name = "penCheckBox";
            this.penCheckBox.Size = new System.Drawing.Size(67, 51);
            this.penCheckBox.TabIndex = 5;
            this.penCheckBox.TabStop = false;
            this.penCheckBox.UseVisualStyleBackColor = true;
            this.penCheckBox.Click += new System.EventHandler(this.penCheckBox_Click);
            // 
            // paletteCheckBox
            // 
            this.paletteCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.paletteCheckBox.BackColor = System.Drawing.Color.LightCyan;
            this.paletteCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("paletteCheckBox.Image")));
            this.paletteCheckBox.Location = new System.Drawing.Point(405, 1);
            this.paletteCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.paletteCheckBox.Name = "paletteCheckBox";
            this.paletteCheckBox.Size = new System.Drawing.Size(67, 51);
            this.paletteCheckBox.TabIndex = 6;
            this.paletteCheckBox.TabStop = false;
            this.paletteCheckBox.UseVisualStyleBackColor = true;
            this.paletteCheckBox.Click += new System.EventHandler(this.paletteCheckBox_Click);
            // 
            // eraserCheckBox
            // 
            this.eraserCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.eraserCheckBox.AutoCheck = false;
            this.eraserCheckBox.BackColor = System.Drawing.Color.LightCyan;
            this.eraserCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("eraserCheckBox.Image")));
            this.eraserCheckBox.Location = new System.Drawing.Point(474, 1);
            this.eraserCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.eraserCheckBox.Name = "eraserCheckBox";
            this.eraserCheckBox.Size = new System.Drawing.Size(67, 51);
            this.eraserCheckBox.TabIndex = 7;
            this.eraserCheckBox.TabStop = false;
            this.eraserCheckBox.UseVisualStyleBackColor = true;
            this.eraserCheckBox.Click += new System.EventHandler(this.eraserCheckBox_Click);
            // 
            // tagButton
            // 
            this.tagButton.BackColor = System.Drawing.Color.LightCyan;
            this.tagButton.Image = ((System.Drawing.Image)(resources.GetObject("tagButton.Image")));
            this.tagButton.Location = new System.Drawing.Point(543, 1);
            this.tagButton.Margin = new System.Windows.Forms.Padding(1);
            this.tagButton.Name = "tagButton";
            this.tagButton.Size = new System.Drawing.Size(67, 51);
            this.tagButton.TabIndex = 8;
            this.tagButton.TabStop = false;
            this.tagButton.UseVisualStyleBackColor = true;
            this.tagButton.Click += new System.EventHandler(this.tagButton_Click);
            // 
            // historySlider
            // 
            this.historySlider.AutoSize = false;
            this.historySlider.BackColor = System.Drawing.Color.SkyBlue;
            this.historySlider.LargeChange = 10;
            this.historySlider.Location = new System.Drawing.Point(612, 1);
            this.historySlider.Margin = new System.Windows.Forms.Padding(1);
            this.historySlider.Maximum = 100;
            this.historySlider.Name = "historySlider";
            this.historySlider.Size = new System.Drawing.Size(139, 51);
            this.historySlider.SmallChange = 2;
            this.historySlider.TabIndex = 9;
            this.historySlider.TabStop = false;
            this.historySlider.TickFrequency = 10;
            this.historySlider.Value = 100;
            this.historySlider.Scroll += new System.EventHandler(this.historySlider_Scroll);
            // 
            // liveCheckBox
            // 
            this.liveCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.liveCheckBox.BackColor = System.Drawing.Color.LightCyan;
            this.liveCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("liveCheckBox.Image")));
            this.liveCheckBox.Location = new System.Drawing.Point(753, 1);
            this.liveCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.liveCheckBox.Name = "liveCheckBox";
            this.liveCheckBox.Size = new System.Drawing.Size(60, 51);
            this.liveCheckBox.TabIndex = 10;
            this.liveCheckBox.TabStop = false;
            this.liveCheckBox.UseVisualStyleBackColor = true;
            this.liveCheckBox.Click += new System.EventHandler(this.liveCheckBox_Click);
            // 
            // configButton
            // 
            this.configButton.BackColor = System.Drawing.Color.LightCyan;
            this.configButton.Image = ((System.Drawing.Image)(resources.GetObject("configButton.Image")));
            this.configButton.Location = new System.Drawing.Point(815, 1);
            this.configButton.Margin = new System.Windows.Forms.Padding(1);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(60, 51);
            this.configButton.TabIndex = 11;
            this.configButton.TabStop = false;
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.configButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.LightCyan;
            this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
            this.saveButton.Location = new System.Drawing.Point(877, 1);
            this.saveButton.Margin = new System.Windows.Forms.Padding(1);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(60, 51);
            this.saveButton.TabIndex = 12;
            this.saveButton.TabStop = false;
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionStatusLabel,
            this.proximityStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 687);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1305, 22);
            this.statusStrip1.TabIndex = 1;
            // 
            // connectionStatusLabel
            // 
            this.connectionStatusLabel.AutoSize = false;
            this.connectionStatusLabel.Name = "connectionStatusLabel";
            this.connectionStatusLabel.Size = new System.Drawing.Size(400, 17);
            this.connectionStatusLabel.Text = "接続中の相手： 0人";
            this.connectionStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // proximityStatusLabel
            // 
            this.proximityStatusLabel.AutoSize = false;
            this.proximityStatusLabel.Name = "proximityStatusLabel";
            this.proximityStatusLabel.Size = new System.Drawing.Size(885, 17);
            this.proximityStatusLabel.Spring = true;
            this.proximityStatusLabel.Text = "近くにいる人： 0人";
            this.proximityStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PhotoChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1305, 709);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolBarPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PhotoChatForm";
            this.Text = "PhotoChat";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolBarPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.historySlider)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private MyListBox thumbnailListBox;
        private System.Windows.Forms.FlowLayoutPanel toolBarPanel;
        private System.Windows.Forms.Button sessionButton;
        private System.Windows.Forms.Button cameraButton;
        private System.Windows.Forms.Button noteButton;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Button leftButton;
        private System.Windows.Forms.Button rightButton;
        private System.Windows.Forms.CheckBox penCheckBox;
        private System.Windows.Forms.CheckBox paletteCheckBox;
        private System.Windows.Forms.CheckBox eraserCheckBox;
        private System.Windows.Forms.Button tagButton;
        private System.Windows.Forms.TrackBar historySlider;
        private System.Windows.Forms.CheckBox liveCheckBox;
        private System.Windows.Forms.Button configButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel connectionStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel proximityStatusLabel;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}

