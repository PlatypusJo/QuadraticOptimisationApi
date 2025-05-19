using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationSolver.DataModels;

namespace QuadraticOptimizationApi.ResponseModels
{
    public class FixedModelResponse
    {
        public BalanceRequest FixedModel { get; set; }
        public BalanceResponse BalanceFixedModel { get; set; }

        public FixedModelResponse(BalanceRequest fixedModel, BalanceResponse balanceFixedModel)
        {
            FixedModel = fixedModel;
            BalanceFixedModel = balanceFixedModel;

            #region 

            for (int i = 0; i < FixedModel.Flows.Count; i++)
            {
                FixedModel.Flows[i].IsMeasured = true;
            }

            #endregion
        }
    }
}
