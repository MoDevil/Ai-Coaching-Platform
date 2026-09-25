using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Application.Substances.Interfaces;
using AiCoachOs.Domain.Substances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/supplements")]
[Authorize]
public class SupplementsController : ControllerBase
{
    private readonly ISubstanceService _substanceService;

    public SupplementsController(ISubstanceService substanceService)
    {
        _substanceService = substanceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SupplementKnowledgeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SupplementKnowledgeSummaryDto>>> GetSupplements(
        [FromQuery] string? name = null,
        [FromQuery] SupplementEvidenceStatus? evidenceStatus = null,
        [FromQuery] bool includeProvisional = false,
        CancellationToken cancellationToken = default)
    {
        var supplements = await _substanceService.GetSupplementsAsync(name, evidenceStatus, includeProvisional, cancellationToken);
        return Ok(supplements);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplementKnowledgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplementKnowledgeDto>> GetSupplementById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var supplement = await _substanceService.GetSupplementByIdAsync(id, cancellationToken);
        if (supplement == null)
            return NotFound();

        return Ok(supplement);
    }
}
