using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationApi.ResponseModels;
using QuadraticOptimizationSolver.DataModels;

namespace QuadraticOptimizationApi.Services.Interfaces
{
    public interface IBalanceService
    {
        BalanceResponse Solve(BalanceRequest request);

        BalanceResponse Solve(BalanceDataModel model);
    }
}
