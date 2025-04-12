using Accord.Math;
using QuadraticOptimizationSolver.DataModels;
using QuadraticOptimizationSolver.Interfaces;
using QuadraticOptimizationSolver.Solvers;

namespace ApiTests
{
    public class BalanceSolverTests
    {
        #region Поля

        const double accuracy = 0.1E-14;

        #endregion

        #region Тесты

        [Fact]
        public void SolveOriginal_PassCorrectParameters_GetExpectedResult()
        {
            // Arrange
            IQuadraticOptimizationSolver<BalanceDataModel> solver = new BalanceSolver();
            BalanceDataModel dataEntity = GetDataModelOriginal();
            double[,] A = dataEntity.MatrixA;

            // Act
            double[] result = solver.Solve(dataEntity);
            double[] actual = A.Dot(result);

            // Assert
            Assert.All(actual, item => Assert.True(item <= accuracy));
        }

        [Fact]
        public void SolveV1_PassCorrectParameters_GetExpectedResult()
        {
            // Arrange
            IQuadraticOptimizationSolver<BalanceDataModel> solver = new BalanceSolver();
            BalanceDataModel dataEntity = GetDataModelV1();
            double[,] A = dataEntity.MatrixA;

            // Act
            double[] result = solver.Solve(dataEntity);
            double[] actual = A.Dot(result);

            // Assert
            Assert.All(actual, item => Assert.True(item <= accuracy));
        }

        [Fact]
        public void SolveV1WithConditions_PassCorrectParameters_FirstFlowLargerSecond10Times()
        {
            //Arrange:
            int expectedDifference = 10;
            IQuadraticOptimizationSolver<BalanceDataModel> solver = new BalanceSolver();
            BalanceDataModel dataEntity = GetDataModelV1WithConditions();

            // Act:
            double[] result = solver.Solve(dataEntity);
            int actualDifference = (int)(Math.Round(result[0] / result[1], MidpointRounding.AwayFromZero));

            // Assert:
            Assert.Equal(expectedDifference, actualDifference);
        }

        [Fact]
        public void SolveV1WithConditions_PassCorrectParameters_GetExpectedResult()
        {
            // Arrange
            IQuadraticOptimizationSolver<BalanceDataModel> solver = new BalanceSolver();
            BalanceDataModel dataEntity = GetDataModelV1WithConditions();
            double[,] A = dataEntity.MatrixA;

            // Act
            double[] result = solver.Solve(dataEntity);
            double[] actual = A.Dot(result);

            // Assert
            Assert.All(actual, item => Assert.True(item <= accuracy));
        }

    #endregion

        #region Внутренние методы

        private BalanceDataModel GetDataModelOriginal()
        {
            BalanceDataModel dataEntity = new BalanceDataModel()
            {
                MatrixA = new double[,] {
                    { 1, -1, -1, 0, 0, 0, 0 },
                    { 0, 0, 1, -1, -1, 0, 0 },
                    { 0, 0, 0, 0, 1, -1, -1 },
                },
                VectorY = new double[] { 0, 0, 0 },
                Tolerance = new double[] { 0.200, 0.121, 0.683, 0.040, 0.102, 0.081, 0.020 },
                VectorI = new double[] { 1, 1, 1, 1, 1, 1, 1 },
                VectorX0 = new double[] { 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991 }
            };

            return dataEntity;
        }

        private BalanceDataModel GetDataModelV1()
        {
            BalanceDataModel dataEntity = new BalanceDataModel()
            {
                MatrixA = new double[,] {
                    { 1, -1, -1, 0, 0, 0, 0, -1 },
                    { 0, 0, 1, -1, -1, 0, 0, 0 },
                    { 0, 0, 0, 0, 1, -1, -1, 0 },
                },
                VectorY = new double[] { 0, 0, 0 },
                Tolerance = new double[] { 0.200, 0.121, 0.683, 0.040, 0.102, 0.081, 0.020, 0.667 },
                VectorI = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 },
                VectorX0 = new double[] { 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991, 6.667 }
            };

            return dataEntity;
        }

        private BalanceDataModel GetDataModelV1WithConditions()
        {
            BalanceDataModel dataEntity = new BalanceDataModel()
            {
                MatrixA = new double[,] {
                    { 1, -1, -1, 0, 0, 0, 0, -1 },
                    { 0, 0, 1, -1, -1, 0, 0, 0 },
                    { 0, 0, 0, 0, 1, -1, -1, 0 },
                    { 1, -10, 0, 0, 0, 0, 0, 0 },
                },
                VectorY = new double[] { 0, 0, 0, 0 },
                Tolerance = new double[] { 0.200, 0.121, 0.683, 0.040, 0.102, 0.081, 0.020, 0.667 },
                VectorI = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 },
                VectorX0 = new double[] { 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991, 6.667 }
            };

            return dataEntity;
        }

        #endregion
    }
}