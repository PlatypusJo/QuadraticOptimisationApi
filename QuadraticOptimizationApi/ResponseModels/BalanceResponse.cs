﻿namespace QuadraticOptimizationApi.ResponseModels
{
    public class BalanceResponse
    {
        public required List<BalancedFlow> BalancedFlows { get; set; }
        public bool IsBalanced { get; set; }
        public required GlobalTestResult GlobalTestResult { get; set; }
    }
}
