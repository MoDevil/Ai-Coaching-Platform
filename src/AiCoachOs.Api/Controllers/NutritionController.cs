using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Nutrition.Dtos;
using AiCoachOs.Application.Nutrition.Engine;
using AiCoachOs.Application.Nutrition.Interfaces;
using AiCoachOs.Domain.Nutrition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NutritionController : ControllerBase
{
    private readonly INutritionService _nutritionService;
    private readonly ICurrentCoachService _currentCoachService;

    public NutritionController(
        INutritionService nutritionService,
        ICurrentCoachService currentCoachService)
    {
        _nutritionService = nutritionService;
        _currentCoachService = currentCoachService;
    }

    [HttpGet("clients/{clientId:guid}")]
    public async Task<ActionResult<ClientNutritionProfileDto?>> GetProfile(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.GetClientNutritionProfileAsync(coachId, clientId, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost("profiles")]
    public async Task<ActionResult<ClientNutritionProfileDto>> CreateOrUpdateProfile(
        [FromBody] CreateOrUpdateNutritionProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.CreateOrUpdateProfileAsync(coachId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("calculate-targets")]
    public async Task<ActionResult<CalculateNutritionTargetsResponseDto>> CalculateTargets(
        [FromBody] CalculateNutritionTargetsRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.CalculateTargetsAsync(coachId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("clients/{clientId:guid}/calibrations")]
    public async Task<ActionResult<NutritionCalibrationRecordDto>> RecordCalibration(
        Guid clientId,
        [FromBody] RecordCalibrationRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.RecordCalibrationAsync(coachId, clientId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("calibrations/{calibrationId:guid}/decision")]
    public async Task<ActionResult<NutritionCalibrationRecordDto>> RecordCalibrationDecision(
        Guid calibrationId,
        [FromBody] RecordCalibrationDecisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.RecordCalibrationDecisionAsync(coachId, calibrationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("foods")]
    public async Task<ActionResult<IReadOnlyList<EgyptianFoodDto>>> GetFoods(
        [FromQuery] BudgetTier? budgetTier,
        [FromQuery] FoodCategory? category,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.GetEgyptianFoodsAsync(coachId, budgetTier, category, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clients/{clientId:guid}/suggestions")]
    public async Task<ActionResult<FoodSuggestionResult>> GetFoodSuggestions(
        Guid clientId,
        [FromQuery] BudgetTier? budgetTier,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _nutritionService.GetFoodSuggestionsAsync(coachId, clientId, budgetTier, cancellationToken);
        return Ok(result);
    }
}
