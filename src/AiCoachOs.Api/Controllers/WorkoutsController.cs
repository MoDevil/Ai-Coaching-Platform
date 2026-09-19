using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Application.Workouts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;
    private readonly ICurrentCoachService _currentCoachService;

    public WorkoutsController(IWorkoutService workoutService, ICurrentCoachService currentCoachService)
    {
        _workoutService = workoutService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start([FromBody] StartWorkoutRequestDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.StartWorkoutAsync(coachId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.GetWorkoutByIdAsync(coachId, id, cancellationToken);
        if (workout == null)
        {
            return NotFound(new { message = $"Workout session with ID '{id}' was not found." });
        }

        return Ok(workout);
    }

    [HttpGet("client/{clientId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkoutSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClientId(Guid clientId, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workouts = await _workoutService.GetWorkoutsByClientIdAsync(coachId, clientId, cancellationToken);
        return Ok(workouts);
    }

    [HttpPost("{id:guid}/exercises")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddExercise(Guid id, [FromBody] AddWorkoutExerciseRequestDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.AddExerciseAsync(coachId, id, request, cancellationToken);
        return Ok(workout);
    }

    [HttpPost("{id:guid}/exercises/{workoutExerciseId:guid}/sets")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordSet(Guid id, Guid workoutExerciseId, [FromBody] RecordWorkoutSetRequestDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.RecordSetAsync(coachId, id, workoutExerciseId, request, cancellationToken);
        return Ok(workout);
    }

    [HttpPut("{id:guid}/sets/{setId:guid}")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSet(Guid id, Guid setId, [FromBody] UpdateWorkoutSetRequestDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.UpdateSetAsync(coachId, id, setId, request, cancellationToken);
        return Ok(workout);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteWorkoutRequestDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.CompleteWorkoutAsync(coachId, id, request, cancellationToken);
        return Ok(workout);
    }

    [HttpPost("{id:guid}/abandon")]
    [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Abandon(Guid id, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var workout = await _workoutService.AbandonWorkoutAsync(coachId, id, cancellationToken);
        return Ok(workout);
    }
}
