using AiCoachOs.Application.Exercises.DTOs;
using AiCoachOs.Application.Exercises.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/exercises")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExerciseSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExercises([FromQuery] ExerciseFilterDto filter, CancellationToken ct)
    {
        var exercises = await _exerciseService.GetExercisesAsync(filter, ct);
        return Ok(exercises);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExerciseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExerciseById(Guid id, CancellationToken ct)
    {
        var exercise = await _exerciseService.GetExerciseByIdAsync(id, ct);
        return Ok(exercise);
    }

    [HttpGet("{id:guid}/substitutions")]
    [ProducesResponseType(typeof(IReadOnlyList<ExerciseSubstitutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExerciseSubstitutions(Guid id, CancellationToken ct)
    {
        var substitutions = await _exerciseService.GetExerciseSubstitutionsAsync(id, ct);
        return Ok(substitutions);
    }

    [HttpGet("meta/movement-patterns")]
    [ProducesResponseType(typeof(IReadOnlyList<MovementPatternDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovementPatterns(CancellationToken ct)
    {
        var patterns = await _exerciseService.GetMovementPatternsAsync(ct);
        return Ok(patterns);
    }

    [HttpGet("meta/muscles")]
    [ProducesResponseType(typeof(IReadOnlyList<MuscleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMuscles(CancellationToken ct)
    {
        var muscles = await _exerciseService.GetMusclesAsync(ct);
        return Ok(muscles);
    }

    [HttpGet("meta/equipment")]
    [ProducesResponseType(typeof(IReadOnlyList<EquipmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEquipment(CancellationToken ct)
    {
        var equipment = await _exerciseService.GetEquipmentAsync(ct);
        return Ok(equipment);
    }
}
