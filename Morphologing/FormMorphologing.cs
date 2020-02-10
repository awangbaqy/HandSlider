using Accord.Imaging.Filters;
using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Morphologing
{
    public partial class FormMorphologing : Form
    {
        Bitmap bit, destImage = new Bitmap(480, 360);
        BitmapData bitmapData;
        FilterInfoCollection fic;
        Graphics graphics, g;
        ImageAttributes wrapMode;
        Rectangle destRect = new Rectangle(0, 0, 480, 360);
        VideoCaptureDevice vcd;

        AdaptiveSmoothing adaptiveSmoothing = new AdaptiveSmoothing();
        BilateralSmoothing bilateralSmoothing = new BilateralSmoothing();
        Closing closing = new Closing();
        ConservativeSmoothing conservativeSmoothing = new ConservativeSmoothing();
        Dilation dilation = new Dilation();
        Erosion erosion = new Erosion();
        Mean mean = new Mean();
        Median median = new Median();
        Opening opening = new Opening();
        TopHat topHat = new TopHat();
        
        double ye, cebe, ceer;

        byte[] pixels;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, y, x, currentLine;
        int oldBlue, oldGreen, oldRed;

        public FormMorphologing()
        {
            InitializeComponent();
        }

        private void FormMorphologing_Load(object sender, EventArgs e)
        {
            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            vcd = new VideoCaptureDevice(fic[0].MonikerString);
            vcd.NewFrame += new NewFrameEventHandler(newFrame);
            vcd.Start();
        }

        void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bit = resize(eventArgs.Frame);

            lock (pictureBox0)
            {
                pictureBox0.Image = bit.Clone() as Bitmap;
            }

            bit = thresholding(bit);
            bit = Grayscale.CommonAlgorithms.BT709.Apply(bit);

            lock (pictureBox1)
            {
                if (radioButton1.Checked)
                {
                    bit = conservativeSmoothing.Apply(dilation.Apply(erosion.Apply(bit)));
                }
                else if (radioButton2.Checked)
                {
                    bit = topHat.Apply(bit);
                }
                else if (radioButton3.Checked)
                {
                    bit = closing.Apply(opening.Apply(erosion.Apply(dilation.Apply(bit))));
                }
                else if (radioButton4.Checked)
                {
                    
                }
                else if (radioButton5.Checked)
                {
                    
                }
                else if (radioButton6.Checked)
                {
                    
                }
                else if (radioButton7.Checked)
                {
                    
                }
                else if (radioButton8.Checked)
                {

                }
                else if (radioButton9.Checked)
                {

                }
                else if (radioButton10.Checked)
                {

                }
                else if (radioButton11.Checked)
                {

                }
                else if (radioButton12.Checked)
                {

                }
                else if (radioButton13.Checked)
                {

                }
                else if (radioButton14.Checked)
                {

                }
                else if (radioButton15.Checked)
                {

                }
                else if (radioButton16.Checked)
                {

                }
                else if (radioButton17.Checked)
                {

                }
                else if (radioButton18.Checked)
                {

                }
                else if (radioButton19.Checked)
                {

                }
                else if (radioButton20.Checked)
                {

                }

                pictureBox1.Image = bit;
            }
        }

        private Bitmap resize(Bitmap bit)
        {
            destImage.SetResolution(bit.HorizontalResolution, bit.VerticalResolution);

            using (graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(bit, destRect, 0, 0, bit.Width, bit.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private Bitmap thresholding(Bitmap bit)
        {
            bitmapData = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, bit.PixelFormat);

            bytesPerPixel = Bitmap.GetPixelFormatSize(bit.PixelFormat) / 8;
            byteCount = bitmapData.Stride * bit.Height;
            pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            heightInPixels = bitmapData.Height;
            widthInBytes = bitmapData.Width * bytesPerPixel;

            for (y = 0; y < heightInPixels; y++)
            {
                currentLine = y * bitmapData.Stride;
                for (x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];

                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        pixels[currentLine + x] = (byte)255;
                        pixels[currentLine + x + 1] = (byte)255;
                        pixels[currentLine + x + 2] = (byte)255;
                    }
                    else
                    {
                        pixels[currentLine + x] = (byte)0;
                        pixels[currentLine + x + 1] = (byte)0;
                        pixels[currentLine + x + 2] = (byte)0;
                    }
                }
            }

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }
        
        private void FormMorphologing_FormClosing(object sender, FormClosingEventArgs e)
        {
            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();
        }
    }
}
