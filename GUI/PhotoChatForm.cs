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
    /// GUI部を表す
    /// </summary>
    public partial class PhotoChatForm : Form
    {
        #region フィールド・プロパティ

        // モード
        private enum GUIModes { Camera, Pen, Eraser, Live };
        private GUIModes guiMode;
        private bool isAutoScrollMode = true;
        private bool isLiveMode = false;
        private bool isReplayMode = false;

        // 全体
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

        // カメラ関連
        private System.Threading.Timer autoStopCameraTimer;
        private TimerCallback stopCameraCallback;
        private const int capturingTime = PhotoChat.CapturingTime;

        // サムネイル関連
        private Thumbnail currentThumbnail;
        private Action<Thumbnail> addThumbnailAction;
        private Action<List<Thumbnail>> addThumbnailListAction;
        private System.Threading.Timer saveThumbnailDataTimer;
        private const int SaveInterval = PhotoChat.SaveThumbnailsInterval;

        // ステータスラベル関連
        private System.Threading.Timer labelResetTimer;
        private TimerCallback labelResetCallback;
        private const int LabelResetInterval = PhotoChat.ShowingConnectionInfoTime;
        private System.Threading.Timer updateProximityTimer;
        private const int ProximityInterval = PhotoChat.UpdateProximityInterval;

        // マウス操作関連
        private Point mouseDownPoint = Point.Empty;
        private Point screenOffset;


        /// <summary>
        /// 親となるPhotoChatClientの取得
        /// </summary>
        public PhotoChatClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// 閲覧パネルの取得
        /// </summary>
        public ReviewPanel ReviewPanel
        {
            get { return reviewPanel; }
        }

        /// <summary>
        /// ペンボタンの背景色の設定
        /// </summary>
        public Color PenColor
        {
            get { return penCheckBox.BackColor; }
            set { penCheckBox.BackColor = value; }
        }

        /// <summary>
        /// 左矢印ボタンが有効かどうかの取得または設定
        /// </summary>
        public bool LeftButtonEnabled
        {
            get { return leftButton.Enabled; }
            set { leftButton.Enabled = value; }
        }

        /// <summary>
        /// 右矢印ボタンが有効かどうかの取得または設定
        /// </summary>
        public bool RightButtonEnabled
        {
            get { return rightButton.Enabled; }
            set { rightButton.Enabled = value; }
        }

        /// <summary>
        /// サムネイル一覧オートスクロールモードかどうかの取得または設定
        /// </summary>
        public bool IsAutoScrollMode
        {
            get { return isAutoScrollMode; }
            set { isAutoScrollMode = value; }
        }

        /// <summary>
        /// 新着データ自動表示モードかどうかの取得
        /// </summary>
        public bool IsLiveMode
        {
            get { return isLiveMode; }
        }

        /// <summary>
        /// 書き込み再生モードかどうかの取得または設定
        /// </summary>
        public bool IsReplayMode
        {
            get { return isReplayMode; }
            set { isReplayMode = value; }
        }

        /// <summary>
        /// ログ表示ウィンドウを取得する。
        /// </summary>
        public LogWindow LogWindow
        {
            get { return logWindow; }
        }

        /*
        /// <summary>
        /// GPS受信部を取得する。
        /// </summary>
        public GpsReceiver GpsReceiver
        {
            get { return gpsReceiver; }
        }
*/

        #endregion




        #region コンストラクタ・デストラクタ

        /// <summary>
        /// フォームの初期化
        /// </summary>
        /// <param name="client">親となるPhotoChatClient</param>
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

            // カメラの初期化
            cameraPanel = new CameraPanel();
            if (!cameraPanel.IsActive)
                cameraButton.Enabled = false;

            // 閲覧パネル・パレット作成
            reviewPanel = new ReviewPanel(this);
            splitContainer2.Panel1.Controls.Add(reviewPanel);
            paletteWindow = new PaletteWindow(this);
            paletteWindow.Owner = this;

            // 一覧パネル・ログ表示ウィンドウ・GPS受信部の初期化
            photoListWindow = new PhotoListWindow(this);
            photoListWindow.Owner = this;
            logWindow = new LogWindow();
            logWindow.Owner = this;
            //GPS初期化の停止
            //gpsReceiver = new GpsReceiver();

            // モード初期化
            SetPenMode();
            leftButton.Enabled = false;
            rightButton.Enabled = false;

            ResumeLayout(false);

            // 文字認識初期化
            //InkRecognizer.Initialize();

            // 近接関係ラベル更新・サムネイルデータ保存タイマー起動
            updateProximityTimer = new System.Threading.Timer(
                new TimerCallback(UpdateProximityLabel), null, ProximityInterval, ProximityInterval);
            saveThumbnailDataTimer = new System.Threading.Timer(
                new TimerCallback(SaveThumbnails), null, SaveInterval, SaveInterval);
        }


        /// <summary>
        /// 使用中のリソースをすべてクリーンアップする
        /// </summary>
        /// <param name="disposing">マネージリソースが破棄される場合true</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.IsDisposed) return;

                client.Close();
                if (cameraPanel != null)
                    cameraPanel.CloseCamera();

                /*
                // 各リソースの解放・停止
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

                // 文字認識終了
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




        #region モード設定

        /// <summary>
        /// カメラモードに設定する。
        /// </summary>
        public void SetCameraMode()
        {
            try
            {
                if (guiMode == GUIModes.Camera) return;
                guiMode = GUIModes.Camera;

                // パレットウィンドウと一覧ウィンドウを隠す
                paletteWindow.Hide();
                paletteCheckBox.Checked = false;
                photoListWindow.HideDown();

                // ツールバーボタンの設定
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
        /// ペンモードに設定する。
        /// </summary>
        public void SetPenMode()
        {
            try
            {
                if (guiMode == GUIModes.Pen) return;
                guiMode = GUIModes.Pen;

                // ツールバーボタンの設定
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

                // 閲覧パネルを閲覧モードに設定
                reviewPanel.SetReviewMode();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 消しゴムモードに設定する。
        /// </summary>
        public void SetEraserMode()
        {
            try
            {
                if (guiMode == GUIModes.Eraser) return;
                guiMode = GUIModes.Eraser;

                // ツールバーボタンの設定
                penCheckBox.Checked = false;
                eraserCheckBox.Checked = true;

                // 閲覧パネルを消しゴムモードに設定
                reviewPanel.SetRemoveMode();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ライブモードに設定する。
        /// </summary>
        public void SetLiveMode()
        {
            try
            {
                if (guiMode == GUIModes.Live) return;
                guiMode = GUIModes.Live;

                // パレットウィンドウを隠す
                paletteWindow.Hide();
                paletteCheckBox.Checked = false;

                // ツールバーボタンの設定
                penCheckBox.Enabled = false;
                paletteCheckBox.Enabled = false;
                eraserCheckBox.Enabled = false;
                tagButton.Enabled = false;
                historySlider.Enabled = false;
                configButton.Enabled = false;
                saveButton.Enabled = false;

                // 閲覧パネルをライブモードに設定
                reviewPanel.SetLiveMode();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 新規データ処理

        /// <summary>
        /// 撮影した写真を記録・転送・表示する
        /// </summary>
        /// <param name="photo">撮影した写真</param>
        public void NewData(Photo photo)
        {
            try
            {
                client.NewData(photo, true);
                PhotoChat.WriteLog("Take a Photo", photo.PhotoName, string.Empty);
                ShowPhoto(photo);
                client.ConnectionManager.SendAll(photo);

                // コンテキスト情報タグを付ける
                ThreadPool.QueueUserWorkItem(addContextTagCallback, photo);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 新たな書き込みデータを記録・転送する
        /// </summary>
        /// <param name="note">新たな書き込み</param>
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
        /// 新たな共有ファイルを記録・転送する。
        /// </summary>
        /// <param name="sharedFile">新たな共有ファイル</param>
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
        /// Photoにコンテキスト情報タグを付ける。（WaitCallbackに対応）
        /// </summary>
        /// <param name="state">新たに作成されたPhoto</param>
        private void AddContextTag(object state)
        {
            try
            {
                Photo photo = (Photo)state;
                //GpsReceiver.GeoData geoData = gpsReceiver.GetCurrentGeoData();

                // 撮影者
                NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, photo.Author));
                // セッション名
                NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, client.CurrentSessionName));
/*
                if (geoData != null)
                {
                    if (gpsReceiver.DataValid)
                    {
                        // 都道府県名
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.Prefecture));
                        // 市町村名
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.City));
                        // 町名
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.Town));
                    }
                    else
                    {
                        // 都道府県名
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.Prefecture));
                        // 市町村名
                        NewData(new Tag(client.UserName, client.GetNewSerialNumber(), photo.PhotoName, geoData.City));
                        // 屋内
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




        #region カメラ

        /// <summary>
        /// カメラモードに切り替え写真撮影を開始する。
        /// </summary>
        private void StartCamera()
        {
            try
            {
                // カメラモードに切り替え
                this.splitContainer2.Panel1.Controls.Clear();
                this.splitContainer2.Panel1.Controls.Add(cameraPanel);
                SetCameraMode();

                // 撮影開始
                cameraPanel.StartCapture();
                autoStopCameraTimer = new System.Threading.Timer(
                    stopCameraCallback, null, capturingTime, Timeout.Infinite);

                // 写真選択コマンド送信とログ記録
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
        /// 写真を撮影する
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
                MessageBox.Show("写真データの作成に失敗しました。");
            }
        }


        /// <summary>
        /// カメラを停止して閲覧モードに切り替える。
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

                // カメラ自動停止タイマーを破棄
                if (autoStopCameraTimer != null)
                {
                    autoStopCameraTimer.Dispose();
                    autoStopCameraTimer = null;
                }

                // カメラを停止して閲覧モードへ
                cameraPanel.StopCapture();
                this.splitContainer2.Panel1.Controls.Clear();
                this.splitContainer2.Panel1.Controls.Add(reviewPanel);
                SetPenMode();

                // 撮影停止ログ
                PhotoChat.WriteLog("Auto Camera Stop", string.Empty, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// カメラを停止して閲覧モードに切り替える。（TimerCallbackに対応）
        /// </summary>
        private void StopCamera(object state)
        {
            StopCamera();
        }

        #endregion




        #region 写真閲覧・編集パネル

        /// <summary>
        /// 閲覧パネルに写真を表示する。
        /// </summary>
        /// <param name="photoName">表示する写真の名前</param>
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
        /// 閲覧パネルに写真を表示する。
        /// </summary>
        /// <param name="photo">表示する写真</param>
        public void ShowPhoto(Photo photo)
        {
            if (photo != null)
            {
                try
                {
                    // 撮影モードであればカメラを停止して閲覧モードへ
                    if (guiMode == GUIModes.Camera) StopCamera();

                    // 表示していた写真からの退出とサムネイル更新
                    if (currentThumbnail != null)
                    {
                        currentThumbnail.Leave();
                        UpdateCurrentThumbnailImage();
                    }

                    // 選択中写真の設定
                    currentThumbnail = thumbnailDictionary[photo.PhotoName];
                    if (currentThumbnail.Visit())
                        UpdateCurrentThumbnailData();
                    thumbnailListBox.SelectedItem = currentThumbnail;

                    // 閲覧パネルに表示
                    ResetSliderValue();
                    reviewPanel.SetPhoto(photo);
                    UpdateCurrentThumbnailImage();
                    thumbnailListBox.Invalidate();

                    // 写真選択コマンド送信とログ記録
                    client.ConnectionManager.SendAll(
                        Command.CreateSelectCommand(client.ID, client.UserName, photo.PhotoName));
                    PhotoChat.WriteLog("Photo Select", photo.PhotoName, string.Empty);

                    // パレットウィンドウと一覧ウィンドウを隠す
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
        /// 現在GUI部で保持しているPhotoの中に写真名に対応するものがあれば返す。
        /// </summary>
        /// <param name="photoName">写真名</param>
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
        /// 閲覧パネルを適切に再描画する。
        /// </summary>
        /// <param name="note">新しく追加された書き込み</param>
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
        /// 閲覧パネルを初期状態にする。
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
        /// 書き込み再生スライダーの値を最大に戻す。
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




        #region 一覧ウィンドウ

        /// <summary>
        /// 指定したタグの付いた写真を一覧ウィンドウに表示する。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
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




        #region 接続状況・近接関係表示（StatusStrip）

        /// <summary>
        /// 接続・切断情報を表示する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="userName">ユーザ名</param>
        /// <param name="isConnecting">接続のときはtrue、切断のときはfalse</param>
        public void NewConnection(string id, string userName, bool isConnecting)
        {
            try
            {
                lock (connectionDictionary)
                {
                    if (isConnecting)
                    {
                        // 接続の場合
                        connectionDictionary[id] = userName;
                        connectionStatusLabel.Text = " " + userName + " さんと接続しました";
                    }
                    else
                    {
                        // 切断の場合
                        if (connectionDictionary.TryGetValue(id, out userName))
                        {
                            if (userName == null) userName = string.Empty;
                            connectionStatusLabel.Text = " " + userName + " さんとの接続が切れました";
                            InformSelection(id, userName, null);
                            connectionDictionary.Remove(id);
                        }
                    }
                }

                // 一定時間後に接続数表示に戻す
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
        /// 接続状況表示ラベルを接続数表示に戻す。（TimerCallbackに対応）
        /// </summary>
        private void ResetLabel(object state)
        {
            try
            {
                StringBuilder sb = new StringBuilder("接続中の相手： ", 100);
                lock (connectionDictionary)
                {
                    // 接続数文字列作成
                    sb.Append(connectionDictionary.Count).Append("人");

                    // 接続中のユーザ名リスト作成
                    if (connectionDictionary.Count != 0)
                    {
                        sb.Append("（ ");
                        foreach (string userName in connectionDictionary.Values)
                        {
                            sb.Append(userName).Append(' ');
                        }
                        sb.Append('）');
                    }
                }

                // 接続数表示に戻す
                connectionStatusLabel.Text = sb.ToString();

                // タイマー破棄
                labelResetTimer.Dispose();
                labelResetTimer = null;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 近接関係にあるユーザ名を更新・表示する。（TimerCallbackに対応）
        /// </summary>
        private void UpdateProximityLabel(object state)
        {
            try
            {
                StringBuilder sb = new StringBuilder("近くにいる人：", 100);
                string[] userList = client.GetNearUsers();
                if (userList.Length == 0)
                {
                    // 近接関係にある端末が無い場合
                    sb.Append(" 0人");
                }
                else
                {
                    // 近くにいるユーザ名を並べる
                    for (int i = 0; i < userList.Length; i++)
                        sb.Append(userList[i]).Append(' ');
                }

                // ラベル更新
                proximityStatusLabel.Text = sb.ToString();
                PhotoChat.WriteLog("Proximity", proximityStatusLabel.Text, string.Empty);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 表示中画像のエクスポート・アップロード

        /// <summary>
        /// 現在表示している画像を外部に保存する。
        /// </summary>
        private void SaveReviewingImage()
        {
            try
            {
                if (reviewPanel.CurrentPhoto == null) return;

                // 保存先ファイル入力ダイアログの作成
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "表示中の画像を名前を付けて保存";
                saveDialog.FileName = reviewPanel.CurrentPhoto.Author
                    + reviewPanel.CurrentPhoto.SerialNumber.ToString();
                saveDialog.Filter = "PNGイメージ形式 (*.png)|*.png|"
                    + "JPEGイメージ形式 (*.jpg;*.jpeg)|*.jpg;*.jpeg|"
                    + "GIFイメージ形式 (*.gif)|*.gif|"
                    + "ビットマップイメージ形式 (*.bmp)|*.bmp";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

                // ダイアログ表示
                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // 画像形式に応じた保存
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
                        MessageBox.Show("画像の保存に失敗しました。");
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
        /// 現在表示している画像をFlickrにアップロードする。
        /// </summary>
        private void UploadReviewingImage()
        {
            try
            {
                if (reviewPanel.CurrentPhoto == null) return;

                // アップロードダイアログ表示
                FlickrUploadDialog uploadDialog = new FlickrUploadDialog();
                if (uploadDialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // アップロード設定読み取り
                        FlickrNet.Flickr flickr = uploadDialog.Flickr;
                        string description = uploadDialog.Description;
                        bool isPublic = uploadDialog.IsPublic;
                        bool isFamily = uploadDialog.IsFamily;
                        bool isFriend = uploadDialog.IsFamily;

                        // 一時ファイルに画像保存
                        string tempFile = "temp_FlickrUpload.jpg";
                        Bitmap image = reviewPanel.GetCurrentImage();
                        image.Save(tempFile, ImageFormat.Jpeg);
                        image.Dispose();

                        // タイトル・タグ取得
                        string title = client.CurrentSessionName + "_" + currentThumbnail.PhotoName;
                        string tags = currentThumbnail.Tags;

                        // アップロード
                        flickr.UploadPicture(
                            tempFile, title, description, tags, isPublic, isFamily, isFriend);
                        System.IO.File.Delete(tempFile);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Flickrにアクセスできませんでした");
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




        #region サムネイル一覧

        /// <summary>
        /// 一覧にサムネイルを追加する。
        /// </summary>
        /// <param name="thumbnail">追加するサムネイル</param>
        public void AddThumbnail(Thumbnail thumbnail)
        {
            try
            {
                if (InvokeRequired)
                {
                    // 別スレッドからの呼び出しの場合
                    Invoke(addThumbnailAction, thumbnail);
                    return;
                }

                // 後ろから比較して挿入箇所を探す
                int index = thumbnailListBox.Items.Count - 1;
                for (; index >= 0; index--)
                {
                    if (thumbnail.CompareTo(thumbnailListBox.Items[index]) > 0)
                        break;
                }
                index++;

                // 一覧へ挿入
                lock (thumbnailDictionary)
                {
                    thumbnailListBox.Items.Insert(index, thumbnail);
                    thumbnailDictionary[thumbnail.PhotoName] = thumbnail;
                }

                // オートスクロール
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
        /// 一覧にサムネイルをまとめて追加する。
        /// </summary>
        /// <param name="thumbnailList">追加するサムネイルの配列</param>
        public void AddThumbnailList(List<Thumbnail> thumbnailList)
        {
            try
            {
                if (InvokeRequired)
                {
                    // 別スレッドからの呼び出しの場合
                    Invoke(addThumbnailAction, thumbnailList);
                    return;
                }

                // リストのソート
                thumbnailList.Sort();

                // 一覧への挿入
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

                // 最新の写真を表示
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
        /// 一覧をクリアする。
        /// </summary>
        public void ClearThumbnailList()
        {
            try
            {
                if (InvokeRequired)
                {
                    // 別スレッドからの呼び出しの場合
                    Invoke(new MethodInvoker(ClearThumbnailList));
                    return;
                }

                // クリア
                thumbnailListBox.Items.Clear();
                currentThumbnail = null;

                // リソース解放
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
        /// 一覧に写真名に対応するサムネイルがあれば返す。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>写真名に対応するサムネイル。なければnullを返す。</returns>
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
        /// 次の写真を選択する。
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
        /// 1つ前の写真を選択する。
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
        /// 現在表示中の画像でサムネイル画像を更新する。
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
        /// 現在表示中のサムネイルのデータをサムネイルデータ保存キューに入れる。
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
        /// サムネイルの書き込みカウントを増減する。
        /// </summary>
        /// <param name="note"></param>
        public void CountThumbnailNote(PhotoChatNote note)
        {
            try
            {
                Thumbnail thumbnail;
                if (thumbnailDictionary.TryGetValue(note.PhotoName, out thumbnail))
                {
                    // 削除ならカウントを1減らしそれ以外なら1増やす
                    if (note.Type == PhotoChatNote.TypeRemoval)
                        thumbnail.DecrementNoteCount();
                    else if (note.Type == PhotoChatNote.TypeTag)
                        thumbnail.AddTag(((Tag)note).TagString);
                    else
                        thumbnail.IncrementNoteCount();

                    // データ更新と再描画
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
        /// 他のユーザが閲覧中の写真を変更する。
        /// photoNameがnullのときは非選択状態にする。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="userName">ユーザ名</param>
        /// <param name="photoName">選択された写真名</param>
        public void InformSelection(string id, string userName, string photoName)
        {
            try
            {
                // 前のサムネイルから名前を消す
                lock (selectionDictionary)
                {
                    Thumbnail thumbnail;
                    if (selectionDictionary.TryGetValue(id, out thumbnail))
                    {
                        thumbnail.RemoveUserName(id);
                    }

                    // サムネイルに名前を追加
                    if (photoName != null)
                    {
                        if (thumbnailDictionary.TryGetValue(photoName, out thumbnail))
                        {
                            thumbnail.AddUserName(id, userName);
                            selectionDictionary[id] = thumbnail;
                        }
                    }
                }

                // 再描画
                thumbnailListBox.Invalidate();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// キューに入っているサムネイルのデータを保存する。（TimerCallbackに対応）
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
        /// サムネイル項目のサイズをパネルに合わせて変更する。
        /// </summary>
        private void ResizeItem()
        {
            try
            {
                // サムネイルサイズの変更
                thumbnailListBox.ItemHeight = Thumbnail.Resize(thumbnailListBox.ClientSize.Width);

                // サムネイル画像の更新
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateThumbnailsWorker));
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サムネイル画像を更新する。（WaitCallbackに対応）
        /// </summary>
        private void UpdateThumbnailsWorker(object state)
        {
            try
            {
                // サムネイル画像更新
                for (int i = 0; i < thumbnailListBox.Items.Count; i++)
                {
                    ((Thumbnail)thumbnailListBox.Items[i]).ReloadImage();
                }

                // サムネイル一覧を再描画
                thumbnailListBox.RefreshAll();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region サムネイル一覧イベント

        /// <summary>
        /// リサイズ時の処理
        /// </summary>
        private void thumbnailListBox_Layout(object sender, LayoutEventArgs e)
        {
            ResizeItem();
        }


        /// <summary>
        /// 項目を描画する。
        /// </summary>
        private void thumbnailListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index < 0) return;

                // 描画
                ((Thumbnail)thumbnailListBox.Items[e.Index]).Paint(e.Graphics, e.Bounds);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// サムネイル一覧でマウスボタンが押された位置を記憶する。
        /// </summary>
        private void thumbnailListBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // サムネイル項目の上で押されたときのみ処理
                if (thumbnailListBox.IndexFromPoint(e.Location) >= 0)
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


        /// <summary>
        /// マウスボタンが離されたら写真選択。
        /// </summary>
        private void thumbnailListBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // ドラッグが開始されていなければ写真選択
                if (mouseDownPoint != Point.Empty)
                {
                    int index = thumbnailListBox.IndexFromPoint(mouseDownPoint);
                    if (index >= 0)
                        ShowPhoto(((Thumbnail)thumbnailListBox.Items[index]).PhotoName);
                }

                // リセット
                mouseDownPoint = Point.Empty;
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// マウスボタンを押してから一定範囲以上動いたらドラッグ開始
        /// </summary>
        private void thumbnailListBox_MouseMove(object sender, MouseEventArgs e)
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
                        int index = thumbnailListBox.IndexFromPoint(mouseDownPoint);
                        if (index >= 0)
                        {
                            Thumbnail thumbnail = (Thumbnail)thumbnailListBox.Items[index];
                            thumbnailListBox.DoDragDrop(thumbnail.PhotoName, DragDropEffects.Copy);
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


        /// <summary>
        /// ハイパーリンクドラッグがフォームから外れたらキャンセル
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
        /// ファイルのドラッグ＆ドロップを受け入れる。
        /// </summary>
        private void thumbnailListBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in dropFiles)
            {
                try
                {
                    // 画像ファイルであれば受け入れる
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
        /// ファイルのドラッグ＆ドロップを許可する。
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


        #region ツールバーボタンイベント

        /// <summary>
        /// セッションボタンを押したときの処理。
        /// </summary>
        private void sessionButton_Click(object sender, EventArgs e)
        {
            try
            {
                // セッション選択
                client.SelectSession(false);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// カメラボタンを押したときの処理。
        /// </summary>
        private void cameraButton_Click(object sender, EventArgs e)
        {
            try
            {
                // カメラモードであれば撮影、それ以外のときはカメラ起動
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
        /// メモボタンを押したときの処理。
        /// </summary>
        private void noteButton_Click(object sender, EventArgs e)
        {
            // 新規メモページを作成する。
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
                MessageBox.Show("メモページの作成に失敗しました。");
            }
        }


        /// <summary>
        /// 左矢印ボタンを押したときの処理。
        /// </summary>
        private void leftButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 前の写真に戻る。
                reviewPanel.Back();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// 右矢印ボタンを押したときの処理。
        /// </summary>
        private void rightButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 次の写真に進む。
                reviewPanel.Forward();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// ペンボタンが押されたときの処理。
        /// </summary>
        private void penCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // ペンモードに切り替え
                SetPenMode();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// パレットボタンが押されたときの処理。
        /// </summary>
        private void paletteCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // パレットの表示切替
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
        /// 消しゴムボタンが押されたときの処理。
        /// </summary>
        private void eraserCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // 消しゴムモードに切り替え
                SetEraserMode();
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// タグボタンが押されたときの処理。
        /// </summary>
        private void tagButton_Click(object sender, EventArgs e)
        {
            try
            {
                // タグ入力ダイアログ
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
        /// スライダーが動かされたときの処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void historySlider_Scroll(object sender, EventArgs e)
        {
            try
            {
                // スライダーの値（％）で書き込み再生
                reviewPanel.Replay(historySlider.Value);
            }
            catch (Exception ex)
            {
                PhotoChat.WriteErrorLog(ex.ToString());
            }
        }


        /// <summary>
        /// ライブモードボタンが押されたときの処理。
        /// </summary>
        private void liveCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // ライブモードのON/OFF切り替え
                if (liveCheckBox.Checked)
                {
                    // ライブモードにする
                    isLiveMode = true;
                    sessionButton.Enabled = false;
                    cameraButton.Enabled = false;
                    noteButton.Enabled = false;
                    SetLiveMode();
                }
                else
                {
                    // ライブモードをやめる
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
        /// 設定ボタンが押されたときの処理。
        /// </summary>
        private void configButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 設定ダイアログを表示
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
        /// 保存ボタンが押されたときの処理。
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 画像保存ダイアログを表示
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


        #region キーイベント

        /// <summary>
        /// キーによる操作
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.C:  // カメラボタン
                        if (cameraButton.Enabled)
                        {
                            cameraButton.PerformClick();
                            Cursor.Position = this.Location;
                        }
                        break;

                    case Keys.R:  // 文字認識ボタン
                        reviewPanel.CurrentPhoto.Recognize(this);
                        break;

                    case Keys.Up:  // 前の写真
                        SelectPreviousPhoto();
                        break;

                    case Keys.Down:  // 次の写真
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
        /// キーイベント処理を実行する。
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
        /// ボタンなどにフォーカスがあるときに上下キーが押されたときの特別処理
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