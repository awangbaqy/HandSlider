// Accord.NET Sample Applications
// http://accord-framework.net
//
// Copyright © 2009-2017, César Souza
// All rights reserved. 3-BSD License:
//
//   Redistribution and use in source and binary forms, with or without
//   modification, are permitted provided that the following conditions are met:
//
//      * Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//
//      * Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//
//      * Neither the name of the Accord.NET Framework authors nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
//  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using Accord.Controls;
using Accord.IO;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Components;
using System;
using System.Data;
using Accord.Math;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Accord.Statistics.Analysis;
using System.Media;
using Accord.Statistics.Models.Markov.Topology;

namespace Sequences.HMMs
{
    public partial class MainForm : Form
    {
        bool switching;
        HiddenMarkovClassifier hmmc;
        HaeMeM haeMeM;

        /// <summary>
        ///   Creates the ensemble
        /// </summary>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            buat(); return;

            switching = false;

            DataTable source = dgvSequenceSource.DataSource as DataTable;

            if (source == null)
            {
                MessageBox.Show("Please load some data by clicking 'Open' under the 'File' menu first. " +
                    "A sample dataset can be found in the folder 'Resources' contained in the same " +
                    "directory as this application.");
                return;
            }

            DataTable k = source.DefaultView.ToTable(true, "Label", "States");

            // Get the number of different classes in the data
            int classes = k.Rows.Count;

            string[] categories = new string[classes];
            int[] states = new int[classes];

            for (int i = 0; i < classes; i++)
            {
                // Gets the label name
                categories[i] = k.Rows[i]["Label"] as string;
                
                // Gets the number of states to recognize each label
                states[i] = int.Parse(k.Rows[i]["States"] as string);
            }

            // Creates a new hidden Markov classifier for the number of classes
            hmmc = new HiddenMarkovClassifier(classes, states, 2, categories); // 2 symbols
            
            //Console.WriteLine("Classes:" + classes);
            //Console.WriteLine("States:" + string.Join(",", states));
            //Console.WriteLine("Categories:" + string.Join(",", categories));

            //setMatrix();

            // Show the untrained model onscreen
            dgvModels.DataSource = hmmc.Models;
        }

        private void buat()
        {
            switching = true;

            DataTable source = dgvSequenceSource.DataSource as DataTable;

            if (source == null)
            {
                MessageBox.Show("Please load some data by clicking 'Open' under the 'File' menu first. " +
                    "A sample dataset can be found in the folder 'Resources' contained in the same " +
                    "directory as this application.");
                return;
            }

            DataTable k = source.DefaultView.ToTable(true, "Label", "States");

            // Get the number of different classes in the data
            int classes = k.Rows.Count;

            string[] categories = new string[classes];
            int[] states = new int[classes];

            for (int i = 0; i < classes; i++)
            {
                // Gets the label name
                categories[i] = k.Rows[i]["Label"] as string;
                
                // Gets the number of states to recognize each label
                states[i] = int.Parse(k.Rows[i]["States"] as string);
            }

            haeMeM = new HaeMeM(classes, states, 2); // 2 symbols

            // Show the untrained model onscreen

            for (int i = 0; i < categories.Length; i++)
            {
                dgvModels.Rows.Add(categories[i], states[i]);
            }

            //dgvModels.DataSource = haeMeM.mModels;

            //getHiMaMo();
        }

        /// <summary>
        ///   Trains the hidden Markov classifier
        /// </summary>
        /// 
        private void btnTrain_Click(object sender, EventArgs e)
        {
            latih(); return;

            DataTable source = dgvSequenceSource.DataSource as DataTable;
            if (source == null || hmmc == null)
            {
                MessageBox.Show("Please create a sequence classifier first.");
                return;
            }
            
            int rows = source.Rows.Count;

            // Gets the input sequences
            int[][] sequences = new int[rows][];
            int[] labels = new int[rows];

            // Foreach row in the datagridview
            for (int i = 0; i < rows; i++)
            {
                // Get the row at the index
                DataRow row = source.Rows[i];

                // Get the label associated with this sequence
                string label = row["Label"] as string;

                // Extract the sequence and the expected label for it
                sequences[i] = decode(row["Sequences"] as string);
                labels[i] = hmmc.Models.Find(x => x.Tag as string == label)[0];
            }
            
            // Grab training parameters
            int iterations = (int)numIterations.Value;
            double limit = (double)numConvergence.Value;

            if (rbStopIterations.Checked)
            {
                limit = 0;
            }
            else
            {
                iterations = 0;
            }

            // Create a new hidden Markov model learning algorithm
            var teacher = new HiddenMarkovClassifierLearning(hmmc, i =>
            {
                return new BaumWelchLearning(hmmc.Models[i])
                {
                    MaxIterations = iterations,
                    Tolerance = limit
                };
            });
            
            // Learn the classifier
            teacher.Learn(sequences, labels);
            
            // Update the GUI
            dgvModels_CurrentCellChanged(this, EventArgs.Empty);

            //getHMMC();
        }

        private void latih()
        {
            DataTable source = dgvSequenceSource.DataSource as DataTable;
            //if (source == null || hmmc == null)
            if (source == null)
            {
                MessageBox.Show("Please create a sequence classifier first.");
                return;
            }

            int rows = source.Rows.Count;

            // Gets the input sequences
            int[][] sequences = new int[rows][];
            int[] labels = new int[rows];

            // Foreach row in the datagridview
            for (int i = 0; i < rows; i++)
            {
                // Get the row at the index
                DataRow row = source.Rows[i];

                // Get the label associated with this sequence
                string label = row["Label"] as string;

                // Extract the sequence and the expected label for it
                sequences[i] = decode(row["Sequences"] as string);
                
                labels[i] = label == "F" ? 0 : label == "S" ? 1 : 2;
            }

            // Grab training parameters
            int iterations = (int)numIterations.Value;
            double limit = (double)numConvergence.Value;

            if (rbStopIterations.Checked)
            {
                limit = 0;
            }
            else
            {
                iterations = 0;
            }

            // Learn the classifier
            haeMeM.Learn(sequences, labels, iterations);

            // Update the GUI
            dgvModels_CurrentCellChanged(this, EventArgs.Empty);
            
            //getHiMaMo();
        }

        /// <summary>
        ///   Tests the ensemble
        /// </summary>
        private void btnTest_Click(object sender, EventArgs e)
        {
            uji(); return;
            //setHMMC();

            int wrong_total, right_total, f_total, ff, fs, fv, s_total, sf, ss, sv, v_total, vf, vs, vv;
            wrong_total = right_total = f_total = ff = fs = fv = s_total = sf = ss = sv = v_total = vf = vs = vv = 0;

            int rows = dgvTesting.Rows.Count - 1;
            
            int[] expected = new int[rows];
            int[] actual = new int[rows];
            
            // Gets the input sequences
            int[][] sequences = new int[rows][];

            // For each row in the testing data
            for (int i = 0; i < rows; i++)
            {
                // Get the row at the current index
                DataGridViewRow row = dgvTesting.Rows[i];

                // Get the training sequence to feed the model
                int[] sequence = decode(row.Cells["colTestSequence"].Value as string);

                // Get the label associated with this sequence
                string label = row.Cells["colTestTrueClass"].Value as string;
                expected[i] = hmmc.Models.Find(x => x.Tag as string == label)[0];

                // Compute the model output for this sequence and its likelihood.
                double likelihood = hmmc.LogLikelihood(sequence, out actual[i]);

                row.Cells["colTestAssignedClass"].Value = hmmc.Models[actual[i]].Tag as string;
                row.Cells["colTestLikelihood"].Value = likelihood;
                row.Cells["colTestMatch"].Value = actual[i] == expected[i];

                if (actual[i] == expected[i]) { right_total += 1; }
                if (actual[i] != expected[i]) { wrong_total += 1; }

                string expect = row.Cells["colTestAssignedClass"].Value.ToString();

                if (label.Equals("F"))
                {
                    f_total += 1;
                    if (expect.Equals("F")) { ff += 1; }
                    if (expect.Equals("S")) { fs += 1; }
                    if (expect.Equals("V")) { fv += 1; }
                }

                if (label.Equals("S"))
                {
                    s_total += 1;
                    if (expect.Equals("F")) { sf += 1; }
                    if (expect.Equals("S")) { ss += 1; }
                    if (expect.Equals("V")) { sv += 1; }
                }

                if (label.Equals("V"))
                {
                    v_total += 1;
                    if (expect.Equals("F")) {vf += 1; }
                    if (expect.Equals("S")) {vs += 1; }
                    if (expect.Equals("V")) {vv += 1; }
                }
            }

            // Use confusion matrix to compute some performance metrics
            dgvPerformance.DataSource = new[] { new GeneralConfusionMatrix(hmmc.NumberOfClasses, actual, expected) };

            MessageBox.Show(
                    "benar = " + right_total + " salah = " + wrong_total + "\n" +
                    "-===- \n" +
                    "F = " + f_total + " > F = " + ff + " / S = " + fs + " / V = " + fv + "\n" +
                    "S = " + s_total + " > F = " + sf + " / S = " + ss + " / V = " + sv + "\n" +
                    "V = " + v_total + " > F = " + vf + " / S = " + vs + " / V = " + vv
                );
        }

        private void uji()
        {
            //setHiMaMo();

            int wrong_total, right_total, f_total, ff, fs, fv, s_total, sf, ss, sv, v_total, vf, vs, vv;
            wrong_total = right_total = f_total = ff = fs = fv = s_total = sf = ss = sv = v_total = vf = vs = vv = 0;

            int rows = dgvTesting.Rows.Count - 1;

            int[] expected = new int[rows];
            int[] actual = new int[rows];

            // Gets the input sequences
            int[][] sequences = new int[rows][];

            // For each row in the testing data
            for (int i = 0; i < rows; i++)
            {
                // Get the row at the current index
                DataGridViewRow row = dgvTesting.Rows[i];

                // Get the training sequence to feed the model
                int[] sequence = decode(row.Cells["colTestSequence"].Value as string);

                // Get the label associated with this sequence
                int label = row.Cells["colTestTrueClass"].Value as string == "F" ? 0 : row.Cells["colTestTrueClass"].Value as string == "S" ? 1 : 2;
                
                expected[i] = label;

                // Compute the model output for this sequence and its likelihood.
                double[] likelihood;
                actual[i] = haeMeM.Compute(sequence, out likelihood);

                row.Cells["colTestAssignedClass"].Value = actual[i];
                Console.WriteLine(string.Join(" ", likelihood));
                row.Cells["colTestLikelihood"].Value = likelihood[actual[i]];
                row.Cells["colTestMatch"].Value = actual[i] == expected[i];

                if (actual[i] == expected[i]) { right_total += 1; }
                if (actual[i] != expected[i]) { wrong_total += 1; }

                string expect = row.Cells["colTestAssignedClass"].Value.ToString();

                if (label == 0)
                {
                    f_total += 1;
                    if (expect.Equals("0")) { ff += 1; }
                    if (expect.Equals("1")) { fs += 1; }
                    if (expect.Equals("2")) { fv += 1; }
                }

                if (label == 1)
                {
                    s_total += 1;
                    if (expect.Equals("0")) { sf += 1; }
                    if (expect.Equals("1")) { ss += 1; }
                    if (expect.Equals("2")) { sv += 1; }
                }

                if (label == 2)
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

        /// <summary>
        ///   Decodes a sequence in string form into is integer array form.
        ///   Example: Converts "1-2-1-3-5" into int[] {1,2,1,3,5}
        /// </summary>
        /// <returns></returns>
        private int[] decode(String sequence)
        {
            string[] elements = sequence.Split('-');
            int[] integers = new int[elements.Length];

            for (int j = 0; j < elements.Length; j++)
                integers[j] = int.Parse(elements[j]);
            
            return integers;
        }
        
        public MainForm()
        {
            InitializeComponent();

            dgvModels.AutoGenerateColumns = false;
            dgvTesting.AutoGenerateColumns = false;

            openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Resources");
        }
        
        private void MenuFileOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                string extension = Path.GetExtension(filename);
                if (extension == ".xls" || extension == ".xlsx")
                {
                    ExcelReader db = new ExcelReader(filename, true, false);
                    TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

                    if (t.ShowDialog(this) == DialogResult.OK)
                    {
                        DataTable tableSource = db.GetWorksheet(t.Selection);
                        this.dgvSequenceSource.DataSource = tableSource;
                        //loadTesting(tableSource);
                    }
                }
            }
        }

        private void inputTestingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                string extension = Path.GetExtension(filename);
                if (extension == ".xls" || extension == ".xlsx")
                {
                    ExcelReader db = new ExcelReader(filename, true, false);
                    TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

                    if (t.ShowDialog(this) == DialogResult.OK)
                    {
                        DataTable tableSource = db.GetWorksheet(t.Selection);
                        //this.dgvSequenceSource.DataSource = tableSource;
                        loadTesting(tableSource);
                    }
                }
            }
        }

        private void dgvModels_CurrentCellChanged(object sender, EventArgs e)
        {
            if (switching)
            {
                int classeses = 0;
                if (dgvModels.CurrentRow == null || dgvModels.CurrentRow.Cells[0].Value == null) { return; }
                if (dgvModels.CurrentRow.Cells[0].Value.ToString() == "F")
                {
                    classeses = 0;
                }
                if (dgvModels.CurrentRow.Cells[0].Value.ToString() == "S")
                {
                    classeses = 1;
                }
                if (dgvModels.CurrentRow.Cells[0].Value.ToString() == "V")
                {
                    classeses = 2;
                }

                //var model = dgvModels.CurrentRow.DataBoundItem as ModelHMM;
                dgvProbabilities.DataSource = new ArrayDataView(haeMeM.mModels[classeses].mLogProbabilityVector);
                dgvEmissions.DataSource = new ArrayDataView(toJaggedArray(haeMeM.mModels[classeses].mLogEmissionMatrix));
                dgvTransitions.DataSource = new ArrayDataView(toJaggedArray(haeMeM.mModels[classeses].mLogTransitionMatrix));

                return;
            }
            
            if (dgvModels.CurrentRow != null)
            {
                var model = dgvModels.CurrentRow.DataBoundItem as HiddenMarkovModel;
                dgvProbabilities.DataSource = new ArrayDataView(model.LogInitial);
                dgvEmissions.DataSource = new ArrayDataView(model.LogEmissions);
                dgvTransitions.DataSource = new ArrayDataView(model.LogTransitions);
            }
        }

        private void loadTesting(DataTable table)
        {
            int rows = table.Rows.Count;

            // Gets the input sequences
            int[][] sequences = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                dgvTesting.Rows.Add(
                    table.Rows[i]["Sequences"],
                    table.Rows[i]["Label"],
                    null,
                    0,
                    false);
            }
        }
        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void clearTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvSequenceSource.DataSource = null;
            dgvSequenceSource.Rows.Clear();
            dgvSequenceSource.Refresh();

            dgvTesting.DataSource = null;
            dgvTesting.Rows.Clear();
            dgvTesting.Refresh();
        }

        ///

        private double[][] toJaggedArray(double[,] obj2D)
        {
            // Take my 2D array and cast it as a 1D array
            double[] obj1D = ((double[,])obj2D).Cast<double>().ToArray();

            // using linq, chunk the 1D array back into a jagged array
            Int32 j = 0;
            double[][] jagged = obj1D.GroupBy(x => j++ / obj2D.GetLength(1)).Select(y => y.ToArray()).ToArray();

            return jagged;
        }

        private void getHmmcMatrix()
        {
            for (int i = 0; i < hmmc.Models.Length; i++)
            {
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

                    for (int ke = 0; ke < hmmc.Models[i].LogEmissions[j].Length; ke++)
                    {
                        Console.WriteLine(hmmc.Models[i].LogEmissions[j][ke]);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Transitions : ");

                for (int j = 0; j < hmmc.Models[i].LogTransitions.Length; j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int ke = 0; ke < hmmc.Models[i].LogTransitions[j].Length; ke++)
                    {
                        Console.WriteLine(hmmc.Models[i].LogTransitions[j][ke]);
                    }
                }

                Console.WriteLine();
            }
        }

        private void setMatrix()
        {
            // set forward topology
            // F
            hmmc.Models[0].LogInitial[0] = 0;
            hmmc.Models[0].LogInitial[1] = double.NegativeInfinity;

            hmmc.Models[0].LogEmissions[0][0] = -0.693147180559945;
            hmmc.Models[0].LogEmissions[0][1] = -0.693147180559945;

            hmmc.Models[0].LogEmissions[1][0] = -0.693147180559945;
            hmmc.Models[0].LogEmissions[1][1] = -0.693147180559945;

            hmmc.Models[0].LogTransitions[0][0] = -0.693147180559945;
            hmmc.Models[0].LogTransitions[0][1] = -0.693147180559945;

            hmmc.Models[0].LogTransitions[1][0] = -0.693147180559945;
            hmmc.Models[0].LogTransitions[1][1] = -0.693147180559945;

            // S
            hmmc.Models[1].LogInitial[0] = 0;
            hmmc.Models[1].LogInitial[1] = double.NegativeInfinity;

            hmmc.Models[1].LogEmissions[0][0] = -0.693147180559945;
            hmmc.Models[1].LogEmissions[0][1] = -0.693147180559945;

            hmmc.Models[1].LogEmissions[1][0] = -0.693147180559945;
            hmmc.Models[1].LogEmissions[1][1] = -0.693147180559945;

            hmmc.Models[1].LogTransitions[0][0] = -0.693147180559945;
            hmmc.Models[1].LogTransitions[0][1] = -0.693147180559945;

            hmmc.Models[1].LogTransitions[1][0] = -0.693147180559945;
            hmmc.Models[1].LogTransitions[1][1] = -0.693147180559945;

            // V
            hmmc.Models[2].LogInitial[0] = 0;
            hmmc.Models[2].LogInitial[1] = double.NegativeInfinity;

            hmmc.Models[2].LogEmissions[0][0] = -0.693147180559945;
            hmmc.Models[2].LogEmissions[0][1] = -0.693147180559945;

            hmmc.Models[2].LogEmissions[1][0] = -0.693147180559945;
            hmmc.Models[2].LogEmissions[1][1] = -0.693147180559945;

            hmmc.Models[2].LogTransitions[0][0] = -0.693147180559945;
            hmmc.Models[2].LogTransitions[0][1] = -0.693147180559945;

            hmmc.Models[2].LogTransitions[1][0] = -0.693147180559945;
            hmmc.Models[2].LogTransitions[1][1] = -0.693147180559945;
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

        private void setHMMC()
        {
            // set ergodic topology
            int classes = 3;
            string[] categories = new string[] { "F", "S", "V" };
            int[] states = new int[] { 2, 2, 2 };

            hmmc = new HiddenMarkovClassifier(classes, states, 3, categories);

            hmmc.NumberOfClasses = 3;
            hmmc.NumberOfInputs = 0;
            hmmc.NumberOfOutputs = 3;
            hmmc.Sensitivity = 1;

            for (int i = 0; i < 3; i++)
            {
                hmmc.Models[i].Algorithm = HiddenMarkovModelAlgorithm.Forward;
                hmmc.Models[i].NumberOfClasses = 2;
                hmmc.Models[i].NumberOfInputs = 1;
                hmmc.Models[i].NumberOfOutputs = 2;
            }

            // F
            hmmc.Models[0].LogInitial[0] = -0.00000000000000532907051820075;
            hmmc.Models[0].LogInitial[1] = double.NegativeInfinity;

            hmmc.Models[0].LogEmissions[0][0] = -0.693147180559945;
            hmmc.Models[0].LogEmissions[0][1] = -0.693147180559945;

            hmmc.Models[0].LogEmissions[1][0] = -0.693147180559945;
            hmmc.Models[0].LogEmissions[1][1] = -0.693147180559945;

            hmmc.Models[0].LogTransitions[0][0] = -0.149665500478042;
            hmmc.Models[0].LogTransitions[0][1] = -1.97325207249895;

            hmmc.Models[0].LogTransitions[1][0] = -1.92267496034109;
            hmmc.Models[0].LogTransitions[1][1] = -0.158076246151765;

            // S
            hmmc.Models[1].LogInitial[0] = -0.00000000000000532907051820075;
            hmmc.Models[1].LogInitial[1] = double.NegativeInfinity;

            hmmc.Models[1].LogEmissions[0][0] = -0.000111085092445635;
            hmmc.Models[1].LogEmissions[0][1] = -9.10526959376109;

            hmmc.Models[1].LogEmissions[1][0] = -5.72395648804492;
            hmmc.Models[1].LogEmissions[1][1] = -0.00327210791247331;

            hmmc.Models[1].LogTransitions[0][0] = -0.119162494908311;
            hmmc.Models[1].LogTransitions[0][1] = -2.18625687693831;

            hmmc.Models[1].LogTransitions[1][0] = -1.6546885847363;
            hmmc.Models[1].LogTransitions[1][1] = -0.212143738944055;

            // V
            hmmc.Models[2].LogInitial[0] = -0.00000000000000444089209850063;
            hmmc.Models[2].LogInitial[1] = double.NegativeInfinity;

            hmmc.Models[2].LogEmissions[0][0] = -0.00146935608066791;
            hmmc.Models[2].LogEmissions[0][1] = -6.52366560260292;

            hmmc.Models[2].LogEmissions[1][0] = -4.53630094234046;
            hmmc.Models[2].LogEmissions[1][1] = -0.0107707581359007;

            hmmc.Models[2].LogTransitions[0][0] = -0.107015025065289;
            hmmc.Models[2].LogTransitions[0][1] = -2.28781641539695;

            hmmc.Models[2].LogTransitions[1][0] = -1.6356633576454;
            hmmc.Models[2].LogTransitions[1][1] = -0.216693262802576;
        }

        ///

        private void getHiMaMo()
        {
            for (int i = 0; i < haeMeM.mModels.Length; i++)
            {
                Console.WriteLine();
                Console.WriteLine("Initial : ");

                for (int j = 0; j < haeMeM.mModels[i].mLogProbabilityVector.Length; j++)
                {
                    Console.WriteLine(haeMeM.mModels[i].mLogProbabilityVector[j].ToString("F99").TrimEnd('0'));
                }

                Console.WriteLine();
                Console.WriteLine("Emissions : ");

                for (int j = 0; j < haeMeM.mModels[i].mLogEmissionMatrix.GetLength(0); j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int ke = 0; ke < haeMeM.mModels[i].mLogEmissionMatrix.GetLength(1); ke++)
                    {
                        Console.WriteLine(haeMeM.mModels[i].mLogEmissionMatrix[j, ke]);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Transitions : ");

                for (int j = 0; j < haeMeM.mModels[i].mLogTransitionMatrix.GetLength(0); j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int ke = 0; ke < haeMeM.mModels[i].mLogTransitionMatrix.GetLength(1); ke++)
                    {
                        Console.WriteLine(haeMeM.mModels[i].mLogTransitionMatrix[j, ke]);
                    }
                }

                Console.WriteLine();
            }
        }

        private void setHiMaMo()
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
    }
}