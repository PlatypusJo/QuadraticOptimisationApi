using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.AspNetCore.Mvc;
using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationApi.ResponseModels;
using QuadraticOptimizationApi.Services.Interfaces;
using System;

namespace QuadraticOptimizationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BalanceController : ControllerBase
{
    #region Fields

    private readonly IBalanceService _balanceService;

    #endregion

    #region Constructors

    public BalanceController(IBalanceService balanceService)
    {
        _balanceService = balanceService;
    }

    #endregion

    #region Methods

    [HttpPost("solve")]
    public IActionResult Solve([FromBody] BalanceRequest request)
    {
        var response = _balanceService.Solve(request);

        return Ok(response);
    }

    #endregion
}
