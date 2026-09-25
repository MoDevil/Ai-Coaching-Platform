using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Application.Substances.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/substance-safety")]
[Authorize]
public class SubstanceSafetyController : ControllerBase
{
    private readonly ISubstanceService _substanceService;
    private readonly ICurrentCoachService _currentCoachService;

    public SubstanceSafetyController(
        ISubstanceService substanceService,
        ICurrentCoachService currentCoachService)
    {
        _substanceService = substanceService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(SubstanceSafetyEvaluationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubstanceSafetyEvaluationResultDto>> Evaluate(
        [FromBody] EvaluateSubstanceSafetyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _substanceService.EvaluateSafetyAsync(coachId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("escalations")]
    [ProducesResponseType(typeof(IReadOnlyList<SubstanceEscalationRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubstanceEscalationRecordDto>>> GetCoachEscalations(
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var escalations = await _substanceService.GetCoachEscalationsAsync(coachId, cancellationToken);
        return Ok(escalations);
    }

    [HttpGet("rules")]
    [ProducesResponseType(typeof(IReadOnlyList<PEDRedFlagRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PEDRedFlagRuleDto>>> GetActiveRules(
        CancellationToken cancellationToken = default)
    {
        var rules = await _substanceService.GetActivePEDRedFlagRulesAsync(cancellationToken);
        return Ok(rules);
    }
}
