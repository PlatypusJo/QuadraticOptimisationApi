using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.AspNetCore.Mvc;
using QuadraticOptimizationApi.Converters;
using QuadraticOptimizationApi.MathTools;
using QuadraticOptimizationApi.RequestModels;
using QuadraticOptimizationApi.ResponseModels;
using QuadraticOptimizationApi.Services.Interfaces;
using QuadraticOptimizationSolver.DataModels;
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

    [HttpPost("DetectErrors")]
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

    [HttpPost("FixModel")]
    public IActionResult FixModel([FromBody] BalanceRequest data, int width, int depth)
    {
        var model = BalanceDataModelConverter.Convert(data);
        var input = new BasicScheme(model.MatrixA, model.VectorX0, model.Tolerance, model.FlowMeasured);
        var res = new List<BasicSchemeGT>();

        _modelValidator.DetectErrors(
            input,
            res,
            depth,
            width);

        if (res.Count <= 0)
            return Ok("Ошибок не найдено");

        var errorScenarios = res.ToArray();
        errorScenarios = errorScenarios.OrderBy(e => e.GlobalTestRes).ToArray();
        var bestScenario = errorScenarios.First();
        Error[] errors = bestScenario.Errors;
        (int origFlow, int newFlow)[] flowInds = errors.Where(e => e.Type == ErrorTypes.MeasError).Select(e => (e.FlowIndex, e.NewFlowIndex)).ToArray();

        BalanceDataModel newModel = BalanceDataModelConverter.ConvertFromBasicScheme(model, bestScenario, flowInds);
        var newRequest = BalanceDataModelConverter.ConvertToBalanceRequest(newModel, newModel.VectorX0.Select((f, i) => $"X{i + 1}").ToList());
        BalanceResponse balancedModel = _balanceService.Solve(newRequest);
        BalanceDataModel fixedModel = _modelValidator.FixModel(model, balancedModel, flowInds);
        var balanceFixedModel = _balanceService.Solve(BalanceDataModelConverter.ConvertToBalanceRequest(fixedModel, fixedModel.VectorX0.Select((f, i) => $"X{i + 1}").ToList()));
        var fixedRequest = BalanceDataModelConverter.ConvertToBalanceRequest(fixedModel, fixedModel.VectorX0.Select((f, i) => $"X{i + 1}").ToList());
        var response = new FixedModelResponse() { BalanceFixedModel = balanceFixedModel, FixedModel = fixedRequest };

        return Ok(response);
    }

    #endregion
}
