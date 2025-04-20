using Microsoft.AspNetCore.Mvc;
using Moq;
using QuadraticOptimizationApi.Controllers;
using QuadraticOptimizationApi.DTOs;
using QuadraticOptimizationApi.MathTools;
using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationApi.ResponseModels;
using QuadraticOptimizationApi.Services.Interfaces;
using QuadraticOptimizationSolver.DataModels;
using QuadraticOptimizationSolver.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTests
{
    public class BalanceControllerTests
    {
        private readonly Mock<GlobalTestCalculator> _mockCalculator = new();
        private readonly Mock<IBalanceService> _mockBalanceService = new();
        private BalanceController _controller;

        [Fact]
        public void SolveBalance_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            _controller = new BalanceController(_mockBalanceService.Object);
            var request = new BalanceRequest
            {
                Flows = new List<FlowDto>
            {
                new() { 
                    Name = "X1", 
                    MeasuredValue = 100, 
                    Tolerance = 5, 
                    IsMeasured = true, 
                    MetrologicRange = new() { Min = 95, Max = 105 }, 
                    TechnologicRange = new() { Min = 0, Max = 1000 } }
            },
                Nodes = new List<NodeDto>(),
                Constraints = new List<ConstraintDto>()
            };

            _mockBalanceService.Setup(x => x.Solve(It.IsAny<BalanceRequest>()))
                .Returns(new BalanceResponse { BalancedFlows = [new() { Name = "X1", Value = 105.0 }], GlobalTestResult = new() { IsBalanceValid = true }, IsBalanced = true });

            // Act
            var result = _controller.Solve(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BalanceResponse>(okResult.Value);
            Assert.Equal(105.0, response.BalancedFlows[0].Value);
            Assert.True(response.IsBalanced);
            Assert.True(response.GlobalTestResult.IsBalanceValid);
        }

    }
}
