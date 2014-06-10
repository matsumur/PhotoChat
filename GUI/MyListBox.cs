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
    /// 背景描画をしないListBox
    /// </summary>
    public partial class MyListBox : ListBox
    {
        /// <summary>
        /// MyListBoxを作成する。
        /// </summary>
        public MyListBox()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.Opaque, true);
        }


        /// <summary>
        /// 背景も含めて再描画する。
        /// </summary>
        public void RefreshAll()
        {
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(RefreshAll));
                    return;
                }

                // 一時的に背景描画を許可して再描画
                this.SetStyle(ControlStyles.Opaque, false);
                this.Refresh();
                this.SetStyle(ControlStyles.Opaque, true);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }
    }
}
