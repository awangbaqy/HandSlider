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
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.rbOriginal = new System.Windows.Forms.RadioButton();
            this.rbThreshold = new System.Windows.Forms.RadioButton();
            this.rbMorphology = new System.Windows.Forms.RadioButton();
            this.rbBlobDetection = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelX1 = new System.Windows.Forms.Label();
            this.labelX2 = new System.Windows.Forms.Label();
            this.pictureBoxBlob = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbMoveDetection = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelCountdown = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabelFPS = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBlob)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(498, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 35);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Mulai";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(497, 360);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 35);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Berhenti";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // rbOriginal
            // 
            this.rbOriginal.AutoSize = true;
            this.rbOriginal.Checked = true;
            this.rbOriginal.Location = new System.Drawing.Point(128, 12);
            this.rbOriginal.Name = "rbOriginal";
            this.rbOriginal.Size = new System.Drawing.Size(41, 17);
            this.rbOriginal.TabIndex = 3;
            this.rbOriginal.TabStop = true;
            this.rbOriginal.Text = "Asli";
            this.rbOriginal.UseVisualStyleBackColor = true;
            // 
            // rbThreshold
            // 
            this.rbThreshold.AutoSize = true;
            this.rbThreshold.Location = new System.Drawing.Point(175, 12);
            this.rbThreshold.Name = "rbThreshold";
            this.rbThreshold.Size = new System.Drawing.Size(72, 17);
            this.rbThreshold.TabIndex = 4;
            this.rbThreshold.Text = "Threshold";
            this.rbThreshold.UseVisualStyleBackColor = true;
            // 
            // rbMorphology
            // 
            this.rbMorphology.AutoSize = true;
            this.rbMorphology.Location = new System.Drawing.Point(253, 12);
            this.rbMorphology.Name = "rbMorphology";
            this.rbMorphology.Size = new System.Drawing.Size(68, 17);
            this.rbMorphology.TabIndex = 5;
            this.rbMorphology.Text = "Morfologi";
            this.rbMorphology.UseVisualStyleBackColor = true;
            // 
            // rbBlobDetection
            // 
            this.rbBlobDetection.AutoSize = true;
            this.rbBlobDetection.Location = new System.Drawing.Point(327, 12);
            this.rbBlobDetection.Name = "rbBlobDetection";
            this.rbBlobDetection.Size = new System.Drawing.Size(85, 17);
            this.rbBlobDetection.TabIndex = 6;
            this.rbBlobDetection.Text = "Deteksi Blob";
            this.rbBlobDetection.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(12, 35);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(480, 360);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(457, 336);
            this.label1.TabIndex = 9;
            this.label1.Text = "Menyiapkan Dataset...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelX1
            // 
            this.labelX1.AutoSize = true;
            this.labelX1.Location = new System.Drawing.Point(577, 87);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(19, 13);
            this.labelX1.TabIndex = 10;
            this.labelX1.Text = ": 0";
            // 
            // labelX2
            // 
            this.labelX2.AutoSize = true;
            this.labelX2.Location = new System.Drawing.Point(577, 118);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(19, 13);
            this.labelX2.TabIndex = 11;
            this.labelX2.Text = ": 0";
            // 
            // pictureBoxBlob
            // 
            this.pictureBoxBlob.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxBlob.Location = new System.Drawing.Point(497, 248);
            this.pictureBoxBlob.Name = "pictureBoxBlob";
            this.pictureBoxBlob.Size = new System.Drawing.Size(100, 100);
            this.pictureBoxBlob.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxBlob.TabIndex = 12;
            this.pictureBoxBlob.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(498, 232);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Blob Tangan :";
            // 
            // cbMoveDetection
            // 
            this.cbMoveDetection.AutoSize = true;
            this.cbMoveDetection.Location = new System.Drawing.Point(498, 53);
            this.cbMoveDetection.Name = "cbMoveDetection";
            this.cbMoveDetection.Size = new System.Drawing.Size(106, 17);
            this.cbMoveDetection.TabIndex = 14;
            this.cbMoveDetection.Text = "Deteksi Gerakan";
            this.cbMoveDetection.UseVisualStyleBackColor = true;
            this.cbMoveDetection.CheckedChanged += new System.EventHandler(this.cbMoveDetection_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(498, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Titik Akhir (X2)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(498, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Titik Awal (X1)";
            // 
            // labelCountdown
            // 
            this.labelCountdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCountdown.Location = new System.Drawing.Point(498, 131);
            this.labelCountdown.Name = "labelCountdown";
            this.labelCountdown.Size = new System.Drawing.Size(100, 100);
            this.labelCountdown.TabIndex = 17;
            this.labelCountdown.Text = "0";
            this.labelCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelFPS,
            this.statusProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 399);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(609, 22);
            this.statusStrip1.TabIndex = 18;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabelFPS
            // 
            this.statusLabelFPS.Name = "statusLabelFPS";
            this.statusLabelFPS.Size = new System.Drawing.Size(35, 17);
            this.statusLabelFPS.Text = "0 FPS";
            // 
            // statusProgressBar1
            // 
            this.statusProgressBar1.Name = "statusProgressBar1";
            this.statusProgressBar1.Size = new System.Drawing.Size(555, 16);
            // 
            // FormHandSlider
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(609, 421);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.labelCountdown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbMoveDetection);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBoxBlob);
            this.Controls.Add(this.labelX2);
            this.Controls.Add(this.labelX1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.rbBlobDetection);
            this.Controls.Add(this.rbMorphology);
            this.Controls.Add(this.rbThreshold);
            this.Controls.Add(this.rbOriginal);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormHandSlider";
            this.Text = "Hand Slider";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormHandSlider_FormClosing);
            this.Load += new System.EventHandler(this.FormHandSlider_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBlob)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.RadioButton rbOriginal;
        private System.Windows.Forms.RadioButton rbThreshold;
        private System.Windows.Forms.RadioButton rbMorphology;
        private System.Windows.Forms.RadioButton rbBlobDetection;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelX1;
        private System.Windows.Forms.Label labelX2;
        private System.Windows.Forms.PictureBox pictureBoxBlob;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbMoveDetection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelCountdown;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelFPS;
        private System.Windows.Forms.ToolStripProgressBar statusProgressBar1;
    }
}

