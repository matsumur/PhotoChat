using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// 写真一覧ウィンドウ
    /// </summary>
    public partial class PhotoListWindow : Form
    {
        private const int TagHeight = 20;
        private PhotoChatForm form;
        private int selectedIndex = -1;
        private Bitmap selectedImage = null;
        private Point selectedPoint = Point.Empty;
        private Point mouseDownPoint = Point.Empty;
        private Point screenOffset = Point.Empty;
        private Rectangle showBounds;
        private Rectangle hideBounds;
        private bool hiding = false;


        /// <summary>
        /// 写真一覧ウィンドウを作成する。
        /// </summary>
        public PhotoListWindow(PhotoChatForm form)
        {
            this.form = form;
            InitializeComponent();
            showBounds = this.Bounds;
            hideBounds = new Rectangle(form.Right - 40, form.Bottom - 35, 10, 20);
        }


        /// <summary>
        /// 指定したタグの付いたサムネイルの一覧を表示する。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
        /// <param name="itemWidth">項目の幅</param>
        /// <param name="itemHeight">項目の高さ</param>
        public void SetPhotoList(string tag, int itemWidth, int itemHeight)
        {
            try
            {
                // サムネイルリスト作成
                //string[] photoArray = PhotoChatClient.SearchTaggedPhoto(tag);
                string[] photoArray = PhotoChatClient.SearchTaggedPhoto(
                    new string[] { tag, form.Client.CurrentSessionName }, true);
                List<Thumbnail> thumbnailList = new List<Thumbnail>(photoArray.Length);
                foreach (string photoName in photoArray)
                {
                    Thumbnail thumbnail = form.GetThumbnail(photoName);
                    if (thumbnail != null)
                        thumbnailList.Add(thumbnail);
                }
                thumbnailList.Sort();

                // 一覧への挿入
                photoListBox.BeginUpdate();
                selectedIndex = -1;
                if (selectedImage != null)
                {
                    selectedImage.Dispose();
                    selectedImage = null;
                }
                ShowUp();
                photoListBox.Items.Clear();
                this.Text = "タグ： " + tag;
                photoListBox.Focus();
                foreach (Thumbnail thumbnail in thumbnailList)
                    photoListBox.Items.Add(thumbnail);
                photoListBox.ColumnWidth = itemWidth;
                photoListBox.ItemHeight = itemHeight;
                photoListBox.EndUpdate();

                // 現在のセッションで使用されたタグ一覧の作成
                SetTagPanel();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 現在のセッションで使用されたタグ一覧を作成する。
        /// </summary>
        private void SetTagPanel()
        {
            tagPanel.SuspendLayout();
            tagPanel.Controls.Clear();
            int tagCount = 0;
            foreach (string tag in form.Client.SessionTagList)
            {
                // タグごとにリンクラベルを作成して追加
                LinkLabel label = new LinkLabel();
                label.Bounds = new Rectangle(0, TagHeight * tagCount, tagPanel.Width, TagHeight);
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.LinkBehavior = LinkBehavior.NeverUnderline;
                label.Text = tag;
                label.Links.Add(0, tag.Length, tag);
                label.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
                label.ResumeLayout(false);
                tagPanel.Controls.Add(label);
                tagCount++;
            }
            tagPanel.ResumeLayout(false);
        }




        #region ウィンドウ移動

        /// <summary>
        /// ウィンドウを上に移動する。
        /// </summary>
        public void ShowUp()
        {
            if (hiding)
            {
                hideBounds = this.Bounds;
                this.Bounds = showBounds;
                hiding = false;
            }
        }


        /// <summary>
        /// ウィンドウを下に移動する。
        /// </summary>
        public void HideDown()
        {
            if (!hiding)
            {
                showBounds = this.Bounds;
                this.Bounds = hideBounds;
                hiding = true;
                form.Activate();
            }
        }

        #endregion




        #region イベント

        /// <summary>
        /// キーイベント
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    break;

                default:
                    form.DoKeyDown(e);
                    break;
            }
            base.OnKeyDown(e);
        }


        /// <summary>
        /// ウィンドウ復帰。
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            ShowUp();
            base.OnActivated(e);
        }


        /// <summary>
        /// ウィンドウを隠す。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivate(EventArgs e)
        {
            HideDown();
            base.OnDeactivate(e);
        }


        /// <summary>
        /// 項目を描画する。
        /// </summary>
        private void photoListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index < 0) return;

                // 描画
                ((Thumbnail)photoListBox.Items[e.Index]).Paint(e.Graphics, e.Bounds);
                e.DrawFocusRectangle();

                // 選択されたサムネイルのポップアップ描画
                if (e.Index == selectedIndex && selectedImage != null)
                {
                    e.Graphics.DrawImage(selectedImage, selectedPoint);
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void photoListBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // サムネイル項目の上で押されたときのみ処理
                if (photoListBox.IndexFromPoint(e.Location) >= 0)
                {
                    // ボタンが押された位置を記憶
                    mouseDownPoint = e.Location;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void photoListBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // ドラッグが開始されていなければ写真選択
                if (mouseDownPoint != Point.Empty)
                {
                    // 選択中の写真をクリックしたのであれば写真表示
                    if (photoListBox.IndexFromPoint(mouseDownPoint) == selectedIndex)
                        form.ShowPhoto(((Thumbnail)photoListBox.SelectedItem).PhotoName);
                    else
                    {
                        // 選択されたサムネイルの画像と表示位置を取得
                        selectedIndex = photoListBox.IndexFromPoint(mouseDownPoint);
                        string photoName = ((Thumbnail)photoListBox.SelectedItem).PhotoName;
                        selectedImage = Thumbnail.GetImage(
                            photoName, PhotoChat.ThumbnailWidth, PhotoChat.ThumbnailHeight);
                        Rectangle rect = photoListBox.GetItemRectangle(photoListBox.SelectedIndex);
                        selectedPoint = new Point(rect.Right, rect.Top);
                        photoListBox.Refresh();
                    }
                }

                // リセット
                mouseDownPoint = Point.Empty;
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void photoListBox_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // マウスボタンが押されているときのみ処理
                if (mouseDownPoint != Point.Empty)
                {
                    // マウスの移動が一定範囲を超えていないか調べる
                    if (e.X < mouseDownPoint.X - PhotoChat.HyperlinkDragSize
                        || e.X > mouseDownPoint.X + PhotoChat.HyperlinkDragSize
                        || e.Y < mouseDownPoint.Y - PhotoChat.HyperlinkDragSize
                        || e.Y > mouseDownPoint.Y + PhotoChat.HyperlinkDragSize)
                    {
                        // ドラッグ＆ドロップ開始
                        int index = photoListBox.IndexFromPoint(mouseDownPoint);
                        if (index >= 0)
                        {
                            Thumbnail thumbnail = (Thumbnail)photoListBox.Items[index];
                            photoListBox.DoDragDrop(thumbnail.PhotoName, DragDropEffects.Copy);
                            screenOffset = SystemInformation.WorkingArea.Location;
                        }

                        // リセット
                        mouseDownPoint = Point.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void photoListBox_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            try
            {
                if ((Control.MousePosition.X - screenOffset.X) < DesktopBounds.Left
                    || (Control.MousePosition.X - screenOffset.X) > DesktopBounds.Right
                    || (Control.MousePosition.Y - screenOffset.Y) < DesktopBounds.Top
                    || (Control.MousePosition.Y - screenOffset.Y) > DesktopBounds.Bottom)
                {
                    HideDown();
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string tag = e.Link.LinkData as string;
                SetPhotoList(tag, photoListBox.ColumnWidth, photoListBox.ItemHeight);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion
    }
}