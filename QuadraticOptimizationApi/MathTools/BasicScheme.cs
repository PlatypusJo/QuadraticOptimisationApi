using Accord.Math;

namespace QuadraticOptimizationApi.MathTools
{
    public class BasicScheme
    {
        public BasicScheme(int a, int b)
        {
            AdjacencyMatrix = new double[a, b];
            Flows = new double[b];
            AbsoluteTolerance = new double[b];
            Measurability = new bool[b];
        }

        public BasicScheme(double[,] adjacencyMatrix, double[] flows, double[] absoluteTolerance, bool[] measurability)
        {
            AdjacencyMatrix = (double[,])adjacencyMatrix.Clone();
            Flows = (double[])flows.Clone();
            AbsoluteTolerance = (double[])absoluteTolerance.Clone();
            Measurability = (bool[])measurability.Clone();
        }

        public double[,] AdjacencyMatrix { get; set; }

        public double[] Flows { get; set; }

        public double[] AbsoluteTolerance { get; set; }

        public bool[] Measurability { get; set; }

        public void CopyValues(double[,] adjacencyMatrix, double[] flows, double[] absoluteTolerance, bool[] measurability)
        {
            adjacencyMatrix.CopyTo(AdjacencyMatrix);
            flows.CopyTo(Flows);
            absoluteTolerance.CopyTo(AbsoluteTolerance);
            measurability.CopyTo(Measurability);
        }
    }
}
