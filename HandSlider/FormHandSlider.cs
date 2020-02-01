using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HandSlider
{
    public partial class FormHandSlider : Form
    {
        Bitmap bit, bit2, bit3, destImage = new Bitmap(320, 240);
        BlobCounter blobCounter = new BlobCounter();
        Blob[] blobs;
        Color color, c;
        Dictionary<int, double> histoR = new Dictionary<int, double>();
        Dictionary<int, double> histoG = new Dictionary<int, double>();
        Dictionary<int, double> histoB = new Dictionary<int, double>();
        Dilation dilation = new Dilation();
        FilterInfoCollection fic;
        Graphics graphics, g;
        Grayscale grayscale = Grayscale.CommonAlgorithms.BT709;
        ImageAttributes wrapMode;
        Opening opening = new Opening(new short[3,3]);
        Pen rectPen = new Pen(Color.Blue);
        Rectangle destRect = new Rectangle(0, 0, 320, 240);
        Threshold t = new Threshold();
        VideoCaptureDevice vcd;

        double hue, saturation, value, ylum, cb, cr;
        int i, j, max, min;
        
        public FormHandSlider()
        {
            InitializeComponent();
            //pictureBox3.Image = pictureBox2.Image;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            vcd = new VideoCaptureDevice(fic[0].MonikerString);
            vcd.NewFrame += new NewFrameEventHandler(newFrame);
            vcd.Start();

            pictureBox3.SuspendLayout();
            pictureBox3.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox3_Paint);
            pictureBox3.ResumeLayout();

            //panel1.HorizontalScroll.Value = panel1.HorizontalScroll.Maximum;
        }

        void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bit = resize(eventArgs.Frame);

            lock (pictureBox1)
            {
                pictureBox1.Image = bit.Clone() as Bitmap;
            }

            //bit = Grayscale.CommonAlgorithms.BT709.Apply(bit);
            //bit = t.Apply(bit);
            //IterativeThreshold it = new IterativeThreshold();
            //bit = it.Apply(bit);

            bit = threshold_lockbit_hsv(bit);

            //bit = grayscale.Apply(bit);
            //dilation.ApplyInPlace(bit);
            //opening.ApplyInPlace(bit);
            
            //bit = OpenMorphologyFilter(bit, 3, false, false, false);

            //CannyEdgeDetector filter = new CannyEdgeDetector();
            //bit = filter.Apply(bit);

            //FillHoles fillholes = new FillHoles();
            //fillholes.MaxHoleHeight = 100;
            //fillholes.MaxHoleWidth = 100;
            //fillholes.CoupledSizeFiltering = true;
            //bit = fillholes.Apply(bit);

            //RobinsonEdgeDetector robinson = new RobinsonEdgeDetector();
            //bit = robinson.Apply(bit);

            lock (pictureBox2)
            {
                pictureBox2.Image = bit.Clone() as Bitmap;
            }

            lock (pictureBox3)
            {
                pictureBox3.Image = bit.Clone() as Bitmap;
            }

            bit3 = bit.Clone() as Bitmap;

            //throw new NotImplementedException();
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (bit3 != null)
            {
                bit2 = Accord.Imaging.Image.Clone(bit3.Clone() as Bitmap, PixelFormat.Format24bppRgb);
                blobCounter.ProcessImage(bit2);
                blobs = blobCounter.GetObjectsInformation();
                g = e.Graphics;

                foreach (Blob blob in blobs)
                {
                    g.DrawRectangle(rectPen, blob.Rectangle);
                }

                if (blobs != null && blobs.Length != 0)
                {
                    blobCounter.ExtractBlobsImage(bit2, blobs[0], true);

                    pictureBox4.Image = blobs[0].Image.ToManagedImage();

                    pictureBox4.Image = new ExtractBiggestBlob().Apply(bit2);
                }
            }

            //pictureBox3.Image = new Bitmap(320, 240, g);

            //throw new NotImplementedException();
        }

        private Bitmap he(Bitmap bit)
        {
            List<string> l1 = new List<string>();
            List<string> l2 = new List<string>();

            //Proses inisiasi nilai awal pixel 0 - 255 diset bernilai 0
            for (int counter = 0; counter <= 255; counter++)
            {
                histoR[counter] = 0;
                histoG[counter] = 0;
                histoB[counter] = 0;
            }

            //Untuk tiap baris dan kolom citra, nilai histogram ditambahkan
            for (int i = 0; i < bit.Width; i++)
            {
                for (int j = 0; j < bit.Height; j++)
                {
                    c = bit.GetPixel(i, j);

                    histoR[c.R] += 1;
                    histoG[c.G] += 1;
                    histoB[c.B] += 1; //kerja histogram

                    l1.Add("R " + c.R + "; G " + c.G + "; B " + c.B + ";");
                }
            }

            //Proses menghitung nilai transform function,
            double[] transformR = new double[256];
            double[] transformG = new double[256];
            double[] transformB = new double[256];
            double jumlahR, jumlahG, jumlahB;
            jumlahR = jumlahG = jumlahB = 0;

            foreach (int i in histoR.Keys.ToList())
            {
                jumlahR += 255 * (histoR[i] / (bit.Width * bit.Height));
                transformR[i] = jumlahR;

                jumlahG += 255 * (histoG[i] / (bit.Width * bit.Height));
                transformG[i] = jumlahG;

                jumlahB += 255 * (histoB[i] / (bit.Width * bit.Height));
                transformB[i] = jumlahB;
            }

            //Proses mengubah nilai pixel ke nilai baru sesuai transform function
            for (int i = 0; i < bit.Width; i++)
            {
                for (int j = 0; j < bit.Height; j++)
                {
                    c = bit.GetPixel(i, j);

                    //r = Convert.ToInt16(transformR[c.R]);
                    //g = Convert.ToInt16(transformG[c.G]);
                    //b = Convert.ToInt16(transformB[c.B]);

                    //bit.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }

            return bit;
        }

        private Bitmap threshold_getset(Bitmap bit)
        {
            for (i = 0; i < bit.Width; i++)
            {
                for (j = 0; j < bit.Height; j++)
                {
                    color = bit.GetPixel(i, j);

                    max = Math.Max(color.R, Math.Max(color.G, color.B));
                    min = Math.Min(color.R, Math.Min(color.G, color.B));

                    hue = color.GetHue();
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;

                    if (0 <= hue && hue <= 50 && 0.23 <= saturation && saturation <= 0.68)
                    {
                        bit.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    }
                    else
                    {
                        bit.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }

                    //y = 16 + 65.738 * color.R / 256 + 129.057 * color.G / 256 + 25.064 * color.B / 256;
                    //cb = 128 - 37.945 * color.R / 256 - 74.494 * color.G / 256 + 112.439 * color.B / 256;
                    //cr = 128 + 112.439 * color.R - 94.154 * color.G / 256 - 18.285 * color.B / 256;

                    //y = (0.299 * color.R) + (0.287 * color.G) + (0.11 * color.B);
                    //cr = color.R - y;
                    //cb = color.B - y;

                    //if (100 <= cb && cb <= 150 && 150 <= cr && cr <= 200)
                    //{
                    //    bit.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    //}
                    //else
                    //{
                    //    bit.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    //}
                }
            }

            return bit;
        }

        private Bitmap threshold_lockbit_hsv(Bitmap bit)
        {
            BitmapData bitmapData = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, bit.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bit.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * bit.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = pixels[currentLine + x];
                    int oldGreen = pixels[currentLine + x + 1];
                    int oldRed = pixels[currentLine + x + 2];

                    double redAksen = 1 / 3;
                    double greenAksen = 1 / 3;
                    double blueAksen = 1 / 3;

                    if (oldBlue != 0 || oldGreen != 0 || oldRed != 0)
                    {
                        redAksen = oldRed / (oldRed + oldGreen + oldBlue);
                        greenAksen = oldGreen / (oldRed + oldGreen + oldBlue);
                        blueAksen = oldBlue / (oldRed + oldGreen + oldBlue);
                    }

                    double equation1 = redAksen / greenAksen;
                    double equation2 = (redAksen * blueAksen) / ((redAksen + greenAksen + blueAksen) * (redAksen + greenAksen + blueAksen));
                    double equation3 = (redAksen * greenAksen) / ((redAksen + greenAksen + blueAksen) * (redAksen + greenAksen + blueAksen));

                    max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    hue = color.GetHue();
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;

                    double ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    double cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    double ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    //if (0 <= hue && hue <= 50 && 0.23 <= saturation && saturation <= 0.68)
                    if (100 < cebe && cebe < 150 && 150 < ceer && ceer < 200)
                    //if (equation1 > 1.185 ||
                    //    (((0 < hue && hue < 25) || (335 < hue && hue < 360)) && 0.2 < saturation && saturation < 0.6) ||
                    //    (77 < cebe && cebe < 127 && 133 < ceer && ceer < 173))
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

            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        private Bitmap threshold_lockbit_ycbcr(Bitmap bmp)
        {
            BitmapData bitmapData = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, bit.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bit.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * bit.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = pixels[currentLine + x];
                    int oldGreen = pixels[currentLine + x + 1];
                    int oldRed = pixels[currentLine + x + 2];

                    //ylum = (0.299 * oldRed) + (int)(0.287 * oldGreen) + (int)(0.11 * oldBlue);
                    //cr = oldRed - ylum;
                    //cb = oldBlue - ylum;
                    //-==-//
                    //float fr = (float)oldRed / 255;
                    //float fg = (float)oldGreen / 255;
                    //float fb = (float)oldBlue / 255;

                    //ylum = (0.2989 * fr + 0.5866 * fg + 0.1145 * fb) * 255;
                    //cb = ((-0.1687 * fr - 0.3313 * fg + 0.5000 * fb)+0.5) * 255;
                    //cr = ((0.5000 * fr - 0.4184 * fg - 0.0816 * fb) +0.5)* 255;
                    //-==-//
                    //int xPor3 = x * 3;
                    //float blue = currentLine[xPor3++];
                    //float green = currentLine[xPor3++];
                    //float red = currentLine[xPor3];

                    ylum = ((0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue));
                    cb = (128 - (0.168736 * oldRed) + (0.331264 * oldGreen) + (0.5 * oldBlue));
                    cr = (128 + (0.5 * oldRed) + (0.418688 * oldGreen) + (0.081312 * oldBlue));

                    //Console.WriteLine("rgb"+oldRed+","+oldGreen+","+oldBlue+" : "+cb+" "+cr);

                    //if (80 <= y && 85 <= cb && cb <= 135 && 135 <= cr && cr <= 180)
                    if (100 <= cb && cb <= 150 && 150 <= cr && cr <= 200)
                    //if (100 <= cb && cb <= 122 && 132 <= cr && cr <= 150)
                    //if (100 <= cb && cb <= 125 && 132 <= cr && cr <= 151)
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

            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        private Bitmap threshold_lockbit_ycbcr2(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            byte[,] yData = new byte[width, height]; //luma
            byte[,] bData = new byte[width, height]; //Cb
            byte[,] rData = new byte[width, height]; //Cr

            unsafe
            {
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                int heightInPixels = bitmapData.Height;
                int widthInBytes = width * 3;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                //Convert to YCbCr
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        //int xPor3 = x * 3;
                        //float blue = currentLine[xPor3++];
                        //float green = currentLine[xPor3++];
                        //float red = currentLine[xPor3];

                        //ylum = ((0.299 * red) + (0.587 * green) + (0.114 * blue));
                        //cb = (128 - (0.168736 * red) + (0.331264 * green) + (0.5 * blue));
                        //cr = (128 + (0.5 * red) + (0.418688 * green) + (0.081312 * blue));

                        //yData[x, y] = (byte)((0.299 * red) + (0.587 * green) + (0.114 * blue));
                        //bData[x, y] = (byte)(128 - (0.168736 * red) + (0.331264 * green) + (0.5 * blue));
                        //rData[x, y] = (byte)(128 + (0.5 * red) + (0.418688 * green) + (0.081312 * blue));
                        
                        if (100 <= cb && cb <= 150 && 150 <= cr && cr <= 200)
                        {
                            yData[x, y] = (byte)0;
                            bData[x, y] = (byte)0;
                            rData[x, y] = (byte)0;
                        }
                        else
                        {
                            yData[x, y] = (byte)255;
                            bData[x, y] = (byte)200;
                            rData[x, y] = (byte)255;
                        }
                    }
                }
                bmp.UnlockBits(bitmapData);
            }

            return bmp;
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

        private Bitmap OpenMorphologyFilter(Bitmap sourceBitmap, int matrixSize, bool applyBlue = true, bool applyGreen = true, bool applyRed = true)
        {
            Bitmap resultBitmap = DilateAndErodeFilter( sourceBitmap,
                                matrixSize, "Erosion",
                                applyBlue, applyGreen, applyRed);


            resultBitmap = DilateAndErodeFilter( sourceBitmap,
                               matrixSize,
                               "Dilation",
                               applyBlue, applyGreen, applyRed);


            return resultBitmap;
        }

        private Bitmap DilateAndErodeFilter(Bitmap sourceBitmap, int matrixSize, string morphType, bool applyBlue = true, bool applyGreen = true,bool applyRed = true)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;


            int byteOffset = 0;


            byte blue = 0;
            byte green = 0;
            byte red = 0;


            byte morphResetValue = 0;


            if (morphType == "Erosion")
            {
                morphResetValue = 255;
            }


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    blue = morphResetValue;
                    green = morphResetValue;
                    red = morphResetValue;


                    if (morphType == "Dilation")
                    {
                        for (int filterY = -filterOffset;
                            filterY <= filterOffset; filterY++)
                        {
                            for (int filterX = -filterOffset;
                                filterX <= filterOffset; filterX++)
                            {
                                calcOffset = byteOffset +
                                             (filterX * 4) +
                                (filterY * sourceData.Stride);


                                if (pixelBuffer[calcOffset] > blue)
                                {
                                    blue = pixelBuffer[calcOffset];
                                }


                                if (pixelBuffer[calcOffset + 1] > green)
                                {
                                    green = pixelBuffer[calcOffset + 1];
                                }


                                if (pixelBuffer[calcOffset + 2] > red)
                                {
                                    red = pixelBuffer[calcOffset + 2];
                                }
                            }
                        }
                    }
                    else if (morphType == "Erosion")
                    {
                        for (int filterY = -filterOffset;
                            filterY <= filterOffset; filterY++)
                        {
                            for (int filterX = -filterOffset;
                                filterX <= filterOffset; filterX++)
                            {
                                calcOffset = byteOffset +
                                             (filterX * 4) +
                                (filterY * sourceData.Stride);


                                if (pixelBuffer[calcOffset] < blue)
                                {
                                    blue = pixelBuffer[calcOffset];
                                }


                                if (pixelBuffer[calcOffset + 1] < green)
                                {
                                    green = pixelBuffer[calcOffset + 1];
                                }


                                if (pixelBuffer[calcOffset + 2] < red)
                                {
                                    red = pixelBuffer[calcOffset + 2];
                                }
                            }
                        }
                    }


                    if (applyBlue == false)
                    {
                        blue = pixelBuffer[byteOffset];
                    }


                    if (applyGreen == false)
                    {
                        green = pixelBuffer[byteOffset + 1];
                    }


                    if (applyRed == false)
                    {
                        red = pixelBuffer[byteOffset + 2];
                    }


                    resultBuffer[byteOffset] = blue;
                    resultBuffer[byteOffset + 1] = green;
                    resultBuffer[byteOffset + 2] = red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();
        }
    }
}