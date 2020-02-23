using Accord.Imaging.Filters;
using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Thresholding
{
    public partial class FormThresholding : Form
    {
        Bitmap bitmap, bitmap2, bitmap3, destitationBitmap;
        FilterInfoCollection filterInfoCollection;
        Graphics graphics;
        ImageAttributes imageAttributes;
        Rectangle convertRectangle, destinationRectangle;
        VideoCaptureDevice videoCaptureDevice;

        GammaCorrection gammaCorrection;
        GrayWorld grayWorld;
        TestThresholding tt1, tt2, tt3, tt4, tt5, tt6, tt7, tt8, tt9, tt10, tt11;

        Timer moveTimer = new Timer();
        int counter = 0;

        public FormThresholding()
        {
            InitializeComponent();

            destitationBitmap = new Bitmap(196, 144);
            convertRectangle = destinationRectangle = new Rectangle(0, 0, 196, 144);

            gammaCorrection = new GammaCorrection();
            grayWorld = new GrayWorld();
            tt1 = tt2 = tt3 = tt4 = tt5 = tt6 = tt7 = tt8 = tt9 = tt10 = tt11 = new TestThresholding();
        }

        private void FormThresholding_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
            videoCaptureDevice.NewFrame += new NewFrameEventHandler(newFrame);
            videoCaptureDevice.Start();

            //moveTimer.Interval = 2000;
            //moveTimer.Tick += new EventHandler(moveTimer_Tick);
            //moveTimer.Start();
        }

        void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bitmap = resize(eventArgs.Frame);

            lock (pictureBox0)
            {
                pictureBox0.Image = bitmap.Clone() as Bitmap;
            }

            if (radioButton_RGB.Checked)
            {
                lock (pictureBox1)
                {
                    bitmap2 = gammaCorrection.Apply(bitmap.Clone(convertRectangle, PixelFormat.Format24bppRgb) as Bitmap);
                    bitmap2 = grayWorld.Apply(bitmap2);
                    pictureBox1.Image = tt1.threshold_rgb(bitmap2.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_rgb(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_rgb(bitmap.Clone() as Bitmap, 3);
                }

                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
            else if (radioButton_HSV.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = tt1.threshold_hsv(bitmap.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_hsv(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_hsv(bitmap.Clone() as Bitmap, 3);
                }

                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
            else if (radioButton_YCbCr.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = tt1.threshold_ycbcr(bitmap.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_ycbcr(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_ycbcr(bitmap.Clone() as Bitmap, 3);
                }

                lock (pictureBox4)
                {
                    pictureBox4.Image = tt4.threshold_ycbcr(bitmap.Clone() as Bitmap, 4);
                }

                lock (pictureBox5)
                {
                    pictureBox5.Image = tt5.threshold_ycbcr(bitmap.Clone() as Bitmap, 5);
                }

                lock (pictureBox6)
                {
                    pictureBox6.Image = tt6.threshold_ycbcr(bitmap.Clone() as Bitmap, 6);
                }

                lock (pictureBox7)
                {
                    pictureBox7.Image = tt7.threshold_ycbcr(bitmap.Clone() as Bitmap, 7);
                }

                lock (pictureBox8)
                {
                    pictureBox8.Image = tt8.threshold_ycbcr(bitmap.Clone() as Bitmap, 8);
                }

                lock (pictureBox9)
                {
                    pictureBox9.Image = tt9.threshold_ycbcr(bitmap.Clone() as Bitmap, 9);
                }

                lock (pictureBox10)
                {
                    pictureBox10.Image = tt10.threshold_ycbcr(bitmap.Clone() as Bitmap, 10);
                }
            }
            else if (radioButton_Combined.Checked)
            {
                lock (pictureBox1)
                {
                    bitmap3 = grayWorld.Apply(bitmap.Clone() as Bitmap);
                    pictureBox1.Image = tt1.threshold_combined(bitmap3, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_combined(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_combined(bitmap.Clone() as Bitmap, 3);
                }

                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
            else if (radioButton_Final.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = tt1.threshold_final(bitmap.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_final(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_final(bitmap.Clone() as Bitmap, 3);
                }

                lock (pictureBox4)
                {
                    pictureBox4.Image = tt4.threshold_final(bitmap.Clone() as Bitmap, 4);
                }

                lock (pictureBox5)
                {
                    pictureBox5.Image = tt5.threshold_final(bitmap.Clone() as Bitmap, 5);
                }

                lock (pictureBox6)
                {
                    pictureBox6.Image = tt6.threshold_final(bitmap.Clone() as Bitmap, 6);
                }
                
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
        }
        
        private Bitmap resize(Bitmap bitmap)
        {
            destitationBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            using (graphics = Graphics.FromImage(destitationBitmap))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(bitmap, destinationRectangle, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }

            return destitationBitmap;
        }

        private void FormThresholding_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoCaptureDevice.NewFrame -= new NewFrameEventHandler(newFrame);
            videoCaptureDevice.Stop();
            videoCaptureDevice.SignalToStop();
        }
        
        private void moveTimer_Tick(object sender, System.EventArgs e)
        {
            string[] images = Directory.GetFiles(@"C:\Users\hp\Desktop\Ve", "*.jpg");
            Image image = Image.FromFile(images[counter]);
            lblNama.Text = Path.GetFileNameWithoutExtension(images[counter]);

            testdata((Bitmap)image);

            if (counter < images.Count() - 1)
            {
                counter = counter + 1;
            }
            else
            {
                counter = 0;
            }
        }

        public void testdata(Bitmap bitmapInput)
        {
            bitmap = resize(bitmapInput);

            lock (pictureBox0)
            {
                pictureBox0.Image = bitmap.Clone() as Bitmap;
            }

            if (radioButton_RGB.Checked)
            {
                lock (pictureBox1)
                {
                    bitmap2 = gammaCorrection.Apply(bitmap.Clone(convertRectangle, PixelFormat.Format24bppRgb) as Bitmap);
                    bitmap2 = grayWorld.Apply(bitmap2);
                    pictureBox1.Image = tt1.threshold_rgb(bitmap2.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_rgb(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_rgb(bitmap.Clone() as Bitmap, 3);
                }

                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
            else if (radioButton_HSV.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = tt1.threshold_hsv(bitmap.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_hsv(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_hsv(bitmap.Clone() as Bitmap, 3);
                }

                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
            else if (radioButton_YCbCr.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = tt1.threshold_ycbcr(bitmap.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_ycbcr(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_ycbcr(bitmap.Clone() as Bitmap, 3);
                }

                lock (pictureBox4)
                {
                    pictureBox4.Image = tt4.threshold_ycbcr(bitmap.Clone() as Bitmap, 4);
                }

                lock (pictureBox5)
                {
                    pictureBox5.Image = tt5.threshold_ycbcr(bitmap.Clone() as Bitmap, 5);
                }

                lock (pictureBox6)
                {
                    pictureBox6.Image = tt6.threshold_ycbcr(bitmap.Clone() as Bitmap, 6);
                }

                lock (pictureBox7)
                {
                    pictureBox7.Image = tt7.threshold_ycbcr(bitmap.Clone() as Bitmap, 7);
                }

                lock (pictureBox8)
                {
                    pictureBox8.Image = tt8.threshold_ycbcr(bitmap.Clone() as Bitmap, 8);
                }

                lock (pictureBox9)
                {
                    pictureBox9.Image = tt9.threshold_ycbcr(bitmap.Clone() as Bitmap, 9);
                }

                lock (pictureBox10)
                {
                    pictureBox10.Image = tt10.threshold_ycbcr(bitmap.Clone() as Bitmap, 10);
                }
            }
            else if (radioButton_Combined.Checked)
            {
                lock (pictureBox1)
                {
                    bitmap3 = grayWorld.Apply(bitmap.Clone() as Bitmap);
                    pictureBox1.Image = tt1.threshold_combined(bitmap3, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_combined(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_combined(bitmap.Clone() as Bitmap, 3);
                }

                pictureBox4.Image = null;
                pictureBox5.Image = null;
                pictureBox6.Image = null;
                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
            else if (radioButton_Final.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = tt1.threshold_final(bitmap.Clone() as Bitmap, 1);
                }

                lock (pictureBox2)
                {
                    pictureBox2.Image = tt2.threshold_final(bitmap.Clone() as Bitmap, 2);
                }

                lock (pictureBox3)
                {
                    pictureBox3.Image = tt3.threshold_final(bitmap.Clone() as Bitmap, 3);
                }

                lock (pictureBox4)
                {
                    pictureBox4.Image = tt4.threshold_final(bitmap.Clone() as Bitmap, 4);
                }

                lock (pictureBox5)
                {
                    pictureBox5.Image = tt5.threshold_final(bitmap.Clone() as Bitmap, 5);
                }

                lock (pictureBox6)
                {
                    pictureBox6.Image = tt6.threshold_final(bitmap.Clone() as Bitmap, 6);
                }

                pictureBox7.Image = null;
                pictureBox8.Image = null;
                pictureBox9.Image = null;
                pictureBox10.Image = null;
            }
        }
    }
}
