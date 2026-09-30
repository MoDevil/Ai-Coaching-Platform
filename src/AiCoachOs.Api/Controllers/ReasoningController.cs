using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Memory.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReasoningController : ControllerBase
{
    private readonly IAiReasoningService _reasoningService;
    private readonly ICurrentCoachService _currentCoachService;

    public ReasoningController(
        IAiReasoningService reasoningService,
        ICurrentCoachService currentCoachService)
    {
        _reasoningService = reasoningService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(AIRecommendationRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIRecommendationRecordDto>> GenerateReasoning(
        [FromBody] GenerateReasoningRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _reasoningService.GenerateReasoningAsync(coachId, request, cancellationToken);
        return CreatedAtAction(nameof(GetReasoningById), new { recommendationId = result.Id }, result);
    }

    [HttpGet("{recommendationId:guid}")]
    [ProducesResponseType(typeof(AIRecommendationRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIRecommendationRecordDto>> GetReasoningById(
        [FromRoute] Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _reasoningService.GetReasoningByIdAsync(coachId, recommendationId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{recommendationId:guid}/review")]
    [ProducesResponseType(typeof(AIRecommendationRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIRecommendationRecordDto>> ReviewRecommendation(
        [FromRoute] Guid recommendationId,
        [FromBody] ReviewAIRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _reasoningService.ReviewRecommendationAsync(coachId, recommendationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{recommendationId:guid}/link-program-version")]
    [ProducesResponseType(typeof(AIRecommendationRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIRecommendationRecordDto>> LinkProgramVersion(
        [FromRoute] Guid recommendationId,
        [FromBody] LinkProgramVersionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _reasoningService.LinkProgramVersionAsync(coachId, recommendationId, request.ProgramVersionId, cancellationToken);
        return Ok(result);
    }
}
