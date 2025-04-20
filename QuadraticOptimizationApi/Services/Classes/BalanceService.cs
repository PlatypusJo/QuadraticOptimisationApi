using Accord.Math;
using QuadraticOptimizationApi.Converters;
using QuadraticOptimizationApi.MathTools;
using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationApi.ResponseModels;
using QuadraticOptimizationApi.Services.Interfaces;
using QuadraticOptimizationSolver.Interfaces;
using QuadraticOptimizationSolver.Solvers;

namespace QuadraticOptimizationApi.Services.Classes
{
    public class BalanceService : IBalanceService
    {
        #region Fields

        private BalanceSolver _solver;

        private GlobalTestCalculator _globalTestCalculator;

        private const double accuracy = 0.1E-14;

        #endregion

        #region Constructors

        public BalanceService()
        {
            _solver = new BalanceSolver();
            _globalTestCalculator = new GlobalTestCalculator();
        }

        #endregion

        #region Methods

        public BalanceResponse Solve(BalanceRequest request)
        {
            var dataModel = BalanceDataModelConverter.Convert(request);
            var result = _solver.Solve(dataModel);

            var gtResult = _globalTestCalculator.Calculate(dataModel);

            var response = new BalanceResponse
            {
                BalancedFlows = request.Flows
                .Select((f, i) => new BalancedFlow { Name = f.Name, Value = result[i] })
                .ToList(),
                IsBalanced = CheckBalaced(dataModel.MatrixA, result),
                GlobalTestResult = gtResult
            };

            return response;
        }

        #endregion

        #region Private Methods

        private bool CheckBalaced(double[,] A, double[] balancedFlows)
        {
            bool isBalanced = false;
            double[] result = A.Dot(balancedFlows);

            foreach (double x in result)
            {
                isBalanced = x <= accuracy;

                if (isBalanced is false)
                    return false;
            }

            return isBalanced;
        }

        #endregion
    }
}
