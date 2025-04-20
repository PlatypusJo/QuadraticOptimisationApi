using QuadraticOptimizationApi.DTOs;
using QuadraticOptimizationApi.MathTools;
using QuadraticOptimizationSolver.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTests
{
    public class GlobalTestCalculatorTests
    {
        #region Поля

        private readonly GlobalTestCalculator _calculator = new GlobalTestCalculator();

        #endregion

        #region Тесты

        [Fact]
        public void Calculate_ValidInput_ReturnsCorrectResult()
        {
            // Arrange
            var dataModel = GetDataModelOriginal();

            // Act
            var result = _calculator.Calculate(dataModel);

            //// Assert
            Assert.NotNull(result);
            Assert.InRange(result.NormalizedValue, 0, double.MaxValue);
            Assert.True(result.LimitValue > 0);
            Assert.True(result.IsBalanceValid);
        }

        [Fact]
        public void Calculate_PassModelWithErrors_BalanceIsNotValid()
        {
            // Arrange
            var dataModel = GetDataModelOriginalWithErrors();

            // Act
            var result = _calculator.Calculate(dataModel);

            //// Assert
            Assert.False(result.IsBalanceValid);
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
                VectorX0 = new double[] { 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991 },
                FlowRanges = new (RangeDto metrologicRange, RangeDto technologicRange)[] {
                (metrologicRange: new(){Min = 9.805, Max = 10.205}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 2.912, Max = 3.154}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 6.148, Max = 7.514}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 1.945, Max = 2.025}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 4.991, Max = 5.195}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 3.976, Max = 4.138}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 0.971, Max = 1.011}, technologicRange: new(){Min = 0.0, Max = 1000.0}) },
                FlowMeasured = [true, true, true, true, true, true, true]
            };

            return dataEntity;
        }

        private BalanceDataModel GetDataModelOriginalWithErrors()
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
                VectorX0 = new double[] { 1000000.0, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991 },
                FlowRanges = new (RangeDto metrologicRange, RangeDto technologicRange)[] {
                (metrologicRange: new(){Min = 9.805, Max = 10.205}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 2.912, Max = 3.154}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 6.148, Max = 7.514}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 1.945, Max = 2.025}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 4.991, Max = 5.195}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 3.976, Max = 4.138}, technologicRange: new(){Min = 0.0, Max = 1000.0}),
                (metrologicRange: new(){Min = 0.971, Max = 1.011}, technologicRange: new(){Min = 0.0, Max = 1000.0}) },
                FlowMeasured = [true, true, true, true, true, true, true]
            };

            return dataEntity;
        }

        #endregion
    }
}
