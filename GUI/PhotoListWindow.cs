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
    /// �ʐ^�ꗗ�E�B���h�E
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
        /// �ʐ^�ꗗ�E�B���h�E���쐬����B
        /// </summary>
        public PhotoListWindow(PhotoChatForm form)
        {
            this.form = form;
            InitializeComponent();
            showBounds = this.Bounds;
            hideBounds = new Rectangle(form.Right - 40, form.Bottom - 35, 10, 20);
        }


        /// <summary>
        /// �w�肵���^�O�̕t�����T���l�C���̈ꗗ��\������B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        /// <param name="itemWidth">���ڂ̕�</param>
        /// <param name="itemHeight">���ڂ̍���</param>
        public void SetPhotoList(string tag, int itemWidth, int itemHeight)
        {
            try
            {
                // �T���l�C�����X�g�쐬
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

                // �ꗗ�ւ̑}��
                photoListBox.BeginUpdate();
                selectedIndex = -1;
                if (selectedImage != null)
                {
                    selectedImage.Dispose();
                    selectedImage = null;
                }
                ShowUp();
                photoListBox.Items.Clear();
                this.Text = "�^�O�F " + tag;
                photoListBox.Focus();
                foreach (Thumbnail thumbnail in thumbnailList)
                    photoListBox.Items.Add(thumbnail);
                photoListBox.ColumnWidth = itemWidth;
                photoListBox.ItemHeight = itemHeight;
                photoListBox.EndUpdate();

                // ���݂̃Z�b�V�����Ŏg�p���ꂽ�^�O�ꗗ�̍쐬
                SetTagPanel();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���݂̃Z�b�V�����Ŏg�p���ꂽ�^�O�ꗗ���쐬����B
        /// </summary>
        private void SetTagPanel()
        {
            tagPanel.SuspendLayout();
            tagPanel.Controls.Clear();
            int tagCount = 0;
            foreach (string tag in form.Client.SessionTagList)
            {
                // �^�O���ƂɃ����N���x�����쐬���Ēǉ�
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




        #region �E�B���h�E�ړ�

        /// <summary>
        /// �E�B���h�E����Ɉړ�����B
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
        /// �E�B���h�E�����Ɉړ�����B
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




        #region �C�x���g

        /// <summary>
        /// �L�[�C�x���g
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
        /// �E�B���h�E���A�B
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            ShowUp();
            base.OnActivated(e);
        }


        /// <summary>
        /// �E�B���h�E���B���B
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivate(EventArgs e)
        {
            HideDown();
            base.OnDeactivate(e);
        }


        /// <summary>
        /// ���ڂ�`�悷��B
        /// </summary>
        private void photoListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index < 0) return;

                // �`��
                ((Thumbnail)photoListBox.Items[e.Index]).Paint(e.Graphics, e.Bounds);
                e.DrawFocusRectangle();

                // �I�����ꂽ�T���l�C���̃|�b�v�A�b�v�`��
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
                // �T���l�C�����ڂ̏�ŉ����ꂽ�Ƃ��̂ݏ���
                if (photoListBox.IndexFromPoint(e.Location) >= 0)
                {
                    // �{�^���������ꂽ�ʒu���L��
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
                // �h���b�O���J�n����Ă��Ȃ���Ύʐ^�I��
                if (mouseDownPoint != Point.Empty)
                {
                    // �I�𒆂̎ʐ^���N���b�N�����̂ł���Ύʐ^�\��
                    if (photoListBox.IndexFromPoint(mouseDownPoint) == selectedIndex)
                        form.ShowPhoto(((Thumbnail)photoListBox.SelectedItem).PhotoName);
                    else
                    {
                        // �I�����ꂽ�T���l�C���̉摜�ƕ\���ʒu���擾
                        selectedIndex = photoListBox.IndexFromPoint(mouseDownPoint);
                        string photoName = ((Thumbnail)photoListBox.SelectedItem).PhotoName;
                        selectedImage = Thumbnail.GetImage(
                            photoName, PhotoChat.ThumbnailWidth, PhotoChat.ThumbnailHeight);
                        Rectangle rect = photoListBox.GetItemRectangle(photoListBox.SelectedIndex);
                        selectedPoint = new Point(rect.Right, rect.Top);
                        photoListBox.Refresh();
                    }
                }

                // ���Z�b�g
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
                // �}�E�X�{�^����������Ă���Ƃ��̂ݏ���
                if (mouseDownPoint != Point.Empty)
                {
                    // �}�E�X�̈ړ������͈͂𒴂��Ă��Ȃ������ׂ�
                    if (e.X < mouseDownPoint.X - PhotoChat.HyperlinkDragSize
                        || e.X > mouseDownPoint.X + PhotoChat.HyperlinkDragSize
                        || e.Y < mouseDownPoint.Y - PhotoChat.HyperlinkDragSize
                        || e.Y > mouseDownPoint.Y + PhotoChat.HyperlinkDragSize)
                    {
                        // �h���b�O���h���b�v�J�n
                        int index = photoListBox.IndexFromPoint(mouseDownPoint);
                        if (index >= 0)
                        {
                            Thumbnail thumbnail = (Thumbnail)photoListBox.Items[index];
                            photoListBox.DoDragDrop(thumbnail.PhotoName, DragDropEffects.Copy);
                            screenOffset = SystemInformation.WorkingArea.Location;
                        }

                        // ���Z�b�g
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