using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.TrainingProfiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/clients/{clientId:guid}/training-profile")]
[Authorize]
public class TrainingProfilesController : ControllerBase
{
    private readonly ITrainingProfileService _trainingProfileService;

    public TrainingProfilesController(ITrainingProfileService trainingProfileService)
    {
        _trainingProfileService = trainingProfileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TrainingProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid clientId, CancellationToken ct)
    {
        var profile = await _trainingProfileService.GetProfileByClientIdAsync(clientId, ct);
        return Ok(profile);
    }

    [HttpPut]
    [ProducesResponseType(typeof(TrainingProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(Guid clientId, [FromBody] UpdateTrainingProfileRequestDto request, CancellationToken ct)
    {
        var profile = await _trainingProfileService.UpdateProfileAsync(clientId, request, ct);
        return Ok(profile);
    }

    [HttpPut("availability")]
    [ProducesResponseType(typeof(TrainingProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailability(Guid clientId, [FromBody] TrainingAvailabilityDto availability, CancellationToken ct)
    {
        var profile = await _trainingProfileService.UpdateAvailabilityAsync(clientId, availability, ct);
        return Ok(profile);
    }

    [HttpPut("priorities")]
    [ProducesResponseType(typeof(TrainingProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePriorities(Guid clientId, [FromBody] List<ClientTrainingPriorityDto> priorities, CancellationToken ct)
    {
        var profile = await _trainingProfileService.UpdatePrioritiesAsync(clientId, priorities, ct);
        return Ok(profile);
    }
}
