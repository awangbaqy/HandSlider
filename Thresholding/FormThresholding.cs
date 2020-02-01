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
using System.Text;
using System.Windows.Forms;

namespace Thresholding
{
    public partial class FormThresholding : Form
    {
        Bitmap bit, bit2, bit3, destImage = new Bitmap(320, 240);
        FilterInfoCollection fic;
        Graphics graphics, g;
        GrayWorld grayWorld = new GrayWorld();
        ImageAttributes wrapMode;
        Rectangle destRect = new Rectangle(0, 0, 320, 240);
        TestThresholding tt2, tt3, tt4, tt5, tt6, tt7, tt8, tt9, tt10, tt11, tt12;
        VideoCaptureDevice vcd;

        public FormThresholding()
        {
            InitializeComponent();
            tt2 = tt3 = tt4 = tt5 = tt6 = tt7 = tt8 = tt9 = tt10 = tt11 = tt12 = new TestThresholding();
        }

        private void FormThresholding_Load(object sender, EventArgs e)
        {
            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            vcd = new VideoCaptureDevice(fic[0].MonikerString);
            vcd.NewFrame += new NewFrameEventHandler(newFrame);
            vcd.Start();
        }

        void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bit = resize(eventArgs.Frame);

            lock (pictureBox1)
            {
                pictureBox1.Image = bit.Clone() as Bitmap;
            }

            /// R G B ///

            //lock (pictureBox2)
            //{
            //    pictureBox2.Image = tt2.threshold_rgb(bit.Clone() as Bitmap, 1);
            //}

            //lock (pictureBox3)
            //{
            //    pictureBox3.Image = tt3.threshold_rgb(bit.Clone() as Bitmap, 2);
            //}

            //lock (pictureBox4)
            //{
            //    pictureBox4.Image = tt4.threshold_rgb(bit.Clone() as Bitmap, 3);
            //}

            //lock (pictureBox5)
            //{
            //    pictureBox5.Image = tt5.threshold_rgb(bit.Clone() as Bitmap, 4);
            //}

            //lock (pictureBox6)
            //{
            //    pictureBox6.Image = tt6.threshold_rgb(bit.Clone() as Bitmap, 5);
            //}

            /// H S V ///

            //lock (pictureBox2)
            //{
            //    pictureBox2.Image = tt2.threshold_hsv(bit.Clone() as Bitmap, 1);
            //}

            //lock (pictureBox3)
            //{
            //    pictureBox3.Image = tt3.threshold_hsv(bit.Clone() as Bitmap, 2);
            //}

            //lock (pictureBox4)
            //{
            //    pictureBox4.Image = tt4.threshold_hsv(bit.Clone() as Bitmap, 3);
            //}

            //lock (pictureBox5)
            //{
            //    bit2 = grayWorld.Apply(bit.Clone() as Bitmap);
            //    pictureBox5.Image = tt5.threshold_hsv(bit2.Clone() as Bitmap, 4);
            //}

            //lock (pictureBox6)
            //{
            //    pictureBox6.Image = tt6.threshold_hsv(bit.Clone() as Bitmap, 5);
            //}

            //lock (pictureBox7)
            //{
            //    pictureBox7.Image = tt7.threshold_hsv(bit.Clone() as Bitmap, 6);
            //}

            /// Y Cb Cr ///

            //lock (pictureBox2)
            //{
            //    pictureBox2.Image = tt2.threshold_ybr(bit.Clone() as Bitmap, 1);
            //}

            //lock (pictureBox3)
            //{
            //    pictureBox3.Image = tt3.threshold_ybr(bit.Clone() as Bitmap, 2);
            //}

            //lock (pictureBox4)
            //{
            //    pictureBox4.Image = tt4.threshold_ybr(bit.Clone() as Bitmap, 3);
            //}

            //lock (pictureBox5)
            //{
            //    pictureBox5.Image = tt5.threshold_ybr(bit.Clone() as Bitmap, 4);
            //}

            //lock (pictureBox6)
            //{
            //    pictureBox6.Image = tt6.threshold_ybr(bit.Clone() as Bitmap, 5);
            //}

            //lock (pictureBox7)
            //{
            //    pictureBox7.Image = tt7.threshold_ybr(bit.Clone() as Bitmap, 6);
            //}

            //lock (pictureBox8)
            //{
            //    pictureBox8.Image = tt8.threshold_ybr(bit.Clone() as Bitmap, 7);
            //}

            //lock (pictureBox9)
            //{
            //    pictureBox9.Image = tt9.threshold_ybr(bit.Clone() as Bitmap, 8);
            //}

            //lock (pictureBox10)
            //{
            //    pictureBox10.Image = tt10.threshold_ybr(bit.Clone() as Bitmap, 9);
            //}

            //lock (pictureBox11)
            //{
            //    pictureBox11.Image = tt11.threshold_ybr(bit.Clone() as Bitmap, 10);
            //}

            //lock (pictureBox12)
            //{
            //    pictureBox12.Image = tt12.threshold_ybr(bit.Clone() as Bitmap, 11);
            //}

            /// F I N A L ///

            lock (pictureBox2)
            {
                pictureBox2.Image = tt2.threshold_final(bit.Clone() as Bitmap, 1);
            }

            lock (pictureBox3)
            {
                pictureBox3.Image = tt3.threshold_final(bit.Clone() as Bitmap, 2);
            }

            lock (pictureBox4)
            {
                pictureBox4.Image = tt4.threshold_final(bit.Clone() as Bitmap, 3);
            }

            lock (pictureBox5)
            {
                pictureBox5.Image = tt5.threshold_final(bit.Clone() as Bitmap, 4);
            }

            lock (pictureBox6)
            {
                pictureBox6.Image = tt6.threshold_final(bit.Clone() as Bitmap, 5);
            }

            lock (pictureBox7)
            {
                pictureBox7.Image = tt7.threshold_final(bit.Clone() as Bitmap, 6);
            }

            lock (pictureBox8)
            {
                pictureBox8.Image = tt8.threshold_final(bit.Clone() as Bitmap, 7);
            }

            lock (pictureBox9)
            {
                pictureBox9.Image = tt9.threshold_final(bit.Clone() as Bitmap, 8);
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

        private void FormThresholding_FormClosing(object sender, FormClosingEventArgs e)
        {
            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();
        }
    }
}
