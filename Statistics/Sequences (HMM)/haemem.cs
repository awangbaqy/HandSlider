using System;
using System.Linq;

namespace Sequences.HMMs
{
    public class haemem
    {
        private bool random;
        private readonly double[] classPriors;
        private double[] pi;
        private double[] initialProbability;
        private double[][] transitionProbability;
        private double[][] models;
        private double[][] emissionProbability;
        private int NumberOfClasses;
        private int NumberOfOutputs;
        private int states, classes;
        private int symbols;
        private int NumberOfInputs;
        private int Algorithm;
        public double[][] initialModel;
        public double[][][] transitionModel, emissionModel;

        public void haememce(int classes, int[] states, int symbols, string[] names) //: base(classes)
        {
            this.classes = classes;

            //double[][] Tmodel = new double[classes][];
            //baseClasses(Tmodel);

            //if (states.Length != classes)
            //    throw new ArgumentException("The number of state specifications should equal the number of classes.", "classes");

            //initialModel = new double[classes][];
            initialModel = new double[classes][];
            transitionModel = new double[classes][][];
            emissionModel = new double[classes][][];

            for (int i = 0; i < classes; i++)
            {
                //Models[i] = new HiddenMarkovModel(new Ergodic(states[i]), symbols) { Tag = names[i] };
                createHMM(getPi(states[i]), symbols);

                for (int j = 0; j < initialProbability.Length; j++)
                {
                    initialModel[i] = initialProbability;
                }

                for (int j = 0; j < transitionProbability.Length; j++)
                {
                    for (int k = 0; k < transitionProbability[j].Length; k++)
                    {
                        transitionModel[i] = transitionProbability;
                    }
                }

                for (int j = 0; j < emissionProbability.Length; j++)
                {
                    for (int k = 0; k < emissionProbability[j].Length; k++)
                    {
                        emissionModel[i] = emissionProbability;
                    }
                }
            }
        }

        //public void baseClasses(double[][] c)
        //{
        //    int classes = c.Length;
        //    models = c;
        //    this.NumberOfOutputs = classes;
        //    this.NumberOfClasses = classes;
        //    classPriors = new double[classes];
        //    for (int i = 0; i < classPriors.Length; i++)
        //        classPriors[i] = 1.0 / classPriors.Length;

        //}

        public double[] getPi(int s)
        {
            //this method same in ergodic or forward

            //if (s <= 0)
            //{
            //    throw new ArgumentOutOfRangeException(
            //        "states", "Number of states should be higher than zero.");
            //}

            //this.states = states;
            states = s;
            //this.random = random;
            random = false;
            pi = new double[states];

            //setPi(states, pi);


            //for (int i = 0; i < pi.Length; i++)
            //    pi[i] = 1.0 / states;
            pi[0] = 1.0;

            ///
            return pi;
        }

        //public double[] setPi(int s, double[] p)
        //{
        //    var v = new double[s];
        //    if (p != null)
        //    {
        //        for (int i = 0; i < Math.Min(p.Length, s); i++)
        //            v[i] = p[i];
        //    }
        //    return v;
        //}

        public void createHMM(double[] topology, int symbols)
        {
            baseTopology(topology);

            double[][] B;

            //if (symbols <= 0)
            //    throw new ArgumentOutOfRangeException("symbols");

            this.symbols = symbols;

            double[][] b;

            random = false;
            if (random)
            {
                b = randomMethod(states, symbols, 0.0, 1.0); //b = Jagged.Random(states, symbols);
            }
            else
            {
                b = ones(states, symbols); //b = Jagged.Ones(States, symbols);
            }


            //base.Emissions = GeneralDiscreteDistribution.FromMatrix(b.Log(), true);
            B = fromMatrix(logB(b), true);
            emissionProbability = B;
            //return B;
        }

        public void baseTopology(double[] topology)
        {
            consHMM();


            //this.states = topology.Create(true, out a, out logPi);

            //this.states = forwardTopology(true, out a, out initialProbability); // topologi forward
            states = ergodicTopology(true, out double[,] a, out initialProbability); // topologi ergodic
            // that differences is bullshit

            //this.logA = a.ToJagged();
            transitionProbability = toJagged(a);
            NumberOfOutputs = states;
        }

        public void consHMM()
        {
            NumberOfInputs = 1;
            //this.Algorithm = HiddenMarkovModelAlgorithm.Forward;
            Algorithm = 0;
        }

        public int ergodicTopology(bool logarithm, out double[,] transitionMatrix, out double[] initialState)
        {
            double[,] A = new double[states, states];

            if (random)
            {
                // Create pi
                double sum = 0;
                for (int i = 0; i < states; i++)
                {
                    sum += pi[i] = Accord.Math.Random.Generator.Random.NextDouble();
                }

                for (int i = 0; i < states; i++)
                {
                    pi[i] /= sum;
                }

                // Create A using random uniform distribution
                for (int i = 0; i < states; i++)
                {
                    sum = 0.0;
                    for (int j = 0; j < states; j++)
                    {
                        sum += A[i, j] = Accord.Math.Random.Generator.Random.NextDouble();
                    }

                    for (int j = 0; j < states; j++)
                    {
                        A[i, j] /= sum;
                    }
                }
            }
            else
            {
                // Create A using equal uniform probabilities,

                for (int i = 0; i < states; i++)
                {
                    for (int j = 0; j < states; j++)
                    {
                        A[i, j] = 1.0 / states;
                    }
                }
            }

            if (logarithm)
            {
                //transitionMatrix = A.Log();
                transitionMatrix = logA(A);
                //initialState = pi.Log();
                initialState = logPi(pi);
            }
            else
            {
                transitionMatrix = A;
                initialState = (double[])pi.Clone();
            }

            return states;
        }

        public int forwardTopology(bool logarithm, out double[,] transitionMatrix, out double[] initialState)
        {
            //int m = System.Math.Min(states, Deepness);
            int m = System.Math.Min(states, states);
            double[,] A = new double[states, states];

            if (random)
            {
                Random rand = Accord.Math.Random.Generator.Random;

                // Create pi
                double sum = 0;
                for (int i = 0; i < states; i++)
                {
                    sum += pi[i] = rand.NextDouble();
                }

                for (int i = 0; i < states; i++)
                {
                    pi[i] /= sum;
                }

                // Create A using random uniform distribution,
                //  without allowing backward transitions
                for (int i = 0; i < states; i++)
                {
                    sum = 0.0;
                    for (int j = i; j < m; j++)
                    {
                        sum += A[i, j] = rand.NextDouble();
                    }

                    for (int j = i; j < m; j++)
                    {
                        A[i, j] /= sum;
                    }
                }
            }
            else
            {
                // Create A using equal uniform probabilities,
                //   without allowing backward transitions.

                for (int i = 0; i < states; i++)
                {
                    double d = 1.0 / Math.Min(m, states - i);
                    for (int j = i; j < states && (j - i) < m; j++)
                    {
                        A[i, j] = d;
                    }
                }
            }

            if (logarithm)
            {
                //transitionMatrix = Elementwise.Log(A);
                transitionMatrix = logA(A);
                //initialState = Elementwise.Log(pi);
                initialState = logPi(pi);
            }
            else
            {
                transitionMatrix = A;
                initialState = (double[])pi.Clone();
            }

            return states;
        }

        public double[,] logA(double[,] value)
        {
            double[,] result = new double[value.GetLength(0), value.GetLength(1)];

            unsafe
            {
                fixed (double* ptrV = value)
                fixed (double* ptrR = result)
                {
                    double* pv = ptrV;
                    double* pr = ptrR;
                    for (int j = 0; j < result.Length; j++, pv++, pr++)
                    {
                        double v = *pv;
                        *pr = Math.Log(v);
                    }
                }
            }

            return result;
        }

        public double[] logPi(double[] value)
        {
            double[] result = new double[value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                double v = value[i];
                result[i] = Math.Log(v);
            }

            return result;
        }

        //public static T[][] ToJagged<T>(this T[,] matrix, bool transpose = false)
        public double[][] toJagged(double[,] matrix, bool transpose = false)
        {
            //T[][] array;
            double[][] array;

            if (transpose)
            {
                int cols = matrix.GetLength(1);

                //array = new T[cols][];
                array = new double[cols][];
                for (int i = 0; i < cols; i++)
                {
                    array[i] = getColumn(matrix, i); //array[i] = matrix.GetColumn(i);
                }
            }
            else
            {
                int rows = matrix.GetLength(0);

                //array = new T[rows][];
                array = new double[rows][];
                for (int i = 0; i < rows; i++)
                {
                    array[i] = getRow(matrix, i); //array[i] = matrix.GetRow(i);
                }
            }

            return array;
        }

        public double[] getColumn(double[,] m, int index, double[] result = null)
        {
            if (result == null)
            {
                result = new double[m.GetLength(0)];
            }

            index = indexMatrix(index, m.GetLength(1));
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = m[i, index];
            }

            return result;
        }

        public double[] getRow(double[,] m, int index, double[] result = null)
        {
            if (result == null)
            {
                result = new double[m.GetLength(1)];
            }

            index = indexMatrix(index, m.GetLength(0));
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = m[index, i];
            }

            return result;
        }

        public int indexMatrix(int end, int length)
        {
            if (end < 0)
            {
                end = length + end;
            }

            return end;
        }

        public double[][] randomMethod(int rows, int columns, double min, double max, double[][] result = null)
        {
            if (result == null)
            {
                result = jaggedCreate(rows, columns);  //result = Jagged.Create<double>(rows, columns);
            }

            Random random = Accord.Math.Random.Generator.Random;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i][j] = random.NextDouble() * (max - min) + min;
                }
            }

            return result;
        }

        public double[][] jaggedCreate(int rows, int columns, params double[] values)
        {
            if (values.Length == 0)
            {
                return zeros(rows, columns); //return Zeros<T>(rows, columns);
            }

            return toJagged(reshape(values, rows, columns, new double[rows, columns])); //return values.Reshape(rows, columns).ToJagged();
        }

        public double[][] zeros(int rows, int columns)
        {
            double[][] matrix = new double[rows][];
            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = new double[columns];
            }

            return matrix;
        }

        //public static T[,] Reshape<T>(this T[] array, int rows, int cols, T[,] result, MatrixOrder order = MatrixOrder.Default)
        public double[,] reshape(double[] array, int rows, int cols, double[,] result, int order = 1)
        {
            //if (order == MatrixOrder.CRowMajor)
            if (order == 1)
            {
                int k = 0;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        result[i, j] = array[k++];
                    }
                }
            }
            else
            {
                int k = 0;
                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        result[i, j] = array[k++];
                    }
                }
            }

            return result;
        }

        public double[][] ones(int rows, int columns)
        {
            int value = 1;

            double[][] matrix = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                double[] row = matrix[i] = new double[columns];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = value;
                }
            }

            return matrix;
        }

        public double[][] logB(double[][] value)
        {
            double[][] r = new double[value.Length][];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = new double[value[i].Length];
            }

            unsafe
            {
                for (int i = 0; i < value.Length; i++)
                {
                    for (int j = 0; j < value[i].Length; j++)
                    {
                        double v = value[i][j];
                        r[i][j] = Math.Log(v);
                    }
                }
            }
            return r;
        }

        public double[][] fromMatrix(double[][] probabilities, bool logarithm = false)
        {
            //var B = new GeneralDiscreteDistribution[probabilities.Rows()];
            double[][] B = new double[probabilities.Length][];
            for (int i = 0; i < B.Length; i++)
            {
                B[i] = generalDiscreteDistribution(logarithm, probabilities[i]); //B[i] = new GeneralDiscreteDistribution(logarithm, probabilities[i]);
            }

            return B;
        }

        public double[] generalDiscreteDistribution(bool logarithm, params double[] probabilities)
        {
            //if (probabilities == null)
            //    throw new ArgumentNullException("probabilities");

            //if (probabilities.Length < 2)
            //    Trace.TraceWarning("Creating a discrete distribution that is actually constant.");

            return initialize(0, probabilities, logarithm); //initialize(0, probabilities, logarithm);
        }

        public double[] initialize(int s, double[] prob, bool logarithm)
        {
            if (logarithm)
            {
                // assert that probabilities sum up to 1.
                double sum = logSumExp(prob); //double sum = prob.LogSumExp();
                if (sum != double.NegativeInfinity && sum != 0)
                {
                    subtractMethod(prob, sum, result: prob); //prob.Subtract(sum, result: prob);
                }
            }
            else
            {
                double sum = prob.Sum();
                if (sum != 0 && sum != 1)
                {
                    divideMethod(prob, sum, result: prob); //prob.Divide(sum, result: prob);
                }
            }

            //this.start = s;
            //this.probabilities = prob;

            //this.log = logarithm;
            //this.mean = null;
            //this.variance = null;
            //this.entropy = null;

            return prob;
        }

        public double logSumExp(double[] array)
        {
            double sum = double.NegativeInfinity;
            for (int i = 0; i < array.Length; i++)
            {
                sum = logSum(array[i], sum); //sum = Special.LogSum(array[i], sum);
            }

            return sum;
        }

        public double logSum(double lnx, double lny)
        {
            if (lnx == double.NegativeInfinity)
            {
                return lny;
            }

            if (lny == double.NegativeInfinity)
            {
                return lnx;
            }

            if (lnx > lny)
            {
                return lnx + log1p(Math.Exp(lny - lnx)); //return lnx + Special.Log1p(Math.Exp(lny - lnx));
            }

            return lny + log1p(Math.Exp(lnx - lny)); //return lny + Special.Log1p(Math.Exp(lnx - lny));
        }

        public double log1p(double x)
        {
            if (x <= -1.0)
            {
                return double.NaN;
            }

            if (Math.Abs(x) > 1e-4)
            {
                return Math.Log(1.0 + x);
            }

            // Use Taylor approx. log(1 + x) = x - x^2/2 with error roughly x^3/3
            // Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8
            return (-0.5 * x + 1.0) * x;
        }

        public double[] subtractMethod(double[] a, double b, double[] result)
        {
            //check<double, double, double>(a: a, b: b, result: result);

            //if (a.Length != result.Length)
            //    throw new DimensionMismatchException("result");

            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] - b;
            }

            return result;
        }

        public double[] divideMethod(double[] a, double b, double[] result)
        {
            //check<double, double, double>(a: a, b: b, result: result);

            //if (a.Length != result.Length)
            //    throw new DimensionMismatchException("result");

            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] / b;
            }

            return result;
        }

        /// Learn

        public void learning(int[][] seqs, int loop)
        {
            for (int i = 0; i < classes; i++)
            {
                Learn(seqs, loop, initialModel[i], To2D(transitionModel[i]), To2D(emissionModel[i]), i);

                Console.WriteLine();
                Console.WriteLine("Initial : ");

                for (int j = 0; j < initialModel[i].Length; j++)
                {
                    Console.WriteLine(initialModel[i][j].ToString("F99").TrimEnd('0'));
                }

                Console.WriteLine();
                Console.WriteLine("Emissions : ");

                for (int j = 0; j < emissionModel[i].Length; j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int k = 0; k < emissionModel[i][j].Length; k++)
                    {
                        Console.WriteLine(emissionModel[i][j][k].ToString("F99").TrimEnd('0'));
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Transitions : ");

                for (int j = 0; j < transitionModel[i].Length; j++)
                {
                    Console.WriteLine("Baris : " + j);

                    for (int k = 0; k < transitionModel[i][j].Length; k++)
                    {
                        Console.WriteLine(transitionModel[i][j][k].ToString("F99").TrimEnd('0'));
                    }
                }

                Console.WriteLine();
            }
        }

        /// summary
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// /summary
        /// remarks
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// /remarks
        /// param name="iterations"
        ///   The maximum number of iterations to be performed by the learning algorithm. If
        ///   specified as zero, the algorithm will learn until convergence of the model average
        ///   likelihood respecting the desired limit.
        /// /param
        /// param name="observations"
        ///   An array of observation sequences to be used to train the model.
        /// /param
        /// param name="tolerance"
        ///   The likelihood convergence limit L between two iterations of the algorithm. The
        ///   algorithm will stop when the change in the likelihood for two consecutive iterations
        ///   has not changed by more than L percent of the likelihood. If left as zero, the
        ///   algorithm will ignore this parameter and iterates over a number of fixed iterations
        ///   specified by the previous parameter.
        /// /param
        /// returns
        ///   The average log-likelihood for the observations after the model has been trained.
        /// /returns
        //public double Learn(int[][] observations, int iterations, double tolerance)
        public double Learn(int[][] observations, int iterations, double[] Probabilities, double[,] Transitions, double[,] Emissions, int index)
        {
            double tolerance = 0;
            if (iterations == 0 && tolerance == 0)
            {
                throw new ArgumentException("Iterations and limit cannot be both zero.");
            }

            // Baum-Welch algorithm.

            // The Baum–Welch algorithm is a particular case of a generalized expectation-maximization
            // (GEM) algorithm. It can compute maximum likelihood estimates and posterior mode estimates
            // for the parameters (transition and emission probabilities) of an HMM, when given only
            // emissions as training data.

            // The algorithm has two steps:
            //  - Calculating the forward probability and the backward probability for each HMM state;
            //  - On the basis of this, determining the frequency of the transition-emission pair values
            //    and dividing it by the probability of the entire string. This amounts to calculating
            //    the expected count of the particular transition-emission pair. Each time a particular
            //    transition is found, the value of the quotient of the transition divided by the probability
            //    of the entire string goes up, and this value can then be made the new value of the transition.
            
            int N = observations.Length;
            int currentIteration = 1;
            bool stop = false;

            double[] pi = Probabilities;
            double[,] A = Transitions;
            double[,] B = Emissions;

            // Initialization
            double[][,,] epsilon = new double[N][,,]; // also referred as ksi or psi
            double[][,] gamma = new double[N][,];

            for (int i = 0; i < N; i++)
            {
                int T = observations[i].Length;
                epsilon[i] = new double[T, states, states];
                gamma[i] = new double[T, states];
            }
            
            // Calculate initial model log-likelihood
            double oldLikelihood = double.MinValue;
            double newLikelihood = 0;
            
            do // Until convergence or max iterations is reached
            {
                // For each sequence in the observations input
                for (int i = 0; i < N; i++)
                {
                    int[] sequence = observations[i];
                    int T = sequence.Length;

                    // 1st step - Calculating the forward probability and the
                    //            backward probability for each HMM state.
                    //double[,] fwd = forward(observations[i], out double[] scaling);
                    double[,] fwd = forward(observations[i], out double[] scaling, pi, A, B);
                    //double[,] bwd = backward(observations[i], scaling);
                    double[,] bwd = backward(observations[i], scaling, pi, A, B);


                    // 2nd step - Determining the frequency of the transition-emission pair values
                    //            and dividing it by the probability of the entire string.


                    // Calculate gamma values for next computations
                    for (int t = 0; t < T; t++)
                    {
                        double s = 0;

                        for (int k = 0; k < states; k++)
                            s += gamma[i][t, k] = fwd[t, k] * bwd[t, k];

                        if (s != 0) // Scaling
                        {
                            for (int k = 0; k < states; k++)
                                gamma[i][t, k] /= s;
                        }
                    }

                    // Calculate epsilon values for next computations
                    for (int t = 0; t < T - 1; t++)
                    {
                        double s = 0;

                        for (int k = 0; k < states; k++)
                            for (int l = 0; l < states; l++)
                                s += epsilon[i][t, k, l] = fwd[t, k] * A[k, l] * bwd[t + 1, l] * B[l, sequence[t + 1]];

                        if (s != 0) // Scaling
                        {
                            for (int k = 0; k < states; k++)
                                for (int l = 0; l < states; l++)
                                    epsilon[i][t, k, l] /= s;
                        }
                    }

                    // Compute log-likelihood for the given sequence
                    for (int t = 0; t < scaling.Length; t++)
                        newLikelihood += Math.Log(scaling[t]);
                }
                
                // Average the likelihood for all sequences
                newLikelihood /= observations.Length;

                // Check if the model has converged or we should stop
                //if (checkConvergence(oldLikelihood, newLikelihood, currentIteration, iterations, tolerance))
                if (currentIteration == iterations)
                {
                    stop = true;
                }
                else
                {
                    // 3. Continue with parameter re-estimation
                    currentIteration++;
                    oldLikelihood = newLikelihood;
                    newLikelihood = 0.0;
                    
                    // 3.1 Re-estimation of initial state probabilities 
                    for (int k = 0; k < states; k++)
                    {
                        double sum = 0;
                        for (int i = 0; i < N; i++)
                            sum += gamma[i][0, k];
                        pi[k] = sum / N;
                    }

                    // 3.2 Re-estimation of transition probabilities 
                    for (int i = 0; i < states; i++)
                    {
                        for (int j = 0; j < states; j++)
                        {
                            double den = 0, num = 0;

                            for (int k = 0; k < N; k++)
                            {
                                int T = observations[k].Length;

                                for (int l = 0; l < T - 1; l++)
                                    num += epsilon[k][l, i, j];

                                for (int l = 0; l < T - 1; l++)
                                    den += gamma[k][l, i];
                            }

                            A[i, j] = (den != 0) ? num / den : 0.0;
                        }
                    }

                    // 3.3 Re-estimation of emission probabilities
                    for (int i = 0; i < states; i++)
                    {
                        for (int j = 0; j < symbols; j++)
                        {
                            double den = 0, num = 0;

                            for (int k = 0; k < N; k++)
                            {
                                int T = observations[k].Length;

                                for (int l = 0; l < T; l++)
                                {
                                    if (observations[k][l] == j)
                                    {
                                        num += gamma[k][l, i];
                                    }
                                }

                                for (int l = 0; l < T; l++)
                                    den += gamma[k][l, i];
                            }

                            // avoid locking a parameter in zero.
                            B[i, j] = (num == 0) ? 1e-10 : num / den;
                        }
                    }
                }

            } while (!stop);

            initialModel[index] = pi;
            transitionModel[index] = toJagged(A);
            emissionModel[index] = toJagged(B);

            // Returns the model average log-likelihood
            return newLikelihood;
        }

        /// summary
        ///   Baum-Welch forward pass (with scaling)
        /// /summary
        /// remarks
        ///   Reference: http://courses.media.mit.edu/2010fall/mas622j/ProblemSets/ps4/tutorial.pdf
        /// /remarks
        private double[,] forward(int[] observations, out double[] c, double[] Probabilities, double[,] Transitions, double[,] Emissions)
        {
            int T = observations.Length;
            double[] pi = Probabilities;
            double[,] A = Transitions;
            double[,] B = Emissions;

            double[,] fwd = new double[T, states];
            c = new double[T];
            
            // 1. Initialization
            for (int i = 0; i < states; i++)
                c[0] += fwd[0, i] = pi[i] * B[i, observations[0]];

            if (c[0] != 0) // Scaling
            {
                for (int i = 0; i < states; i++)
                    fwd[0, i] = fwd[0, i] / c[0];
            }
            
            // 2. Induction
            for (int t = 1; t < T; t++)
            {
                for (int i = 0; i < states; i++)
                {
                    double p = B[i, observations[t]];

                    double sum = 0.0;
                    for (int j = 0; j < states; j++)
                        sum += fwd[t - 1, j] * A[j, i];
                    fwd[t, i] = sum * p;

                    c[t] += fwd[t, i]; // scaling coefficient
                }

                if (c[t] != 0) // Scaling
                {
                    for (int i = 0; i < states; i++)
                        fwd[t, i] = fwd[t, i] / c[t];
                }
            }
 
            return fwd;
        }
        
        //private double[,] backward(int[] observations, out double[] c)
        private double[,] backward(int[] observations, double[] c, double[] Probabilities, double[,] Transitions, double[,] Emissions)
        {
            double[,] A = Transitions;
            //int states = function.States;
            int T = observations.Length;

            double[,] bwd = new double[T, states];

            // Ensures minimum requirements
            //Accord.Diagnostics.Debug.Assert(bwd.GetLength(0) >= T);
            //Accord.Diagnostics.Debug.Assert(bwd.GetLength(1) == states);
            //Array.Clear(bwd, 0, bwd.Length);

            // For backward variables, we use the same scale factors
            //   for each time t as were used for forward variables.

            // 1. Initialization
            for (int i = 0; i < states; i++)
                bwd[T - 1, i] = 1.0 / c[T - 1];

            // 2. Induction
            for (int t = T - 2; t >= 0; t--)
            {
                for (int i = 0; i < states; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < states; j++)
                        sum += bwd[t + 1, j] * A[j, i];
                    bwd[t, i] += sum / c[t];
                }
            }

            return bwd;
        }

        private double[,] To2D(double[][] source)
        {
            try
            {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

                var result = new double[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The given jagged array is not rectangular.");
            }
        }

        

        /// Evaluate

        /// summary
        ///   Calculates the probability that this model has generated the given sequence.
        /// /summary
        /// remarks
        ///   Evaluation problem. Given the HMM  M = (A, B, pi) and  the observation
        ///   sequence O = {o1, o2, ..., oK}, calculate the probability that model
        ///   M has generated sequence O. This can be computed efficiently using the
        ///   either the Viterbi or the Forward algorithms.
        /// /remarks
        /// param name="observations"
        ///   A sequence of observations.
        /// /param
        /// param name="logarithm"
        ///   True to return the log-likelihood, false to return
        ///   the likelihood. Default is false.
        /// /param
        /// returns
        ///   The probability that the given sequence has been generated by this model.
        /// /returns
        //public double Evaluate(int[] observations, bool logarithm)
        //{
        //    if (observations == null)
        //        throw new ArgumentNullException("observations");

        //    if (observations.Length == 0)
        //        return 0.0;
            
        //    // Forward algorithm
        //    double likelihood = 0;
        //    double[] coefficients;

        //    // Compute forward probabilities
        //    forward(observations, out coefficients);

        //    for (int i = 0; i < coefficients.Length; i++)
        //        likelihood += Math.Log(coefficients[i]);

        //    // Return the sequence probability
        //    return logarithm ? likelihood : Math.Exp(likelihood);
        //}
    }
}
