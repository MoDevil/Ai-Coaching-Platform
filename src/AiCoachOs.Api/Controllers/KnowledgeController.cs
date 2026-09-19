using AiCoachOs.Application.Knowledge.DTOs;
using AiCoachOs.Application.Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/knowledge")]
[Authorize]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;

    public KnowledgeController(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    // Sources endpoints
    [HttpPost("sources")]
    [ProducesResponseType(typeof(KnowledgeSourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSource([FromBody] CreateKnowledgeSourceDto dto, CancellationToken ct)
    {
        var source = await _knowledgeService.CreateSourceAsync(dto, ct);
        return CreatedAtAction(nameof(GetSourceById), new { id = source.Id }, source);
    }

    [HttpGet("sources")]
    [ProducesResponseType(typeof(IReadOnlyList<KnowledgeSourceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources(CancellationToken ct)
    {
        var sources = await _knowledgeService.GetSourcesAsync(ct);
        return Ok(sources);
    }

    [HttpGet("sources/{id:guid}")]
    [ProducesResponseType(typeof(KnowledgeSourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSourceById(Guid id, CancellationToken ct)
    {
        var source = await _knowledgeService.GetSourceByIdAsync(id, ct);
        return Ok(source);
    }

    // Claims endpoints
    [HttpPost("claims")]
    [ProducesResponseType(typeof(KnowledgeClaimDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateClaim([FromBody] CreateKnowledgeClaimDto dto, CancellationToken ct)
    {
        var claim = await _knowledgeService.CreateClaimAsync(dto, ct);
        return CreatedAtAction(nameof(GetClaimById), new { id = claim.Id }, claim);
    }

    [HttpGet("claims")]
    [ProducesResponseType(typeof(IReadOnlyList<KnowledgeClaimSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClaims([FromQuery] KnowledgeFilterDto filter, CancellationToken ct)
    {
        var claims = await _knowledgeService.GetClaimsAsync(filter, ct);
        return Ok(claims);
    }

    [HttpGet("claims/{id:guid}")]
    [ProducesResponseType(typeof(KnowledgeClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimById(Guid id, CancellationToken ct)
    {
        var claim = await _knowledgeService.GetClaimByIdAsync(id, ct);
        return Ok(claim);
    }

    // Claim-Source relationship
    [HttpPost("claims/{id:guid}/sources")]
    [ProducesResponseType(typeof(KnowledgeClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSourceToClaim(Guid id, [FromBody] AddClaimSourceDto dto, CancellationToken ct)
    {
        var claim = await _knowledgeService.AddSourceToClaimAsync(id, dto, ct);
        return Ok(claim);
    }

    // Versioning / Supersession
    [HttpPost("claims/{id:guid}/supersede")]
    [ProducesResponseType(typeof(KnowledgeClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SupersedeClaim(Guid id, [FromBody] SupersedeClaimDto dto, CancellationToken ct)
    {
        var claim = await _knowledgeService.SupersedeClaimAsync(id, dto, ct);
        return Ok(claim);
    }
}
