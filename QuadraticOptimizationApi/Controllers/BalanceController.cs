using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.AspNetCore.Mvc;
using QuadraticOptimizationApi.Converters;
using QuadraticOptimizationApi.MathTools;
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

    private readonly IModelValidator _modelValidator;

    #endregion

    #region Constructors

    public BalanceController(IBalanceService balanceService, IModelValidator modelValidator)
    {
        _balanceService = balanceService;
        _modelValidator = modelValidator;
    }

    #endregion

    #region Methods

    [HttpPost("solve")]
    public IActionResult Solve([FromBody] BalanceRequest request)
    {
        var response = _balanceService.Solve(request);

        return Ok(response);
    }

    [HttpPost("FixModel")]
    public ResponseStatus<List<BasicSchemeOutputGT>> DetectErrors(BalanceRequest data, int width, int depth)
    {
        var model = BalanceDataModelConverter.Convert(data);
        var input = new BasicScheme(model.MatrixA, model.VectorX0, model.Tolerance, model.FlowMeasured);
        var res = new List<BasicSchemeGT>();

        _modelValidator.DetectErrors(
            input,
            res,
            depth,
            width);

        return new ResponseStatus<List<BasicSchemeOutputGT>>(res.Select(x => new BasicSchemeOutputGT(x)).ToList(), "Fixed models.");
    }

    #endregion
}
