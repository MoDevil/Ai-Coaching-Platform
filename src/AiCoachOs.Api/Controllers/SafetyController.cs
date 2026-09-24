using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Safety.Dtos;
using AiCoachOs.Application.Safety.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SafetyController : ControllerBase
{
    private readonly ISafetyService _safetyService;
    private readonly ICurrentCoachService _currentCoachService;

    public SafetyController(
        ISafetyService safetyService,
        ICurrentCoachService currentCoachService)
    {
        _safetyService = safetyService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("screen")]
    public async Task<ActionResult<SafetyScreeningDto>> ScreenReport(
        [FromBody] CreateSafetyReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _safetyService.ScreenReportAsync(coachId, request, cancellationToken);
        return CreatedAtAction(nameof(GetClientScreenings), new { clientId = request.ClientId }, result);
    }

    [HttpPost("screenings/{screeningId:guid}/acknowledge")]
    public async Task<ActionResult<SafetyScreeningDto>> AcknowledgeScreening(
        Guid screeningId,
        [FromBody] AcknowledgeSafetyScreeningRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _safetyService.AcknowledgeScreeningAsync(coachId, screeningId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clients/{clientId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SafetyScreeningDto>>> GetClientScreenings(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var results = await _safetyService.GetClientScreeningsAsync(coachId, clientId, cancellationToken);
        return Ok(results);
    }

    [HttpGet("rules")]
    public async Task<ActionResult<IReadOnlyList<RedFlagRuleDto>>> GetActiveRules(
        CancellationToken cancellationToken)
    {
        var rules = await _safetyService.GetActiveRulesAsync(cancellationToken);
        return Ok(rules);
    }
}
