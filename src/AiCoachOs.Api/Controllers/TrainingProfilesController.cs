using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.TrainingProfiles.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/clients/{clientId:guid}/training-profile")]
[Authorize]
public class TrainingProfilesController : ControllerBase
{
    private readonly ITrainingProfileService _trainingProfileService;
    private readonly IValidator<UpdateTrainingProfileRequestDto> _updateValidator;
    private readonly IValidator<TrainingAvailabilityDto> _availabilityValidator;

    public TrainingProfilesController(
        ITrainingProfileService trainingProfileService,
        IValidator<UpdateTrainingProfileRequestDto> updateValidator,
        IValidator<TrainingAvailabilityDto> availabilityValidator)
    {
        _trainingProfileService = trainingProfileService;
        _updateValidator = updateValidator;
        _availabilityValidator = availabilityValidator;
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
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.Exceptions.ValidationException(errors);
        }

        var profile = await _trainingProfileService.UpdateProfileAsync(clientId, request, ct);
        return Ok(profile);
    }

    [HttpPut("availability")]
    [ProducesResponseType(typeof(TrainingProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailability(Guid clientId, [FromBody] TrainingAvailabilityDto availability, CancellationToken ct)
    {
        var validation = await _availabilityValidator.ValidateAsync(availability, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.Exceptions.ValidationException(errors);
        }

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
