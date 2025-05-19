namespace QuadraticOptimizationApi.MathTools
{
    public class Error
    {
        public string[]? Nodes { get; set; }

        public int FlowIndex { get; set; }
        
        public int NewFlowIndex { get; set; }

        public string? Type { get; set; }
    }
}
