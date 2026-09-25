using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Application.Substances.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/ped-safety")]
[Authorize]
public class PedSafetyController : ControllerBase
{
    private readonly ISubstanceService _substanceService;

    public PedSafetyController(ISubstanceService substanceService)
    {
        _substanceService = substanceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PEDSafetyRecordSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PEDSafetyRecordSummaryDto>>> GetPEDSafetyRecords(
        CancellationToken cancellationToken = default)
    {
        var records = await _substanceService.GetPEDSafetyRecordsAsync(cancellationToken);
        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PEDSafetyRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PEDSafetyRecordDto>> GetPEDSafetyRecordById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var record = await _substanceService.GetPEDSafetyRecordByIdAsync(id, cancellationToken);
        if (record == null)
            return NotFound();

        return Ok(record);
    }
}
