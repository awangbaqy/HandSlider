namespace HandSlider
{
    partial class FormHandSlider
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormHandSlider));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.rbOriginal = new System.Windows.Forms.RadioButton();
            this.rbThreshold = new System.Windows.Forms.RadioButton();
            this.rbMorphology = new System.Windows.Forms.RadioButton();
            this.rbBlobOfHand = new System.Windows.Forms.RadioButton();
            this.rbCenterPoint = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(105, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(640, 480);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 75);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(12, 417);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 75);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // rbOriginal
            // 
            this.rbOriginal.AutoSize = true;
            this.rbOriginal.Checked = true;
            this.rbOriginal.Location = new System.Drawing.Point(12, 134);
            this.rbOriginal.Name = "rbOriginal";
            this.rbOriginal.Size = new System.Drawing.Size(60, 17);
            this.rbOriginal.TabIndex = 3;
            this.rbOriginal.TabStop = true;
            this.rbOriginal.Text = "Original";
            this.rbOriginal.UseVisualStyleBackColor = true;
            // 
            // rbThreshold
            // 
            this.rbThreshold.AutoSize = true;
            this.rbThreshold.Location = new System.Drawing.Point(12, 157);
            this.rbThreshold.Name = "rbThreshold";
            this.rbThreshold.Size = new System.Drawing.Size(72, 17);
            this.rbThreshold.TabIndex = 4;
            this.rbThreshold.Text = "Threshold";
            this.rbThreshold.UseVisualStyleBackColor = true;
            // 
            // rbMorphology
            // 
            this.rbMorphology.AutoSize = true;
            this.rbMorphology.Location = new System.Drawing.Point(12, 180);
            this.rbMorphology.Name = "rbMorphology";
            this.rbMorphology.Size = new System.Drawing.Size(80, 17);
            this.rbMorphology.TabIndex = 5;
            this.rbMorphology.Text = "Morphology";
            this.rbMorphology.UseVisualStyleBackColor = true;
            // 
            // rbBlobOfHand
            // 
            this.rbBlobOfHand.AutoSize = true;
            this.rbBlobOfHand.Location = new System.Drawing.Point(12, 203);
            this.rbBlobOfHand.Name = "rbBlobOfHand";
            this.rbBlobOfHand.Size = new System.Drawing.Size(87, 17);
            this.rbBlobOfHand.TabIndex = 6;
            this.rbBlobOfHand.Text = "Blob of Hand";
            this.rbBlobOfHand.UseVisualStyleBackColor = true;
            // 
            // rbCenterPoint
            // 
            this.rbCenterPoint.AutoSize = true;
            this.rbCenterPoint.Location = new System.Drawing.Point(12, 226);
            this.rbCenterPoint.Name = "rbCenterPoint";
            this.rbCenterPoint.Size = new System.Drawing.Size(83, 17);
            this.rbCenterPoint.TabIndex = 7;
            this.rbCenterPoint.Text = "Center Point";
            this.rbCenterPoint.UseVisualStyleBackColor = true;
            // 
            // FormHandSlider
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(755, 505);
            this.Controls.Add(this.rbCenterPoint);
            this.Controls.Add(this.rbBlobOfHand);
            this.Controls.Add(this.rbMorphology);
            this.Controls.Add(this.rbThreshold);
            this.Controls.Add(this.rbOriginal);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormHandSlider";
            this.Text = "Hand Slider";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.RadioButton rbOriginal;
        private System.Windows.Forms.RadioButton rbThreshold;
        private System.Windows.Forms.RadioButton rbMorphology;
        private System.Windows.Forms.RadioButton rbBlobOfHand;
        private System.Windows.Forms.RadioButton rbCenterPoint;
    }
}

