using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Application.Substances.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/hormones")]
[Authorize]
public class HormonesController : ControllerBase
{
    private readonly ISubstanceService _substanceService;

    public HormonesController(ISubstanceService substanceService)
    {
        _substanceService = substanceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HormoneKnowledgeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HormoneKnowledgeSummaryDto>>> GetHormones(
        CancellationToken cancellationToken = default)
    {
        var hormones = await _substanceService.GetHormonesAsync(cancellationToken);
        return Ok(hormones);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HormoneKnowledgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HormoneKnowledgeDto>> GetHormoneById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var hormone = await _substanceService.GetHormoneByIdAsync(id, cancellationToken);
        if (hormone == null)
            return NotFound();

        return Ok(hormone);
    }
}
