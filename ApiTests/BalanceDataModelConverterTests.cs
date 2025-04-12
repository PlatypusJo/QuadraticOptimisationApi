using QuadraticOptimizationApi.Converters;
using QuadraticOptimizationApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTests
{
    public class BalanceDataModelConverterTests
    {
        [Fact]
        public void BuildBalanceMatrix_ValidNodes_CorrectMatrix()
        {
            // Arrange:
            var nodes = new List<NodeDto> 
            {
                new NodeDto
                {
                    Name = "N1",
                    InputFlows = ["X1"],
                    OutputFlows = ["X2", "X3"]
                }
            };
            var flowNames = new List<string> { "X1", "X2", "X3" };

            // Act:
            var matrixA = BalanceDataModelConverter.BuildBalanceMatrix(nodes, flowNames);

            // Assert:
            Assert.Equal(1, matrixA[0, 0]);   // X1
            Assert.Equal(-1, matrixA[0, 1]);  // X2
            Assert.Equal(-1, matrixA[0, 2]);  // X3
        }

        [Fact]
        public void AddConstraints_RatioConstraint_AddsCorrectRow()
        {
            // Arrange:
            var matrixA = new double[1, 3] { { 1, -1, -1 } };
            var constraints = new List<ConstraintDto> { new() { Type = "Ratio", Flow1 = "X1", Flow2 = "X2", Ratio = 10 } };
            var flowNames = new List<string> { "X1", "X2", "X3" };

            // Act:
            BalanceDataModelConverter.AddConstraints(ref matrixA, constraints, flowNames);

            // Assert:
            Assert.Equal(2, matrixA.GetLength(0)); // Новая строка добавлена
            Assert.Equal(1, matrixA[1, 0]);        // X1
            Assert.Equal(-10, matrixA[1, 1]);      // -10 * X2
        }
    }
}
