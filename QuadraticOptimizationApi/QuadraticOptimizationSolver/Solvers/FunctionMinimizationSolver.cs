using Accord.Math.Optimization;
using MathNet.Symbolics;
using QuadraticOptimizationSolver.DataModels;
using QuadraticOptimizationSolver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadraticOptimizationSolver.Solvers
{
    public class FunctionMinimizationSolver : IQuadraticOptimizationSolver<FunctionMinimizationDataModel>
    {
        #region Fields

        private Dictionary<string, double> coeffs;

        #endregion

        #region Public Methods

        public double[] Solve(FunctionMinimizationDataModel data)
        {
            coeffs = GetCoeffs(data.Function);
            var f = GetFunction();
            var constraints = GetConstraints(data.FirstLimitX, data.FirstLimitY, data.FirstLimit, data.SecondLimitX, data.SecondLimitY, data.SecondLimit);
            var solver = new GoldfarbIdnani(f, constraints);
            solver.Maximize();

            return solver.Solution;
        }

        #endregion

        #region Private Methods

        private QuadraticObjectiveFunction GetFunction()
        {
            double[,] Q =
           {
                { GetCoeffByKey("x^2"),  GetCoeffByKey("x*y") },
                { GetCoeffByKey("x*y"), GetCoeffByKey("y^2") }
            };
            double[] d = { GetCoeffByKey("x"), GetCoeffByKey("y") };

            return new QuadraticObjectiveFunction(Q, d);
        }

        private List<LinearConstraint> GetConstraints(
            double firstLimitX, 
            double firstLimitY, 
            double firstLimit, 
            double secondLimitX, 
            double secondLimitY, 
            double secondLimit)
        {
            List<LinearConstraint> constraints = new List<LinearConstraint>()
            {
                new LinearConstraint(numberOfVariables: 2)
                {
                    VariablesAtIndices = new[] { 0, 1 },
                    CombinedAs = new double[] { firstLimitX, firstLimitY },
                    ShouldBe = ConstraintType.LesserThanOrEqualTo,
                    Value = firstLimit
                },

                new LinearConstraint(numberOfVariables: 2)
                {
                    VariablesAtIndices = new int[] { 0, 1 },
                    CombinedAs = new double[] { secondLimitX, secondLimitY },
                    ShouldBe = ConstraintType.LesserThanOrEqualTo,
                    Value = secondLimit
                }
            };

            return constraints;
        }

        private Dictionary<string, double> GetCoeffs(SymbolicExpression expr)
        {
            Dictionary<string, double> coefficients = new Dictionary<string, double>();
            for (int i = 0; i < expr.NumberOfOperands; i++)
            {
                string currentOperand = string.Empty;
                double currentCoeff = 0;
                switch (expr[i].NumberOfOperands)
                {
                    case 0:
                        currentOperand = expr[i].ToString();
                        currentCoeff = 1;
                        break;
                    case 2:
                        currentOperand = expr[i][1].ToString();
                        currentCoeff = expr[i][0].RealNumberValue;
                        break;
                    case 3:
                        currentOperand = (expr[i][1] * expr[i][2]).ToString();
                        currentCoeff = expr[i][0].RealNumberValue;
                        break;
                }
                coefficients.Add(currentOperand, currentCoeff);
            }
            return coefficients;
        }

        private double GetCoeffByKey(string key)
        {
            double coeff = coeffs.GetValueOrDefault(key);
            if (key.Contains('^'))
                coeff *= 2;
            return coeff;
        }

        #endregion
    }
}
