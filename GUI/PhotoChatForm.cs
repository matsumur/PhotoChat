using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Resources;
using Tuat.Hands;

namespace PhotoChat
{
    /// <summary>
    /// GUI����\��
    /// </summary>
    public partial class PhotoChatForm : Form
    {
        #region �t�B�[���h�E�v���p�e�B

        // ���[�h
        private enum GUIModes { Camera, Pen, Eraser, Live };
        private GUIModes guiMode;
        private bool isAutoScrollMode = true;
        private bool isLiveMode = false;
        private bool isReplayMode = false;

        // �S��
        private PhotoChatClient client;
        private CameraPanel cameraPanel;
        private ReviewPanel reviewPanel;
        private PaletteWindow paletteWindow;
        private PhotoListWindow photoListWindow;
        private LogWindow logWindow;
        //private GpsReceiver gpsReceiver;
        private WaitCallback addContextTagCallback;

        // Collection
        private Dictionary<string, Thumbnail> thumbnailDictionary = new Dictionary<string, Thumbnail>();
        private Dictionary<string, Thumbnail> selectionDictionary = new Dictionary<string, Thumbnail>();
        private Dictionary<string, string> connectionDictionary = new Dictionary<string, string>();
        private Queue<Thumbnail> thumbnailQueue = new Queue<Thumbnail>();

        // �J�����֘A
        private System.Threading.Timer autoStopCameraTimer;
        private TimerCallback stopCameraCallback;
        private const int capturingTime = PhotoChat.CapturingTime;

        // �T���l�C���֘A
        private Thumbnail currentThumbnail;
        private Action<Thumbnail> addThumbnailAction;
        private Action<List<Thumbnail>> addThumbnailListAction;
        private System.Threading.Timer saveThumbnailDataTimer;
        private const int SaveInterval = PhotoChat.SaveThumbnailsInterval;

        // �X�e�[�^�X���x���֘A
        private System.Threading.Timer labelResetTimer;
        private TimerCallback labelResetCallback;
        private const int LabelResetInterval = PhotoChat.ShowingConnectionInfoTime;
        private System.Threading.Timer updateProximityTimer;
        private const int ProximityInterval = PhotoChat.UpdateProximityInterval;

        // �}�E�X����֘A
        private Point mouseDownPoint = Point.Empty;
        private Point screenOffset;


        /// <summary>
        /// �e�ƂȂ�PhotoChatClient�̎擾
        /// </summary>
        public PhotoChatClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// �{���p�l���̎擾
        /// </summary>
        public ReviewPanel ReviewPanel
        {
            get { return reviewPanel; }
        }

        /// <summary>
        /// �y���{�^���̔w�i�F�̐ݒ�
        /// </summary>
        public Color PenColor
        {
            get { return penCheckBox.BackColor; }
            set { penCheckBox.BackColor = value; }
        }

        /// <summary>
        /// �����{�^�����L�����ǂ����̎擾�܂��͐ݒ�
        /// </summary>
        public bool LeftButtonEnabled
        {
            get { return leftButton.Enabled; }
            set { leftButton.Enabled = value; }
        }

        /// <summary>
        /// �E���{�^�����L�����ǂ����̎擾�܂��͐ݒ�
        /// </summary>
        public bool RightButtonEnabled
        {
            get { return rightButton.Enabled; }
            set { rightButton.Enabled = value; }
        }

        /// <summary>
        /// �T���l�C���ꗗ�I�[�g�X�N���[�����[�h���ǂ����̎擾�܂��͐ݒ�
        /// </summary>
        public bool IsAutoScrollMode
        {
            get { return isAutoScrollMode; }
            set { isAutoScrollMode = value; }
        }

        /// <summary>
        /// �V���f�[�^�����\�����[�h���ǂ����̎擾
        /// </summary>
        public bool IsLiveMode
        {
            get { return isLiveMode; }
        }

        /// <summary>
        /// �������ݍĐ����[�h���ǂ����̎擾�܂��͐ݒ�
        /// </summary>
        public bool IsReplayMode
        {
            get { return isReplayMode; }
            set { isReplayMode = value; }
        }

        /// <summary>
        /// ���O�\���E�B���h�E���擾����B
        /// </summary>
        public LogWindow LogWindow
        {
            get { return logWindow; }
        }

        /*
        /// <summary>
        /// GPS��M�����擾����B
        /// </summary>
        public GpsReceiver GpsReceiver
        {
            get { return gpsReceiver; }
        }
*/

        #endregion




        #region �R���X�g���N�^�E�f�X�g���N�^

        /// <summary>
        /// �t�H�[���̏�����
        /// </summary>
        /// <param name="client">�e�ƂȂ�PhotoChatClient</param>
        public PhotoChatForm(PhotoChatClient client)
        {
            this.client = client;
            InitializeComponent();
            addContextTagCallback = new WaitCallback(AddContextTag);
            stopCameraCallback = new TimerCallback(StopCamera);
            addThumbnailAction = new Action<Thumbnail>(AddThumbnail);
            addThumbnailListAction = new Action<List<Thumbnail>>(AddThumbnailList);
            labelResetCallback = new TimerCallback(ResetLabel);

            SuspendLayout();

            // �J�����̏�����
            cameraPanel = new CameraPanel();
            if (!cameraPanel.IsActive)
                cameraButton.Enabled = false;

            // �{���p�l���E�p���b�g�쐬
            reviewPanel = new ReviewPanel(this);
            splitContainer2.Panel1.Controls.Add(reviewPanel);
            paletteWindow = new PaletteWindow(this);
            paletteWindow.Owner = this;

            // �ꗗ�p�l���E���O�\���E�B���h�E�EGPS��M���̏�����
            photoListWindow = new PhotoListWindow(this);
            photoListWindow.Owner = this;
            logWindow = new LogWindow();
            logWindow.Owner = this;
            //GPS�������̒�~
            //gpsReceiver = new GpsReceiver();

            // ���[�h������
            SetPenMode();
            leftButton.Enabled = false;
            rightButton.Enabled = false;

            ResumeLayout(false);

            // �����F��������
            //InkRecognizer.Initialize();

            // �ߐڊ֌W���x���X�V�E�T���l�C���f�[�^�ۑ��^�C�}�[�N��
            updateProximityTimer = new System.Threading.Timer(
                new TimerCallback(UpdateProximityLabel), null, ProximityInterval, ProximityInterval);
            saveThumbnailDataTimer = new System.Threading.Timer(
                new TimerCallback(SaveThumbnails), null, SaveInterval, SaveInterval);
        }


        /// <summary>
        /// �g�p���̃��\�[�X�����ׂăN���[���A�b�v����
        /// </summary>
        /// <param name="disposing">�}�l�[�W���\�[�X���j�������ꍇtrue</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.IsDisposed) return;

                client.Close();
                if (cameraPanel != null)
                    cameraPanel.CloseCamera();

                /*
                // �e���\�[�X�̉���E��~
                if (gpsReceiver.IsActive)
                    gpsReceiver.Close();
                 */ 
                saveThumbnailDataTimer.Dispose();
                updateProximityTimer.Dispose();
                ClearThumbnailList();
                if (guiMode == GUIModes.Camera)
                    reviewPanel.Dispose();
                else
                    cameraPanel.Dispose();

                // �����F���I��
                //InkRecognizer.Close();

                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ���[�h�ݒ�

        /// <summary>
        /// �J�������[�h�ɐݒ肷��B
        /// </summary>
        public void SetCameraMode()
        {
            try
            {
                if (guiMode == GUIModes.Camera) return;
                guiMode = GUIModes.Camera;

                // �p���b�g�E�B���h�E�ƈꗗ�E�B���h�E���B��
                paletteWindow.Hide();
                paletteCheckBox.Checked = false;
                photoListWindow.HideDown();

                // �c�[���o�[�{�^���̐ݒ�
                leftButton.Enabled = false;
                rightButton.Enabled = false;
                penCheckBox.Enabled = false;
                paletteCheckBox.Enabled = false;
                eraserCheckBox.Enabled = false;
                tagButton.Enabled = false;
                historySlider.Enabled = false;
                liveCheckBox.Enabled = false;
                saveButton.Enabled = false;

                cameraPanel.Click += new EventHandler(clickCameraPanel);

            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        void clickCameraPanel(object sender, EventArgs ev)
        {
            if (cameraButton.Enabled)
            {
                cameraButton.PerformClick();
            }
        }


        /// <summary>
        /// �y�����[�h�ɐݒ肷��B
        /// </summary>
        public void SetPenMode()
        {
            try
            {
                if (guiMode == GUIModes.Pen) return;
                guiMode = GUIModes.Pen;

                // �c�[���o�[�{�^���̐ݒ�
                penCheckBox.Enabled = true;
                penCheckBox.Checked = true;
                paletteCheckBox.Enabled = true;
                eraserCheckBox.Enabled = true;
                eraserCheckBox.Checked = false;
                tagButton.Enabled = true;
                historySlider.Enabled = true;
                liveCheckBox.Enabled = true;
                configButton.Enabled = true;
                saveButton.Enabled = true;

                // �{���p�l�����{�����[�h�ɐݒ�
                reviewPanel.SetReviewMode();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �����S�����[�h�ɐݒ肷��B
        /// </summary>
        public void SetEraserMode()
        {
            try
            {
                if (guiMode == GUIModes.Eraser) return;
                guiMode = GUIModes.Eraser;

                // �c�[���o�[�{�^���̐ݒ�
                penCheckBox.Checked = false;
                eraserCheckBox.Checked = true;

                // �{���p�l���������S�����[�h�ɐݒ�
                reviewPanel.SetRemoveMode();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���C�u���[�h�ɐݒ肷��B
        /// </summary>
        public void SetLiveMode()
        {
            try
            {
                if (guiMode == GUIModes.Live) return;
                guiMode = GUIModes.Live;

                // �p���b�g�E�B���h�E���B��
                paletteWindow.Hide();
                paletteCheckBox.Checked = false;

                // �c�[���o�[�{�^���̐ݒ�
                penCheckBox.Enabled = false;
                paletteCheckBox.Enabled = false;
                eraserCheckBox.Enabled = false;
                tagButton.Enabled = false;
                historySlider.Enabled = false;
                configButton.Enabled = false;
                saveButton.Enabled = false;

                // �{���p�l�������C�u���[�h�ɐݒ�
                reviewPanel.SetLiveMode();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �V�K�f�[�^����

        /// <summary>
        /// �B�e�����ʐ^���L�^�E�]���E�\������
        /// </summary>
        /// <param name="photo">�B�e�����ʐ^</param>
        public void NewData(Photo photo)
        {
            try
            {
                client.NewData(photo, true);
                PhotoChat.WriteLog("Take a Photo", photo.PhotoName, string.Empty);
                ShowPhoto(photo);
                client.ConnectionManager.SendAll(photo);

                // �R���e�L�X�g���^�O��t����
                ThreadPool.QueueUserWorkItem(addContextTagCallback, photo);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �V���ȏ������݃f�[�^���L�^�E�]������
        /// </summary>
        /// <param name="note">�V���ȏ�������</param>
        public void NewData(PhotoChatNote note)
        {
            try
            {
                client.NewData(note, true);
                PhotoChat.WriteLog("Write a Note", note.Author + note.SerialNumber, note.PhotoName);
                client.ConnectionManager.SendAll(note);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �V���ȋ��L�t�@�C�����L�^�E�]������B
        /// </summary>
        /// <param name="sharedFile">�V���ȋ��L�t�@�C��</param>
        public void NewData(SharedFile sharedFile)
        {
            try
            {
                client.NewData(sharedFile, true);
                PhotoChat.WriteLog("Create a Shared File", sharedFile.FileName, string.Empty);
                client.ConnectionManager.SendAll(sharedFile);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// Photo�ɃR���e�L�X�g���^�O��t����B�iWaitCallback�ɑΉ��j
        /// </summary>
        /// <param name="state">�V���ɍ쐬���ꂽPhoto</param>
        private void AddContextTag(object state)
        {
            try
            {
                Photo photo = (Photo)state;
                //GpsReceiver.GeoData geoData = gpsReceiver.GetCurrentGeoData();

                // �B�e��
                NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, photo.Author));
                // �Z�b�V������
                NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, client.CurrentSessionName));
/*
                if (geoData != null)
                {
                    if (gpsReceiver.DataValid)
                    {
                        // �s���{����
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.Prefecture));
                        // �s������
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.City));
                        // ����
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.Town));
                    }
                    else
                    {
                        // �s���{����
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.Prefecture));
                        // �s������
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.City));
                        // ����
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, PhotoChat.Indoor));
                    }
                }
 * */
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �J����

        /// <summary>
        /// �J�������[�h�ɐ؂�ւ��ʐ^�B�e���J�n����B
        /// </summary>
        private void StartCamera()
        {
            try
            {
                // �J�������[�h�ɐ؂�ւ�
                this.splitContainer2.Panel1.Controls.Clear();
                this.splitContainer2.Panel1.Controls.Add(cameraPanel);
                SetCameraMode();

                // �B�e�J�n
                cameraPanel.StartCapture();
                autoStopCameraTimer = new System.Threading.Timer(
                    stopCameraCallback, null, capturingTime, Timeout.Infinite);

                // �ʐ^�I���R�}���h���M�ƃ��O�L�^
                client.ConnectionManager.SendAll(
                    Command.CreateSelectCommand(client.ID, client.UserName, null));
                PhotoChat.WriteLog("Camera Mode", string.Empty, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ʐ^���B�e����
        /// </summary>
        private void CaptureImage()
        {
            try
            {
                cameraPanel.StopCapture();
                Photo photo;
                /*
                if (gpsReceiver.IsActive && gpsReceiver.DataValid)
                    photo = new Photo(client.UserName, client.GetNewSerialNumber(),
                        cameraPanel.CurrentImage, client.GetNearUsers(),
                        gpsReceiver.Latitude, gpsReceiver.Longitude);
                else

                 */
                photo = new Photo(client.UserName, client.GetNewSerialNumber(),
                    cameraPanel.CurrentImage(), client.GetNearUsers());
                NewData(photo);
                NewData(new Tag(client.UserName,
                    client.GetNewSerialNumber(), photo.PhotoName, PhotoChat.Photograph));

                cameraPanel.Click -= new EventHandler(clickCameraPanel);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                MessageBox.Show("�ʐ^�f�[�^�̍쐬�Ɏ��s���܂����B");
            }
        }


        /// <summary>
        /// �J�������~���ĉ{�����[�h�ɐ؂�ւ���B
        /// </summary>
        private void StopCamera()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(StopCamera));
                    return;
                }

                // �J����������~�^�C�}�[��j��
                if (autoStopCameraTimer != null)
                {
                    autoStopCameraTimer.Dispose();
                    autoStopCameraTimer = null;
                }

                // �J�������~���ĉ{�����[�h��
                cameraPanel.StopCapture();
                this.splitContainer2.Panel1.Controls.Clear();
                this.splitContainer2.Panel1.Controls.Add(reviewPanel);
                SetPenMode();

                // �B�e��~���O
                PhotoChat.WriteLog("Auto Camera Stop", string.Empty, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �J�������~���ĉ{�����[�h�ɐ؂�ւ���B�iTimerCallback�ɑΉ��j
        /// </summary>
        private void StopCamera(object state)
        {
            StopCamera();
        }

        #endregion




        #region �ʐ^�{���E�ҏW�p�l��

        /// <summary>
        /// �{���p�l���Ɏʐ^��\������B
        /// </summary>
        /// <param name="photoName">�\������ʐ^�̖��O</param>
        public void ShowPhoto(string photoName)
        {
            try
            {
                Photo photo = new Photo(photoName);
                if (photo != null)
                    ShowPhoto(photo);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �{���p�l���Ɏʐ^��\������B
        /// </summary>
        /// <param name="photo">�\������ʐ^</param>
        public void ShowPhoto(Photo photo)
        {
            if (photo != null)
            {
                try
                {
                    // �B�e���[�h�ł���΃J�������~���ĉ{�����[�h��
                    if (guiMode == GUIModes.Camera) StopCamera();

                    // �\�����Ă����ʐ^����̑ޏo�ƃT���l�C���X�V
                    if (currentThumbnail != null)
                    {
                        currentThumbnail.Leave();
                        UpdateCurrentThumbnailImage();
                    }

                    // �I�𒆎ʐ^�̐ݒ�
                    currentThumbnail = thumbnailDictionary[photo.PhotoName];
                    if (currentThumbnail.Visit())
                        UpdateCurrentThumbnailData();
                    thumbnailListBox.SelectedItem = currentThumbnail;

                    // �{���p�l���ɕ\��
                    ResetSliderValue();
                    reviewPanel.SetPhoto(photo);
                    UpdateCurrentThumbnailImage();
                    thumbnailListBox.Invalidate();

                    // �ʐ^�I���R�}���h���M�ƃ��O�L�^
                    client.ConnectionManager.SendAll(
                        Command.CreateSelectCommand(client.ID, client.UserName, photo.PhotoName));
                    PhotoChat.WriteLog("Photo Select", photo.PhotoName, string.Empty);

                    // �p���b�g�E�B���h�E�ƈꗗ�E�B���h�E���B��
                    paletteWindow.Hide();
                    paletteCheckBox.Checked = false;
                    photoListWindow.HideDown();
                    this.reviewPanel.Focus();
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// ����GUI���ŕێ����Ă���Photo�̒��Ɏʐ^���ɑΉ�������̂�����ΕԂ��B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        public Photo GetCurrentPhoto(string photoName)
        {
            try
            {
                if (reviewPanel.CurrentPhoto != null
                    && reviewPanel.CurrentPhoto.PhotoName == photoName)
                {
                    return reviewPanel.CurrentPhoto;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }


        /// <summary>
        /// �{���p�l����K�؂ɍĕ`�悷��B
        /// </summary>
        /// <param name="note">�V�����ǉ����ꂽ��������</param>
        public void UpdateReviewPanel(PhotoChatNote note)
        {
            try
            {
                if (guiMode != GUIModes.Camera)
                {
                    if (note.Type == PhotoChatNote.TypeTag)
                        reviewPanel.AddTag(((Tag)note).TagString);
                    else
                        reviewPanel.UpdateImage();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �{���p�l����������Ԃɂ���B
        /// </summary>
        public void ClearReviewPanel()
        {
            try
            {
                reviewPanel.Clear();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �������ݍĐ��X���C�_�[�̒l���ő�ɖ߂��B
        /// </summary>
        public void ResetSliderValue()
        {
            try
            {
                historySlider.Value = historySlider.Maximum;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �ꗗ�E�B���h�E

        /// <summary>
        /// �w�肵���^�O�̕t�����ʐ^���ꗗ�E�B���h�E�ɕ\������B
        /// </summary>
        /// <param name="tag">�^�O������</param>
        public void ShowPhotoListWindow(string tag)
        {
            try
            {
                photoListWindow.Show();
                photoListWindow.SetPhotoList(
                    tag, thumbnailListBox.ClientSize.Width, thumbnailListBox.ItemHeight);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �ڑ��󋵁E�ߐڊ֌W�\���iStatusStrip�j

        /// <summary>
        /// �ڑ��E�ؒf����\������B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="userName">���[�U��</param>
        /// <param name="isConnecting">�ڑ��̂Ƃ���true�A�ؒf�̂Ƃ���false</param>
        public void NewConnection(string id, string userName, bool isConnecting)
        {
            try
            {
                lock (connectionDictionary)
                {
                    if (isConnecting)
                    {
                        // �ڑ��̏ꍇ
                        connectionDictionary[id] = userName;
                        connectionStatusLabel.Text = " " + userName + " ����Ɛڑ����܂���";
                    }
                    else
                    {
                        // �ؒf�̏ꍇ
                        if (connectionDictionary.TryGetValue(id, out userName))
                        {
                            if (userName == null) userName = string.Empty;
                            connectionStatusLabel.Text = " " + userName + " ����Ƃ̐ڑ����؂�܂���";
                            InformSelection(id, userName, null);
                            connectionDictionary.Remove(id);
                        }
                    }
                }

                // ��莞�Ԍ�ɐڑ����\���ɖ߂�
                if (labelResetTimer != null)
                {
                    labelResetTimer.Change(LabelResetInterval, Timeout.Infinite);
                }
                else
                {
                    labelResetTimer = new System.Threading.Timer(
                        labelResetCallback, null, LabelResetInterval, Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ڑ��󋵕\�����x����ڑ����\���ɖ߂��B�iTimerCallback�ɑΉ��j
        /// </summary>
        private void ResetLabel(object state)
        {
            try
            {
                StringBuilder sb = new StringBuilder("�ڑ����̑���F ", 100);
                lock (connectionDictionary)
                {
                    // �ڑ���������쐬
                    sb.Append(connectionDictionary.Count).Append("�l");

                    // �ڑ����̃��[�U�����X�g�쐬
                    if (connectionDictionary.Count != 0)
                    {
                        sb.Append("�i ");
                        foreach (string userName in connectionDictionary.Values)
                        {
                            sb.Append(userName).Append(' ');
                        }
                        sb.Append('�j');
                    }
                }

                // �ڑ����\���ɖ߂�
                connectionStatusLabel.Text = sb.ToString();

                // �^�C�}�[�j��
                labelResetTimer.Dispose();
                labelResetTimer = null;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ߐڊ֌W�ɂ��郆�[�U�����X�V�E�\������B�iTimerCallback�ɑΉ��j
        /// </summary>
        private void UpdateProximityLabel(object state)
        {
            try
            {
                StringBuilder sb = new StringBuilder("�߂��ɂ���l�F", 100);
                string[] userList = client.GetNearUsers();
                if (userList.Length == 0)
                {
                    // �ߐڊ֌W�ɂ���[���������ꍇ
                    sb.Append(" 0�l");
                }
                else
                {
                    // �߂��ɂ��郆�[�U������ׂ�
                    for (int i = 0; i < userList.Length; i++)
                        sb.Append(userList[i]).Append(' ');
                }

                // ���x���X�V
                proximityStatusLabel.Text = sb.ToString();
                PhotoChat.WriteLog("Proximity", proximityStatusLabel.Text, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �\�����摜�̃G�N�X�|�[�g�E�A�b�v���[�h

        /// <summary>
        /// ���ݕ\�����Ă���摜���O���ɕۑ�����B
        /// </summary>
        private void SaveReviewingImage()
        {
            try
            {
                if (reviewPanel.CurrentPhoto == null) return;

                // �ۑ���t�@�C�����̓_�C�A���O�̍쐬
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "�\�����̉摜�𖼑O��t���ĕۑ�";
                saveDialog.FileName = reviewPanel.CurrentPhoto.Author
                    + reviewPanel.CurrentPhoto.SerialNumber.ToString();
                saveDialog.Filter = "PNG�C���[�W�`�� (*.png)|*.png|"
                    + "JPEG�C���[�W�`�� (*.jpg;*.jpeg)|*.jpg;*.jpeg|"
                    + "GIF�C���[�W�`�� (*.gif)|*.gif|"
                    + "�r�b�g�}�b�v�C���[�W�`�� (*.bmp)|*.bmp";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

                // �_�C�A���O�\��
                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // �摜�`���ɉ������ۑ�
                        Bitmap image = reviewPanel.GetCurrentImage();
                        switch (saveDialog.FilterIndex)
                        {
                            case 1:
                                image.Save(saveDialog.FileName, ImageFormat.Png);
                                break;

                            case 2:
                                image.Save(saveDialog.FileName, ImageFormat.Jpeg);
                                break;

                            case 3:
                                image.Save(saveDialog.FileName, ImageFormat.Gif);
                                break;

                            case 4:
                                image.Save(saveDialog.FileName, ImageFormat.Bmp);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        PhotoChat.WriteErrorLog(e.ToString());
                        MessageBox.Show("�摜�̕ۑ��Ɏ��s���܂����B");
                    }
                }
                saveDialog.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���ݕ\�����Ă���摜��Flickr�ɃA�b�v���[�h����B
        /// </summary>
        private void UploadReviewingImage()
        {
            try
            {
                if (reviewPanel.CurrentPhoto == null) return;

                // �A�b�v���[�h�_�C�A���O�\��
                FlickrUploadDialog uploadDialog = new FlickrUploadDialog();
                if (uploadDialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // �A�b�v���[�h�ݒ�ǂݎ��
                        FlickrNet.Flickr flickr = uploadDialog.Flickr;
                        string description = uploadDialog.Description;
                        bool isPublic = uploadDialog.IsPublic;
                        bool isFamily = uploadDialog.IsFamily;
                        bool isFriend = uploadDialog.IsFamily;

                        // �ꎞ�t�@�C���ɉ摜�ۑ�
                        string tempFile = "temp_FlickrUpload.jpg";
                        Bitmap image = reviewPanel.GetCurrentImage();
                        image.Save(tempFile, ImageFormat.Jpeg);
                        image.Dispose();

                        // �^�C�g���E�^�O�擾
                        string title = client.CurrentSessionName + "_" + currentThumbnail.PhotoName;
                        string tags = currentThumbnail.Tags;

                        // �A�b�v���[�h
                        flickr.UploadPicture(
                            tempFile, title, description, tags, isPublic, isFamily, isFriend);
                        System.IO.File.Delete(tempFile);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Flickr�ɃA�N�Z�X�ł��܂���ł���");
                        PhotoChat.WriteErrorLog(e.ToString());
                    }
                }
                uploadDialog.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �T���l�C���ꗗ

        /// <summary>
        /// �ꗗ�ɃT���l�C����ǉ�����B
        /// </summary>
        /// <param name="thumbnail">�ǉ�����T���l�C��</param>
        public void AddThumbnail(Thumbnail thumbnail)
        {
            try
            {
                if (InvokeRequired)
                {
                    // �ʃX���b�h����̌Ăяo���̏ꍇ
                    Invoke(addThumbnailAction, thumbnail);
                    return;
                }

                // ��납���r���đ}���ӏ���T��
                int index = thumbnailListBox.Items.Count - 1;
                for (; index >= 0; index--)
                {
                    if (thumbnail.CompareTo(thumbnailListBox.Items[index]) > 0)
                        break;
                }
                index++;

                // �ꗗ�֑}��
                lock (thumbnailDictionary)
                {
                    thumbnailListBox.Items.Insert(index, thumbnail);
                    thumbnailDictionary[thumbnail.PhotoName] = thumbnail;
                }

                // �I�[�g�X�N���[��
                if (isAutoScrollMode && thumbnailListBox.Height < thumbnailListBox.PreferredHeight)
                {
                    thumbnailListBox.TopIndex =
                        index + 1 - thumbnailListBox.Height / thumbnailListBox.ItemHeight;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ꗗ�ɃT���l�C�����܂Ƃ߂Ēǉ�����B
        /// </summary>
        /// <param name="thumbnailList">�ǉ�����T���l�C���̔z��</param>
        public void AddThumbnailList(List<Thumbnail> thumbnailList)
        {
            try
            {
                if (InvokeRequired)
                {
                    // �ʃX���b�h����̌Ăяo���̏ꍇ
                    Invoke(addThumbnailAction, thumbnailList);
                    return;
                }

                // ���X�g�̃\�[�g
                thumbnailList.Sort();

                // �ꗗ�ւ̑}��
                thumbnailListBox.BeginUpdate();
                lock (thumbnailDictionary)
                {
                    foreach (Thumbnail thumbnail in thumbnailList)
                    {
                        thumbnailListBox.Items.Add(thumbnail);
                        thumbnailDictionary[thumbnail.PhotoName] = thumbnail;
                    }
                }
                thumbnailListBox.EndUpdate();

                // �ŐV�̎ʐ^��\��
                int index = thumbnailListBox.Items.Count - 1;
                if (index >= 0)
                    ShowPhoto(((Thumbnail)thumbnailListBox.Items[index]).PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ꗗ���N���A����B
        /// </summary>
        public void ClearThumbnailList()
        {
            try
            {
                if (InvokeRequired)
                {
                    // �ʃX���b�h����̌Ăяo���̏ꍇ
                    Invoke(new MethodInvoker(ClearThumbnailList));
                    return;
                }

                // �N���A
                thumbnailListBox.Items.Clear();
                currentThumbnail = null;

                // ���\�[�X���
                lock (thumbnailDictionary)
                {
                    foreach (string key in thumbnailDictionary.Keys)
                    {
                        thumbnailDictionary[key].Dispose();
                    }
                    thumbnailDictionary.Clear();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ꗗ�Ɏʐ^���ɑΉ�����T���l�C��������ΕԂ��B
        /// </summary>
        /// <param name="photoName">�ʐ^��</param>
        /// <returns>�ʐ^���ɑΉ�����T���l�C���B�Ȃ����null��Ԃ��B</returns>
        public Thumbnail GetThumbnail(string photoName)
        {
            try
            {
                Thumbnail thumbnail;
                thumbnailDictionary.TryGetValue(photoName, out thumbnail);
                return thumbnail;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// ���̎ʐ^��I������B
        /// </summary>
        public void SelectNextPhoto()
        {
            try
            {
                int index = thumbnailListBox.SelectedIndex + 1;
                if (index < thumbnailListBox.Items.Count)
                {
                    ShowPhoto(((Thumbnail)thumbnailListBox.Items[index]).PhotoName);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 1�O�̎ʐ^��I������B
        /// </summary>
        public void SelectPreviousPhoto()
        {
            try
            {
                int index = thumbnailListBox.SelectedIndex - 1;
                if (index >= 0)
                {
                    ShowPhoto(((Thumbnail)thumbnailListBox.Items[index]).PhotoName);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���ݕ\�����̉摜�ŃT���l�C���摜���X�V����B
        /// </summary>
        public void UpdateCurrentThumbnailImage()
        {
            try
            {
                Bitmap image = reviewPanel.GetThumbnailImage();
                currentThumbnail.UpdateImage(image);
                image.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���ݕ\�����̃T���l�C���̃f�[�^���T���l�C���f�[�^�ۑ��L���[�ɓ����B
        /// </summary>
        public void UpdateCurrentThumbnailData()
        {
            try
            {
                lock (thumbnailQueue)
                {
                    if (!thumbnailQueue.Contains(currentThumbnail))
                        thumbnailQueue.Enqueue(currentThumbnail);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���̏������݃J�E���g�𑝌�����B
        /// </summary>
        /// <param name="note"></param>
        public void CountThumbnailNote(PhotoChatNote note)
        {
            try
            {
                Thumbnail thumbnail;
                if (thumbnailDictionary.TryGetValue(note.PhotoName, out thumbnail))
                {
                    // �폜�Ȃ�J�E���g��1���炵����ȊO�Ȃ�1���₷
                    if (note.Type == PhotoChatNote.TypeRemoval)
                        thumbnail.DecrementNoteCount();
                    else if (note.Type == PhotoChatNote.TypeTag)
                        thumbnail.AddTag(((Tag)note).TagString);
                    else
                        thumbnail.IncrementNoteCount();

                    // �f�[�^�X�V�ƍĕ`��
                    UpdateCurrentThumbnailData();
                    thumbnailListBox.Invalidate();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���̃��[�U���{�����̎ʐ^��ύX����B
        /// photoName��null�̂Ƃ��͔�I����Ԃɂ���B
        /// </summary>
        /// <param name="id">�[��ID</param>
        /// <param name="userName">���[�U��</param>
        /// <param name="photoName">�I�����ꂽ�ʐ^��</param>
        public void InformSelection(string id, string userName, string photoName)
        {
            try
            {
                // �O�̃T���l�C�����疼�O������
                lock (selectionDictionary)
                {
                    Thumbnail thumbnail;
                    if (selectionDictionary.TryGetValue(id, out thumbnail))
                    {
                        thumbnail.RemoveUserName(id);
                    }

                    // �T���l�C���ɖ��O��ǉ�
                    if (photoName != null)
                    {
                        if (thumbnailDictionary.TryGetValue(photoName, out thumbnail))
                        {
                            thumbnail.AddUserName(id, userName);
                            selectionDictionary[id] = thumbnail;
                        }
                    }
                }

                // �ĕ`��
                thumbnailListBox.Invalidate();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �L���[�ɓ����Ă���T���l�C���̃f�[�^��ۑ�����B�iTimerCallback�ɑΉ��j
        /// </summary>
        private void SaveThumbnails(object state)
        {
            try
            {
                Thumbnail thumbnail;
                while (thumbnailQueue.Count != 0)
                {
                    lock (thumbnailQueue)
                    {
                        thumbnail = thumbnailQueue.Dequeue();
                        if (thumbnail != null)
                            thumbnail.SaveData();
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C�����ڂ̃T�C�Y���p�l���ɍ��킹�ĕύX����B
        /// </summary>
        private void ResizeItem()
        {
            try
            {
                // �T���l�C���T�C�Y�̕ύX
                thumbnailListBox.ItemHeight = Thumbnail.Resize(thumbnailListBox.ClientSize.Width);

                // �T���l�C���摜�̍X�V
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateThumbnailsWorker));
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���摜���X�V����B�iWaitCallback�ɑΉ��j
        /// </summary>
        private void UpdateThumbnailsWorker(object state)
        {
            try
            {
                // �T���l�C���摜�X�V
                for (int i = 0; i < thumbnailListBox.Items.Count; i++)
                {
                    ((Thumbnail)thumbnailListBox.Items[i]).ReloadImage();
                }

                // �T���l�C���ꗗ���ĕ`��
                thumbnailListBox.RefreshAll();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region �T���l�C���ꗗ�C�x���g

        /// <summary>
        /// ���T�C�Y���̏���
        /// </summary>
        private void thumbnailListBox_Layout(object sender, LayoutEventArgs e)
        {
            ResizeItem();
        }


        /// <summary>
        /// ���ڂ�`�悷��B
        /// </summary>
        private void thumbnailListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index < 0) return;

                // �`��
                ((Thumbnail)thumbnailListBox.Items[e.Index]).Paint(e.Graphics, e.Bounds);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �T���l�C���ꗗ�Ń}�E�X�{�^���������ꂽ�ʒu���L������B
        /// </summary>
        private void thumbnailListBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // �T���l�C�����ڂ̏�ŉ����ꂽ�Ƃ��̂ݏ���
                if (thumbnailListBox.IndexFromPoint(e.Location) >= 0)
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


        /// <summary>
        /// �}�E�X�{�^���������ꂽ��ʐ^�I���B
        /// </summary>
        private void thumbnailListBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // �h���b�O���J�n����Ă��Ȃ���Ύʐ^�I��
                if (mouseDownPoint != Point.Empty)
                {
                    int index = thumbnailListBox.IndexFromPoint(mouseDownPoint);
                    if (index >= 0)
                        ShowPhoto(((Thumbnail)thumbnailListBox.Items[index]).PhotoName);
                }

                // ���Z�b�g
                mouseDownPoint = Point.Empty;
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �}�E�X�{�^���������Ă�����͈͈ȏ㓮������h���b�O�J�n
        /// </summary>
        private void thumbnailListBox_MouseMove(object sender, MouseEventArgs e)
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
                        int index = thumbnailListBox.IndexFromPoint(mouseDownPoint);
                        if (index >= 0)
                        {
                            Thumbnail thumbnail = (Thumbnail)thumbnailListBox.Items[index];
                            thumbnailListBox.DoDragDrop(thumbnail.PhotoName, DragDropEffects.Copy);
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


        /// <summary>
        /// �n�C�p�[�����N�h���b�O���t�H�[������O�ꂽ��L�����Z��
        /// </summary>
        private void thumbnailListBox_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            try
            {
                if ((Control.MousePosition.X - screenOffset.X) < DesktopBounds.Left
                    || (Control.MousePosition.X - screenOffset.X) > DesktopBounds.Right
                    || (Control.MousePosition.Y - screenOffset.Y) < DesktopBounds.Top
                    || (Control.MousePosition.Y - screenOffset.Y) > DesktopBounds.Bottom)
                {
                    e.Action = DragAction.Cancel;
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �t�@�C���̃h���b�O���h���b�v���󂯓����B
        /// </summary>
        private void thumbnailListBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in dropFiles)
            {
                try
                {
                    // �摜�t�@�C���ł���Ύ󂯓����
                    if (client.CheckImageFileExtension(file))
                    {
                        Photo photo;
                        /*
                        if (gpsReceiver.IsActive && gpsReceiver.DataValid)
                            photo = new Photo(client.UserName, client.GetNewSerialNumber(), file,
                                client.GetNearUsers(), gpsReceiver.Latitude, gpsReceiver.Longitude);
                        else
                         */ 
                            photo = new Photo(client.UserName,
                                client.GetNewSerialNumber(), file, client.GetNearUsers());
                        NewData(photo);
                        NewData(new Tag(client.UserName,
                            client.GetNewSerialNumber(), photo.PhotoName, PhotoChat.ImageData));
                    }
                }
                catch (Exception ex)
                {
                    PhotoChat.WriteErrorLog(ex.ToString());
                }
            }
        }


        /// <summary>
        /// �t�@�C���̃h���b�O���h���b�v��������B
        /// </summary>
        private void thumbnailListBox_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region �c�[���o�[�{�^���C�x���g

        /// <summary>
        /// �Z�b�V�����{�^�����������Ƃ��̏����B
        /// </summary>
        private void sessionButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �Z�b�V�����I��
                client.SelectSession(false);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �J�����{�^�����������Ƃ��̏����B
        /// </summary>
        private void cameraButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �J�������[�h�ł���ΎB�e�A����ȊO�̂Ƃ��̓J�����N��
                if (guiMode == GUIModes.Camera)
                    CaptureImage();
                else
                    StartCamera();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �����{�^�����������Ƃ��̏����B
        /// </summary>
        private void noteButton_Click(object sender, EventArgs e)
        {
            // �V�K�����y�[�W���쐬����B
            try
            {
                Bitmap whiteImage = new Bitmap(PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
                using (Graphics g = Graphics.FromImage(whiteImage))
                {
                    g.Clear(Color.White);
                }
                Photo photo;
                /*
                if (gpsReceiver.IsActive && gpsReceiver.DataValid)
                    photo = new Photo(client.UserName, client.GetNewSerialNumber(),
                        whiteImage, client.GetNearUsers(),
                        gpsReceiver.Latitude, gpsReceiver.Longitude);
                else
                 */ 
                    photo = new Photo(client.UserName, client.GetNewSerialNumber(),
                        whiteImage, client.GetNearUsers());
                NewData(photo);
                NewData(new Tag(client.UserName,
                    client.GetNewSerialNumber(), photo.PhotoName, PhotoChat.Notepad));
            }
            catch (Exception exception)
            {
                PhotoChat.WriteErrorLog(exception.ToString());
                MessageBox.Show("�����y�[�W�̍쐬�Ɏ��s���܂����B");
            }
        }


        /// <summary>
        /// �����{�^�����������Ƃ��̏����B
        /// </summary>
        private void leftButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �O�̎ʐ^�ɖ߂�B
                reviewPanel.Back();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �E���{�^�����������Ƃ��̏����B
        /// </summary>
        private void rightButton_Click(object sender, EventArgs e)
        {
            try
            {
                // ���̎ʐ^�ɐi�ށB
                reviewPanel.Forward();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �y���{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void penCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // �y�����[�h�ɐ؂�ւ�
                SetPenMode();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �p���b�g�{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void paletteCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // �p���b�g�̕\���ؑ�
                if (paletteCheckBox.Checked)
                    paletteWindow.Show();
                else
                    paletteWindow.Hide();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �����S���{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void eraserCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // �����S�����[�h�ɐ؂�ւ�
                SetEraserMode();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �^�O�{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void tagButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �^�O���̓_�C�A���O
                TagDialog tagDialog = new TagDialog();
                if (tagDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Photo photo = reviewPanel.CurrentPhoto;
                    foreach (string tag in tagDialog.TagString.Split(','))
                    {
                        if (!photo.ContainsTag(tag))
                        {
                            NewData(new Tag(client.UserName,
                                client.GetNewSerialNumber(), photo.PhotoName, tag));
                        }
                    }
                }
                tagDialog.Dispose();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �X���C�_�[���������ꂽ�Ƃ��̏����B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void historySlider_Scroll(object sender, EventArgs e)
        {
            try
            {
                // �X���C�_�[�̒l�i���j�ŏ������ݍĐ�
                reviewPanel.Replay(historySlider.Value);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// ���C�u���[�h�{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void liveCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // ���C�u���[�h��ON/OFF�؂�ւ�
                if (liveCheckBox.Checked)
                {
                    // ���C�u���[�h�ɂ���
                    isLiveMode = true;
                    sessionButton.Enabled = false;
                    cameraButton.Enabled = false;
                    noteButton.Enabled = false;
                    SetLiveMode();
                }
                else
                {
                    // ���C�u���[�h����߂�
                    isLiveMode = false;
                    sessionButton.Enabled = true;
                    if (cameraPanel.IsActive)
                        cameraButton.Enabled = true;
                    noteButton.Enabled = true;
                    SetPenMode();
                }
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �ݒ�{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void configButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �ݒ�_�C�A���O��\��
                ConfigDialog configDialog = new ConfigDialog(this);
                configDialog.ShowDialog(this);
                configDialog.Dispose();
                client.UpdateConfigFile();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �ۑ��{�^���������ꂽ�Ƃ��̏����B
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �摜�ۑ��_�C�A���O��\��
                SaveImageDialog saveImageDialog = new SaveImageDialog();
                if (saveImageDialog.ShowDialog(this) == DialogResult.OK)
                {
                    switch (saveImageDialog.SelectedSaveMode)
                    {
                        case SaveImageDialog.SaveImageMode.ServerUpload:
                            client.UploadToServer();
                            break;

                        case SaveImageDialog.SaveImageMode.SaveCurrent:
                            SaveReviewingImage();
                            break;

                        case SaveImageDialog.SaveImageMode.SaveAll:
                            client.SaveSessionImages();
                            break;

                        case SaveImageDialog.SaveImageMode.FlickrUploadCurrent:
                            UploadReviewingImage();
                            break;

                        case SaveImageDialog.SaveImageMode.FlickrUploadAll:
                            client.UploadSessionImagesToFlickr();
                            break;

                        case SaveImageDialog.SaveImageMode.SmartCalendarExport:
                            client.ExportToSmartCalendar();
                            break;
                    }
                }
                saveImageDialog.Dispose();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }

        #endregion


        #region �L�[�C�x���g

        /// <summary>
        /// �L�[�ɂ�鑀��
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.C:  // �J�����{�^��
                        if (cameraButton.Enabled)
                        {
                            cameraButton.PerformClick();
                            Cursor.Position = this.Location;
                        }
                        break;

                    case Keys.R:  // �����F���{�^��
                        reviewPanel.CurrentPhoto.Recognize(this);
                        break;

                    case Keys.Up:  // �O�̎ʐ^
                        SelectPreviousPhoto();
                        break;

                    case Keys.Down:  // ���̎ʐ^
                        SelectNextPhoto();
                        break;

                    case Keys.Left:
                    case Keys.Right:
                        break;

                    default:
                        base.OnKeyDown(e);
                        return;
                }
                e.Handled = true;
                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �L�[�C�x���g���������s����B
        /// </summary>
        internal void DoKeyDown(KeyEventArgs e)
        {
            try
            {
                OnKeyDown(e);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// �{�^���ȂǂɃt�H�[�J�X������Ƃ��ɏ㉺�L�[�������ꂽ�Ƃ��̓��ʏ���
        /// </summary>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                    SelectPreviousPhoto();
                    return true;

                case Keys.Down:
                    SelectNextPhoto();
                    return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        #endregion

        private void splitContainer2_Panel2_Click(object sender, EventArgs e)
        {
            if (cameraButton.Enabled)
            {
                cameraButton.PerformClick();
            }
        }
    }
}