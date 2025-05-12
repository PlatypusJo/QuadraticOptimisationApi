using MathNet.Numerics.LinearAlgebra;
using QuadraticOptimizationApi.ResponseModels;
using Accord.Statistics.Distributions.Univariate;
using QuadraticOptimizationSolver.DataModels;

namespace QuadraticOptimizationApi.MathTools
{
    public class GlobalTestCalculator
    {
        public GlobalTestResult Calculate(BalanceDataModel data)
        {
            var x0 = data.VectorX0;
            var aeq = data.MatrixA;
            var absTols = data.Tolerance;
            var isMeasured = data.FlowMeasured;
            return CalculateGlobalTest(x0, aeq, absTols, isMeasured);
        }

        private GlobalTestResult CalculateGlobalTest(
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
    }
}
