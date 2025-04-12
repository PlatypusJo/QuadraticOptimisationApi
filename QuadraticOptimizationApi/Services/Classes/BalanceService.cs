using QuadraticOptimizationApi.Converters;
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

        BalanceSolver _solver;

        #endregion

        #region Constructors

        public BalanceService()
        {
            _solver = new BalanceSolver();
        }

        #endregion

        #region Methods

        public BalanceResponse Solve(BalanceRequest request)
        {
            var dataModel = BalanceDataModelConverter.Convert(request);
            var result = _solver.Solve(dataModel);

            var response = new BalanceResponse
            {
                BalancedFlows = request.Flows
                .Select((f, i) => new BalancedFlow { Name = f.Name, Value = result[i] })
                .ToList(),
                IsBalanced = true
            };

            return response;
        }

        #endregion
        
    }
}
