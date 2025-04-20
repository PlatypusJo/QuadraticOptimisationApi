using Accord.Math;
using Accord.Math.Optimization;
using MathNet.Numerics.LinearAlgebra;
using QuadraticOptimizationApi.DTOs;
using QuadraticOptimizationSolver.DataModels;
using QuadraticOptimizationSolver.Interfaces;
using System;

namespace QuadraticOptimizationSolver.Solvers
{
    public class BalanceSolver : IQuadraticOptimizationSolver<BalanceDataModel>
    {
        #region Public Methods

        public double[] Solve(BalanceDataModel data)
        {
            var i = GetMatrixI(data.VectorI);
            var w = GetMatrixW(data.Tolerance);
            var h = GetMatrixH(w, i);
            var d = GetVectorD(h, data.VectorX0);
            return FindMinimum(h, d, data.MatrixA, data.VectorY, data.FlowRanges, data.FlowMeasured);
        }

        #endregion

        #region Private Methods

        private double[,] GetMatrixW(double[] tolerance)
        {
            double[,] result = new double[tolerance.Length, tolerance.Length];
            for (int i = 0; i < tolerance.Length; i++)
            {
                for (int j = 0; j < tolerance.Length; j++)
                {
                    result[i, j] = i == j ? 1 / (tolerance[j] * tolerance[j]) : 0;
                }
            }
            return result;
        }

        private double[,] GetMatrixI(double[] vectorI)
        {
            double[,] result = new double[vectorI.Length, vectorI.Length];
            for (int i = 0; i < vectorI.Length; i++)
            {
                for (int j = 0; j < vectorI.Length; j++)
                {
                    result[i, j] = i == j ? vectorI[j] : 0;
                }
            }
            return result;
        }

        private double[,] GetMatrixH(double[,] matrixW, double[,] matrixI)
        {
            var w = Matrix<double>.Build.SparseOfArray(matrixW);
            var i = Matrix<double>.Build.SparseOfArray(matrixI);
            return w.Multiply(i).ToArray();
        }

        private double[] GetVectorD(double[,] matrixH, double[] vectorX0)
        {
            var h = Matrix<double>.Build.SparseOfArray(matrixH);
            var x0 = Vector<double>.Build.SparseOfArray(vectorX0);
            return h.Multiply(x0).Multiply(-1).ToArray();
        }

        private double[] FindMinimum(
            double[,] matrixH, 
            double[] vectorD, 
            double[,] matrixA, 
            double[] vectorY, 
            (RangeDto metrologicRange, RangeDto technologicRange)[] ranges, 
            bool[] isMeasured)
        {
            var constraints = new List<LinearConstraint>();
            int n = ranges.Length;

            AddBalancingConstraints(ref constraints, matrixA, vectorY, n);
            AddRangeConstraints(ref constraints, ranges, isMeasured);

            var solver = new GoldfarbIdnani(
                function: new QuadraticObjectiveFunction(matrixH, vectorD),
                constraints: constraints);

            if (!solver.Minimize())
                throw new Exception("Не удалось найти решение. Проверьте ограничения.");

            return solver.Solution;
        }

        private void AddBalancingConstraints(ref List<LinearConstraint> constraints, double[,] matrixA, double[] vectorY, int flowsNumber)
        {
            for (int i = 0; i < matrixA.GetLength(0); i++)
            {
                var coefficients = matrixA.GetRow(i);
                constraints.Add(new LinearConstraint(numberOfVariables: flowsNumber)
                {
                    CombinedAs = coefficients,
                    ShouldBe = ConstraintType.EqualTo,
                    Value = vectorY[i]
                });
            }
        }

        private void AddRangeConstraints(ref List<LinearConstraint> constraints, (RangeDto metrologicRange, RangeDto technologicRange)[] ranges, bool[] isMeasured)
        {
            int n = ranges.Length;
            for (int i = 0; i < ranges.Length; i++)
            {
                RangeDto range = isMeasured[i] ? ranges[i].metrologicRange : ranges[i].technologicRange;
                AddRangeConstraint(ref constraints, range, n, i);
            }
        }

        private void AddRangeConstraint(ref List<LinearConstraint> constraints, RangeDto range, int flowsNumber, int index)
        {
            var lowerCoeff = new double[flowsNumber];
            lowerCoeff[index] = 1;
            constraints.Add(new LinearConstraint(flowsNumber)
            {
                CombinedAs = lowerCoeff,
                ShouldBe = ConstraintType.GreaterThanOrEqualTo,
                Value = range.Min
            });

            var upperCoeff = new double[flowsNumber];
            upperCoeff[index] = 1;
            constraints.Add(new LinearConstraint(flowsNumber)
            {
                CombinedAs = upperCoeff,
                ShouldBe = ConstraintType.LesserThanOrEqualTo,
                Value = range.Max
            });
        }

        #endregion
    }
}
