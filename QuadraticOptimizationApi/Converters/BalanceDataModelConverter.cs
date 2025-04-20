using QuadraticOptimizationApi.DTOs;
using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationSolver.DataModels;

namespace QuadraticOptimizationApi.Converters
{
    public static class BalanceDataModelConverter
    {
        public static BalanceDataModel Convert(BalanceRequest request)
        {
            // 1. Создаём матрицу баланса (A) на основе узлов (Nodes)
            var flowNames = request.Flows.Select(f => f.Name).ToList();
            var matrixA = BuildBalanceMatrix(request.Nodes, flowNames);

            // 2. Вектор допусков (Tolerance) и начальных значений (X0)
            var tolerance = request.Flows.Select(f => f.Tolerance).ToArray();
            var x0 = request.Flows.Select(f => f.MeasuredValue).ToArray();
            var ranges = request.Flows.Select(f => (f.MetrologicRange, f.TechnologicRange)).ToArray();
            var isMeasured = request.Flows.Select(f => f.IsMeasured).ToArray();

            // 3. Добавляем ограничения (Constraints) в матрицу A
            AddConstraints(ref matrixA, request.Constraints, flowNames);

            return new BalanceDataModel
            {
                MatrixA = matrixA,
                VectorY = new double[matrixA.GetLength(0)], // Все нули (A*x = 0)
                Tolerance = tolerance,
                VectorI = Enumerable.Repeat(1.0, flowNames.Count).ToArray(),
                VectorX0 = x0,
                FlowRanges = ranges,
                FlowMeasured = isMeasured
            };
        }

        public static double[,] BuildBalanceMatrix(List<NodeDto> nodes, List<string> flowNames)
        {
            int rows = nodes.Count;
            int cols = flowNames.Count;
            var matrixA = new double[rows, cols];

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                // Входящие потоки: +1
                foreach (var inputFlow in node.InputFlows)
                {
                    int flowIndex = flowNames.IndexOf(inputFlow);
                    if (flowIndex >= 0)
                        matrixA[i, flowIndex] = 1.0;
                }

                // Выходящие потоки: -1
                foreach (var outputFlow in node.OutputFlows)
                {
                    int flowIndex = flowNames.IndexOf(outputFlow);
                    if (flowIndex >= 0)
                        matrixA[i, flowIndex] = -1.0;
                }
            }

            return matrixA;
        }

        public static void AddConstraints(ref double[,] matrixA, List<ConstraintDto> constraints, List<string> flowNames)
        {
            if (constraints == null || constraints.Count == 0)
                return;

            int oldRows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            int newRows = oldRows + constraints.Count;

            // Создаём новую матрицу с добавленными строками
            var newMatrixA = new double[newRows, cols];

            // Копируем старые данные
            for (int i = 0; i < oldRows; i++)
                for (int j = 0; j < cols; j++)
                    newMatrixA[i, j] = matrixA[i, j];

            // Добавляем ограничения
            for (int k = 0; k < constraints.Count; k++)
            {
                var constraint = constraints[k];
                int row = oldRows + k;

                if (constraint.Type == "Ratio" && !string.IsNullOrEmpty(constraint.Flow1) && !string.IsNullOrEmpty(constraint.Flow2))
                {
                    int flow1Index = flowNames.IndexOf(constraint.Flow1);
                    int flow2Index = flowNames.IndexOf(constraint.Flow2);

                    if (flow1Index >= 0 && flow2Index >= 0)
                    {
                        newMatrixA[row, flow1Index] = 1.0;
                        newMatrixA[row, flow2Index] = -constraint.Ratio;
                    }
                }
            }

            matrixA = newMatrixA;
        }
    }
}
