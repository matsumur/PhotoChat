using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PhotoChat
{
    class CameraPipeline : UtilMPipeline
    {
        pxcmStatus sts;
        Bitmap cameraImage;
        PXCMSession session;
        bool isCapturing;
        private CameraPanel cameraPanel;

        public CameraPipeline(CameraPanel cameraPanel, Bitmap cameraImage)
        {
            // TODO: Complete member initialization
            this.cameraPanel = cameraPanel;
            this.cameraImage = cameraImage;

            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, 640, 480);
            sts = PXCMSession.CreateInstance(out session);
            this.isCapturing = false;
        }

        public void setCapturing(bool enable)
        {
            this.isCapturing = enable;
        }

        public Bitmap CurrentImage
        {
            get { return cameraImage; }
        }

        public override bool OnNewFrame()
        {
            if (isCapturing)
            {
                try
                {
                    PXCMImage img = QueryImage(PXCMImage.ImageType.IMAGE_TYPE_COLOR);

                    Bitmap bitmap;
                    sts = img.QueryBitmap(session, out bitmap);
                    Graphics g = cameraPanel.CreateGraphics();
                    g.DrawImage(bitmap, new Point(0, 0));
                    cameraImage = new Bitmap(bitmap);
                    
                }
                catch (Exception ex)
                {
                    ; ;
                }
            }
            return true;
        }
    }
}
