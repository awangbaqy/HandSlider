﻿using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace HandSlider
{
    public partial class FormHandSlider : Form
    {
        private BinaryDilation3x3 binaryDilation3x3;
        private BinaryErosion3x3 binaryErosion3x3;
        private Bitmap bitmapBlob, destinationBitmap, destinationBitmapBlob, frame, frameBlobs, frameForeground, frameBackground;
        private BitmapData bitmapData;
        private BlobCounter blobCounter;
        private Blob[] blobs;
        private Dilation dilation;
        private FilterInfoCollection fic;
        private Graphics graphics, graphicsBlob;
        private Grayscale grayscale;
        private HaeMeM haeMeM;
        private HiddenMarkovClassifier hmmc; // HMMC
        private ImageAttributes imageAttributes, imageAttributesBlob;
        private Intersect intersect;
        private List<int> sequenceCodeList;
        private Median median;
        private Opening opening;
        private Pen penRed, penGreen, penBlue;
        private Point point;
        private Rectangle destinationRectangle, destinationRectangleBlob;
        private Thread thread;
        private Threshold threshold;
        private ThresholdedDifference thresholdedDifference;
        private System.Timers.Timer timerFPS, timerFrame, timerLabel;
        private VideoCaptureDevice vcd;
        private bool getFrame, foregroundChecked, blobChecked, moveChecked;
        private byte[] pixels;
        private double ye, cebe, ceer, ratio;
        private int actual;
        private int b, i, j, x, y;
        private int bytesPerPixel, byteCount, heightInPixels, widthInBytes, currentLine;
        private int oldBlue, oldGreen, oldRed;
        private int pointX1, pointX2, pointY1, pointY2, blobHeight, blobWidth, travelX, travelY;
        private int fps, durationElapsed, delay, timerFrameInterval, duration;
        private string blobPosition, label, hand;
        
        public FormHandSlider()
        {
            InitializeComponent();

            destinationBitmap = new Bitmap(480, 360);
            destinationBitmapBlob = new Bitmap(20, 20);
            binaryDilation3x3 = new BinaryDilation3x3();
            binaryErosion3x3 = new BinaryErosion3x3();
            blobCounter = new BlobCounter();
            destinationRectangle = new Rectangle(0, 0, 480, 360);
            destinationRectangleBlob = new Rectangle(0, 0, 20, 20);
            dilation = new Dilation();
            imageAttributes = new ImageAttributes();
            intersect = new Intersect();
            median = new Median();
            notifyIcon = new NotifyIcon();
            opening = new Opening(new short[3, 3]);
            point = new Point(0, 0);
            penGreen = new Pen(Color.Green, 3);
            penRed = new Pen(Color.Red, 3);
            penBlue = new Pen(Color.Blue, 3);
            sequenceCodeList = new List<int>(400);
            threshold = new Threshold(128);
            thresholdedDifference = new ThresholdedDifference(64);
            thread = new Thread(datasetTraining);
            timerFPS = new System.Timers.Timer(1000); // check FPS
            timerFrame = new System.Timers.Timer(100); // 10 fps
            timerLabel = new System.Timers.Timer(500);
        }

        // Events

        private void FormHandSlider_Load(object sender, EventArgs e)
        {
            blobCounter.CoupledSizeFiltering = true;
            blobCounter.FilterBlobs = true;

            // vertical rectangle
            //blobCounter.MinHeight = 35;
            //blobCounter.MaxHeight = 75;
            //blobCounter.MinWidth = 15;
            //blobCounter.MaxWidth = 35;

            // horizontal rectangle
            blobCounter.MinHeight = 15;
            blobCounter.MaxHeight = 65;
            blobCounter.MinWidth = 35;
            blobCounter.MaxWidth = 125;

            destinationBitmap.SetResolution(destinationBitmap.HorizontalResolution, destinationBitmap.VerticalResolution);
            getFrame = true;

            graphics = Graphics.FromImage(destinationBitmap);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            grayscale = Grayscale.CommonAlgorithms.BT709;

            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

            notifyIcon.Icon = new Icon(Icon, 40, 40);
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;

            resetDetection();

            timerFPS.Elapsed += TimerFPS_Elapsed;
            timerFPS.AutoReset = true;

            //timerLabel.Elapsed += TimerLabel_Elapsed;
            //timerLabel.AutoReset = true;
            //timerLabel.Enabled = true;

            timerFrameInterval = Convert.ToInt16(timerFrame.Interval);

            // GUI
            btnStart.Enabled = false;

            rbOriginal.Enabled = false;
            rbBGS.Enabled = false;
            rbThreshold.Enabled = false;
            rbIntersect.Enabled = false;
            rbMorphology.Enabled = false;

            cbForegroundDetection.Enabled = false;
            cbBlobDetection.Enabled = false;
            cbMoveDetection.Enabled = false;

            pictureBox1.SuspendLayout();
            pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
            pictureBox1.ResumeLayout();

            label1.Visible = true;

            btnStop.Enabled = false;

            // Training
            //thread.Start();
            setHMM();

            btnStart.Enabled = true;

            label1.Text = "Aplikasi Siap!";
        }

        private void FormHandSlider_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipText = "Aplikasi tersimpan di TRAY";
                notifyIcon.ShowBalloonTip(duration / 5);

                Hide();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            label1.Visible = false;

            btnStart.Enabled = false;

            rbOriginal.Enabled = true;
            rbThreshold.Enabled = true;
            rbMorphology.Enabled = true;

            cbForegroundDetection.Enabled = true;
            cbBlobDetection.Enabled = true;
            cbMoveDetection.Enabled = true;

            btnStop.Enabled = true;

            timerFPS.Enabled = true;

            timerFrame.Elapsed += TimerFrame_Elapsed;
            timerFrame.AutoReset = true;
            timerFrame.Enabled = true;

            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            vcd = new VideoCaptureDevice(fic[0].MonikerString);
            vcd.NewFrame += new NewFrameEventHandler(newFrame);
            vcd.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Berhenti merekam?", "Perhatian", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.No) { return; }

            resetDetection();

            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();

            timerFPS.Enabled = false;
            timerFPS.Stop();

            timerFrame.Enabled = false;
            timerFrame.Stop();

            btnStart.Enabled = true;

            // set default controls state
            rbOriginal.Checked = true;

            cbForegroundDetection.Checked = false;
            cbBlobDetection.Checked = false;
            cbMoveDetection.Checked = false;

            // disable control
            rbOriginal.Enabled = false;
            rbBGS.Enabled = false;
            rbThreshold.Enabled = false;
            rbIntersect.Enabled = false;
            rbMorphology.Enabled = false;

            cbForegroundDetection.Enabled = false;
            cbBlobDetection.Enabled = false;
            cbMoveDetection.Enabled = false;

            pictureBox1.Image = null;
            pictureBoxBlob.Image = null;
            label1.Text = "Aplikasi Siap!";
            label1.Visible = true;


            btnStop.Enabled = false;

            statusLabelFPS.Text = "0 FPS";
        }

        private void cbBlobDetection_CheckedChanged(object sender, EventArgs e)
        {
            if (cbBlobDetection.Checked)
            {
                blobChecked = true;
            }
            else
            {
                pictureBoxBlob.Image = null;
                blobChecked = false;
            }
        }

        private void cbForegroundDetection_CheckedChanged(object sender, EventArgs e)
        {
            if (cbForegroundDetection.Checked)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Kamera akan mengambil BACKGROUND.\n" +
                    "Pastikan tidak ada objek bergerak.\n" +
                    "Ambil Background?",
                    "Background", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.No) { cbForegroundDetection.Checked = false; return; }

                thresholdedDifference.OverlayImage = frameBackground.Clone() as Bitmap;

                rbBGS.Enabled = true;
                rbIntersect.Enabled = true;

                foregroundChecked = true;
            }
            else
            {
                rbBGS.Checked = false;
                rbIntersect.Checked = false;

                rbBGS.Enabled = false;
                rbIntersect.Enabled = false;

                frameForeground = null;
                foregroundChecked = false;
            }
        }

        private void cbMoveDetection_CheckedChanged(object sender, EventArgs e)
        {
            resetDetection();

            if (cbMoveDetection.Checked)
            {
                if (!cbForegroundDetection.Checked)
                {
                    MessageBox.Show("Harap Centang Deteksi Objek Gerak!");
                    cbMoveDetection.Checked = false;
                    return;
                }

                rbOriginal.Enabled = false;
                rbBGS.Enabled = false;
                rbThreshold.Enabled = false;
                rbIntersect.Enabled = false;
                rbMorphology.Enabled = false;

                notifyIcon.Visible = true;

                moveChecked = true;
            }
            else
            {
                rbOriginal.Enabled = true;
                rbBGS.Enabled = true;
                rbThreshold.Enabled = true;
                rbIntersect.Enabled = true;
                rbMorphology.Enabled = true;

                notifyIcon.Visible = false;

                moveChecked = false;
            }
        }

        private void FormHandSlider_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Icon = null;

            if (vcd == null) { return; }

            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();

            timerFrame.Enabled = false;
            timerFrame.Stop();
            timerFrame.Dispose();
        }

        private void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!getFrame) { return; }

            frame = resizing(eventArgs.Frame);

            frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
            frameBackground = frame;

            if (rbOriginal.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }

            // get diff foreground
            if (foregroundChecked && frameBackground != null)
            {
                frameForeground = thresholdedDifference.Apply(frame.Clone() as Bitmap);

                if (rbBGS.Checked)
                {
                    lock (pictureBox1)
                    {
                        pictureBox1.Image = frameForeground.Clone() as Bitmap;
                    }
                }
            }

            // get skin foreground
            frame = grayscale.Apply(thresholding(frame.Clone() as Bitmap));

            if (rbThreshold.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }

            // intersect to get foreground
            if (foregroundChecked && frameForeground != null)
            {
                intersect.OverlayImage = frame.Clone() as Bitmap;
                Console.WriteLine(frame.GetPixelFormatSize());
                frame = intersect.Apply(frameForeground.Clone() as Bitmap);

                if (rbIntersect.Checked)
                {
                    lock (pictureBox1)
                    {
                        pictureBox1.Image = frame.Clone() as Bitmap;
                    }
                }
            }

            // morphology
            frame = median.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(frame.Clone() as Bitmap)));

            if (rbMorphology.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }

            getFrame = false;

            fps += 1;

            frameBlobs = frame.Clone() as Bitmap;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // update label
            if (delay > 0)
            {
                labelCountdown.Font = new Font("Microsoft Sans Serif", 50, FontStyle.Regular);
                labelCountdown.ForeColor = Color.Black;

                labelCountdown.Text = Math.Ceiling((double)delay / 1000).ToString();
            }
            else if (durationElapsed > 0)
            {
                labelCountdown.Font = new Font("Microsoft Sans Serif", 50, FontStyle.Underline);
                labelCountdown.ForeColor = Color.Blue;

                labelCountdown.Text = Math.Ceiling((double)(duration - durationElapsed) * 5 / duration).ToString();
            }
            else if (durationElapsed == 0)
            {
                labelCountdown.Font = new Font("Microsoft Sans Serif", 50, FontStyle.Regular);
                labelCountdown.ForeColor = Color.Black;
                labelCountdown.Text = "0";
            }

            // update progres bar
            if (hand.Equals("KIRI"))
            {
                statusProgressBar1.RightToLeft = RightToLeft.No;
                statusProgressBar1.RightToLeftLayout = false;
            }
            else if (hand.Equals("KANAN"))
            {
                statusProgressBar1.RightToLeft = RightToLeft.Yes;
                statusProgressBar1.RightToLeftLayout = true;
            }

            statusProgressBar1.Value = durationElapsed / 50;

            if (frameBlobs == null) { return; }

            // detect blob
            blobCounter.ProcessImage(frameBlobs);
            blobs = blobCounter.GetObjectsInformation();

            for (b = 0; b < blobs.Length; b++)
            {
                blobCounter.ExtractBlobsImage(frameBlobs, blobs[b], false);

                bitmapBlob = blobs[b].Image.ToManagedImage();

                //ratio = Convert.ToDouble(bitmapBlob.Height) / Convert.ToDouble(bitmapBlob.Width); // vertical horizontal
                ratio = Convert.ToDouble(bitmapBlob.Width) / Convert.ToDouble(bitmapBlob.Height);

                if (ratio < 1.25 || 3.0 < ratio) { continue; }

                point.X = Convert.ToInt16(blobs[b].CenterOfGravity.X);
                point.Y = Convert.ToInt16(blobs[b].CenterOfGravity.Y);

                if (hand.Equals("KIRI"))
                {
                    bitmapBlob.RotateFlip(RotateFlipType.Rotate90FlipNone); // left-handed
                }
                else if (hand.Equals("KANAN"))
                {
                    bitmapBlob.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                else
                {
                    if (point.X <= frameBlobs.Width / 2)
                    {
                        bitmapBlob.RotateFlip(RotateFlipType.Rotate90FlipNone); // left-handed
                        blobPosition = "KIRI";
                    }
                    else
                    {
                        bitmapBlob.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        blobPosition = "KANAN";
                    }
                }

                label = getHandLabel(code(threshold.Apply(Grayscale.CommonAlgorithms.BT709.Apply(resizingBlob(bitmapBlob)))));

                if (blobChecked && label.Equals("F"))
                {
                    e.Graphics.DrawRectangle(penRed, blobs[b].Rectangle); continue;
                }
                else if (blobChecked && label.Equals("S"))
                {
                    e.Graphics.DrawRectangle(penGreen, blobs[b].Rectangle); continue;
                }
                else if (blobChecked && label.Equals("V"))
                {
                    e.Graphics.DrawRectangle(penBlue, blobs[b].Rectangle);
                    pictureBoxBlob.Image = bitmapBlob;
                }

                if (delay > 0) { return; }

                if (moveChecked && label.Equals("V"))
                {
                    moving(point, bitmapBlob.Height, bitmapBlob.Width);
                }
            }

            //throw new NotImplementedException();
        }

        private void TimerFPS_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                statusLabelFPS.Text = fps + " FPS";
            }));

            fps = 0;

            //throw new NotImplementedException();
        }

        private void TimerFrame_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            getFrame = true;
            // Console.WriteLine("d " + duration + " de " + durationElapsed + " dl " + delay);
            if (delay > 0)
            {
                delay -= timerFrameInterval;

                if (delay <= 0)
                {
                    resetDetection();
                }
            }
            else if (duration > 0)
            {
                durationElapsed = durationElapsed + timerFrameInterval;

                if (durationElapsed > duration)
                {
                    resetDetection();

                    notifyIcon.BalloonTipText = "GAGAL Mendeteksi Gerakan";
                    notifyIcon.ShowBalloonTip(duration / 5);

                    delay = 2500;
                    durationElapsed = 5000;
                }
            }

            //throw new NotImplementedException();
        }

        private void TimerLabel_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsHandleCreated) { return; }

            Invoke(new Action(() =>
            {

                if (label1.Text == "Menyiapkan Dataset")
                {
                    label1.Text = "Menyiapkan Dataset.";
                }
                else if (label1.Text == "Menyiapkan Dataset.")
                {
                    label1.Text = "Menyiapkan Dataset..";
                }
                else if (label1.Text == "Menyiapkan Dataset..")
                {
                    label1.Text = "Menyiapkan Dataset...";
                }
                else if (label1.Text == "Menyiapkan Dataset...")
                {
                    label1.Text = "Menyiapkan Dataset";
                }

            }));

            //throw new NotImplementedException();
        }

        // Functions | Methods

        private Bitmap resizing(Bitmap bitmap)
        {
            graphics.DrawImage(bitmap, destinationRectangle, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);

            return destinationBitmap;
        }

        private Bitmap resizingBlob(Bitmap bitmap)
        {
            destinationBitmapBlob.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            using (graphicsBlob = Graphics.FromImage(destinationBitmapBlob))
            {
                graphicsBlob.CompositingMode = CompositingMode.SourceCopy;
                graphicsBlob.CompositingQuality = CompositingQuality.HighQuality;
                graphicsBlob.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsBlob.SmoothingMode = SmoothingMode.HighQuality;
                graphicsBlob.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (imageAttributesBlob = new ImageAttributes())
                {
                    imageAttributesBlob.SetWrapMode(WrapMode.TileFlipXY);
                    graphicsBlob.DrawImage(bitmap, destinationRectangleBlob, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributesBlob);
                }
            }

            return destinationBitmapBlob;
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
                    oldBlue = pixels[currentLine + x];
                    oldGreen = pixels[currentLine + x + 1];
                    oldRed = pixels[currentLine + x + 2];

                    ye = (0.299 * oldRed) + (0.587 * oldGreen) + (0.114 * oldBlue);
                    cebe = 128 + (-0.168736 * oldRed) + (-0.331264 * oldGreen) + (0.5 * oldBlue);
                    ceer = 128 + (0.5 * oldRed) + (-0.418688 * oldGreen) + (-0.081312 * oldBlue);

                    if (78 <= cebe && cebe <= 126 && 134 <= ceer && ceer <= 172)
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
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private int[] code(Bitmap bitmap)
        {
            sequenceCodeList.Clear();

            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    if (bitmap.GetPixel(i, j).R == 0)
                    {
                        sequenceCodeList.Add(0);
                    }
                    else
                    {
                        sequenceCodeList.Add(1);
                    }
                }
            }

            return sequenceCodeList.ToArray();
        }

        private string getHandLabel(int[] seq)
        {
            //hmmc.LogLikelihood(seq, out actual);

            //return hmmc.Models[actual].Tag as string;
            
            double[] likelihood;
            actual = haeMeM.Compute(seq, out likelihood);

            return actual == 0 ? "F" : actual == 1 ? "S" : "V";
        }

        private void moving(Point point, int bitmapHeight, int bitmapWidth)
        {
            if (pointX1 == 0 && pointY1 == 0)
            {
                duration = 5000;

                hand = blobPosition;

                notifyIcon.Visible = true;
                notifyIcon.BalloonTipText = "Tangan " + hand;
                notifyIcon.ShowBalloonTip(duration / 5);

                pointX1 = point.X;
                pointY1 = point.Y;

                blobHeight = bitmapWidth;
                blobWidth = bitmapHeight;

                return;
            }

            pointX2 = point.X;
            pointY2 = point.Y;
            //Console.WriteLine(blobHeight + "h:w" + blobWidth + " > " + bitmapWidth + "h:w" + bitmapHeight + " >>> ");
            //Console.WriteLine(pointX1 + " - " + pointX2 + " = " + (pointX1 - pointX2));
            //Console.WriteLine(pointY1 + " - " + pointY2 + " = " + (pointY1 - pointY2));
            if (bitmapWidth < blobHeight - 15 || blobHeight + 15 < bitmapWidth)
            { return; }

            if (bitmapHeight < blobWidth - 30 || blobWidth + 30 < bitmapHeight)
            { return; }

            travelX = pointX1 - pointX2;
            travelY = pointY1 - pointY2;

            //if (hand.Equals("KANAN") && blobWidth / 2 < travelX)
            if (hand.Equals("KANAN") && blobWidth / 2 < travelX && travelX < blobWidth * 2)
            {
                notifyIcon.BalloonTipText = "Gerakan KE KIRI";
                notifyIcon.ShowBalloonTip(duration / 5);

                SendKeys.Send("{DOWN}");
            }
            //else if (hand.Equals("KIRI") && -blobWidth * 2 < travelX)
            else if (hand.Equals("KIRI") && -blobWidth * 2 < travelX && travelX < -blobWidth / 2)
            {
                notifyIcon.BalloonTipText = "Gerakan KE KANAN";
                notifyIcon.ShowBalloonTip(duration / 5);

                SendKeys.Send("{UP}");
            }
            //else if (-blobHeight * 2 < travelY)
            else if (-blobHeight * 2 < travelY && travelY < -blobHeight / 2)
            {
                notifyIcon.BalloonTipText = "Gerakan KE BAWAH";
                notifyIcon.ShowBalloonTip(duration / 5);

                SendKeys.Send("{END}");
            }
            //else if (blobHeight / 2 < travelY)
            else if (blobHeight / 2 < travelY && travelY < blobHeight * 2)
            {
                notifyIcon.BalloonTipText = "Gerakan KE ATAS";
                notifyIcon.ShowBalloonTip(duration / 5);

                SendKeys.Send("{HOME}");
            }
            else
            { return; }

            delay = 5000;
        }

        private void resetDetection()
        {
            duration = 0;
            durationElapsed = 0;
            delay = 0;

            blobHeight = blobWidth = pointX1 = pointX2 = pointY1 = pointY2 = 0;
            hand = "NETRAL";
        }

        // Dataset

        private void datasetTesting()
        {
            int wrong_total, right_total, f_total, ff, fs, fv, s_total, sf, ss, sv, v_total, vf, vs, vv;
            wrong_total = right_total = f_total = ff = fs = fv = s_total = sf = ss = sv = v_total = vf = vs = vv = 0;

            int[] expected = new int[51];
            int[] actual = new int[51];

            for (int i = 0; i < 51; i++)
            {
                expected[i] = hmmc.Models.Find(x => x.Tag as string == Dataset.labelsTesting[i])[0];

                hmmc.LogLikelihood(Dataset.sequencesTesting[i], out actual[i]);

                if (actual[i] == expected[i]) { right_total += 1; }
                if (actual[i] != expected[i]) { wrong_total += 1; }

                string expect = hmmc.Models[actual[i]].Tag as string;

                if (Dataset.labelsTesting[i].Equals("F"))
                {
                    f_total += 1;
                    if (expect.Equals("F")) { ff += 1; }
                    if (expect.Equals("S")) { fs += 1; }
                    if (expect.Equals("V")) { fv += 1; }
                }

                if (Dataset.labelsTesting[i].Equals("S"))
                {
                    s_total += 1;
                    if (expect.Equals("F")) { sf += 1; }
                    if (expect.Equals("S")) { ss += 1; }
                    if (expect.Equals("V")) { sv += 1; }
                }

                if (Dataset.labelsTesting[i].Equals("V"))
                {
                    v_total += 1;
                    if (expect.Equals("F")) { vf += 1; }
                    if (expect.Equals("S")) { vs += 1; }
                    if (expect.Equals("V")) { vv += 1; }
                }
            }

            MessageBox.Show(
                    "benar = " + right_total + " salah = " + wrong_total + "\n" +
                    "-===- \n" +
                    "F = " + f_total + " > F = " + ff + " / S = " + fs + " / V = " + fv + "\n" +
                    "S = " + s_total + " > F = " + sf + " / S = " + ss + " / V = " + sv + "\n" +
                    "V = " + v_total + " > F = " + vf + " / S = " + vs + " / V = " + vv
                );
        }

        private void datasetTraining()
        {
            //int classes = 3;
            //string[] categories = { "F", "S", "V" };
            //int[] states = { 2, 2, 2 };

            //hmmc = new HiddenMarkovClassifier(classes, states, 3, categories);

            //int iterations = 100;
            //double limit = 0;

            //HiddenMarkovClassifierLearning teacher = new HiddenMarkovClassifierLearning(hmmc, i =>
            //{
            //    return new BaumWelchLearning(hmmc.Models[i])
            //    {
            //        MaxIterations = iterations,
            //        Tolerance = limit
            //    };
            //});

            //teacher.Learn(Dataset.sequencesTraining, Dataset.labelsTraining);

            //getHMMC();
            //datasetTesting();

            setHMM();
            //setHMM2();
            //setHMM3();
            //dataTesting();

            timerLabel.Stop();
            timerLabel.Dispose();

            Invoke(new Action(() =>
            {

                btnStart.Enabled = true;

                label1.Text = "Aplikasi Siap!";

            }));
        }

        private void getHMMC()
        {
            Console.WriteLine("Number Of Classes : " + hmmc.NumberOfClasses);
            Console.WriteLine("Number Of Inputs : " + hmmc.NumberOfInputs);
            Console.WriteLine("Number Of Outputs : " + hmmc.NumberOfOutputs);
            Console.WriteLine("Sensitivity : " + hmmc.Sensitivity.ToString("F99").TrimEnd('0'));
            Console.WriteLine("Threshold : " + hmmc.Threshold);

            Console.WriteLine();

            for (int i = 0; i < hmmc.Models.Length; i++)
            {
                Console.WriteLine("Class : ", hmmc.Models[i].Tag as string);

                Console.WriteLine("Algorithm : " + hmmc.Models[i].Algorithm);
                Console.WriteLine("Number Of Classes : " + hmmc.Models[i].NumberOfClasses);
                Console.WriteLine("Number Of Inputs : " + hmmc.Models[i].NumberOfInputs);
                Console.WriteLine("Number Of Outputs : " + hmmc.Models[i].NumberOfOutputs);

                Console.WriteLine();
                Console.WriteLine("Initial : ");

                for (int j = 0; j < hmmc.Models[i].LogInitial.Length; j++)
                {
                    Console.WriteLine(hmmc.Models[i].LogInitial[j].ToString("F99").TrimEnd('0'));
                }

                Console.WriteLine();
                Console.WriteLine("Emissions : ");

                for (int j = 0; j < hmmc.Models[i].LogEmissions.Length; j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int k = 0; k < hmmc.Models[i].LogEmissions[j].Length; k++)
                    {
                        Console.WriteLine(hmmc.Models[i].LogEmissions[j][k]);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Transitions : ");

                for (int j = 0; j < hmmc.Models[i].LogTransitions.Length; j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int k = 0; k < hmmc.Models[i].LogTransitions[j].Length; k++)
                    {
                        Console.WriteLine(hmmc.Models[i].LogTransitions[j][k]);
                    }
                }

                Console.WriteLine();
            }
        }

        ///

        private void dataTesting()
        {
            int wrong_total, right_total, f_total, ff, fs, fv, s_total, sf, ss, sv, v_total, vf, vs, vv;
            wrong_total = right_total = f_total = ff = fs = fv = s_total = sf = ss = sv = v_total = vf = vs = vv = 0;

            int[] expected = new int[51];
            int[] actual = new int[51];

            for (int i = 0; i < 51; i++)
            {
                int label = Dataset.labelsTesting[i] == "F" ? 0 : Dataset.labelsTesting[i] == "S" ? 1 : 2;
                expected[i] = label;

                double[] likelihood;
                actual[i] = haeMeM.Compute(Dataset.sequencesTesting[i], out likelihood);

                if (actual[i] == expected[i]) { right_total += 1; }
                if (actual[i] != expected[i]) { wrong_total += 1; }

                string expect = actual[i].ToString();

                if (Dataset.labelsTesting[i].Equals("F"))
                {
                    f_total += 1;
                    if (expect.Equals("0")) { ff += 1; }
                    if (expect.Equals("1")) { fs += 1; }
                    if (expect.Equals("2")) { fv += 1; }
                }

                if (Dataset.labelsTesting[i].Equals("S"))
                {
                    s_total += 1;
                    if (expect.Equals("0")) { sf += 1; }
                    if (expect.Equals("1")) { ss += 1; }
                    if (expect.Equals("2")) { sv += 1; }
                }

                if (Dataset.labelsTesting[i].Equals("V"))
                {
                    v_total += 1;
                    if (expect.Equals("0")) { vf += 1; }
                    if (expect.Equals("1")) { vs += 1; }
                    if (expect.Equals("2")) { vv += 1; }
                }
            }

            MessageBox.Show(
                    "benar = " + right_total + " salah = " + wrong_total + "\n" +
                    "-===- \n" +
                    "F = " + f_total + " > F = " + ff + " / S = " + fs + " / V = " + fv + "\n" +
                    "S = " + s_total + " > F = " + sf + " / S = " + ss + " / V = " + sv + "\n" +
                    "V = " + v_total + " > F = " + vf + " / S = " + vs + " / V = " + vv
                );
        }

        private void setHMM()
        {
            haeMeM = new HaeMeM(3, new int[] { 2, 2, 2 }, 2); // 2 symbols

            // F
            haeMeM.mModels[0].mLogProbabilityVector[0] = -0.00000000000000532907051820075;
            haeMeM.mModels[0].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[0].mLogEmissionMatrix[0, 0] = -5.03499971689081E-05;
            haeMeM.mModels[0].mLogEmissionMatrix[0, 1] = -9.89653717192396;

            haeMeM.mModels[0].mLogEmissionMatrix[1, 0] = -4.3689885090358;
            haeMeM.mModels[0].mLogEmissionMatrix[1, 1] = -0.0127449161698205;

            haeMeM.mModels[0].mLogTransitionMatrix[0, 0] = -0.149661550949128;
            haeMeM.mModels[0].mLogTransitionMatrix[0, 1] = -1.97327653636094;

            haeMeM.mModels[0].mLogTransitionMatrix[1, 0] = -1.92269140744535;
            haeMeM.mModels[0].mLogTransitionMatrix[1, 1] = -0.158073429521986;

            // S
            haeMeM.mModels[1].mLogProbabilityVector[0] = -0.00000000000000532907051820075;
            haeMeM.mModels[1].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[1].mLogEmissionMatrix[0, 0] = -0.000111085053831061;
            haeMeM.mModels[1].mLogEmissionMatrix[0, 1] = -9.10526993937915;

            haeMeM.mModels[1].mLogEmissionMatrix[1, 0] = -5.71624233150094;
            haeMeM.mModels[1].mLogEmissionMatrix[1, 1] = -0.00329748889908821;

            haeMeM.mModels[1].mLogTransitionMatrix[0, 0] = -0.119154430996792;
            haeMeM.mModels[1].mLogTransitionMatrix[0, 1] = -2.18632059888411;

            haeMeM.mModels[1].mLogTransitionMatrix[1, 0] = -1.65479286937839;
            haeMeM.mModels[1].mLogTransitionMatrix[1, 1] = -0.212119095404228;

            // V
            haeMeM.mModels[2].mLogProbabilityVector[0] = -0.00000000000000444089209850063;
            haeMeM.mModels[2].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[2].mLogEmissionMatrix[0, 0] = -0.00146927640987471;
            haeMeM.mModels[2].mLogEmissionMatrix[0, 1] = -6.52371978606421;

            haeMeM.mModels[2].mLogEmissionMatrix[1, 0] = -4.53524048530201;
            haeMeM.mModels[2].mLogEmissionMatrix[1, 1] = -0.0107822479535873;

            haeMeM.mModels[2].mLogTransitionMatrix[0, 0] = -0.107012161763762;
            haeMeM.mModels[2].mLogTransitionMatrix[0, 1] = -2.28784176570574;

            haeMeM.mModels[2].mLogTransitionMatrix[1, 0] = -1.6357066766575;
            haeMeM.mModels[2].mLogTransitionMatrix[1, 1] = -0.216682781482586;
        }

        private void setHMM2()
        {
            haeMeM = new HaeMeM(3, new int[] { 2, 2, 2 }, 2); // 2 symbols

            // F
            haeMeM.mModels[0].mLogProbabilityVector[0] = 4.60517018598809;
            haeMeM.mModels[0].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[0].mLogEmissionMatrix[0, 0] = -0.0282737632060392;
            haeMeM.mModels[0].mLogEmissionMatrix[0, 1] = -3.5799245723373;

            haeMeM.mModels[0].mLogEmissionMatrix[1, 0] = -2.84917408089784;
            haeMeM.mModels[0].mLogEmissionMatrix[1, 1] = -0.0596354836935715;

            haeMeM.mModels[0].mLogTransitionMatrix[0, 0] = -0.486080002584086;
            haeMeM.mModels[0].mLogTransitionMatrix[0, 1] = -0.954596626611644;

            haeMeM.mModels[0].mLogTransitionMatrix[1, 0] = -1.24401842126112;
            haeMeM.mModels[0].mLogTransitionMatrix[1, 1] = -0.339991579241115;

            // S
            haeMeM.mModels[1].mLogProbabilityVector[0] = 4.66343909411207;
            haeMeM.mModels[1].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[1].mLogEmissionMatrix[0, 0] = -0.00204325325356169;
            haeMeM.mModels[1].mLogEmissionMatrix[0, 1] = -6.1942334617523;

            haeMeM.mModels[1].mLogEmissionMatrix[1, 0] = -2.04722409378819;
            haeMeM.mModels[1].mLogEmissionMatrix[1, 1] = -0.138219801536478;

            haeMeM.mModels[1].mLogTransitionMatrix[0, 0] = -0.260224984533663;
            haeMeM.mModels[1].mLogTransitionMatrix[0, 1] = -1.47350123639483;

            haeMeM.mModels[1].mLogTransitionMatrix[1, 0] = -1.09049463421592;
            haeMeM.mModels[1].mLogTransitionMatrix[1, 1] = -0.409548780939733;

            // V
            haeMeM.mModels[2].mLogProbabilityVector[0] = 4.66343909411207;
            haeMeM.mModels[2].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[2].mLogEmissionMatrix[0, 0] = -0.130399133146794;
            haeMeM.mModels[2].mLogEmissionMatrix[0, 1] = -2.10164644672664;

            haeMeM.mModels[2].mLogEmissionMatrix[1, 0] = -1.26825866084412;
            haeMeM.mModels[2].mLogEmissionMatrix[1, 1] = -0.33034057298523;

            haeMeM.mModels[2].mLogTransitionMatrix[0, 0] = -0.223346270437295;
            haeMeM.mModels[2].mLogTransitionMatrix[0, 1] = -1.60862744664449;

            haeMeM.mModels[2].mLogTransitionMatrix[1, 0] = -1.28392656597872;
            haeMeM.mModels[2].mLogTransitionMatrix[1, 1] = -0.32427373471596;
        }

        private void setHMM3()
        {
            haeMeM = new HaeMeM(3, new int[] { 2, 2, 2 }, 2); // 2 symbols

            // F
            haeMeM.mModels[0].mLogProbabilityVector[0] = 4.49980967033027;
            haeMeM.mModels[0].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[0].mLogEmissionMatrix[0, 0] = -0.0323421109261171;
            haeMeM.mModels[0].mLogEmissionMatrix[0, 1] = -3.44751262559617;

            haeMeM.mModels[0].mLogEmissionMatrix[1, 0] = -3.18798397212082;
            haeMeM.mModels[0].mLogEmissionMatrix[1, 1] = -0.0421300979258321;

            haeMeM.mModels[0].mLogTransitionMatrix[0, 0] = -0.502925346946061;
            haeMeM.mModels[0].mLogTransitionMatrix[0, 1] = -0.928259421298462;

            haeMeM.mModels[0].mLogTransitionMatrix[1, 0] = -1.21316535507931;
            haeMeM.mModels[0].mLogTransitionMatrix[1, 1] = -0.352761001237768;

            // S
            haeMeM.mModels[1].mLogProbabilityVector[0] = 4.49980967033027;
            haeMeM.mModels[1].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[1].mLogEmissionMatrix[0, 0] = -0.00141060935977677;
            haeMeM.mModels[1].mLogEmissionMatrix[0, 1] = -6.5644387195691;

            haeMeM.mModels[1].mLogEmissionMatrix[1, 0] = -2.09616532607238;
            haeMeM.mModels[1].mLogEmissionMatrix[1, 1] = -0.131164949382678;

            haeMeM.mModels[1].mLogTransitionMatrix[0, 0] = -0.281310462000217;
            haeMeM.mModels[1].mLogTransitionMatrix[0, 1] = -1.4056564596633;

            haeMeM.mModels[1].mLogTransitionMatrix[1, 0] = -1.15114008474999;
            haeMeM.mModels[1].mLogTransitionMatrix[1, 1] = -0.380200925719366;

            // V
            haeMeM.mModels[2].mLogProbabilityVector[0] = 4.60517018598809;
            haeMeM.mModels[2].mLogProbabilityVector[1] = double.NegativeInfinity;

            haeMeM.mModels[2].mLogEmissionMatrix[0, 0] = -0.137163436146246;
            haeMeM.mModels[2].mLogEmissionMatrix[0, 1] = -2.05438003176161;

            haeMeM.mModels[2].mLogEmissionMatrix[1, 0] = -1.39770568075147;
            haeMeM.mModels[2].mLogEmissionMatrix[1, 1] = -0.28390705428523;

            haeMeM.mModels[2].mLogTransitionMatrix[0, 0] = -0.241516713005511;
            haeMeM.mModels[2].mLogTransitionMatrix[0, 1] = -1.53914570999587;

            haeMeM.mModels[2].mLogTransitionMatrix[1, 0] = -1.21877591968983;
            haeMeM.mModels[2].mLogTransitionMatrix[1, 1] = -0.350397223877858;
        }
    }
}
