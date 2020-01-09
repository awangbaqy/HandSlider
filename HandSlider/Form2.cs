using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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
    public partial class Form2 : Form
    {
        Bitmap bit, bit2, bit3, destImage = new Bitmap(320, 240);
        Color color, c;
        Dictionary<int, double> histoR = new Dictionary<int, double>();
        Dictionary<int, double> histoG = new Dictionary<int, double>();
        Dictionary<int, double> histoB = new Dictionary<int, double>();
        Graphics graphics, g;
        ImageAttributes wrapMode;
        Mat frame = new Mat();
        Pen rectPen = new Pen(Color.Blue);
        Rectangle destRect = new Rectangle(0, 0, 320, 240);
        VideoCapture capture = new VideoCapture(0);

        double hue, saturation, value, ylum, cb, cr;
        int i, j, max, min;
        
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            capture.ImageGrabbed += ProcessFrame;
            capture.Start();

            //var capture = new Emgu.CV.VideoCapture();

            //using (var nextFrame = capture.QueryFrame())
            //{
            //    if (nextFrame != null)
            //    {
            //        pictureBox1.Image = nextFrame.Bitmap;
            //    }
            //}

            //panel1.HorizontalScroll.Value = panel1.HorizontalScroll.Maximum;
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame, 0);

                bit = resize(frame.Bitmap);

                lock (pictureBox1)
                {
                    pictureBox1.Image = bit.Clone() as Bitmap;
                }

                Image<Bgr, Byte> img = new Image<Bgr, byte>(bit);

                Image<Hsv, Byte> hsvimg1 = img.Convert<Hsv, Byte>();
                //Image<Hsv, Byte> hsvimg2 = img.Convert<Hsv, Byte>();

                //hsvimg1 = hsvimg1.ThresholdToZeroInv(new Hsv(0, 57, 128));
                //hsvimg2 = hsvimg2.ThresholdToZeroInv(new Hsv(25, 173, 128));

                //hsvimg1 = hsvimg1.ThresholdBinary(new Hsv(0, 57, 0), new Hsv(0, 255, 255));
                //hsvimg2 = hsvimg2.ThresholdBinary(new Hsv(25, 173, 0), new Hsv(0, 255, 255));

                //Image<Hsv, Byte> hsvimg3 = hsvimg1.And(hsvimg2);

                Image<Gray, Byte> grayimg = hsvimg1.InRange(new Hsv(0, 57, 0), new Hsv(25, 173, 255));

                VectorOfVectorOfPoint vovop = new VectorOfVectorOfPoint();
                Mat m = new Mat();
                CvInvoke.FindContours(grayimg, vovop, m, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                CvInvoke.DrawContours(grayimg, vovop, -1, new MCvScalar(128, 255, 0), 3, LineType.AntiAlias);

                lock (pictureBox2)
                {
                    //pictureBox2.Image = hsvimg1.ToBitmap().Clone() as Bitmap;
                    pictureBox2.Image = grayimg.ToBitmap().Clone() as Bitmap;
                }

                //Image<Gray, Byte> grayimg2 = hsvimg1.InRange(new Hsv(0, 57, 0), new Hsv(25, 173, 255));
                //Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
                //Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
                //grayimg2._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Erode, kernel5, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                //grayimg2._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, kernel3, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                //grayimg2._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, kernel3, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                Matrix<byte> kernel = new Matrix<byte>(new Byte[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } });
                //grayimg._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                grayimg._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Erode, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                grayimg._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Gradient, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                lock (pictureBox3)
                {
                    pictureBox3.Image = grayimg.ToBitmap().Clone() as Bitmap;
                }

                grayimg._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Gradient, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                lock (pictureBox4)
                {
                    pictureBox4.Image = grayimg.ToBitmap().Clone() as Bitmap;
                }
            }
        }

        //void newFrame(object sender, NewFrameEventArgs eventArgs)
        //{
        //    bit = resize(eventArgs.Frame);

        //    lock (pictureBox1)
        //    {
        //        pictureBox1.Image = bit.Clone() as Bitmap;
        //    }

        //    Image<Hsv, byte> img = new Image<Hsv, byte>(fileName);
        //    //lower
        //    img = img.ThresholdToZero(new Hsv(10.0, 22.0, 10.0));
        //    //upper
        //    img = img.ThresholdToZeroInv(new Hsv(100.0, 29.0, 100.0));

        //    //bit = Grayscale.CommonAlgorithms.BT709.Apply(bit);
        //    //bit = t.Apply(bit);
        //    //IterativeThreshold it = new IterativeThreshold();
        //    //bit = it.Apply(bit);

        //    bit = threshold_lockbit_hsv(bit);

        //    //bit = Grayscale.CommonAlgorithms.BT709.Apply(bit);
        //    //bit = opening.Apply(bit);
        //    //bit = OpenMorphologyFilter(bit, 3, false, false, false);

        //    //CannyEdgeDetector filter = new CannyEdgeDetector();
        //    //bit = filter.Apply(bit);

        //    //FillHoles fillholes = new FillHoles();
        //    //fillholes.MaxHoleHeight = 100;
        //    //fillholes.MaxHoleWidth = 100;
        //    //fillholes.CoupledSizeFiltering = true;
        //    //bit = fillholes.Apply(bit);

        //    //RobinsonEdgeDetector robinson = new RobinsonEdgeDetector();
        //    //bit = robinson.Apply(bit);

        //    lock (pictureBox2)
        //    {
        //        //Put into picturebox
        //        pictureBox2.Image = bit.Clone() as Bitmap;
        //    }

        //    //lock (pictureBox3)
        //    //{
        //    //    pictureBox3.Image = bit.Clone() as Bitmap;
        //    //}

        //    //bit3 = bit.Clone() as Bitmap;

        //    //throw new NotImplementedException();
        //}

        //private void pictureBox3_Paint(object sender, PaintEventArgs e)
        //{
        //    if (bit3 != null)
        //    {
        //        bit2 = Accord.Imaging.Image.Clone(bit3.Clone() as Bitmap, PixelFormat.Format24bppRgb);
        //        blobCounter.ProcessImage(bit2);
        //        blobs = blobCounter.GetObjectsInformation();
        //        g = e.Graphics;

        //        foreach (Blob blob in blobs)
        //        {
        //            g.DrawRectangle(rectPen, blob.Rectangle);
        //        }

        //        //if (blobs[0] != null)
        //        //{
        //            //blobCounter.ExtractBlobsImage(bit2, blobs[0], true);

        //            //pictureBox4.Image = blobs[0].Image.ToManagedImage();

        //        //    pictureBox4.Image = new ExtractBiggestBlob().Apply(bit2);
        //        //}
        //    }

        //    //pictureBox3.Image = new Bitmap(320, 240, g);

        //    //throw new NotImplementedException();
        //}

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

                    max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    hue = color.GetHue();
                    //hue = color.GetHue() * 360 / 255;
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;

                    if (0 <= hue && hue <= 50 && 0.23 <= saturation && saturation <= 0.68)
                    //if (0 <= hue && hue <= 20 && 0.22 <= saturation && saturation <= 1)
                    ////if (5 <= hue && hue <= 35)
                    ////if (0 <= hue && hue <= 55 && 0 <= saturation && saturation <= 0.5 && value >= 0.1)
                    ////if (0 <= hue && hue <= 55 && 0 <= saturation && saturation <= 0.5)
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

                    // calculate new pixel value
                    //pixels[currentLine + x] = (byte)oldBlue;
                    //pixels[currentLine + x + 1] = (byte)oldGreen;
                    //pixels[currentLine + x + 2] = (byte)oldRed;
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
            //vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            //vcd.Stop();
            //vcd.SignalToStop();
        }
    }
}