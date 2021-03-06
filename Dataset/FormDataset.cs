﻿using Accord.Imaging;
using Accord.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Dataset
{
    public partial class FormDataset : Form
    {
        BinaryErosion3x3 binaryErosion3x3;
        BinaryDilation3x3 binaryDilation3x3;
        Bitmap destinationBitmap, resultThresholding, resultMorphologing, resultBlobing, resultResizing, resultBinaring, returnedBitmap;
        BitmapData bitmapData;
        BlobCounter blobCounter;
        Blob[] blobs;
        Closing closingRadius10;
        Color c, c0, c1, c2, c3, c4, c5, c6, c7;
        GaborFilter gaborFilter;
        Graphics graphics, g;
        Grayscale grayscale;
        GrayscaleToRGB grayscaleToRGB;
        ImageAttributes imageAttributes;
        Median median;
        Opening opening;
        Pen pen;
        Rectangle destinationRectangle;
        Threshold threshold;
        Timer moveTimer;
        ZhangSuenSkeletonization zhangSuenSkeletonization;

        bool skin;
        byte[] pixels;
        double hue, saturation, value;
        double ye, cebe, ceer;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, y, x, currentLine;
        int counter = 0;
        int jr, r0, r1, r2, r3, r4, r5, r6, r7;
        int oldBlue, oldGreen, oldRed, max, min;
        short[,] kernelShortRadius10 = {
            {0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0},
            {0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0},
            {0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
            {0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
            {0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0},
            {0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0},
            {0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0},
            {0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,0},
        };

        Color color;
        //Stopwatch stopwatchRGB = new Stopwatch(); Stopwatch stopwatchHSV = new Stopwatch(); Stopwatch stopwatchYCbCr = new Stopwatch();
        //List<double> elapsedRGB = new List<double>(), elapsedHSV = new List<double>(), elapsedYCbCr = new List<double>();

        public FormDataset()
        {
            InitializeComponent();

            binaryDilation3x3 = new BinaryDilation3x3();
            binaryErosion3x3 = new BinaryErosion3x3();
            blobCounter = new BlobCounter();
            closingRadius10 = new Closing(kernelShortRadius10);
            destinationBitmap = new Bitmap(20, 20);
            destinationRectangle = new Rectangle(0, 0, 20, 20);
            gaborFilter = new GaborFilter();
            grayscale = Grayscale.CommonAlgorithms.BT709;
            grayscaleToRGB = new GrayscaleToRGB();
            median = new Median();
            moveTimer = new Timer();
            opening = new Opening();
            pen = new Pen(Color.Red);
            threshold = new Threshold(128);
            zhangSuenSkeletonization = new ZhangSuenSkeletonization();
        }

        private void FormDataset_Load(object sender, EventArgs e)
        {
            //moveTimer.Interval = 1500;
            //moveTimer.Tick += new EventHandler(moveTimer_Tick);
            //moveTimer.Start();

            //int[] size = new int[] { 5, 10, 15, 20, 25, 30, 35 };
            //for (int i = 0; i < size.Length; i++)
            //{
                //Console.WriteLine("ukuran " + size[i]);
                //destinationBitmap = new Bitmap(size[i], size[i]);
                //destinationRectangle = new Rectangle(0, 0, size[i], size[i]);

                //Console.WriteLine("F");
                foreach (string item in Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Flat", "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);

                    //returnedBitmap = pre_processing(image);
                    //returnedBitmap.Save(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Flat\" + Path.GetFileName(item) + "_blob.jpg");
                }
                //Console.WriteLine("S");
                foreach (string item in Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Spread", "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);

                    //returnedBitmap = pre_processing(image);
                    //returnedBitmap.Save(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Spread\" + Path.GetFileName(item) + "_blob.jpg");
                }
                //Console.WriteLine("V");
                foreach (string item in Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Ve", "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);

                    //returnedBitmap = pre_processing(image);
                    //returnedBitmap.Save(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Ve\" + Path.GetFileName(item) + "_blob.jpg");
                }

                //Console.WriteLine("bloody hell"); Console.WriteLine("bloody hell"); Console.WriteLine("bloody hell"); Console.WriteLine("bloody hell"); Console.WriteLine("bloody hell");

                // Testing
                //Console.WriteLine("F");
                foreach (string item in Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Testing\Flat", "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);

                    //returnedBitmap = pre_processing(image);
                    //returnedBitmap.Save(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Testing\Flat\" + Path.GetFileName(item) + "_blob.jpg");
                }
                //Console.WriteLine("S");
                foreach (string item in Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Testing\Spread", "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);

                    //returnedBitmap = pre_processing(image);
                    //returnedBitmap.Save(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Testing\Spread\" + Path.GetFileName(item) + "_blob.jpg");
                }
                //Console.WriteLine("V");
                foreach (string item in Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Testing\Ve", "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);

                    //returnedBitmap = pre_processing(image);
                    //returnedBitmap.Save(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Testing\Ve\" + Path.GetFileName(item) + "_blob.jpg");
                }
            //}

            //Console.WriteLine("rgb , sum " + elapsedRGB.Sum() + " mean " + (elapsedRGB.Sum() / 526));
            //Console.WriteLine("hsv , sum " + elapsedHSV.Sum() + " mean " + (elapsedHSV.Sum() / 526));
            //Console.WriteLine("ybr , sum " + elapsedYCbCr.Sum() + " mean " + (elapsedYCbCr.Sum() / 526));

            Close();

            pictureBox3.SuspendLayout();
            pictureBox3.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox3_Paint);
            pictureBox3.ResumeLayout();
        }

        private void moveTimer_Tick(object sender, System.EventArgs e)
        {
            string[] images = Directory.GetFiles(@"B:\My Documents\Polinema\Skripsi\dataset used (set)\==Training\Ve", "*.jpg");
            System.Drawing.Image image = System.Drawing.Image.FromFile(images[counter]);
            pre_processing(image);

            //returnedBitmap.Save(@"C:\Users\hp\Desktop\Ve\image" + counter + ".png");

            if (counter < images.Count() - 1)
            {
                counter = counter + 1;
            }
            else
            {
                counter = 0;
            }
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                blobCounter.ProcessImage((Bitmap)pictureBox3.Image);
                blobs = blobCounter.GetObjectsInformation();
                g = e.Graphics;
                foreach (Blob blob in blobs)
                {
                    g.DrawRectangle(pen, blob.Rectangle);
                }
            }
        }

        private void inputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            
            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                List<System.Drawing.Image> pictureArray = new List<System.Drawing.Image>();
                foreach (string item in Directory.GetFiles(folderDlg.SelectedPath, "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(item);

                    pre_processing(image);
                }
            }
        }

        private Bitmap thresholding(Bitmap bitmap)
        {
            bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            byteCount = bitmapData.Stride * bitmap.Height;
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
                    skin = false;

                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];

                    //stopwatchRGB.Start();
                    //if (oldRed > 95 && oldGreen > 40 && oldBlue > 20 &&
                    //    (Math.Max(oldRed, Math.Max(oldGreen, oldBlue)) - Math.Min(oldRed, Math.Min(oldGreen, oldBlue))) > 15 &&
                    //    Math.Abs(oldRed - oldGreen) > 15 && oldRed > oldGreen && oldRed > oldBlue
                    //   )
                    //{
                    //    pixels[currentLine + x] = (byte)255;
                    //    pixels[currentLine + x + 1] = (byte)255;
                    //    pixels[currentLine + x + 2] = (byte)255;
                    //}
                    //else
                    //{
                    //    pixels[currentLine + x] = (byte)0;
                    //    pixels[currentLine + x + 1] = (byte)0;
                    //    pixels[currentLine + x + 2] = (byte)0;
                    //}
                    //stopwatchRGB.Stop(); elapsedRGB.Add(stopwatchRGB.ElapsedMilliseconds); stopwatchRGB.Reset();

                    //stopwatchHSV.Start();
                    //color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    //hue = color.GetHue();
                    //saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    //value = max / 255d;

                    //if (
                    //    (
                    //     (0 < (hue / 360) && (hue / 360) < 0.24) ||
                    //     (0.74 < (hue / 360) && (hue / 360) < 1)
                    //    ) &&
                    //    0.16 < saturation && saturation < 0.79
                    //   )
                    //{
                    //    pixels[currentLine + x] = (byte)255;
                    //    pixels[currentLine + x + 1] = (byte)255;
                    //    pixels[currentLine + x + 2] = (byte)255;
                    //}
                    //else
                    //{
                    //    pixels[currentLine + x] = (byte)0;
                    //    pixels[currentLine + x + 1] = (byte)0;
                    //    pixels[currentLine + x + 2] = (byte)0;
                    //}
                    //stopwatchHSV.Stop(); elapsedHSV.Add(stopwatchHSV.ElapsedMilliseconds); stopwatchHSV.Reset();

                    //stopwatchYCbCr.Start();
                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (78 <= cebe && cebe <= 126 && 134 <= ceer && ceer <= 172)
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
                    //stopwatchYCbCr.Stop(); elapsedYCbCr.Add(stopwatchYCbCr.ElapsedMilliseconds); stopwatchYCbCr.Reset();
                }
            }

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private Bitmap morphologing(Bitmap bitmap)
        {
            return median.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(grayscale.Apply(bitmap))));
        }

        private Bitmap resizing(Bitmap bitmap)
        {
            destinationBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            using (graphics = Graphics.FromImage(destinationBitmap))
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

            return destinationBitmap;
        }

        private string chaincode(Bitmap bitmap)
        {
            string str = "";
            
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (x == 0 || (x > 1 && x % 2 == 0))
                {
                    for (int y = bitmap.Height - 1; y >= 0; y--)
                    {
                        Color clr = bitmap.GetPixel(x, y);
                        if (x < bitmap.Width - 1)
                        {
                            c0 = bitmap.GetPixel(x + 1, y);
                            r0 = c0.R;
                            if (r0 == 0) { r0 = 0; }
                            else { r0 = 1; }
                        }
                        else { r0 = 0; }

                        if (x < bitmap.Width - 1 && y > 0)
                        {
                            c1 = bitmap.GetPixel(x + 1, y - 1);
                            r1 = c1.R;
                            if (r1 == 0) { r1 = 0; }
                            else { r1 = 1; }
                        }
                        else { r1 = 0; }

                        if (y > 0)
                        {
                            c2 = bitmap.GetPixel(x, y - 1);
                            r2 = c2.R;
                            if (r2 == 0) { r2 = 0; }
                            else { r2 = 1; }
                        }
                        else { r2 = 0; }

                        if (x > 0 && y > 0)
                        {
                            c3 = bitmap.GetPixel(x - 1, y - 1);
                            r3 = c3.R;
                            if (r3 == 0) { r3 = 0; }
                            else { r3 = 1; }
                        }
                        else { r3 = 0; }

                        if (x > 0)
                        {
                            c4 = bitmap.GetPixel(x - 1, y);
                            r4 = c4.R;
                            if (r4 == 0) { r4 = 0; }
                            else { r4 = 1; }
                        }
                        else { r4 = 0; }

                        if (x > 0 && y < bitmap.Height - 1)
                        {
                            c5 = bitmap.GetPixel(x - 1, y + 1);
                            r5 = c5.R;
                            if (r5 == 0) { r5 = 0; }
                            else { r5 = 1; }
                        }
                        else { r5 = 0; }

                        if (y < bitmap.Height - 1)
                        {
                            c6 = bitmap.GetPixel(x, y + 1);
                            r6 = c6.R;
                            if (r6 == 0) { r6 = 0; }
                            else { r6 = 1; }
                        }
                        else { r6 = 0; }

                        if (x < bitmap.Width - 1 && y < bitmap.Height - 1)
                        {
                            c7 = bitmap.GetPixel(x + 1, y + 1);
                            r7 = c7.R;
                            if (r7 == 0) { r7 = 0; }
                            else { r7 = 1; }
                        }
                        else { r7 = 0; }

                        jr = r0 + r1 + r2 + r3 + r4 + r5 + r6 + r7;
                        str = str + jr.ToString();
                    }
                }
                else if (x == 1 || (x > 1 && x % 2 == 1))
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color clr = bitmap.GetPixel(x, y);
                        if (x < bitmap.Width - 1)
                        {
                            c0 = bitmap.GetPixel(x + 1, y);
                            r0 = c0.R;
                            if (r0 == 0) { r0 = 0; }
                            else { r0 = 1; }
                        }
                        else { r0 = 0; }

                        if (x < bitmap.Width - 1 && y > 0)
                        {
                            c1 = bitmap.GetPixel(x + 1, y - 1);
                            r1 = c1.R;
                            if (r1 == 0) { r1 = 0; }
                            else { r1 = 1; }
                        }
                        else { r1 = 0; }

                        if (y > 0)
                        {
                            c2 = bitmap.GetPixel(x, y - 1);
                            r2 = c2.R;
                            if (r2 == 0) { r2 = 0; }
                            else { r2 = 1; }
                        }
                        else { r2 = 0; }

                        if (x > 0 && y > 0)
                        {
                            c3 = bitmap.GetPixel(x - 1, y - 1);
                            r3 = c3.R;
                            if (r3 == 0) { r3 = 0; }
                            else { r3 = 1; }
                        }
                        else { r3 = 0; }

                        if (x > 0)
                        {
                            c4 = bitmap.GetPixel(x - 1, y);
                            r4 = c4.R;
                            if (r4 == 0) { r4 = 0; }
                            else { r4 = 1; }
                        }
                        else { r4 = 0; }

                        if (x > 0 && y < bitmap.Height - 1)
                        {
                            c5 = bitmap.GetPixel(x - 1, y + 1);
                            r5 = c5.R;
                            if (r5 == 0) { r5 = 0; }
                            else { r5 = 1; }
                        }
                        else { r5 = 0; }

                        if (y < bitmap.Height - 1)
                        {
                            c6 = bitmap.GetPixel(x, y + 1);
                            r6 = c6.R;
                            if (r6 == 0) { r6 = 0; }
                            else { r6 = 1; }
                        }
                        else { r6 = 0; }

                        if (x < bitmap.Width - 1 && y < bitmap.Height - 1)
                        {
                            c7 = bitmap.GetPixel(x + 1, y + 1);
                            r7 = c7.R;
                            if (r7 == 0) { r7 = 0; }
                            else { r7 = 1; }
                        }
                        else { r7 = 0; }

                        jr = r0 + r1 + r2 + r3 + r4 + r5 + r6 + r7;
                        str = str + jr.ToString();
                    }
                }
            }
            
            return str;
        }

        private string code(Bitmap bitmap)
        {
            string str = "";

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    c = bitmap.GetPixel(i, j);

                    if (c.R == 0)
                    {
                        str = str + 0;
                    }
                    else
                    {
                        str = str + 1;
                    }
                }
            }

            return str;
        }

        private string lineHX(Bitmap bit)
        {
            string str = "";

            for (int i = 0; i < bit.Width; i++)
            {
                int count = 0;
                for (int j = 0; j < bit.Height; j++)
                {
                    Color c = bit.GetPixel(i, j);

                    if ((c.R + c.G + c.G) / 3 > 127)
                    {
                        count += 1;
                    }
                }

                str += count.ToString();

                if (i != bit.Width - 1)
                {
                    str += "-";
                }
            }

            return str;
        }

        private string lineVY(Bitmap bit)
        {
            string str = "";

            for (int i = 0; i < bit.Width; i++)
            {
                int count = 0;
                for (int j = 0; j < bit.Height; j++)
                {
                    Color c = bit.GetPixel(j, i);

                    if ((c.R + c.G + c.G) / 3 > 127)
                    {
                        count += 1;
                    }
                }

                str += count.ToString();

                if (i != bit.Width - 1)
                {
                    str += "-";
                }
            }

            return str;
        }

        //private Bitmap pre_processing(System.Drawing.Image image)
        private void pre_processing(System.Drawing.Image image)
        {
            string str = "awal";

            pictureBox1.Image = image.Clone() as Bitmap;

            resultThresholding = thresholding((Bitmap)image);
            pictureBox2.Image = resultThresholding.Clone() as Bitmap;

            resultMorphologing = morphologing(resultThresholding);
            pictureBox3.Image = resultMorphologing.Clone() as Bitmap;

            blobCounter.ProcessImage(resultMorphologing);
            resultBlobing = new ExtractBiggestBlob().Apply(resultMorphologing);
            //double ratio = Convert.ToDouble(resultBlobing.Height) / Convert.ToDouble(resultBlobing.Width);
            //Console.WriteLine(ratio.ToString("F5").TrimEnd('0'));
            pictureBox4.Image = resultBlobing.Clone() as Bitmap;

            resultResizing = resizing(resultBlobing);
            pictureBox5.Image = resultResizing.Clone() as Bitmap;

            resultBinaring = threshold.Apply(grayscale.Apply(resultResizing));
            pictureBox6.Image = resultBinaring.Clone() as Bitmap;

            str = code(resultBinaring);
            for (int i = 1; i < (resultBinaring.Width * resultBinaring.Height * 2) - 1; i++)
            {
                if (i % 2 != 0)
                {
                    str = str.Insert(i, "-");
                }
            }
            textBox1.Text = str;

            //Console.WriteLine(str);

            //return resultBlobing;
        }
    }
}
