﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Thresholding
{
    class TestThresholding
    {
        public Bitmap threshold_rgb(Bitmap bit, int method)
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
                    bool skin = false;

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

                    int max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    int min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    double hue = color.GetHue();
                    double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    double value = max / 255d;

                    double ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    double cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    double ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (
                        method == 1 &&
                        (
                         (oldRed > oldGreen && oldRed > oldBlue) &&
                         (
                          (oldGreen >= oldBlue && ((5 * oldRed) - (12 * oldGreen) + (7 * oldBlue)) >= 0) ||
                          (oldGreen < oldBlue || ((5 * oldRed) + (7 * oldGreen) - (12 * oldBlue)) >= 0)
                         ) || //AND atau OR
                         (85 < cebe && cebe < 135 && 135 < ceer && ceer < 180)
                        )
                       )
                    {
                        skin = true;
                    }

                    else if (
                        method == 2 &&
                        oldRed > 95 && oldGreen > 40 && oldBlue > 20 &&
                        (Math.Max(oldRed, Math.Max(oldGreen, oldBlue)) - Math.Min(oldRed, Math.Min(oldGreen, oldBlue))) > 15 &&
                        Math.Abs(oldRed - oldGreen) > 15 && oldRed > oldGreen && oldRed > oldBlue
                       )
                    {
                        skin = true;
                    }

                    else if (method == 3 && 77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                    {
                        skin = true;
                    }
                    
                    else if (
                        method == 4 &&
                        oldRed > 220 && oldGreen > 210 && oldBlue > 170 &&
                        Math.Abs(oldRed - oldGreen) <= 15 &&
                        oldBlue < oldRed && oldBlue < oldGreen
                       )
                    {
                        skin = true;
                    }

                    else if (method == 5)
                    {
                        double bagi1 = (oldRed - oldGreen) / (oldRed + oldGreen);
                        double bagi2 = oldBlue / (oldRed + oldGreen);

                        if (
                            0 <= bagi1 && bagi1 <= 0.5 &&
                            bagi2 <= 0.5
                           )
                        {
                            skin = true;
                        }
                    }

                    if (skin == true)
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

        public Bitmap threshold_hsv(Bitmap bit, int method)
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
                    bool skin = false;

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

                    int max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    int min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    double hue = color.GetHue();
                    double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    double value = max / 255d;

                    double ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    double cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    double ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (
                        method == 1 &&
                        equation1 > 1.185 &&
                        0.2 < saturation && saturation < 0.6 && ((0 < hue && hue < 25) || (335 < hue && hue < 360)) &&
                        77 < cebe && cebe < 127 && 133 < ceer && ceer < 173
                       )
                    {
                        skin = true;
                    }

                    else if (
                        method == 2 &&
                        ((0 < hue && hue < 0.24) || (0.74 < hue && hue < 1)) &&
                        0.16 < saturation && saturation < 0.79
                       )
                    {
                        skin = true;
                    }

                    else if (method == 3)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = 128 + (-0.168636 * oldRed) + (0.232932 * oldGreen) + (-0.064296 * oldBlue);
                        ceer = 128 + (0.499813 * oldRed) + (-0.418531 * oldGreen) + (-0.081282 * oldBlue);

                        if ( 102 < cebe && cebe < 135 && 135 < ceer && ceer < 178)
                        {
                            skin = true;
                        }
                    }

                    else if (method == 4 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        double ceGreen = ((0.439 * oldRed) - (0.368 * oldGreen) - (0.071 * oldBlue)) + 128;
                        double ceRed = ((0.148 * oldRed) - (0.291 * oldGreen) - (0.439 * oldBlue)) + 128;

                        if (0.01 <= (hue / 360) && (hue / 360) <= 0.1 && 140 <= ceGreen && ceGreen <= 165 && 140 <= ceRed && ceRed <= 195)
                        {
                            skin = true;
                        }
                    }

                    else if (
                        method == 5 &&
                        ((0 < hue && hue < 50) || (250 < hue && hue < 360)) &&
                        saturation < 0.9 && value <= 0.95
                       )
                    {
                        skin = true;
                    }

                    else if (
                        method == 6 && 
                        0.12 <= (hue / 360) && (hue / 360) <= 0.18 && 
                        0.6 <= saturation && 0.6 <= value
                       )
                    {
                        skin = true;
                    }

                    if (skin == true)
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

        public Bitmap threshold_ybr(Bitmap bit, int method)
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
                    bool skin = false;

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

                    int max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    int min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    double hue = color.GetHue();
                    double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    double value = max / 255d;

                    double ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    double cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    double ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (
                        method == 1 && ye > 80 &&
                        77 < cebe && cebe < 127 && 133 < ceer && ceer < 173
                       )
                    {
                        skin = true;
                    }

                    else if (method == 2)
                    {
                        ye = 16 + ((65.481 * oldRed) + (128.553 * oldGreen) + (24.966 * oldBlue));
                        cebe = 128 + ((-37.797 * oldRed) - (74.203 * oldGreen) + (112 * oldBlue));
                        ceer = 128 + ((112 * oldRed) + (93.786 * oldGreen) + (18.214 * oldBlue));

                        if (77 <= cebe && cebe <= 127 && 120 <= ceer && ceer <= 173)
                        {
                            skin = true;
                        }
                    }

                    else if (method == 3 && 76 < cebe && cebe < 126 && 132 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 4 && 78 <= cebe && cebe <= 126 && 134 <= ceer && ceer <= 172)
                    {
                        skin = true;
                    }

                    else if (method == 5)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = oldBlue - ye;
                        ceer = oldRed - ye;

                        if (ye > 80 && 85 < cebe && cebe < 135 && 135 < ceer && ceer < 180)
                        {
                            skin = true;
                        }
                    }

                    else if (method == 6 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 7)
                    {
                        ye = (0.257 * oldRed) + (0.564 * oldGreen) + (0.098 * oldBlue) + 16;
                        cebe = (-0.148 * oldRed) - (0.291 * oldGreen) + (0.439 * oldBlue) + 128;
                        ceer = (0.5 * oldRed) - (0.419 * oldGreen) - (0.081 * oldBlue) + 128;

                        if (
                            (30 <= ye && ye <= 255 && 77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 177) ||
                            (30 <= ye && ye <= 235 && 95 <= cebe && cebe <= 240 && 110 <= ceer && ceer <= 240) ||
                            (30 <= ye && ye <= 235 && 80 <= cebe && cebe <= 120 && 133 <= ceer && ceer <= 177)
                           )
                        {
                            skin = true;
                        }
                    }

                    else if (method == 8)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = oldBlue - ye;
                        ceer = oldRed - ye;

                        if (77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                        {
                            skin = true;
                        }
                    }

                    else if (method == 9)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = ((oldBlue - ye) * 0.564) + 128;
                        ceer = ((oldRed - ye) * 0.713) + 128;

                        if (77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                        {
                            skin = true;
                        }
                    }

                    if (method == 10 && 100 < cebe && cebe < 150 && 150 < ceer && ceer < 200)
                    {
                        skin = true;
                    }

                    else if (
                             method == 11 && 69 < ye && ye < 256 &&
                             77 < cebe && cebe < 127 && 133 < ceer && ceer < 173
                            )
                    {
                        skin = true;
                    }

                    if (skin == true)
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

        public Bitmap threshold_final(Bitmap bit, int method)
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
                    bool skin = false;

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

                    int max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    int min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    double hue = color.GetHue();
                    double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    double value = max / 255d;

                    double ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    double cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    double ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (
                        method == 1 &&
                        (
                         (oldRed > oldGreen && oldRed > oldBlue) &&
                         (
                          (oldGreen >= oldBlue && ((5 * oldRed) - (12 * oldGreen) + (7 * oldBlue)) >= 0) ||
                          (oldGreen < oldBlue || ((5 * oldRed) + (7 * oldGreen) - (12 * oldBlue)) >= 0)
                         ) || //AND atau OR
                         (85 < cebe && cebe < 135 && 135 < ceer && ceer < 180)
                        )
                       )
                    {
                        skin = true;
                    }

                    else if (method == 2 && 77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                    {
                        skin = true;
                    }

                    else if (method == 3)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = 128 + (-0.168636 * oldRed) + (0.232932 * oldGreen) + (-0.064296 * oldBlue);
                        ceer = 128 + (0.499813 * oldRed) + (-0.418531 * oldGreen) + (-0.081282 * oldBlue);

                        if (102 < cebe && cebe < 135 && 135 < ceer && ceer < 178)
                        {
                            skin = true;
                        }
                    }

                    else if (
                        method == 4 &&
                        ((0 < hue && hue < 50) || (250 < hue && hue < 360)) &&
                        saturation < 0.9 && value <= 0.95
                       )
                    {
                        skin = true;
                    }

                    else if (method == 5 && 76 < cebe && cebe < 126 && 132 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 6 && 78 <= cebe && cebe <= 126 && 134 <= ceer && ceer <= 172)
                    {
                        skin = true;
                    }

                    else if (method == 7 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 8)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = ((oldBlue - ye) * 0.564) + 128;
                        ceer = ((oldRed - ye) * 0.713) + 128;

                        if (77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                        {
                            skin = true;
                        }
                    }

                    if (skin == true)
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
    }
}
