using MathNet.Numerics.LinearAlgebra;
using QuadraticOptimizationApi.ResponseModels;
using Accord.Statistics.Distributions.Univariate;
using QuadraticOptimizationSolver.DataModels;
using System.Collections.Concurrent;
using Accord.Math;

namespace QuadraticOptimizationApi.MathTools
{
    public class ModelValidator : IModelValidator
    {
        public GlobalTestResult ConductGlobalTest(BalanceDataModel data)
        {
            var x0 = data.VectorX0;
            var aeq = data.MatrixA;
            var absTols = data.Tolerance;
            var isMeasured = data.FlowMeasured;
            return ConductGlobalTest(x0, aeq, absTols, isMeasured);
        }

        private GlobalTestResult ConductGlobalTest(
        double[] x0,
        double[,] aeq,
        double[] absTols,
        bool[] isMeasured)
        {
            // 1. Расчет стандартных отклонений
            const double coefDelta = 1.96; // Квантиль для 95% доверия
            var xStd = new double[x0.Length];
            double maxX0 = x0.Max();

            for (int i = 0; i < x0.Length; i++)
            {
                xStd[i] = isMeasured[i]
                    ? absTols[i] / coefDelta
                    : 100 * maxX0; // Для неизмеряемых потоков
            }

            // 2. Создание ковариационной матрицы
            var xStdV = Vector<double>.Build.DenseOfArray([.. xStd.Select(s => s * s)]);
            var xSigma = Matrix<double>.Build.DiagonalOfDiagonalVector(xStdV);

            // 3. Расчет вектора нарушений баланса
            var aeqMatrix = Matrix<double>.Build.DenseOfArray(aeq);
            var x0Vector = Vector<double>.Build.Dense(x0);
            var r = aeqMatrix * x0Vector;

            // 4. Расчет ковариации нарушений
            var v = aeqMatrix * xSigma * aeqMatrix.Transpose();

            // 5. Расчет статистики GT
            var vPseudoInv = v.PseudoInverse();
            var gtOriginal = (r.ToRowMatrix() * vPseudoInv * r.ToColumnMatrix())[0, 0];

            // 6. Расчет критического значения
            const double alpha = 0.05;
            int degreesOfFreedom = aeq.GetLength(0);
            double gtLimit = ChiSquaredInv(1 - alpha, degreesOfFreedom);

            // 7. Нормированное значение GT
            double gtNormalized = (gtOriginal == 0 && gtLimit == 0)
                ? 0
                : gtOriginal / gtLimit;

            return new GlobalTestResult
            {
                OriginalValue = gtOriginal,
                LimitValue = gtLimit,
                NormalizedValue = gtNormalized,
                IsBalanceValid = gtNormalized <= 1.0
            };
        }

        // Обратная функция хи-квадрат распределения
        private static double ChiSquaredInv(double p, int degreesOfFreedom)
        {
            // Используем Accord.Statistics для точного расчета
            return new ChiSquareDistribution(degreesOfFreedom).InverseDistributionFunction(p);
        }

        public void FixModel(BasicScheme data, List<BasicSchemeGT> result, int maxDepth = 1000, int maxWidth = 1000, int currentDepth = 1)
        {
            var stack = new ConcurrentStack<BasicSchemeGT>(result);
            FixModel(data, stack, maxDepth, maxWidth, currentDepth);
            result.AddRange(stack);
        }

        public void FixModel(BasicScheme data, ConcurrentStack<BasicSchemeGT> result, int maxDepth = 1000, int maxWidth = 1000, int currentDepth = 1)
        {
            double originalScore = ConductGlobalTest(data.Flows, data.AdjacencyMatrix, data.AbsoluteTolerance, data.Measurability).NormalizedValue;

            if (originalScore <= 1)
            {
                result.Push(new BasicSchemeGT(data, originalScore));
                return;
            }

            if (currentDepth <= maxDepth)
            {
                var nodes = GetNodeAdjMatrix(data.AdjacencyMatrix);
                var glrMatrix = GetGLRScores(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability, nodes, originalScore);
                int a = data.AdjacencyMatrix.GetLength(0);
                int b = data.AdjacencyMatrix.GetLength(1);
                int grlLen = glrMatrix.GetLength(0);

                var inds = nodes.GetIndices().ToArray();

                inds = inds.OrderByDescending(i => glrMatrix[i[0], i[1]]).ToArray();
                int currentMaxWidth = maxWidth;

                var res = Parallel.For(0, inds.Length, (int i) =>
                {
                    if (i >= currentMaxWidth || i % 2 == 1)
                    {
                        return;
                    }

                    if (inds[i][0] == inds[i][1])
                    {
                        currentMaxWidth += 2;
                        return;
                    }

                    if (glrMatrix[inds[i][0], inds[i][1]] > 0)
                    {
                        var newData = new BasicScheme(a, b + 1);
                        newData.CopyValues(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability);
                        newData.AdjacencyMatrix[inds[i][0], b] = 1;
                        newData.AdjacencyMatrix[inds[i][1], b] = -1;

                        FixModel(newData, result, maxDepth, maxWidth, currentDepth + 1);
                    }
                });

                while (!res.IsCompleted)
                {
                    Thread.Sleep(1);
                }
            }

            return;
        }

        protected double[,] GetGLRScores(double[,] adjacencyMatrix, double[] flows, double[] absoluteTolerance, bool[] measurability, bool[,] nodes, double originalScore)
        {
            int a = adjacencyMatrix.GetLength(0);
            int b = adjacencyMatrix.GetLength(1);
            var glrM = new double[a + 1, a + 1];

            double[,] adjNew = new double[a, b + 1];
            double[] flowsNew = new double[b + 1];
            double[] absTolNew = new double[b + 1];
            bool[] measNew = new bool[b + 1];

            adjacencyMatrix.CopyTo(adjNew);
            flows.CopyTo(flowsNew);
            absoluteTolerance.CopyTo(absTolNew);
            measurability.CopyTo(measNew);

            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (nodes[i, j])
                    {
                        adjNew[i, b] = 1;
                        adjNew[j, b] = -1;

                        glrM[i, j] = originalScore - ConductGlobalTest(flowsNew, adjNew, absTolNew, measNew).NormalizedValue;
                        glrM[j, i] = glrM[i, j];

                        adjNew[i, b] = 0;
                        adjNew[j, b] = 0;
                    }
                }
            }

            for (int i = 0; i < a; i++)
            {
                adjNew[i, b] = -1;

                glrM[a, i] = originalScore - ConductGlobalTest(flowsNew, adjNew, absTolNew, measNew).NormalizedValue;

                adjNew[i, b] = 0;
            }

            return glrM;
        }

        protected bool[,] GetNodeAdjMatrix(double[,] adjacencyMatrix)
        {
            int a = adjacencyMatrix.GetLength(0);
            int b = adjacencyMatrix.GetLength(1);
            bool[,] res = new bool[a, a];


            for (int i = 0; i < a; i++)
            {
                res[i, i] = true;
                for (int j = 0; j < i; j++)
                {
                    for (int k = 0; k < b; k++)
                    {
                        if (adjacencyMatrix[i, k] == 1 && adjacencyMatrix[j, k] == -1 || adjacencyMatrix[i, k] == -1 && adjacencyMatrix[j, k] == 1)
                        {
                            res[i, j] = true;
                            res[j, i] = true;
                            break;
                        }
                    }
                }
            }

            return res;
        }
    }
}
