using QuadraticOptimizationApi.DTOs;
using QuadraticOptimizationApi.MathTools;
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

        public static BalanceRequest ConvertToBalanceRequest(BalanceDataModel balanceModel, List<string> flowNames)
        {
            if (balanceModel == null)
                throw new ArgumentNullException(nameof(balanceModel));
            if (flowNames == null || flowNames.Count != balanceModel.MatrixA.GetLength(1))
                throw new ArgumentException("Flow names count must match matrix columns", nameof(flowNames));

            var request = new BalanceRequest
            {
                Nodes = new List<NodeDto>(),
                Flows = new List<FlowDto>(),
                Constraints = new List<ConstraintDto>()
            };

            // 1. Восстанавливаем потоки (FlowDto)
            for (int i = 0; i < flowNames.Count; i++)
            {
                request.Flows.Add(new FlowDto
                {
                    Name = flowNames[i],
                    MeasuredValue = balanceModel.VectorX0 != null && i < balanceModel.VectorX0.Length
                        ? balanceModel.VectorX0[i]
                        : 0,
                    Tolerance = balanceModel.Tolerance != null && i < balanceModel.Tolerance.Length
                        ? balanceModel.Tolerance[i]
                        : 0,
                    IsMeasured = balanceModel.FlowMeasured != null && i < balanceModel.FlowMeasured.Length
                        ? balanceModel.FlowMeasured[i]
                        : false,
                    MetrologicRange = balanceModel.FlowRanges != null && i < balanceModel.FlowRanges.Length
                        ? balanceModel.FlowRanges[i].metrologicRange
                        : new RangeDto() { Min = 0, Max = 0 },
                    TechnologicRange = balanceModel.FlowRanges != null && i < balanceModel.FlowRanges.Length
                        ? balanceModel.FlowRanges[i].technologicRange
                        : new RangeDto() { Min = 0, Max = 0 },
                });
            }

            // 2. Восстанавливаем узлы (NodeDto) из матрицы баланса
            int nodeCount = balanceModel.MatrixA.GetLength(0);
            for (int i = 0; i < nodeCount; i++)
            {
                var node = new NodeDto
                {
                    Name = $"N{i + 1}",
                    InputFlows = new List<string>(),
                    OutputFlows = new List<string>()
                };

                for (int j = 0; j < flowNames.Count; j++)
                {
                    double value = balanceModel.MatrixA[i, j];
                    if (Math.Abs(value) > double.Epsilon)
                    {
                        if (value > 0)
                            node.InputFlows.Add(flowNames[j]);
                        else
                            node.OutputFlows.Add(flowNames[j]);
                    }
                }

                request.Nodes.Add(node);
            }

            return request;
        }

        public static BalanceDataModel ConvertFromBasicScheme(BalanceDataModel origData, BasicSchemeGT basicScheme, (int origFlow, int newFlow)[] flowInds)
        {
            // Создаем модель баланса
            var balanceDataModel = new BalanceDataModel
            {
                // Матрица A - это просто AdjacencyMatrix из BasicScheme
                MatrixA = (double[,])basicScheme.AdjacencyMatrix.Clone(),

                // Вектор Y - обычно нулевой вектор для уравнений баланса (A*X = Y)
                VectorY = new double[basicScheme.AdjacencyMatrix.GetLength(0)],

                // Допуски берем из AbsoluteTolerance
                Tolerance = (double[])basicScheme.AbsoluteTolerance.Clone(),

                // Вектор I - можно использовать Flows (измеренные значения потоков)
                VectorI = Enumerable.Repeat(1.0, basicScheme.Flows.Length).ToArray(),

                // Вектор X0 - начальные значения, тоже можно использовать Flows
                VectorX0 = (double[])basicScheme.Flows.Clone(),

                // Признаки наличия датчиков
                FlowMeasured = (bool[])basicScheme.Measurability.Clone(),

                // Интервальные ограничения - создаем пустые, так как в BasicScheme этой информации нет
                FlowRanges = new (RangeDto metrologicRange, RangeDto technologicRange)[basicScheme.Flows.Length]
            };

            for (int i = 0; i < balanceDataModel.FlowRanges.Length; i++)
            {
                balanceDataModel.FlowRanges[i] = (
                    new RangeDto() { Min = 0, Max = 0 },
                    new RangeDto() { Min = -10000, Max = 10000 }
                );
            }

            // Инициализируем интервальные ограничения
            for (int i = 0; i < origData.FlowRanges.Length; i++)
            {
                balanceDataModel.FlowRanges[i] = (
                    origData.FlowRanges[i].metrologicRange, // метрологические ограничения
                    origData.FlowRanges[i].technologicRange // технологические ограничения 
                );
            }

            // Вычисляем технологические границы для добавленных потоков.
            for (int i = 0; i < flowInds.Length; i++)
            {
                int origIndex = flowInds[i].origFlow;
                int newIndex = flowInds[i].newFlow;
                double min = origData.FlowRanges[origIndex].technologicRange.Min - origData.VectorX0[origIndex];
                double max = origData.FlowRanges[origIndex].technologicRange.Max - origData.VectorX0[origIndex];

                balanceDataModel.FlowRanges[newIndex] = (
                    new RangeDto() { Min = 0, Max = 0 },
                    new RangeDto() { Min = min, Max = max }
                );

                balanceDataModel.Tolerance[newIndex] = 1.0;
            }

            return balanceDataModel;
        }
    }
}
