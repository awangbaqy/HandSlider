﻿using Accord.Imaging.Filters;
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
        Bitmap bitmap, destinationBitmap;
        BitmapData bitmapData;
        FilterInfoCollection filterInfoCollection;
        Graphics graphics;
        ImageAttributes imageAttributes;
        Rectangle destinationRectangle;
        VideoCaptureDevice videoCaptureDevice;

        AdaptiveSmoothing adaptiveSmoothing;
        BilateralSmoothing bilateralSmoothing;
        Closing closing, closingFull5x5, closingRadius10;
        ConservativeSmoothing conservativeSmoothing;
        Convolution convolution;
        BinaryDilation3x3 binaryDilation3x3;
        BinaryErosion3x3 binaryErosion3x3;
        Dilation dilation;
        Erosion erosion;
        FillHoles fillHoles;
        Mean mean;
        Median median;
        Opening opening;
        Subtract subtract;
        TopHat topHat;
        
        byte[] pixels;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, y, x, currentLine;
        int oldBlue, oldGreen, oldRed, max, min;
        double hue, saturation, value;
        double ye, cebe, ceer;

        int[,] kernelInt = {
            {0,0,0,1,0,0,0},
            {0,1,1,1,1,1,0},
            {0,1,1,1,1,1,0},
            {1,1,1,1,1,1,1},
            {0,1,1,1,1,1,0},
            {0,1,1,1,1,1,0},
            {0,0,0,1,0,0,0}
        };

        short[,] kernelShort = {
            {0,0,0,1,0,0,0},
            {0,1,1,1,1,1,0},
            {0,1,1,1,1,1,0},
            {1,1,1,1,1,1,1},
            {0,1,1,1,1,1,0},
            {0,1,1,1,1,1,0},
            {0,0,0,1,0,0,0}
        };

        short[,] kernelShortFull5x5 = {
            {1,1,1,1,1},
            {1,1,1,1,1},
            {1,1,1,1,1},
            {1,1,1,1,1},
            {1,1,1,1,1},
        };

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

        public FormMorphologing()
        {
            InitializeComponent();

            destinationBitmap = new Bitmap(480, 360);
            destinationRectangle = new Rectangle(0, 0, 480, 360);

            adaptiveSmoothing = new AdaptiveSmoothing();
            bilateralSmoothing = new BilateralSmoothing();
            binaryDilation3x3 = new BinaryDilation3x3();
            binaryErosion3x3 = new BinaryErosion3x3();
            closing = new Closing();
            closingFull5x5 = new Closing(kernelShortFull5x5);
            closingRadius10 = new Closing(kernelShortRadius10);
            conservativeSmoothing = new ConservativeSmoothing();
            convolution = new Convolution(kernelInt);
            dilation = new Dilation(kernelShort);
            erosion = new Erosion(kernelShort);

            fillHoles = new FillHoles();
            fillHoles.MaxHoleHeight = 20;
            fillHoles.MaxHoleWidth = 20;
            fillHoles.CoupledSizeFiltering = false;

            mean = new Mean();
            median = new Median();
            opening = new Opening();
            topHat = new TopHat();
        }

        private void FormMorphologing_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
            videoCaptureDevice.NewFrame += new NewFrameEventHandler(newFrame);
            videoCaptureDevice.Start();
        }

        void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bitmap = resize(eventArgs.Frame);

            lock (pictureBox0)
            {
                pictureBox0.Image = bitmap.Clone() as Bitmap;
            }

            bitmap = thresholding(bitmap);
            bitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);

            lock (pictureBox1)
            {
                if (radioButton1.Checked)
                {
                    bitmap = conservativeSmoothing.Apply(binaryDilation3x3.Apply(binaryErosion3x3.Apply(bitmap)));
                }
                else if (radioButton2.Checked)
                {
                    bitmap = topHat.Apply(bitmap);
                }
                else if (radioButton3.Checked)
                {
                    bitmap = closing.Apply(opening.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(bitmap))));
                }
                else if (radioButton4.Checked)
                {
                    bitmap = closing.Apply(median.Apply(bitmap));
                }
                else if (radioButton5.Checked)
                {
                    bitmap = closing.Apply(opening.Apply(mean.Apply(bitmap)));
                }
                else if (radioButton6.Checked)
                {
                    bitmap = binaryDilation3x3.Apply(binaryErosion3x3.Apply(bitmap));
                }
                else if (radioButton7.Checked)
                {
                    // fill holes
                    bitmap = fillHoles.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(bitmap)));
                }
                else if (radioButton8.Checked)
                {
                    // circular filter
                    bitmap = convolution.Apply(opening.Apply(closing.Apply(bitmap)));
                }
                else if (radioButton9.Checked)
                {
                    // circular filter
                    bitmap = erosion.Apply(dilation.Apply(bitmap));
                }
                else if (radioButton10.Checked)
                {
                    bitmap = binaryDilation3x3.Apply(median.Apply(bitmap));
                }
                else if (radioButton11.Checked)
                {
                    // circular filer radius 10
                    bitmap = closingRadius10.Apply(opening.Apply(bitmap));
                }
                else if (radioButton12.Checked)
                {
                    bitmap = binaryErosion3x3.Apply(binaryDilation3x3.Apply(bitmap));
                }
                else if (radioButton13.Checked)
                {
                    // fill holes
                    bitmap = fillHoles.Apply(opening.Apply(median.Apply(bitmap)));
                }
                else if (radioButton14.Checked)
                {
                    // gradient = dilation - erosion
                    subtract = new Subtract(binaryErosion3x3.Apply(bitmap));
                    bitmap = subtract.Apply(binaryDilation3x3.Apply(bitmap));
                }
                else if (radioButton15.Checked)
                {
                    bitmap = closingFull5x5.Apply(bitmap);
                }
                else if (radioButton16.Checked)
                {
                    bitmap = opening.Apply(closing.Apply(bitmap));
                }
                else if (radioButton17.Checked)
                {
                    bitmap = opening.Apply(bitmap);
                }
                else if (radioButton18.Checked)
                {
                    bitmap = median.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(bitmap)));
                }

                pictureBox1.Image = bitmap;
            }
        }

        private Bitmap resize(Bitmap bit)
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

                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if ((
                         (0 < (hue / 360) && (hue / 360) < 0.24) ||
                         (0.74 < (hue / 360) && (hue / 360) < 1)
                        ) &&
                        0.16 < saturation && saturation < 0.79)
                    //if (77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
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
            videoCaptureDevice.NewFrame -= new NewFrameEventHandler(newFrame);
            videoCaptureDevice.Stop();
            videoCaptureDevice.SignalToStop();
        }
    }
}
