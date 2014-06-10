using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PhotoChat
{
    /// <summary>
    /// 進行状況表示ウィンドウ
    /// </summary>
    public partial class ProgressWindow : Form
    {
        #region フィールド・プロパティ

        private int max = 0;
        private int count = 0;

        /// <summary>
        /// 作業対象オブジェクトの数を取得・設定する。
        /// </summary>
        public int Max
        {
            get { return max; }
            set { max = value; }
        }

        /// <summary>
        /// 作業内容の説明文を取得・設定する。
        /// </summary>
        public string Description
        {
            get { return descriptionLabel.Text; }
            set { descriptionLabel.Text = value; }
        }

        #endregion


        /// <summary>
        /// 進行状況表示ウィンドウを作成する。
        /// </summary>
        public ProgressWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 進行状況表示ウィンドウを作成する。
        /// </summary>
        /// <param name="max">作業対象オブジェクトの数</param>
        /// <param name="description">作業内容の説明文</param>
        public ProgressWindow(int max, string description)
        {
            this.max = max;
            InitializeComponent();
            descriptionLabel.Text = description;
            SetCount(0);
        }


        /// <summary>
        /// 進行状況表示を更新する。
        /// </summary>
        /// <param name="count">作業完了したオブジェクト数</param>
        public void SetCount(int count)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<int>(SetCount), count);
                    return;
                }

                this.count = count;
                progressLabel.Text = count.ToString() + "/" + max.ToString()
                    + ((float)count / max).ToString(" (##0.0%)");
                this.Refresh();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }
    }
}