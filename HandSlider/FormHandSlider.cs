using Accord.Imaging;
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
        private Bitmap bitmapBlob, destinationBitmap, destinationBitmapBlob, frame, frameBlobs;
        private BitmapData bitmapData;
        private BlobCounter blobCounter;
        private Blob[] blobs;
        private Dilation dilation;
        private FilterInfoCollection fic;
        private Graphics graphics, graphicsBlob;
        private HiddenMarkovClassifier hmmc;
        private ImageAttributes imageAttributes, imageAttributesBlob;
        private List<int> sequenceCodeList;
        private Median median;
        private NotifyIcon notifyIcon;
        private Opening opening;
        private Pen penRed, penGreen, penBlue;
        private Rectangle destinationRectangle, destinationRectangleBlob;
        private Thread thread;
        private Threshold threshold;
        private System.Timers.Timer timerFPS, timerFrame, timerLabel;
        private VideoCaptureDevice vcd;
        private bool getFrame, closing;
        private byte[] pixels;
        private double ye, cebe, ceer, ratio;
        private int actual;
        private int b, i, j, x, y;
        private int bytesPerPixel, byteCount, heightInPixels, widthInBytes, currentLine;
        private int oldBlue, oldGreen, oldRed;
        private int pointX, pointX1, pointX2, blobHeight, blobWidth, travel;
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
            median = new Median();
            notifyIcon = new NotifyIcon();
            opening = new Opening(new short[3, 3]);
            penGreen = new Pen(Color.Green);
            penRed = new Pen(Color.Red);
            penBlue = new Pen(Color.Blue);
            sequenceCodeList = new List<int>(400);
            threshold = new Threshold(128);
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
            blobCounter.MaxHeight = 35;
            blobCounter.MinWidth = 35;
            blobCounter.MaxWidth = 75;

            destinationBitmap.SetResolution(destinationBitmap.HorizontalResolution, destinationBitmap.VerticalResolution);
            getFrame = true;

            graphics = Graphics.FromImage(destinationBitmap);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;

            resetDetection();

            timerFPS.Elapsed += TimerFPS_Elapsed;
            timerFPS.AutoReset = true;

            timerLabel.Elapsed += TimerLabel_Elapsed;
            timerLabel.AutoReset = true;
            timerLabel.Enabled = true;

            timerFrameInterval = Convert.ToInt16(timerFrame.Interval);

            // GUI
            btnStart.Enabled = false;

            rbOriginal.Enabled = false;
            rbThreshold.Enabled = false;
            rbMorphology.Enabled = false;
            rbBlobDetection.Enabled = false;
            cbMoveDetection.Enabled = false;

            pictureBox1.SuspendLayout();
            pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
            pictureBox1.ResumeLayout();

            label1.Visible = true;

            btnStop.Enabled = false;

            // Training
            thread.Start();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            label1.Visible = false;

            btnStart.Enabled = false;

            rbOriginal.Enabled = true;
            rbThreshold.Enabled = true;
            rbMorphology.Enabled = true;
            rbBlobDetection.Enabled = true;
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
            if (dialogResult == DialogResult.Yes)
            {
                resetDetection();

                vcd.NewFrame -= new NewFrameEventHandler(newFrame);
                vcd.Stop();
                vcd.SignalToStop();

                timerFPS.Enabled = false;
                timerFPS.Stop();

                timerFrame.Enabled = false;
                timerFrame.Stop();

                btnStart.Enabled = true;

                rbOriginal.Checked = true;
                cbMoveDetection.Checked = false;

                rbOriginal.Enabled = false;
                rbThreshold.Enabled = false;
                rbMorphology.Enabled = false;
                rbBlobDetection.Enabled = false;
                cbMoveDetection.Enabled = false;

                pictureBox1.Image = null;
                pictureBoxBlob.Image = null;
                label1.Visible = true;

                btnStop.Enabled = false;

                statusLabelFPS.Text = "0 FPS";

                labelX1.Text = ": " + pointX1.ToString();
                labelX2.Text = ": " + pointX2.ToString();
            }
        }

        private void cbMoveDetection_CheckedChanged(object sender, EventArgs e)
        {
            resetDetection();

            if (cbMoveDetection.Checked)
            {
                rbBlobDetection.Checked = true;

                rbOriginal.Enabled = false;
                rbThreshold.Enabled = false;
                rbMorphology.Enabled = false;
                rbBlobDetection.Enabled = false;
            }
            else
            {
                rbOriginal.Enabled = true;
                rbThreshold.Enabled = true;
                rbMorphology.Enabled = true;
                rbBlobDetection.Enabled = true;
            }
        }

        private void FormHandSlider_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (vcd == null) { return; }

            vcd.NewFrame -= new NewFrameEventHandler(newFrame);
            vcd.Stop();
            vcd.SignalToStop();

            closing = true;
            timerFrame.Enabled = false;
            timerFrame.Stop();
            timerFrame.Dispose();
        }

        private void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!getFrame) { return; }

            frame = resizing(eventArgs.Frame);
            frame.RotateFlip(RotateFlipType.RotateNoneFlipX);

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

            // morfologi
            frame = median.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(Grayscale.CommonAlgorithms.BT709.Apply(frame.Clone() as Bitmap))));

            if (rbMorphology.Checked)
            {
                lock (pictureBox1)
                {
                    pictureBox1.Image = frame.Clone() as Bitmap;
                }
            }

            if (rbBlobDetection.Checked)
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

                if (ratio < 1.7 || 2.5 < ratio) { continue; }

                pointX = Convert.ToInt16(blobs[b].CenterOfGravity.X);

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
                    if (pointX <= frameBlobs.Width / 2)
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

                if (rbBlobDetection.Checked && label.Equals("F"))
                {
                    e.Graphics.DrawRectangle(penRed, blobs[b].Rectangle); continue;
                }
                else if (rbBlobDetection.Checked && label.Equals("S"))
                {
                    e.Graphics.DrawRectangle(penGreen, blobs[b].Rectangle); continue;
                }
                else if (rbBlobDetection.Checked && label.Equals("V"))
                {
                    e.Graphics.DrawRectangle(penBlue, blobs[b].Rectangle);
                    pictureBoxBlob.Image = bitmapBlob;
                }

                if (delay > 0) { return; }

                if (cbMoveDetection.Checked && label.Equals("V"))
                {
                    moving(pointX, bitmapBlob.Height, bitmapBlob.Width);
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

            if (closing) { return; }

            //throw new NotImplementedException();
        }

        private void TimerLabel_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsHandleCreated) { return; }

            if (closing) { return; }

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
            hmmc.LogLikelihood(seq, out actual);

            return hmmc.Models[actual].Tag as string;
        }

        private void moving(int xCoordinate, int bitmapHeight, int bitmapWidth)
        {
            if (pointX1 == 0)
            {
                duration = 5000;

                hand = blobPosition;

                notifyIcon.BalloonTipText = "Tangan " + hand;
                notifyIcon.ShowBalloonTip(duration / 5);

                pointX1 = xCoordinate;

                blobHeight = bitmapWidth;
                blobWidth = bitmapHeight;

                return;
            }

            pointX2 = xCoordinate;
            //Console.WriteLine(blobHeight + "h:w" + blobWidth + " > " + bitmapWidth + "h:w" + bitmapHeight + " >>> " + pointX1 + " - " + pointX2 + " = " + (pointX1 - pointX2));
            if (bitmapWidth < blobHeight - 15 || blobHeight + 15 < bitmapWidth)
            { return; }

            if (bitmapHeight < blobWidth - 30 || blobWidth + 30 < bitmapHeight)
            { return; }

            travel = pointX1 - pointX2;

            if (hand.Equals("KANAN") && blobWidth / 2 < travel && travel < blobWidth * 2)
            {
                notifyIcon.BalloonTipText = "Gerakan KE KIRI";
                notifyIcon.ShowBalloonTip(duration / 5);

                SendKeys.Send("{RIGHT}");
            }
            else if (hand.Equals("KIRI") && -blobWidth * 2 < travel && travel < -blobWidth / 2)
            {
                notifyIcon.BalloonTipText = "Gerakan KE KANAN";
                notifyIcon.ShowBalloonTip(duration / 5);

                SendKeys.Send("{LEFT}");
            }
            else
            { return; }

            delay = 5000;

            labelX1.Text = ": " + pointX1.ToString();
            labelX2.Text = ": " + pointX2.ToString();
        }

        private void resetDetection()
        {
            duration = 0;
            durationElapsed = 0;
            delay = 0;

            blobHeight = blobWidth = pointX1 = pointX2 = 0;
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
            int classes = 3;
            string[] categories = { "F", "S", "V" };
            int[] states = { 2, 2, 2 };

            hmmc = new HiddenMarkovClassifier(classes, states, 3, categories);

            int iterations = 100;
            double limit = 0;

            HiddenMarkovClassifierLearning teacher = new HiddenMarkovClassifierLearning(hmmc, i =>
            {
                return new BaumWelchLearning(hmmc.Models[i])
                {
                    MaxIterations = iterations,
                    Tolerance = limit
                };
            });

            teacher.Learn(Dataset.sequencesTraining, Dataset.labelsTraining);

            //getHMMC();

            //datasetTesting();

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
    }
}