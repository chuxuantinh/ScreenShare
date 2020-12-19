using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenShare
{
    public class ScreenCapture
    {
        private static ImageFormat imageFormat = ImageFormat.Jpeg;
        private static int count = 0;

        public static Stream CaptureScreen(RequestInfo ri)
        { 
            return CaptureScreen(ri, null);
        }

        public static Stream CaptureScreen(RequestInfo ri, string fileName)
        {
            Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            MemoryStream ms = null;
            using (Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height))
            { 
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, rectangle.Size);
                }

                try
                {
                    ms = new MemoryStream();
                    ResizeImage(bitmap, ri).Save(ms, imageFormat);
                }
                catch (Exception e)
                {
                    ScrShare.OnException("Capture screen resize error.", e);
                }

                if (!string.IsNullOrEmpty(fileName))
                    // Save bitmap to file
                    SaveToFile(bitmap, fileName);    
            } 
            
            return ms;
        }

        private static void SaveToFile(Bitmap bitmap, string fileName)
        {
            bitmap.Save(string.Format("{0}{1}.{2}", fileName, count, imageFormat.ToString()), imageFormat);
        }

        private static Image ResizeImage(Image image, RequestInfo ri)
        {
            // Get the new size
            Size newSize = ri.IsFullSize ? new Size(image.Width, image.Height)
                                         : new Size((int)(image.Width  * ScrShare.ScreenMagnificationFactor),
                                                    (int)(image.Height * ScrShare.ScreenMagnificationFactor));

            State state = new State();
            state.clientScreenSizeFactor = (double)newSize.Width / (double)image.Width;
            RequestInfo.dctRequestInfo[ri.SessionId] = state;
 
            // Create a new Bitmap the size of the new image
            Bitmap bitmap = new Bitmap(newSize.Width, newSize.Height);

            // Create a new graphic from the Bitmap
            Graphics graphic = Graphics.FromImage(bitmap);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw the newly resized image
            graphic.DrawImage(image, 0, 0, newSize.Width, newSize.Height);

            graphic.Dispose();

            return bitmap;
        }
    }
}
