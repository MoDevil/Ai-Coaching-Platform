using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Application.ExpertIngestion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/expert-sources")]
public class ExpertSourcesController : ControllerBase
{
    private readonly IExpertIngestionService _ingestionService;

    public ExpertSourcesController(IExpertIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExpertSourceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ExpertSourceDto>>> GetSources(
        CancellationToken cancellationToken = default)
    {
        var list = await _ingestionService.GetSourcesAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExpertSourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExpertSourceDto>> CreateSource(
        [FromBody] CreateExpertSourceDto request,
        CancellationToken cancellationToken = default)
    {
        var source = await _ingestionService.CreateSourceAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSources), new { id = source.Id }, source);
    }
}
