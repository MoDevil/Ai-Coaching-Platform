using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Memory.Interfaces;
using AiCoachOs.Domain.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clients/{clientId:guid}/memory")]
public class ClientMemoryController : ControllerBase
{
    private readonly IClientMemoryService _memoryService;

    public ClientMemoryController(IClientMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientMemoryRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClientMemoryRecordDto>>> GetMemories(
        [FromRoute] Guid clientId,
        [FromQuery] MemoryCategory? category,
        [FromQuery] bool includeAnonymized = false,
        CancellationToken cancellationToken = default)
    {
        var memories = await _memoryService.GetClientMemoriesAsync(clientId, category, includeAnonymized, cancellationToken);
        return Ok(memories);
    }

    [HttpGet("{recordId:guid}")]
    [ProducesResponseType(typeof(ClientMemoryRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryRecordDto>> GetMemoryById(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var memory = await _memoryService.GetMemoryByIdAsync(clientId, recordId, cancellationToken);
        return Ok(memory);
    }

    [HttpGet("{recordId:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientMemoryRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClientMemoryRecordDto>>> GetMemoryHistory(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var history = await _memoryService.GetMemoryHistoryAsync(clientId, recordId, cancellationToken);
        return Ok(history);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientMemoryRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryRecordDto>> CreateMemory(
        [FromRoute] Guid clientId,
        [FromBody] CreateClientMemoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var memory = await _memoryService.CreateMemoryAsync(clientId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMemoryById), new { clientId, recordId = memory.Id }, memory);
    }

    [HttpPost("{recordId:guid}/correct")]
    [ProducesResponseType(typeof(ClientMemoryRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryRecordDto>> CorrectMemory(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recordId,
        [FromBody] CorrectClientMemoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var memory = await _memoryService.CorrectMemoryAsync(clientId, recordId, request, cancellationToken);
        return Ok(memory);
    }

    [HttpPatch("{recordId:guid}/archive")]
    [ProducesResponseType(typeof(ClientMemoryRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryRecordDto>> ArchiveMemory(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var memory = await _memoryService.ArchiveMemoryAsync(clientId, recordId, cancellationToken);
        return Ok(memory);
    }

    [HttpPatch("{recordId:guid}/flag-uncertain")]
    [ProducesResponseType(typeof(ClientMemoryRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryRecordDto>> FlagUncertain(
        [FromRoute] Guid clientId,
        [FromRoute] Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var memory = await _memoryService.FlagUncertainAsync(clientId, recordId, cancellationToken);
        return Ok(memory);
    }

    [HttpGet("conflicts")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientMemoryConflictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClientMemoryConflictDto>>> GetConflicts(
        [FromRoute] Guid clientId,
        [FromQuery] bool unresolvedOnly = false,
        CancellationToken cancellationToken = default)
    {
        var conflicts = await _memoryService.GetConflictsAsync(clientId, unresolvedOnly, cancellationToken);
        return Ok(conflicts);
    }

    [HttpPost("conflicts")]
    [ProducesResponseType(typeof(ClientMemoryConflictDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryConflictDto>> CreateConflict(
        [FromRoute] Guid clientId,
        [FromBody] CreateClientMemoryConflictRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var conflict = await _memoryService.CreateConflictAsync(clientId, request, cancellationToken);
        return CreatedAtAction(nameof(GetConflicts), new { clientId }, conflict);
    }

    [HttpPost("conflicts/{conflictId:guid}/resolve")]
    [ProducesResponseType(typeof(ClientMemoryConflictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemoryConflictDto>> ResolveConflict(
        [FromRoute] Guid clientId,
        [FromRoute] Guid conflictId,
        [FromBody] ResolveClientMemoryConflictRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var conflict = await _memoryService.ResolveConflictAsync(clientId, conflictId, request, cancellationToken);
        return Ok(conflict);
    }

    [HttpGet("snapshot")]
    [ProducesResponseType(typeof(ClientMemorySnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemorySnapshotDto>> GetSnapshot(
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _memoryService.GetLatestSnapshotAsync(clientId, cancellationToken);
        if (snapshot == null)
            return NoContent();
        return Ok(snapshot);
    }

    [HttpPost("snapshot/generate")]
    [ProducesResponseType(typeof(ClientMemorySnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientMemorySnapshotDto>> GenerateSnapshot(
        [FromRoute] Guid clientId,
        [FromBody] GenerateClientMemorySnapshotRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _memoryService.GenerateSnapshotAsync(clientId, request, cancellationToken);
        return Ok(snapshot);
    }

    [HttpGet("unresolved-questions")]
    [ProducesResponseType(typeof(IReadOnlyList<UnresolvedQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<UnresolvedQuestionDto>>> GetUnresolvedQuestions(
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var questions = await _memoryService.GetUnresolvedQuestionsAsync(clientId, cancellationToken);
        return Ok(questions);
    }

    [HttpPost("anonymize")]
    [ProducesResponseType(typeof(AnonymizeClientMemoryResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AnonymizeClientMemoryResultDto>> Anonymize(
        [FromRoute] Guid clientId,
        [FromBody] AnonymizeClientMemoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _memoryService.AnonymizeClientMemoriesAsync(clientId, request, cancellationToken);
        return Ok(result);
    }
}
