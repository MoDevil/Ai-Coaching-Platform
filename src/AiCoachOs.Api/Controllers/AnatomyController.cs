using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using AiCoachOs.Application.AnatomyAndBiomechanics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/anatomy")]
[Authorize]
public class AnatomyController : ControllerBase
{
    private readonly IAnatomyService _anatomyService;

    public AnatomyController(IAnatomyService anatomyService)
    {
        _anatomyService = anatomyService;
    }

    [HttpGet("regions")]
    [ProducesResponseType(typeof(IReadOnlyList<AnatomicalRegionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegions(CancellationToken ct)
    {
        var regions = await _anatomyService.GetRegionsAsync(ct);
        return Ok(regions);
    }

    [HttpGet("regions/{id:guid}")]
    [ProducesResponseType(typeof(AnatomicalRegionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegionById(Guid id, CancellationToken ct)
    {
        var region = await _anatomyService.GetRegionByIdAsync(id, ct);
        return Ok(region);
    }

    [HttpGet("joints")]
    [ProducesResponseType(typeof(IReadOnlyList<JointSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJoints([FromQuery] Guid? regionId, CancellationToken ct)
    {
        var joints = await _anatomyService.GetJointsAsync(regionId, ct);
        return Ok(joints);
    }

    [HttpGet("joints/{id:guid}")]
    [ProducesResponseType(typeof(JointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJointById(Guid id, CancellationToken ct)
    {
        var joint = await _anatomyService.GetJointByIdAsync(id, ct);
        return Ok(joint);
    }

    [HttpGet("joint-actions")]
    [ProducesResponseType(typeof(IReadOnlyList<JointActionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJointActions([FromQuery] Guid? jointId, CancellationToken ct)
    {
        var actions = await _anatomyService.GetJointActionsAsync(jointId, ct);
        return Ok(actions);
    }

    [HttpGet("joint-actions/{id:guid}")]
    [ProducesResponseType(typeof(JointActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJointActionById(Guid id, CancellationToken ct)
    {
        var action = await _anatomyService.GetJointActionByIdAsync(id, ct);
        return Ok(action);
    }

    [HttpGet("muscles/{id:guid}")]
    [ProducesResponseType(typeof(MuscleAnatomyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMuscleAnatomy(Guid id, CancellationToken ct)
    {
        var muscleAnatomy = await _anatomyService.GetMuscleAnatomyAsync(id, ct);
        return Ok(muscleAnatomy);
    }
}
