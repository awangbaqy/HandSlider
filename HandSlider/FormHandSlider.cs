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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace HandSlider
{
    public partial class FormHandSlider : Form
    {
        BinaryDilation3x3 binaryDilation3x3;
        BinaryErosion3x3 binaryErosion3x3;
        Bitmap bitmapBlob, destImage, destinationBitmapBlob, frame, frameBlobs;
        BitmapData bitmapData;
        BlobCounter blobCounter;
        Blob[] blobs;
        Dilation dilation;
        FilterInfoCollection fic;
        Graphics graphics, graphicsBlob, g;
        HiddenMarkovClassifier hmmc;
        ImageAttributes imageAttributesBlob, wrapMode;
        List<int> sequenceCodeList;
        Median median;
        Opening opening;
        Pen penRed, penGreen, penBlue;
        Rectangle destRect, destinationRectangleBlob;
        Thread thread;
        Threshold threshold;
        System.Timers.Timer timerMovement, timerLabel, timerFrame, timerFPS;
        VideoCaptureDevice vcd;

        bool getFrame, detected;
        byte[] pixels;
        double ye, cebe, ceer, ratio;
        int actual;
        int b, i, j, x, y;
        int bytesPerPixel, byteCount, heightInPixels, widthInBytes, currentLine;
        int oldBlue, oldGreen, oldRed;
        int pointX, pointX1, pointX2, handWidth;
        int fps, timerElapsed, timerInterval;
        int[] sequenceBlob;
        string hand, label;

        public FormHandSlider()
        {
            InitializeComponent();
            //SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            destImage = new Bitmap(480, 360);
            destinationBitmapBlob = new Bitmap(20, 20);
            binaryDilation3x3 = new BinaryDilation3x3();
            binaryErosion3x3 = new BinaryErosion3x3();
            blobCounter = new BlobCounter();
            destinationRectangleBlob = new Rectangle(0, 0, 20, 20);
            destRect = new Rectangle(0, 0, 480, 360);
            dilation = new Dilation();
            median = new Median();
            opening = new Opening(new short[3, 3]);
            penGreen = new Pen(Color.Green);
            penRed = new Pen(Color.Red);
            penBlue = new Pen(Color.Blue);
            sequenceBlob = new int[400];
            threshold = new Threshold(128);
            thread = new Thread(datasetTraining);
            timerMovement = new System.Timers.Timer(1500); // movement duration
            timerLabel = new System.Timers.Timer(500);
            timerFrame = new System.Timers.Timer(100); // 10 fps
            timerFPS = new System.Timers.Timer(1000); // check FPS
        }

        // Events

        private void FormHandSlider_Load(object sender, EventArgs e)
        {
            blobCounter.CoupledSizeFiltering = true;
            blobCounter.FilterBlobs = true;

            // vertical rectangle
            //blobCounter.MinHeight = 50;
            //blobCounter.MaxHeight = 60;
            //blobCounter.MinWidth = 40;
            //blobCounter.MaxWidth = 50;

            // horizontal rectangle
            //blobCounter.MinHeight = 40;
            //blobCounter.MaxHeight = 50;
            //blobCounter.MinWidth = 50;
            //blobCounter.MaxWidth = 60;

            // vertical rectangle // need research
            //blobCounter.MinHeight = 50;
            //blobCounter.MaxHeight = 140;
            //blobCounter.MinWidth = 30;
            //blobCounter.MaxWidth = 70;

            // horizontal rectangle
            blobCounter.MinHeight = 16;
            //blobCounter.MaxHeight = 70;
            blobCounter.MinWidth = 16;
            //blobCounter.MaxWidth = 140;

            getFrame = true;

            hand = "RIGHT";

            pictureBox1.SuspendLayout();
            pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
            pictureBox1.ResumeLayout();

            timerFPS.Elapsed += TimerFPS_Elapsed;
            timerFPS.AutoReset = true;

            timerLabel.Elapsed += TimerLabel_Elapsed;
            timerLabel.AutoReset = true;
            timerLabel.Enabled = true;

            timerMovement.Elapsed += TimerMovement_Elapsed;
            timerInterval = Convert.ToInt16(timerMovement.Interval);

            // GUI
            btnStart.Enabled = false;

            rbOriginal.Enabled = false;
            rbThreshold.Enabled = false;
            rbMorphology.Enabled = false;
            rbBlobDetection.Enabled = false;
            cbMoveDetection.Enabled = false;

            btnStop.Enabled = false;

            label1.Visible = true;
            
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
                vcd.NewFrame -= new NewFrameEventHandler(newFrame);
                vcd.Stop();
                vcd.SignalToStop();

                btnStart.Enabled = true;

                rbOriginal.Enabled = false;
                rbThreshold.Enabled = false;
                rbMorphology.Enabled = false;
                rbBlobDetection.Enabled = false;
                cbMoveDetection.Enabled = false;

                btnStop.Enabled = false;

                pointX1 = pointX2 = 0;
                pictureBox1.Image = null;
                label1.Visible = true;

                timerFPS.Enabled = false;
                timerFrame.Enabled = false;

                statusLabelFPS.Text = "0 FPS";
                statusProgressBar1.Value = 0;
            }
        }

        private void cbMoveDetection_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMoveDetection.Checked)
            {
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

            frame = morphologing(frame.Clone() as Bitmap);

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
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (frame == null) { return; }

            frameBlobs = frame.Clone() as Bitmap;
            blobCounter.ProcessImage(frameBlobs);
            blobs = blobCounter.GetObjectsInformation();
            g = e.Graphics;
            pictureBoxBlob.Image = null;

            for (b = 0; b < blobs.Length; b++)
            {
                blobCounter.ExtractBlobsImage(frameBlobs, blobs[b], false);
                
                bitmapBlob = blobs[b].Image.ToManagedImage();

                //ratio = Convert.ToDouble(bitmapBlob.Height) / Convert.ToDouble(bitmapBlob.Width); // vertical horizontal
                ratio = Convert.ToDouble(bitmapBlob.Width) / Convert.ToDouble(bitmapBlob.Height);

                //if (ratio < 1.31159 || 1.68420 < ratio ) { continue; } // average from dataset
                //if (ratio < 0.58392 || 3.29167 < ratio) { continue; } // min max from dataset
                if (ratio < 0.72895 || 2.57129 < ratio) { continue; } // average from min max of dataset

                pointX = Convert.ToInt16(blobs[b].CenterOfGravity.X);

                if (pointX <= (frameBlobs.Width / 2)) 
                {
                    bitmapBlob.RotateFlip(RotateFlipType.Rotate90FlipNone); // left-handed
                    hand = "LEFT";
                }
                else
                {
                    bitmapBlob.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    hand = "RIGHT";
                }

                //if (findVe()) { continue; }

                sequenceBlob = code(threshold.Apply(Grayscale.CommonAlgorithms.BT709.Apply(resizingBlob(bitmapBlob))));
                label = getHandLabel(sequenceBlob);

                if (rbBlobDetection.Checked && label.Equals("F"))
                {
                    g.DrawRectangle(penRed, blobs[b].Rectangle);
                }
                else if (rbBlobDetection.Checked && label.Equals("S"))
                {
                    g.DrawRectangle(penGreen, blobs[b].Rectangle);
                }
                else if (rbBlobDetection.Checked && label.Equals("V"))
                {
                    g.DrawRectangle(penBlue, blobs[b].Rectangle);
                    pictureBoxBlob.Image = bitmapBlob;
                }

                if (cbMoveDetection.Checked && label.Equals("V"))
                {
                    moving(pointX, bitmapBlob.Height);
                }
            }
            
            //throw new NotImplementedException();
        }

        private void TimerLabel_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Invoke(new Action(() => {

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

        private void TimerMovement_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            pointX1 = pointX2 = 0;
            timerMovement.Stop();

            Invoke(new Action(() => {

                labelX1.Text = ": " + pointX1.ToString();
                labelX2.Text = ": " + pointX2.ToString();
                labelMovement.Text = "NETRAL";

            }));

            //throw new NotImplementedException();
        }

        private void TimerFPS_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Invoke(new Action(() => {
                statusLabelFPS.Text = fps + " FPS";
            }));
            fps = 0;

            //throw new NotImplementedException();
        }

        private void TimerFrame_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            getFrame = true;

            if (detected)
            {
                timerElapsed = timerElapsed >= timerInterval ? 0 : timerElapsed + 100;
            }
            else
            {
                timerElapsed = 0;
            }

            Invoke(new Action(() => {

                if (hand.Equals("RIGHT"))
                {
                    statusProgressBar1.RightToLeft = RightToLeft.Yes;
                    statusProgressBar1.RightToLeftLayout = true;
                }
                else
                {
                    statusProgressBar1.RightToLeft = RightToLeft.No;
                    statusProgressBar1.RightToLeftLayout = false;
                }

                statusProgressBar1.Value = (timerElapsed * 100) / timerInterval;

            }));

            //throw new NotImplementedException();
        }

        // Functions | Methods

        private Bitmap morphologing(Bitmap bitmap)
        {
            return median.Apply(binaryErosion3x3.Apply(binaryDilation3x3.Apply(Grayscale.CommonAlgorithms.BT709.Apply(bitmap))));
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

        //private bool findVe()
        //{
        //    sequenceBlob = code(threshold.Apply(Grayscale.CommonAlgorithms.BT709.Apply(resizingBlob(bitmapBlob))));

        //    if (!getHandLabel(sequenceBlob).Equals("V")) { return false; }

        //    if (rbBlobDetection.Checked)
        //    {
        //        g.DrawRectangle(penBlue, blobs[b].Rectangle);

        //        pictureBoxBlob.Image = bitmapBlob;
        //    }

        //    if (cbMoveDetection.Checked)
        //    {
        //        moving(Convert.ToInt16(blobs[b].CenterOfGravity.X));
        //    }

        //    return true;
        //}

        private int[] code(Bitmap bitmap)
        {
            sequenceCodeList = new List<int>(400);

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
        
        private void moving(int xCoordinate, int width)
        {
            if (pointX1 == 0 && handWidth == 0)
            {
                pointX1 = xCoordinate;
                timerMovement.Start();
                detected = true;
                handWidth = width;
            }
            else
            {
                pointX2 = xCoordinate;

                if (hand.Equals("RIGHT") && pointX1 - pointX2 < -handWidth)
                {
                    SendKeys.Send("{LEFT}");
                    labelMovement.Text = "KIRI";

                    timerMovement.Stop();
                    pointX1 = pointX2 = 0;
                    detected = false;                    
                }
                else if (hand.Equals("LEFT") && pointX1 - pointX2 > handWidth)
                {
                    SendKeys.Send("{RIGHT}");
                    labelMovement.Text = "KANAN";

                    timerMovement.Stop();
                    pointX1 = pointX2 = 0;
                    detected = false;
                    hand = "RIGHT";
                }
            }

            labelX1.Text = ": " + pointX1.ToString();
            labelX2.Text = ": " + pointX2.ToString();
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

            var teacher = new HiddenMarkovClassifierLearning(hmmc, i =>
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

            Invoke(new Action(() => {

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