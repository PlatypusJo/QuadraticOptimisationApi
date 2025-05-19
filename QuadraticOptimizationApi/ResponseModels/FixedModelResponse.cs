using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationSolver.DataModels;

namespace QuadraticOptimizationApi.ResponseModels
{
    public class FixedModelResponse
    {
        public required BalanceRequest FixedModel { get; set; }
        public required BalanceResponse BalanceFixedModel { get; set; }
    }
}
