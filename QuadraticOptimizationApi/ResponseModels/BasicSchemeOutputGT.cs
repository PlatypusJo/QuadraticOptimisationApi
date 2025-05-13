using QuadraticOptimizationApi.MathTools;

namespace QuadraticOptimizationApi.ResponseModels
{
    public class BasicSchemeOutputGT
    {
        public BasicSchemeOutputGT(BasicSchemeGT data)
        {
            int a = data.AdjacencyMatrix.GetLength(0);
            int b = data.AdjacencyMatrix.GetLength(1);

            AdjacencyMatrix = new double[a][];

            for (int i = 0; i < a; i++)
            {
                AdjacencyMatrix[i] = new double[b];

                for (int j = 0; j < b; j++)
                {
                    AdjacencyMatrix[i][j] = data.AdjacencyMatrix[i, j];
                }
            }

            Flows = data.Flows;
            AbsoluteTolerance = data.AbsoluteTolerance;
            Measurability = data.Measurability;
            GlobalTestResult = data.GlobalTestRes;
        }

        public double[][] AdjacencyMatrix { get; set; }

        public double[] Flows { get; set; }

        public double[] AbsoluteTolerance { get; set; }

        public bool[] Measurability { get; set; }

        public double GlobalTestResult { get; set; }
    }
}
