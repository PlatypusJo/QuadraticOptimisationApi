namespace QuadraticOptimizationApi.MathTools
{
    public class BasicSchemeGT : BasicScheme
    {
        public BasicSchemeGT(int a, int b) : base(a, b)
        {
        }

        public BasicSchemeGT(BasicScheme a, double score) : base(a.AdjacencyMatrix, a.Flows, a.AbsoluteTolerance, a.Measurability)
        {
            GlobalTestRes = score;
        }

        public BasicSchemeGT(double[,] adjacencyMatrix, double[] flows, double[] absoluteTolerance, bool[] measurability, double score)
            : base(adjacencyMatrix, flows, absoluteTolerance, measurability)
        {
            GlobalTestRes = score;
        }

        public double GlobalTestRes { get; set; }
    }
}
