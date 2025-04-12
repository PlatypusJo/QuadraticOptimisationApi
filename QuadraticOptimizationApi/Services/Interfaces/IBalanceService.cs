using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationApi.ResponseModels;

namespace QuadraticOptimizationApi.Services.Interfaces
{
    public interface IBalanceService
    {
        BalanceResponse Solve(BalanceRequest request);
    }
}
