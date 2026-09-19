using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using AiCoachOs.Application.AnatomyAndBiomechanics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/biomechanics")]
[Authorize]
public class BiomechanicsController : ControllerBase
{
    private readonly IBiomechanicsService _biomechanicsService;

    public BiomechanicsController(IBiomechanicsService biomechanicsService)
    {
        _biomechanicsService = biomechanicsService;
    }

    [HttpGet("exercises/{exerciseId:guid}")]
    [ProducesResponseType(typeof(ExerciseBiomechanicsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExerciseBiomechanics(Guid exerciseId, CancellationToken ct)
    {
        var biomechanics = await _biomechanicsService.GetExerciseBiomechanicsAsync(exerciseId, ct);
        return Ok(biomechanics);
    }

    [HttpPost("exercises/{exerciseId:guid}/considerations")]
    [ProducesResponseType(typeof(BiomechanicalConsiderationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddConsideration(
        Guid exerciseId,
        [FromBody] CreateBiomechanicalConsiderationDto dto,
        CancellationToken ct)
    {
        var created = await _biomechanicsService.AddConsiderationAsync(exerciseId, dto, ct);
        return CreatedAtAction(
            nameof(GetExerciseBiomechanics),
            new { exerciseId },
            created);
    }
}
