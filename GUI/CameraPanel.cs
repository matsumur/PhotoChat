using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;


namespace PhotoChat
{
    /// <summary>
    /// カメラ画像表示パネル
    /// </summary>
    public class CameraPanel : Panel
    {
        #region フィールド・プロパティ

        private Bitmap cameraImage;
        private Rectangle imageRect;
        private volatile bool isCapturing;
        private volatile bool isActive;
        private CameraPipeline cameraPipeline;


        /// <summary>
        /// カメラが使用可能かどうかを取得する。
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
        }

        #endregion
        

        #region コンストラクタ

        /// <summary>
        /// カメラパネルを初期化する。
        /// </summary>
        public CameraPanel()
        {
            this.isActive = false;
            this.isCapturing = false;

            // GUI設定
            this.BackColor = Color.PeachPuff;
            this.Dock = DockStyle.Fill;
            this.Name = "CameraPanel";
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.Resize += new EventHandler(CameraPanel_Resize);

            // Intel Perceptual Computing SDK カメラ設定
            // カメラパイプラインを構築
            try
            {
                this.cameraImage = new Bitmap(640, 480);
                this.cameraPipeline = new CameraPipeline(this, cameraImage);
                this.isActive = true;
                Task.Factory.StartNew(() =>
                {
                    this.cameraPipeline.LoopFrames();
                });
            }
            catch
            {
                MessageBox.Show("カメラが接続されていないかカメラにエラーが発生したため、\r\n"
                    + "カメラ機能を使用することができません。");
            }
        }

        #endregion

        public Bitmap CurrentImage()
        {
            return cameraPipeline.CurrentImage;
        }


        #region カメラ操作

        /// <summary>
        /// カメラ画像のキャプチャを開始する。
        /// </summary>
        public void StartCapture()
        {
            if (isCapturing || !isActive) return;
            isCapturing = true;

            try
            {
                cameraPipeline.setCapturing(true);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// カメラ画像のキャプチャを停止する。
        /// </summary>
        public void StopCapture()
        {
            try
            {
                cameraPipeline.setCapturing(false);
                isCapturing = false;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// カメラリソースを開放する。
        /// </summary>
        public void CloseCamera()
        {
            try
            {
                if (!isActive) return;
                isActive = false;
                StopCapture();


                if (cameraImage != null)
                    cameraImage.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region 画像表示サイズ設定

        /// <summary>
        /// 画像表示サイズをパネルのサイズに合わせて設定する。
        /// </summary>
        private void ResetImageRect()
        {
            try
            {
                float widthScale = ((float)this.Width) / 640;
                float heightScale = ((float)this.Height) / 480;
                float scaleFactor =
                    (widthScale < heightScale) ? widthScale : heightScale;
                int width = (int)(640 * scaleFactor);
                int height = (int)(480 * scaleFactor);

                imageRect = new Rectangle(0, 0, width, height);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region イベント

        /// <summary>
        /// サイズ変更があったときは画像表示サイズを変更する。
        /// </summary>
        private void CameraPanel_Resize(object sender, EventArgs e)
        {
            ResetImageRect();
        }

        #endregion
    }
}
