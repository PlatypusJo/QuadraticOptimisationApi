using MathNet.Numerics.LinearAlgebra;
using QuadraticOptimizationApi.ResponseModels;
using Accord.Statistics.Distributions.Univariate;
using QuadraticOptimizationSolver.DataModels;
using System.Collections.Concurrent;
using Accord.Math;
using System.Xml.Linq;

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
                int glrLen = glrMatrix.GetLength(0) - 1;

                var inds = nodes.GetIndices().ToArray();

                inds = inds.OrderByDescending(i => glrMatrix[i[0], i[1]]).ToArray();
                int currentMaxWidth = maxWidth;

                var res = Parallel.For(0, inds.Length + glrLen, (int i) =>
                {
                    if (i >= currentMaxWidth || i % 2 == 1)
                    {
                        return;
                    }

                    if (i < inds.Length && inds[i][0] == inds[i][1])
                    {
                        currentMaxWidth += 2;
                        return;
                    }

                    if (i >= inds.Length)
                    {
                        var newData = new BasicScheme(a, b + 1, data.Errors.Length + 1);
                        newData.CopyValues(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability, data.Errors);

                        newData.AdjacencyMatrix[i - inds.Length, b] = -1;
                        Error error = DetectError(data.AdjacencyMatrix, newData.AdjacencyMatrix);
                        newData.Errors[data.Errors.Length] = error;

                        FixModel(newData, result, maxDepth, maxWidth, currentDepth + 1);
                    }
                    else if (glrMatrix[inds[i][0], inds[i][1]] > 0)
                    {
                        var newData = new BasicScheme(a, b + 1, data.Errors.Length + 1);
                        newData.CopyValues(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability, data.Errors);

                        newData.AdjacencyMatrix[inds[i][0], b] = 1;
                        newData.AdjacencyMatrix[inds[i][1], b] = -1;

                        Error error = DetectError(data.AdjacencyMatrix, newData.AdjacencyMatrix);
                        newData.Errors[data.Errors.Length] = error;

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

        protected Error DetectError(double[,] original, double[,] modified)
        {
            Error error = new();
            if (IsLostFlowOrMeasError(original, modified, out error))
            {
                return error;
            }

            if (IsLeak(original, modified, out error))
            {
                return error;
            }

            error.Type = ErrorTypes.Unknown;
            return error;
        }

        protected bool IsLostFlowOrMeasError(double[,] original, double[,] modified, out Error error)
        {
            error = new();
            int modifNodes = modified.GetLength(0);
            int modifFlows = modified.GetLength(1) - 1;

            int node1 = -1;
            int node2 = -1;

            for (int i = 0; i < modifNodes && (node1 < 0 && node2 < 0); i++)
            {
                if (modified[i, modifFlows] == 1)
                    node1 = i;

                if (modified[i, modifFlows] == -1)
                    node2 = i;
            }

            if (node1 < 0 || node2 < 0)
                return false;

            int origLen = original.GetLength(1);
            bool flag = false;
            for (int i = 0; i < origLen; i++)
            {
                if ((original[node1, i] == 1 && original[node2, i] == -1) 
                    || (original[node2, i] == 1 && original[node1, i] == -1))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                error.Nodes = [$"N{node1 + 1}", $"N{node2 + 1}"];
                error.Type = ErrorTypes.MeasError;
                return true;
            }
            else
            {
                error.Nodes = [$"N{node1 + 1}", $"N{node2 + 1}"];
                error.Type = ErrorTypes.LostFlow;
                return true;
            }
        }

        protected bool IsLeak(double[,] original, double[,] modified, out Error error)
        {
            error = new();
            int modifNodes = modified.GetLength(0);
            int modifFlows = modified.GetLength(1) - 1;

            int node = -1;

            for (int i = 0; i < modifNodes; i++)
            {
                if (modified[i, modifFlows] == -1)
                { 
                    node = i;
                    break;
                }
            }

            if (node < 0)
                return false;

            int origLen = original.GetLength(1);
            int flow = -1;
            for (int i = 0; i < origLen; i++)
            {
                if (original[node, i] == -1)
                {
                    flow = i;
                    break;
                }
            }

            if (flow < 0)
                return false;

            int origNodes = original.GetLength(0);
            bool hasInput = false;
            for (int i = 0; i < origNodes; i++)
            {
                if (original[i, flow] == 1)
                {
                    hasInput = true;
                    break;
                }
            }

            if (!hasInput)
            {
                error.Nodes = [$"N{node + 1}"];
                error.Type = ErrorTypes.Leak;
                return !hasInput;
            }
            else
            {
                error.Nodes = [$"N{node + 1}"];
                return hasInput;
            }
        }
    }
}
