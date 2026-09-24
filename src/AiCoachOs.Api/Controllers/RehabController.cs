using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Rehab.Dtos;
using AiCoachOs.Application.Rehab.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RehabController : ControllerBase
{
    private readonly IRehabService _rehabService;
    private readonly ICurrentCoachService _currentCoachService;

    public RehabController(
        IRehabService rehabService,
        ICurrentCoachService currentCoachService)
    {
        _rehabService = rehabService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("limitations")]
    public async Task<ActionResult<TrainingLimitationDto>> CreateLimitation(
        [FromBody] CreateTrainingLimitationRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _rehabService.CreateLimitationAsync(coachId, request, cancellationToken);
        return CreatedAtAction(nameof(GetClientLimitations), new { clientId = request.ClientId }, result);
    }

    [HttpPost("limitations/{limitationId:guid}/activate")]
    public async Task<ActionResult<TrainingLimitationDto>> ActivateLimitation(
        Guid limitationId,
        [FromBody] ActivateLimitationRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _rehabService.ActivateLimitationAsync(coachId, limitationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("limitations/{limitationId:guid}/status")]
    public async Task<ActionResult<TrainingLimitationDto>> UpdateLimitationStatus(
        Guid limitationId,
        [FromBody] UpdateLimitationStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _rehabService.UpdateLimitationStatusAsync(coachId, limitationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("limitations/{limitationId:guid}/generate")]
    public async Task<ActionResult<IReadOnlyList<RehabAwarenessConsiderationDto>>> GenerateConsiderations(
        Guid limitationId,
        [FromBody] GenerateConsiderationsRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _rehabService.GenerateConsiderationsAsync(coachId, limitationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("considerations/{considerationId:guid}/decision")]
    public async Task<ActionResult<RehabAwarenessConsiderationDto>> RecordDecision(
        Guid considerationId,
        [FromBody] RecordConsiderationDecisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _rehabService.RecordDecisionAsync(coachId, considerationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clients/{clientId:guid}/limitations")]
    public async Task<ActionResult<IReadOnlyList<TrainingLimitationDto>>> GetClientLimitations(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var results = await _rehabService.GetClientLimitationsAsync(coachId, clientId, cancellationToken);
        return Ok(results);
    }
}
