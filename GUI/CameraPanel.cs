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
    /// �J�����摜�\���p�l��
    /// </summary>
    public class CameraPanel : Panel
    {
        #region �t�B�[���h�E�v���p�e�B

        private Bitmap cameraImage;
        private Rectangle imageRect;
        private volatile bool isCapturing;
        private volatile bool isActive;
        private CameraPipeline cameraPipeline;


        /// <summary>
        /// �J�������g�p�\���ǂ������擾����B
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
        }

        #endregion
        

        #region �R���X�g���N�^

        /// <summary>
        /// �J�����p�l��������������B
        /// </summary>
        public CameraPanel()
        {
            this.isActive = false;
            this.isCapturing = false;

            // GUI�ݒ�
            this.BackColor = Color.PeachPuff;
            this.Dock = DockStyle.Fill;
            this.Name = "CameraPanel";
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.Resize += new EventHandler(CameraPanel_Resize);

            // Intel Perceptual Computing SDK �J�����ݒ�
            // �J�����p�C�v���C�����\�z
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
                MessageBox.Show("�J�������ڑ�����Ă��Ȃ����J�����ɃG���[�������������߁A\r\n"
                    + "�J�����@�\���g�p���邱�Ƃ��ł��܂���B");
            }
        }

        #endregion

        public Bitmap CurrentImage()
        {
            return cameraPipeline.CurrentImage;
        }


        #region �J��������

        /// <summary>
        /// �J�����摜�̃L���v�`�����J�n����B
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
        /// �J�����摜�̃L���v�`�����~����B
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
        /// �J�������\�[�X���J������B
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


        #region �摜�\���T�C�Y�ݒ�

        /// <summary>
        /// �摜�\���T�C�Y���p�l���̃T�C�Y�ɍ��킹�Đݒ肷��B
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




        #region �C�x���g

        /// <summary>
        /// �T�C�Y�ύX���������Ƃ��͉摜�\���T�C�Y��ύX����B
        /// </summary>
        private void CameraPanel_Resize(object sender, EventArgs e)
        {
            ResetImageRect();
        }

        #endregion
    }
}
