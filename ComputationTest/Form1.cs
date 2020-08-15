using Accord.Imaging.Filters;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ComputationTest
{
    public partial class Form1 : Form
    {
        private Stopwatch stopwatch = new Stopwatch();
        private bool c;
        private Grayscale grayscale = Grayscale.CommonAlgorithms.BT709;
        private Bitmap bitmap, bitmapGray;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                c = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bitmap = pictureBox1.Image.Clone() as Bitmap;

            bitmapGray = null;
            bitmapGray = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
            bitmapGray = null;

            bitmapGray = null;
            bitmapGray = grayscale.Apply(bitmap);
            bitmapGray = null;

            stopwatch.Start();
            for (int l = 0; l < 600; l++)
            {
                bitmapGray = null;
                bitmapGray = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gray 1m " + stopwatch.Elapsed);

            stopwatch.Reset();

            stopwatch.Start();
            for (int l = 0; l < 600; l++)
            {
                bitmapGray = null;
                bitmapGray = grayscale.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gggg 1m " + stopwatch.Elapsed);

            stopwatch.Start();
            for (int l = 0; l < 3000; l++)
            {
                bitmapGray = null;
                bitmapGray = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gray 5m " + stopwatch.Elapsed);

            stopwatch.Reset();

            stopwatch.Start();
            for (int l = 0; l < 3000; l++)
            {
                bitmapGray = null;
                bitmapGray = grayscale.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gggg 5m " + stopwatch.Elapsed);

            stopwatch.Start();
            for (int l = 0; l < 6000; l++)
            {
                bitmapGray = null;
                bitmapGray = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gray 10m " + stopwatch.Elapsed);

            stopwatch.Reset();

            stopwatch.Start();
            for (int l = 0; l < 6000; l++)
            {
                bitmapGray = null;
                bitmapGray = grayscale.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gggg 10m " + stopwatch.Elapsed);

            stopwatch.Start();
            for (int l = 0; l < 9000; l++)
            {
                bitmapGray = null;
                bitmapGray = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gray 15m " + stopwatch.Elapsed);

            stopwatch.Reset();

            stopwatch.Start();
            for (int l = 0; l < 9000; l++)
            {
                bitmapGray = null;
                bitmapGray = grayscale.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gggg 15m " + stopwatch.Elapsed);

            stopwatch.Start();
            for (int l = 0; l < 18000; l++)
            {
                bitmapGray = null;
                bitmapGray = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gray 30m " + stopwatch.Elapsed);

            stopwatch.Reset();

            stopwatch.Start();
            for (int l = 0; l < 18000; l++)
            {
                bitmapGray = null;
                bitmapGray = grayscale.Apply(bitmap);
                bitmapGray = null;
            }
            stopwatch.Stop();
            Console.WriteLine("gggg 30m " + stopwatch.Elapsed);

            stopwatch.Reset();

            Console.ReadLine();
        }
    }
}
