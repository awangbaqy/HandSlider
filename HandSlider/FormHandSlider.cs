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
        BinaryDilation3x3 binaryDilation3x3;
        BinaryErosion3x3 binaryErosion3x3;
        Bitmap frame, frame2, bit3, destImage;
        BitmapData bitmapData;
        BlobCounter blobCounter;
        Blob[] blobs;
        Color color, c;
        Dictionary<int, double> histoR;
        Dictionary<int, double> histoG;
        Dictionary<int, double> histoB;
        Dilation dilation;
        FilterInfoCollection fic;
        Graphics graphics, g;
        Grayscale grayscale;
        ImageAttributes wrapMode;
        Median median;
        Opening opening;
        Pen rectPen;
        Rectangle destRect;
        Threshold t;
        VideoCaptureDevice vcd;
        
        bool skin;
        byte[] pixels;
        double hue, saturation, value;
        double ye, cebe, ceer;
        int i, j, max, min;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, y, x, currentLine;
        int oldBlue, oldGreen, oldRed;

        public FormHandSlider()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            destImage = new Bitmap(480, 360);
            binaryDilation3x3 = new BinaryDilation3x3();
            binaryErosion3x3 = new BinaryErosion3x3();
            blobCounter = new BlobCounter();
            histoR = new Dictionary<int, double>();
            histoG = new Dictionary<int, double>();
            histoB = new Dictionary<int, double>();
            dilation = new Dilation();
            grayscale = Grayscale.CommonAlgorithms.BT709;
            median = new Median();
            opening = new Opening(new short[3, 3]);
            rectPen = new Pen(Color.Blue);
            destRect = new Rectangle(0, 0, 480, 360);
            t = new Threshold();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SuspendLayout();
            pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            pictureBox1.ResumeLayout();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            vcd = new VideoCaptureDevice(fic[0].MonikerString);
            vcd.NewFrame += new NewFrameEventHandler(newFrame);
            vcd.Start();
        }

        private void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            frame = resizing(eventArgs.Frame);

            if (rbOriginal.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }

            frame = thresholding(frame.Clone() as Bitmap);

            if (rbThreshold.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }
            
            frame = morphologing(frame.Clone() as Bitmap);

            if (rbMorphology.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }

            if (rbBlobOfHand.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = null;
                }
            }

            if (rbCenterPoint.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = null;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (bit3 != null)
            {
                frame2 = Accord.Imaging.Image.Clone(bit3.Clone() as Bitmap, PixelFormat.Format24bppRgb);
                blobCounter.ProcessImage(frame2);
                blobs = blobCounter.GetObjectsInformation();
                g = e.Graphics;

                foreach (Blob blob in blobs)
                {
                    g.DrawRectangle(rectPen, blob.Rectangle);
                }

                if (blobs != null && blobs.Length != 0)
                {
                    blobCounter.ExtractBlobsImage(frame2, blobs[0], true);

                    pictureBox1.Image = blobs[0].Image.ToManagedImage();

                    pictureBox1.Image = new ExtractBiggestBlob().Apply(frame2);
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
                }
            }

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
        
        private Bitmap resizing(Bitmap bitmap)
        {
            destImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

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
                    graphics.DrawImage(bitmap, destRect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private Bitmap morphologing(Bitmap bitmap)
        {
            return median.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(grayscale.Apply(bitmap))));
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

        private void btnStop_Click(object sender, EventArgs e)
        {
            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (vcd != null)
            {
                vcd.NewFrame -= new NewFrameEventHandler(newFrame);
                vcd.Stop();
                vcd.SignalToStop();
            }
        }
    }
}