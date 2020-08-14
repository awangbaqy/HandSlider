using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSlider
{
    class HaeMeM
    {
        int mClassCount;
        int mSymbolCount;
        public ModelHMM[] mModels;
        double[] mClassPriors;

        public HaeMeM(int class_count, int[] state_count_array, int symbol_count)
        {
            mClassCount = class_count;
            mSymbolCount = symbol_count;

            mModels = new ModelHMM[mClassCount];
            for (int i = 0; i < mClassCount; ++i)
            {
                ModelHMM hmm = new ModelHMM(state_count_array[i], symbol_count);
                mModels[i] = hmm;
            }

            mClassPriors = new double[mClassCount];
            for (int i = 0; i < mClassCount; ++i)
            {
                mClassPriors[i] = 1.0 / mClassCount;
            }
        }

        /// Learning

        public double Learn(int[][] observations_db, int[] class_labels, int iterations)
        {
            int class_count = mClassCount;
            double[] logLikelihood = new double[class_count];

            int K = class_labels.Length;

            int[] class_label_counts = new int[class_count];

            Parallel.For(0, class_count, i =>
            {
                List<int> match_record_index_set = new List<int>();
                for (int k = 0; k < K; ++k)
                {
                    if (class_labels[k] == i)
                    {
                        match_record_index_set.Add(k);
                    }
                }

                int K2 = match_record_index_set.Count;

                class_label_counts[i] = K2;

                if (K2 != 0)
                {
                    int[][] observations_subdb = new int[K2][];
                    for (int k = 0; k < K2; ++k)
                    {
                        int record_index = match_record_index_set[k];
                        observations_subdb[k] = observations_db[record_index];
                    }

                    logLikelihood[i] = BaumWelchRun(mModels[i], observations_subdb, iterations);
                }

            });

            return logLikelihood.Sum();
        }

        double BaumWelchRun(ModelHMM mModel, int[][] observations_db, int learnIteration)
        {
            int K = observations_db.Length;

            int N = mModel.mStateCount;

            double[,] logA = mModel.mLogTransitionMatrix;
            double[,] logB = mModel.mLogEmissionMatrix;
            double[] logPi = mModel.mLogProbabilityVector;

            int M = mModel.mSymbolCount;

            double[][,] mLogGamma = new double[K][,];
            double[][][,] mLogKsi = new double[K][][,];

            for (int k = 0; k < K; ++k)
            {
                int T = observations_db[k].Length;
                mLogGamma[k] = new double[T, N];
                mLogKsi[k] = new double[T][,];

                for (int t = 0; t < T; ++t)
                {
                    mLogKsi[k][t] = new double[N, N];
                }
            }

            int maxT = observations_db.Max(x => x.Length);
            double[,] lnfwd = new double[maxT, N]; // Variable forward
            double[,] lnbwd = new double[maxT, N]; // Variable backward

            // Initialize the model log-likelihoods
            double newLogLikelihood = Double.NegativeInfinity;
            double oldLogLikelihood = Double.NegativeInfinity;

            int iteration = 0;
            double deltaLogLikelihood = 0;
            bool should_continue = true;

            do // Until convergence or max iterations is reached
            {
                oldLogLikelihood = newLogLikelihood;

                for (int k = 0; k < K; ++k)
                {
                    int[] observations = observations_db[k];
                    double[,] logGamma = mLogGamma[k];
                    double[][,] logKsi = mLogKsi[k];

                    int T = observations.Length;

                    LogForward(logA, logB, logPi, observations, lnfwd);
                    LogBackward(logA, logB, logPi, observations, lnbwd);

                    // Compute Ksi values
                    for (int t = 0; t < T - 1; ++t)
                    {
                        double lnsum = double.NegativeInfinity;
                        int x = observations[t + 1];

                        for (int i = 0; i < N; ++i)
                        {
                            for (int j = 0; j < N; ++j)
                            {
                                logKsi[t][i, j] = lnfwd[t, i] + logA[i, j] + lnbwd[t + 1, j] + logB[j, x];
                                lnsum = LogSum(lnsum, logKsi[t][i, j]);
                            }
                        }

                        for (int i = 0; i < N; ++i)
                        {
                            for (int j = 0; j < N; ++j)
                            {
                                logKsi[t][i, j] = logKsi[t][i, j] - lnsum;
                            }
                        }
                    }

                    // Compute Gamma values
                    for (int t = 0; t < T; ++t)
                    {
                        double lnsum = double.NegativeInfinity;

                        for (int i = 0; i < N; ++i)
                        {
                            logGamma[t, i] = lnfwd[t, i] + lnbwd[t, i];
                            lnsum = LogSum(lnsum, logGamma[t, i]);
                        }

                        if (lnsum != Double.NegativeInfinity)
                        {
                            for (int i = 0; i < N; ++i)
                            {
                                logGamma[t, i] = logGamma[t, i] - lnsum;
                            }
                        }
                    }
                    
                    newLogLikelihood = Double.NegativeInfinity;

                    for (int i = 0; i < N; ++i)
                    {
                        newLogLikelihood = LogSum(newLogLikelihood, lnfwd[T - 1, i]);
                    }
                }

                newLogLikelihood /= K;

                deltaLogLikelihood = newLogLikelihood - oldLogLikelihood;

                iteration++;

                //Console.WriteLine("Iteration: {0}", iteration);

                if (learnIteration <= iteration)
                {
                    should_continue = false;
                }
                else
                {
                    // Update pi
                    for (int i = 0; i < N; ++i)
                    {
                        double lnsum = double.NegativeInfinity;

                        for (int k = 0; k < K; ++k)
                        {
                            lnsum = LogSum(lnsum, mLogGamma[k][0, i]);
                        }

                        logPi[i] = lnsum;
                    }

                    // Update A
                    for (int i = 0; i < N; ++i)
                    {
                        for (int j = 0; j < N; ++j)
                        {
                            double lndenom = double.NegativeInfinity;
                            double lnnum = double.NegativeInfinity;

                            for (int k = 0; k < K; ++k)
                            {
                                int T = observations_db[k].Length;

                                for (int t = 0; t < T - 1; ++t)
                                {
                                    lnnum = LogSum(lnnum, mLogKsi[k][t][i, j]);
                                    lndenom = LogSum(lndenom, mLogGamma[k][t, i]);
                                }
                            }

                            logA[i, j] = (lnnum == lndenom) ? 0 : lnnum - lndenom;
                        }
                    }

                    // Update B
                    for (int i = 0; i < N; ++i)
                    {
                        for (int m = 0; m < M; ++m)
                        {
                            double lndenom = double.NegativeInfinity;
                            double lnnum = double.NegativeInfinity;

                            for (int k = 0; k < K; ++k)
                            {
                                int[] observations = observations_db[k];
                                int T = observations.Length;

                                for (int t = 0; t < T; ++t)
                                {
                                    lndenom = LogSum(lndenom, mLogGamma[k][t, i]);

                                    if (observations[t] == m)
                                    {
                                        lnnum = LogSum(lnnum, mLogGamma[k][t, i]);
                                    }
                                }
                            }

                            logB[i, m] = lnnum - lndenom;
                        }
                    }
                }
            } while (should_continue);

            return newLogLikelihood;
        }

        void LogForward(double[,] logA, double[,] logB, double[] logPi, int[] observations, double[,] lnfwd)
        {
            int T = observations.Length; // length of the observation
            int N = logPi.Length; // number of states

            System.Array.Clear(lnfwd, 0, lnfwd.Length);

            // Initialization
            for (int i = 0; i < N; ++i)
            {
                lnfwd[0, i] = logPi[i] + logB[i, observations[0]];
            }

            // Recursion
            for (int t = 1; t < T; ++t)
            {
                int obs_t = observations[t];

                for (int i = 0; i < N; ++i)
                {
                    double sum = double.NegativeInfinity;
                    for (int j = 0; j < N; ++j)
                    {
                        sum = LogSum(sum, lnfwd[t - 1, j] + logA[j, i]);
                    }

                    // Termination
                    lnfwd[t, i] = sum + logB[i, obs_t];
                }
            }
        }

        void LogBackward(double[,] logA, double[,] logB, double[] logPi, int[] observations, double[,] lnbwd)
        {
            int T = observations.Length; //length of time series
            int N = logPi.Length; //number of states

            Array.Clear(lnbwd, 0, lnbwd.Length);

            // Initialization
            for (int i = 0; i < N; ++i)
            {
                lnbwd[T - 1, i] = 0;
            }

            // Recursion
            for (int t = T - 2; t >= 0; t--)
            {
                for (int i = 0; i < N; ++i)
                {
                    double sum = double.NegativeInfinity;
                    for (int j = 0; j < N; ++j)
                    {
                        sum = LogSum(sum, logA[i, j] + logB[j, observations[t + 1]] + lnbwd[t + 1, j]);
                    }

                    // Termination
                    lnbwd[t, i] += sum;
                }
            }
        }

        /// Likelihood

        public int Compute(int[] sequence, out double[] class_probabilities)
        {
            double[] logLikelihoods = new double[mModels.Length];
            double thresholdValue = Double.NegativeInfinity;

            Parallel.For(0, mModels.Length + 1, i =>
            {
                if (i < mModels.Length)
                {
                    LogForward(mModels[i].mLogTransitionMatrix, mModels[i].mLogEmissionMatrix, mModels[i].mLogProbabilityVector, sequence, out logLikelihoods[i]);
                }
            });

            double lnsum = Double.NegativeInfinity;
            for (int i = 0; i < mClassPriors.Length; i++)
            {
                logLikelihoods[i] = System.Math.Log(mClassPriors[i]) + logLikelihoods[i];
                lnsum = LogSum(lnsum, logLikelihoods[i]);
            }

            int most_likely_model_index = 0;
            double most_likely_model_probablity = double.NegativeInfinity;
            for (int i = 0; i < mClassCount; ++i)
            {
                if (most_likely_model_probablity < logLikelihoods[i])
                {
                    most_likely_model_probablity = logLikelihoods[i];
                    most_likely_model_index = i;
                }
            }

            if (lnsum != Double.NegativeInfinity)
            {
                for (int i = 0; i < logLikelihoods.Length; i++)
                    logLikelihoods[i] -= lnsum;
            }

            // Convert to probabilities
            class_probabilities = logLikelihoods;
            for (int i = 0; i < logLikelihoods.Length; i++)
            {
                class_probabilities[i] = 1 - System.Math.Exp(logLikelihoods[i]);
            }

            return (thresholdValue > most_likely_model_probablity) ? -1 : most_likely_model_index;
        }

        double[,] LogForward(double[,] logA, double[,] logB, double[] logPi, int[] observations, out double logLikelihood)
        {
            int T = observations.Length; // time series length
            int N = logPi.Length; // number of states

            double[,] lnfwd = new double[T, N];

            LogForward(logA, logB, logPi, observations, lnfwd);

            logLikelihood = double.NegativeInfinity;
            for (int i = 0; i < N; ++i)
            {
                logLikelihood = LogSum(logLikelihood, lnfwd[T - 1, i]);
            }

            return lnfwd;
        }

        ///

        double LogSum(double lna, double lnc)
        {
            if (lna == Double.NegativeInfinity)
                return lnc;
            if (lnc == Double.NegativeInfinity)
                return lna;

            if (lna > lnc)
                return lna + Log1p(System.Math.Exp(lnc - lna));

            return lnc + Log1p(System.Math.Exp(lna - lnc));
        }

        double Log1p(double x)
        {
            if (x <= -1.0)
                return Double.NaN;

            if (System.Math.Abs(x) > 1e-4)
                return System.Math.Log(1.0 + x);

            return (-0.5 * x + 1.0) * x;
        }
    }

    class ModelHMM
    {
        public double[,] mLogTransitionMatrix;
        public double[,] mLogEmissionMatrix;
        public double[] mLogProbabilityVector;
        public int mSymbolCount;
        public int mStateCount;

        public ModelHMM(int state_count, int symbol_count)
        {
            mStateCount = state_count;
            mSymbolCount = symbol_count;

            mLogTransitionMatrix = new double[mStateCount, mStateCount];
            mLogProbabilityVector = new double[mStateCount];
            mLogEmissionMatrix = new double[mStateCount, mSymbolCount];

            mLogProbabilityVector[0] = 1.0;

            for (int i = 0; i < mStateCount; ++i)
            {
                mLogProbabilityVector[i] = System.Math.Log(mLogProbabilityVector[i]);

                for (int j = 0; j < mStateCount; ++j)
                {
                    mLogTransitionMatrix[i, j] = System.Math.Log(1.0 / mStateCount);
                }
            }

            for (int i = 0; i < mStateCount; i++)
            {
                for (int j = 0; j < mSymbolCount; j++)
                    mLogEmissionMatrix[i, j] = System.Math.Log(1.0 / mSymbolCount);
            }
        }
    }
}
