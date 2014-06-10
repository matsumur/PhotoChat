namespace PhotoChat
{
    partial class PhotoListWindow
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
            this.photoListBox = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tagPanel = new System.Windows.Forms.Panel();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // photoListBox
            // 
            this.photoListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.photoListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.photoListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.photoListBox.FormattingEnabled = true;
            this.photoListBox.IntegralHeight = false;
            this.photoListBox.Location = new System.Drawing.Point(0, 0);
            this.photoListBox.MultiColumn = true;
            this.photoListBox.Name = "photoListBox";
            this.photoListBox.ScrollAlwaysVisible = true;
            this.photoListBox.Size = new System.Drawing.Size(600, 498);
            this.photoListBox.TabIndex = 0;
            this.photoListBox.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.photoListBox_QueryContinueDrag);
            this.photoListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.photoListBox_DrawItem);
            this.photoListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.photoListBox_MouseUp);
            this.photoListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.photoListBox_MouseMove);
            this.photoListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.photoListBox_MouseDown);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.photoListBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tagPanel);
            this.splitContainer1.Size = new System.Drawing.Size(694, 498);
            this.splitContainer1.SplitterDistance = 600;
            this.splitContainer1.TabIndex = 2;
            // 
            // tagPanel
            // 
            this.tagPanel.BackColor = System.Drawing.Color.LightCyan;
            this.tagPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagPanel.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tagPanel.ForeColor = System.Drawing.Color.Navy;
            this.tagPanel.Location = new System.Drawing.Point(0, 0);
            this.tagPanel.Name = "tagPanel";
            this.tagPanel.Size = new System.Drawing.Size(90, 498);
            this.tagPanel.TabIndex = 0;
            // 
            // PhotoListWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.ClientSize = new System.Drawing.Size(694, 498);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(50, 20);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PhotoListWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "写真一覧ウィンドウ";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox photoListBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel tagPanel;
    }
}