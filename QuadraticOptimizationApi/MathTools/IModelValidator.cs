using QuadraticOptimizationApi.ResponseModels;
using QuadraticOptimizationSolver.DataModels;

namespace QuadraticOptimizationApi.MathTools
{
    public interface IModelValidator
    {
        GlobalTestResult ConductGlobalTest(BalanceDataModel data);

        void DetectErrors(BasicScheme data, List<BasicSchemeGT> result, int maxDepth = 1000, int maxWidth = 1000, int currentDepth = 1);
    }
}
