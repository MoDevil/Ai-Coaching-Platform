using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Application.ExpertIngestion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/expert-ingestions")]
public class ExpertIngestionsController : ControllerBase
{
    private readonly IExpertIngestionService _ingestionService;
    private readonly ICurrentCoachService _currentCoachService;

    public ExpertIngestionsController(
        IExpertIngestionService ingestionService,
        ICurrentCoachService currentCoachService)
    {
        _ingestionService = ingestionService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExpertContentIngestionSummaryDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpertContentIngestionSummaryDto>> SubmitIngestion(
        [FromBody] SubmitIngestionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _ingestionService.SubmitIngestionAsync(coachId, request, cancellationToken);
        return AcceptedAtAction(nameof(GetIngestionById), new { ingestionId = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExpertContentIngestionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ExpertContentIngestionSummaryDto>>> GetIngestions(
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var list = await _ingestionService.GetIngestionsAsync(coachId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("{ingestionId:guid}")]
    [ProducesResponseType(typeof(ExpertContentIngestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpertContentIngestionDto>> GetIngestionById(
        [FromRoute] Guid ingestionId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var ingestion = await _ingestionService.GetIngestionByIdAsync(coachId, ingestionId, cancellationToken);
        return Ok(ingestion);
    }

    [HttpPatch("{ingestionId:guid}/claims/{claimId:guid}/review")]
    [ProducesResponseType(typeof(ExpertClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpertClaimDto>> ReviewClaim(
        [FromRoute] Guid ingestionId,
        [FromRoute] Guid claimId,
        [FromBody] ReviewClaimRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var updatedClaim = await _ingestionService.ReviewClaimAsync(coachId, ingestionId, claimId, request, cancellationToken);
        return Ok(updatedClaim);
    }
}
