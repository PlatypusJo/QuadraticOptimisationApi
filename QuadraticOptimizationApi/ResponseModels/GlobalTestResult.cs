namespace QuadraticOptimizationApi.ResponseModels
{
    public class GlobalTestResult
    {
        public double OriginalValue { get; set; }
        public double LimitValue { get; set; }
        public double NormalizedValue { get; set; }
        public bool IsBalanceValid { get; set; }
    }
}
