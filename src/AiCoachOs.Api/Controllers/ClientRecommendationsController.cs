using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Domain.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clients/{clientId:guid}/recommendations")]
public class ClientRecommendationsController : ControllerBase
{
    private readonly IAiReasoningService _reasoningService;
    private readonly ICurrentCoachService _currentCoachService;

    public ClientRecommendationsController(
        IAiReasoningService reasoningService,
        ICurrentCoachService currentCoachService)
    {
        _reasoningService = reasoningService;
        _currentCoachService = currentCoachService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AIRecommendationSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AIRecommendationSummaryDto>>> GetClientRecommendations(
        [FromRoute] Guid clientId,
        [FromQuery] AIRecommendationReviewStatus? status,
        [FromQuery] AIRecommendationCategory? category,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _reasoningService.GetClientRecommendationsAsync(coachId, clientId, status, category, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{recommendationId:guid}")]
    [ProducesResponseType(typeof(AIRecommendationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIRecommendationDetailDto>> GetClientRecommendationDetail(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _reasoningService.GetClientRecommendationDetailAsync(coachId, clientId, recommendationId, cancellationToken);
        return Ok(result);
    }
}
