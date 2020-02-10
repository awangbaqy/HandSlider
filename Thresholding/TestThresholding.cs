using System;
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
        BitmapData bitmapData;
        Color color;

        bool skin;
        byte[] pixels;
        double normalizedRed, normalizedGreen, normalizedBlue;
        double equation;
        double hue, saturation, value;
        double ye, cebe, ceer;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, y, x, currentLine;
        int oldBlue, oldGreen, oldRed, max, min;

        public Bitmap threshold_rgb(Bitmap bit, int method)
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
                    skin = false;

                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];
                    
                    if (
                        method == 1 &&
                        oldRed > 220 && oldGreen > 210 && oldBlue > 170 &&
                        Math.Abs(oldRed - oldGreen) <= 15 &&
                        oldBlue < oldRed && oldBlue < oldGreen
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

                    else if (method == 3)
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
            
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        public Bitmap threshold_hsv(Bitmap bit, int method)
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
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    skin = false;

                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];
                    
                    max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    hue = color.GetHue();
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;
                    
                    if (
                        method == 1 &&
                        0.12 <= (hue / 360) && (hue / 360) <= 0.18 &&
                        0.6 <= saturation && 0.6 <= value
                       )
                    {
                        skin = true;
                    }

                    else if (
                        method == 2 &&
                        ((0 < hue && hue < 50) || (250 < hue && hue < 360)) &&
                        saturation < 0.9 && value <= 0.95
                       )
                    {
                        skin = true;
                    }

                    else if (
                        method == 3 &&
                        (
                         (0 < (hue / 360) && (hue / 360) < 0.24) || 
                         (0.74 < (hue / 360) && (hue / 360) < 1)
                        ) &&
                        0.16 < saturation && saturation < 0.79
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
            
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        public Bitmap threshold_ycbcr(Bitmap bit, int method)
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
                    skin = false;

                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];
                    
                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);
                    
                    if (method == 1 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 2)
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

                    else if (method == 3 && 78 <= cebe && cebe <= 126 && 134 <= ceer && ceer <= 172)
                    {
                        skin = true;
                    }

                    else if (method == 4 && ye > 80 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 5 && 77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                    {
                        skin = true;
                    }

                    else if (method == 6 && 69 < ye && ye < 256 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 7)
                    {
                        ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                        cebe = 128 + (-0.168636 * oldRed) + (0.232932 * oldGreen) + (-0.064296 * oldBlue);
                        ceer = 128 + (0.499813 * oldRed) + (-0.418531 * oldGreen) + (-0.081282 * oldBlue);
                        
                        if (102 < cebe && cebe < 135 && 135 < ceer && ceer < 178)
                        {
                            skin = true;
                        }
                    }

                    else if (method == 8 && 76 < cebe && cebe < 126 && 132 < ceer && ceer < 173)
                    {
                        skin = true;
                    }

                    else if (method == 9 && 100 < cebe && cebe < 150 && 150 < ceer && ceer < 200)
                    {
                        skin = true;
                    }

                    else if (method == 10 && 77 <= cebe && cebe <= 127 && 133 <= ceer && ceer <= 173)
                    {
                        // Calculations in the journal are the same as above
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
            
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        public Bitmap threshold_combined(Bitmap bit, int method)
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
                    skin = false;

                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];

                    normalizedRed = 1 / 3;
                    normalizedGreen = 1 / 3;
                    normalizedBlue = 1 / 3;

                    if (oldBlue != 0 || oldGreen != 0 || oldRed != 0)
                    {
                        normalizedRed = oldRed / (oldRed + oldGreen + oldBlue);
                        normalizedGreen = oldGreen / (oldRed + oldGreen + oldBlue);
                        normalizedBlue = oldBlue / (oldRed + oldGreen + oldBlue);
                    }

                    equation = normalizedRed / normalizedGreen;
                    
                    max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    hue = color.GetHue();
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;

                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (method == 1 && 77 < cebe && cebe < 127 && 133 < ceer && ceer < 173)
                    {
                        double ceGreen = ((0.439 * oldRed) - (0.368 * oldGreen) - (0.071 * oldBlue)) + 128;
                        double ceRed = ((0.148 * oldRed) - (0.291 * oldGreen) - (0.439 * oldBlue)) + 128;

                        if (0.01 <= (hue / 360) && (hue / 360) <= 0.1 && 140 <= ceGreen && ceGreen <= 165 && 140 <= ceRed && ceRed <= 195)
                        {
                            skin = true;
                        }
                    }

                    else if (
                        method == 2 &&
                        (
                         (oldRed > oldGreen && oldRed > oldBlue) &&
                         (
                          (oldGreen >= oldBlue && ((5 * oldRed) - (12 * oldGreen) + (7 * oldBlue)) >= 0) ||
                          (oldGreen < oldBlue || ((5 * oldRed) + (7 * oldGreen) - (12 * oldBlue)) >= 0)
                         ) && //|| //AND or OR
                         (85 < cebe && cebe < 135 && 135 < ceer && ceer < 180)
                        )
                       )
                    {
                        skin = true;
                    }

                    else if (
                        method == 3 &&
                        equation > 1.185 &&
                        0.2 < saturation && saturation < 0.6 && ((0 < hue && hue < 25) || (335 < hue && hue < 360)) &&
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
            
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }

        public Bitmap threshold_final(Bitmap bit, int method)
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
                    skin = false;

                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];

                    normalizedRed = 1 / 3;
                    normalizedGreen = 1 / 3;
                    normalizedBlue = 1 / 3;

                    if (oldBlue != 0 || oldGreen != 0 || oldRed != 0)
                    {
                        normalizedRed = oldRed / (oldRed + oldGreen + oldBlue);
                        normalizedGreen = oldGreen / (oldRed + oldGreen + oldBlue);
                        normalizedBlue = oldBlue / (oldRed + oldGreen + oldBlue);
                    }

                    equation = normalizedRed / normalizedGreen;

                    max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    hue = color.GetHue();
                    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    value = max / 255d;

                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

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
            
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bit.UnlockBits(bitmapData);

            return bit;
        }
    }
}
