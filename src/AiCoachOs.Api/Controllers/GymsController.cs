using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Gyms.Dtos;
using AiCoachOs.Application.Gyms.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GymsController : ControllerBase
{
    private readonly IGymService _gymService;
    private readonly ICurrentCoachService _currentCoachService;
    private readonly IApplicationDbContext _context;

    public GymsController(
        IGymService gymService,
        ICurrentCoachService currentCoachService,
        IApplicationDbContext context)
    {
        _gymService = gymService;
        _currentCoachService = currentCoachService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GymProfileDto>>> GetGyms(CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var gyms = await _gymService.GetGymProfilesAsync(coachId, cancellationToken);
        return Ok(gyms);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GymProfileDto>> GetGymById(Guid id, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var gym = await _gymService.GetGymProfileByIdAsync(coachId, id, cancellationToken);
        if (gym == null)
            return NotFound();

        return Ok(gym);
    }

    [HttpPost]
    public async Task<ActionResult<GymProfileDto>> CreateGym(
        [FromBody] CreateGymProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var gym = await _gymService.CreateGymProfileAsync(coachId, request, cancellationToken);
        return CreatedAtAction(nameof(GetGymById), new { id = gym.Id }, gym);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GymProfileDto>> UpdateGym(
        Guid id,
        [FromBody] UpdateGymProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var gym = await _gymService.UpdateGymProfileAsync(coachId, id, request, cancellationToken);
        return Ok(gym);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteGym(
        Guid id,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var success = await _gymService.DeleteGymProfileAsync(coachId, id, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("assign")]
    public async Task<ActionResult> AssignClientGym(
        [FromBody] AssignClientGymRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await _gymService.AssignClientGymAsync(coachId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("assign-client/{clientId:guid}")]
    public async Task<ActionResult> AssignClientGymByRoute(
        Guid clientId,
        [FromBody] AssignClientGymRequestDto request,
        CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var actualRequest = request with { ClientId = clientId };
        await _gymService.AssignClientGymAsync(coachId, actualRequest, cancellationToken);
        return NoContent();
    }

    [HttpGet("equipment-options")]
    public async Task<ActionResult<IReadOnlyList<EquipmentOptionDto>>> GetEquipmentOptions(CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipment
            .OrderBy(e => e.Name)
            .Select(e => new EquipmentOptionDto(e.Id, e.Name, e.Category, true))
            .ToListAsync(cancellationToken);

        return Ok(equipment);
    }
}
