using AiCoachOs.Application.Adaptations.DTOs;
using AiCoachOs.Application.Adaptations.Services;
using AiCoachOs.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdaptationsController : ControllerBase
{
    private readonly IAdaptationService _adaptationService;
    private readonly ICurrentCoachService _currentCoachService;

    public AdaptationsController(
        IAdaptationService adaptationService,
        ICurrentCoachService currentCoachService)
    {
        _adaptationService = adaptationService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("assess/{programVersionId:guid}")]
    [ProducesResponseType(typeof(AdaptationAssessmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assess(Guid programVersionId, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var assessment = await _adaptationService.AssessProgramVersionAsync(coachId, programVersionId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = assessment.Id }, assessment);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdaptationAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var assessment = await _adaptationService.GetAssessmentByIdAsync(coachId, id, cancellationToken);
        if (assessment == null)
            return NotFound();

        return Ok(assessment);
    }

    [HttpGet("program/{programId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AdaptationAssessmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProgramId(Guid programId, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var assessments = await _adaptationService.GetAssessmentsByProgramIdAsync(coachId, programId, cancellationToken);
        return Ok(assessments);
    }

    [HttpPost("recommendations/{id:guid}/decision")]
    [ProducesResponseType(typeof(AdaptationRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecideRecommendation(
        Guid id,
        [FromBody] CoachRecommendationDecisionDto decision,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _adaptationService.DecideRecommendationAsync(coachId, id, decision, cancellationToken);
        return Ok(result);
    }
}
