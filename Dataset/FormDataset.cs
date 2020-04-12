﻿using Accord.Imaging;
using Accord.Imaging.Filters;
using System;
using System.Collections.Generic;
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
        Bitmap resultThresholding, resultMorphologing, destinationBitmap, resultResize;
        BitmapData bitmapData;
        Graphics graphics;
        ImageAttributes imageAttributes;
        Rectangle destinationRectangle;

        BlobCounter blobCounter;
        Blob[] blobs;
        Closing closingRadius10;
        Opening opening;

        Timer moveTimer = new Timer();
        int counter = 0;

        byte[] pixels;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, y, x, currentLine;
        int oldBlue, oldGreen, oldRed, max, min;
        double hue, saturation, value;
        double ye, cebe, ceer;

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

        Color c0, c1, c2, c3, c4, c5, c6, c7;
        int jr, r0, r1, r2, r3, r4, r5, r6, r7;

        public FormDataset()
        {
            InitializeComponent();

            blobCounter = new BlobCounter();
            closingRadius10 = new Closing(kernelShortRadius10);
            opening = new Opening();
            
            destinationBitmap = new Bitmap(9, 9);
            destinationRectangle = new Rectangle(0, 0, 9, 9);
        }

        private void FormDataset_Load(object sender, EventArgs e)
        {
            //moveTimer.Interval = 1000;
            //moveTimer.Tick += new EventHandler(moveTimer_Tick);
            //moveTimer.Start();
        }

        private void moveTimer_Tick(object sender, System.EventArgs e)
        {
            string[] images = Directory.GetFiles(@"C:\Users\hp\Desktop\Ve", "*.jpg");
            System.Drawing.Image image = System.Drawing.Image.FromFile(images[counter]);

            pictureBox1.Image = image;
            resultThresholding = thresholding((Bitmap)image);
            pictureBox2.Image = resultThresholding;
            resultMorphologing = morphologing(resultThresholding);
            pictureBox3.Image = resultMorphologing;
            blobCounter.ProcessImage(resultMorphologing);
            resultMorphologing = new ExtractBiggestBlob().Apply(resultMorphologing);
            pictureBox4.Image = resultMorphologing;
            resultResize = resizing(resultMorphologing);
            pictureBox5.Image = resultResize;

            labelHX.Text = cc(resultResize);
            //labelHX.Text = "H / X : " + lineHX(resultResize);
            //labelVY.Text = "V / Y : " + lineVY(resultResize);
            
            if (counter < images.Count() - 1)
            {
                counter = counter + 1;
            }
            else
            {
                counter = 0;
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
                string str = "";

                List<System.Drawing.Image> pictureArray = new List<System.Drawing.Image>();
                foreach (string item in Directory.GetFiles(folderDlg.SelectedPath, "*.jpg", SearchOption.AllDirectories))
                {
                    System.Drawing.Image _image = System.Drawing.Image.FromFile(item);

                    pictureBox1.Image = _image;
                    resultThresholding = thresholding((Bitmap)_image);
                    pictureBox2.Image = resultThresholding;
                    resultMorphologing = morphologing(resultThresholding);
                    pictureBox3.Image = resultMorphologing;
                    blobCounter.ProcessImage(resultMorphologing);
                    resultMorphologing = new ExtractBiggestBlob().Apply(resultMorphologing);
                    pictureBox4.Image = resultMorphologing;
                    resultResize = resizing(resultMorphologing);
                    pictureBox5.Image = resultResize;

                    //str = cc(resultResize);
                    //for (int i = 1; i < 199; i++)
                    //{
                    //    if (i % 2 != 0)
                    //    {
                    //        str = str.Insert(i, "-");
                    //    }
                    //}

                    labelHX.Text = "H / X : " + lineHX(resultResize);
                    labelVY.Text = "V / Y : " + lineVY(resultResize);

                    str = lineHX(resultResize) + "-" + lineVY(resultResize);

                    Console.WriteLine(str);
                }
            }
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

                    max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    hue = color.GetHue();
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;

                    //ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    //cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    //ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if ((
                         (0 < (hue / 360) && (hue / 360) < 0.24) ||
                         (0.74 < (hue / 360) && (hue / 360) < 1)
                        ) &&
                        0.16 < saturation && saturation < 0.79)
                    //if (77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        pixels[currentLine + x] = 255;
                        pixels[currentLine + x + 1] = 255;
                        pixels[currentLine + x + 2] = 255;
                    }
                    else
                    {
                        pixels[currentLine + x] = 0;
                        pixels[currentLine + x + 1] = 0;
                        pixels[currentLine + x + 2] = 0;
                    }
                }
            }

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        private Bitmap morphologing(Bitmap bit)
        {
            // circular filer radius 10
            return closingRadius10.Apply(opening.Apply(bit));
        }

        private Bitmap resizing(Bitmap bit)
        {
            destinationBitmap.SetResolution(bit.HorizontalResolution, bit.VerticalResolution);

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
                    graphics.DrawImage(bit, destinationRectangle, 0, 0, bit.Width, bit.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }

            return destinationBitmap;
        }

        public string cc(Bitmap bit)
        {
            string str = "";

            for (int x = 0; x < bit.Width; x++)
            {
                if (x == 0 || (x > 1 && x % 2 == 0))
                {
                    for (int y = bit.Height - 1; y >= 0; y--)
                    {
                        Color clr = bit.GetPixel(x, y);
                        if (x < bit.Width - 1)
                        {
                            c0 = bit.GetPixel(x + 1, y);
                            r0 = c0.R;
                            if (r0 == 255) { r0 = 0; }
                            else { r0 = 1; }
                        }
                        else { r0 = 0; }

                        if (x < bit.Width - 1 && y > 0)
                        {
                            c1 = bit.GetPixel(x + 1, y - 1);
                            r1 = c1.R;
                            if (r1 == 255) { r1 = 0; }
                            else { r1 = 1; }
                        }
                        else { r1 = 0; }

                        if (y > 0)
                        {
                            c2 = bit.GetPixel(x, y - 1);
                            r2 = c2.R;
                            if (r2 == 255) { r2 = 0; }
                            else { r2 = 1; }
                        }
                        else { r2 = 0; }

                        if (x > 0 && y > 0)
                        {
                            c3 = bit.GetPixel(x - 1, y - 1);
                            r3 = c3.R;
                            if (r3 == 255) { r3 = 0; }
                            else { r3 = 1; }
                        }
                        else { r3 = 0; }

                        if (x > 0)
                        {
                            c4 = bit.GetPixel(x - 1, y);
                            r4 = c4.R;
                            if (r4 == 255) { r4 = 0; }
                            else { r4 = 1; }
                        }
                        else { r4 = 0; }

                        if (x > 0 && y < bit.Height - 1)
                        {
                            c5 = bit.GetPixel(x - 1, y + 1);
                            r5 = c5.R;
                            if (r5 == 255) { r5 = 0; }
                            else { r5 = 1; }
                        }
                        else { r5 = 0; }

                        if (y < bit.Height - 1)
                        {
                            c6 = bit.GetPixel(x, y + 1);
                            r6 = c6.R;
                            if (r6 == 255) { r6 = 0; }
                            else { r6 = 1; }
                        }
                        else { r6 = 0; }

                        if (x < bit.Width - 1 && y < bit.Height - 1)
                        {
                            c7 = bit.GetPixel(x + 1, y + 1);
                            r7 = c7.R;
                            if (r7 == 255) { r7 = 0; }
                            else { r7 = 1; }
                        }
                        else { r7 = 0; }

                        jr = r0 + r1 + r2 + r3 + r4 + r5 + r6 + r7;
                        str = str + jr.ToString();
                    }
                }
                else if (x == 1 || (x > 1 && x % 2 == 1))
                {
                    for (int y = 0; y < bit.Height; y++)
                    {
                        Color clr = bit.GetPixel(x, y);
                        if (x < bit.Width - 1)
                        {
                            c0 = bit.GetPixel(x + 1, y);
                            r0 = c0.R;
                            if (r0 == 255) { r0 = 0; }
                            else { r0 = 1; }
                        }
                        else { r0 = 0; }

                        if (x < bit.Width - 1 && y > 0)
                        {
                            c1 = bit.GetPixel(x + 1, y - 1);
                            r1 = c1.R;
                            if (r1 == 255) { r1 = 0; }
                            else { r1 = 1; }
                        }
                        else { r1 = 0; }

                        if (y > 0)
                        {
                            c2 = bit.GetPixel(x, y - 1);
                            r2 = c2.R;
                            if (r2 == 255) { r2 = 0; }
                            else { r2 = 1; }
                        }
                        else { r2 = 0; }

                        if (x > 0 && y > 0)
                        {
                            c3 = bit.GetPixel(x - 1, y - 1);
                            r3 = c3.R;
                            if (r3 == 255) { r3 = 0; }
                            else { r3 = 1; }
                        }
                        else { r3 = 0; }

                        if (x > 0)
                        {
                            c4 = bit.GetPixel(x - 1, y);
                            r4 = c4.R;
                            if (r4 == 255) { r4 = 0; }
                            else { r4 = 1; }
                        }
                        else { r4 = 0; }

                        if (x > 0 && y < bit.Height - 1)
                        {
                            c5 = bit.GetPixel(x - 1, y + 1);
                            r5 = c5.R;
                            if (r5 == 255) { r5 = 0; }
                            else { r5 = 1; }
                        }
                        else { r5 = 0; }

                        if (y < bit.Height - 1)
                        {
                            c6 = bit.GetPixel(x, y + 1);
                            r6 = c6.R;
                            if (r6 == 255) { r6 = 0; }
                            else { r6 = 1; }
                        }
                        else { r6 = 0; }

                        if (x < bit.Width - 1 && y < bit.Height - 1)
                        {
                            c7 = bit.GetPixel(x + 1, y + 1);
                            r7 = c7.R;
                            if (r7 == 255) { r7 = 0; }
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
    }
}
