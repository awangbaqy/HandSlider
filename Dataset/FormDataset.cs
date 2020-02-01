using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Dataset
{
    public partial class FormDataset : Form
    {
        public FormDataset()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                Environment.SpecialFolder root = folderDlg.RootFolder;

                //toolStripStatusLabelDirectory.Text = Path.GetFullPath(folderDlg.SelectedPath);

                List<Image> pictureArray = new List<Image>();
                int iente = 0;
                foreach (string item in Directory.GetFiles(folderDlg.SelectedPath, "*.jpg", SearchOption.AllDirectories))
                {
                    iente += 1;
                    
                    Image _image = Image.FromFile(item);
                    string nama = Path.GetFileNameWithoutExtension(item);

                    Bitmap bit = threshold_lockbit_hsv((Bitmap)_image);

                    bit.Save(folderDlg.SelectedPath+"//biner_" + nama + ".jpg");

                    //pictureArray.Add(_image);

                    //if (iente == 1)
                    //{
                    //    break;
                    //}
                }

                //pbOutput.Image = keAverageDenoising(pictureArray);
            }
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

                    double redAksen = 1/3;
                    double greenAksen = 1/3;
                    double blueAksen = 1/3;

                    if (oldBlue != 0 || oldGreen != 0 || oldRed != 0)
                    {
                        redAksen = oldRed / (oldRed + oldGreen + oldBlue);
                        greenAksen = oldGreen / (oldRed + oldGreen + oldBlue);
                        blueAksen = oldBlue / (oldRed + oldGreen + oldBlue);
                    }

                    double equation1 = redAksen / greenAksen;
                    double equation2 = (redAksen * blueAksen) / ((redAksen + greenAksen + blueAksen) * (redAksen + greenAksen + blueAksen));
                    double equation3 = (redAksen * greenAksen) / ((redAksen + greenAksen + blueAksen) * (redAksen + greenAksen + blueAksen));

                    //Console.WriteLine("red:" + oldRed + " green:" + oldGreen + " blue:" + oldBlue);

                    int max = Math.Max(oldRed, Math.Max(oldGreen, oldBlue));
                    int min = Math.Min(oldRed, Math.Min(oldGreen, oldBlue));

                    Color color = Color.FromArgb(oldRed, oldGreen, oldBlue);
                    double hue = color.GetHue();
                    double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
                    double value = max / 255d;

                    //Console.WriteLine("h:" + hue + " s:" + saturation + " v:" + value);

                    double ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    double cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    double ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    //Console.WriteLine("y:" + ye + " cb:" + cebe + " cr:" + ceer);

                    //double gray = (oldBlue + oldGreen + oldRed) / 3;

                    //if (0 <= hue && hue <= 50 && 0.23 <= saturation && saturation <= 0.68)
                    //if (100 < cebe && cebe < 150 && 150 < ceer && ceer < 200)
                    if (equation1 > 1.185 ||
                        (((0 < hue && hue < 25) || (335 < hue && hue < 360)) && 0.2 < saturation && saturation < 0.6) ||
                        (77 < cebe && cebe < 127 && 133 < ceer && ceer < 173))
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

                    //calculate new pixel value
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
    }
}
