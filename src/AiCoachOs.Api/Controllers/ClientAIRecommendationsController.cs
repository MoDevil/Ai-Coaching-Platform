using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Memory.Interfaces;
using AiCoachOs.Domain.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clients/{clientId:guid}/ai-recommendations")]
public class ClientAIRecommendationsController : ControllerBase
{
    private readonly IClientMemoryService _memoryService;

    public ClientAIRecommendationsController(IClientMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AIRecommendationRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AIRecommendationRecordDto>>> GetRecommendations(
        [FromRoute] Guid clientId,
        [FromQuery] AIRecommendationCategory? category,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _memoryService.GetAIRecommendationsAsync(clientId, category, cancellationToken);
        return Ok(recommendations);
    }

    [HttpPatch("{recommendationId:guid}/decide")]
    [ProducesResponseType(typeof(AIRecommendationRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIRecommendationRecordDto>> DecideRecommendation(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recommendationId,
        [FromBody] DecideAIRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _memoryService.DecideAIRecommendationAsync(clientId, recommendationId, request, cancellationToken);
        return Ok(result);
    }
}
