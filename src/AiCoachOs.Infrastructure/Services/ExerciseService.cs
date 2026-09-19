using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Exercises.DTOs;
using AiCoachOs.Application.Exercises.Services;
using AiCoachOs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class ExerciseService : IExerciseService
{
    private readonly ApplicationDbContext _context;

    public ExerciseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExerciseSummaryDto>> GetExercisesAsync(ExerciseFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.ExercisesDbSet
            .AsNoTracking()
            .Include(e => e.MovementPattern)
            .Include(e => e.Muscles).ThenInclude(em => em.Muscle)
            .Include(e => e.Equipment).ThenInclude(ee => ee.Equipment)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(search) ||
                (e.Aliases != null && e.Aliases.ToLower().Contains(search)));
        }

        if (filter.MovementPatternId.HasValue)
        {
            query = query.Where(e => e.MovementPatternId == filter.MovementPatternId.Value);
        }

        if (filter.Category.HasValue)
        {
            query = query.Where(e => e.Category == filter.Category.Value);
        }

        if (filter.MuscleId.HasValue)
        {
            query = query.Where(e => e.Muscles.Any(m => m.MuscleId == filter.MuscleId.Value));
        }

        if (filter.EquipmentId.HasValue)
        {
            query = query.Where(e => e.Equipment.Any(eq => eq.EquipmentId == filter.EquipmentId.Value));
        }

        var exercises = await query.OrderBy(e => e.Name).ToListAsync(ct);

        return exercises.Select(e => new ExerciseSummaryDto(
            e.Id,
            e.Name,
            e.Aliases,
            e.Category,
            e.MovementPatternId,
            e.MovementPattern.Name,
            e.StabilityRequirement,
            e.TechnicalDemand,
            e.LocalFatigueCost,
            e.SystemicFatigueCost,
            e.StimulusPotential,
            e.ProgressionPotential,
            e.ResistanceProfile,
            e.Muscles.Where(m => m.IsPrimary).Select(m => m.Muscle.CommonName ?? m.Muscle.Name).ToList(),
            e.Muscles.Where(m => !m.IsPrimary).Select(m => m.Muscle.CommonName ?? m.Muscle.Name).ToList(),
            e.Equipment.Select(eq => eq.Equipment.Name).ToList(),
            e.SubstitutionGroupId
        )).ToList();
    }

    public async Task<ExerciseDetailDto> GetExerciseByIdAsync(Guid id, CancellationToken ct = default)
    {
        var exercise = await _context.FindExerciseByIdAsync(id, ct);
        if (exercise == null)
            throw new NotFoundException("Exercise", id);

        var muscleDtos = exercise.Muscles.Select(em => new ExerciseMuscleDto(
            em.MuscleId,
            em.Muscle.Name,
            em.Muscle.CommonName,
            em.Muscle.BodyPart,
            em.IsPrimary
        )).ToList();

        var equipmentDtos = exercise.Equipment.Select(ee => new ExerciseEquipmentDto(
            ee.EquipmentId,
            ee.Equipment.Name,
            ee.Equipment.Category,
            ee.IsRequired
        )).ToList();

        var substitutionDtos = exercise.Substitutions.Select(es => new ExerciseSubstitutionDto(
            es.SubstituteExerciseId,
            es.SubstituteExercise.Name,
            es.SubstituteExercise.Category,
            es.SubstituteExercise.MovementPattern.Name,
            es.SubstituteExercise.ResistanceProfile,
            es.SubstituteExercise.StabilityRequirement,
            es.IntentPreservationNotes
        )).ToList();

        return new ExerciseDetailDto(
            exercise.Id,
            exercise.Name,
            exercise.Aliases,
            exercise.Category,
            exercise.MovementPatternId,
            exercise.MovementPattern.Name,
            exercise.JointActions,
            exercise.StabilityRequirement,
            exercise.TechnicalDemand,
            exercise.LocalFatigueCost,
            exercise.SystemicFatigueCost,
            exercise.StimulusPotential,
            exercise.ProgressionPotential,
            exercise.ResistanceProfile,
            exercise.SubstitutionGroupId,
            muscleDtos,
            equipmentDtos,
            substitutionDtos,
            exercise.CreatedAtUtc
        );
    }

    public async Task<IReadOnlyList<ExerciseSubstitutionDto>> GetExerciseSubstitutionsAsync(Guid exerciseId, CancellationToken ct = default)
    {
        var exerciseExists = await _context.ExercisesDbSet.AnyAsync(e => e.Id == exerciseId, ct);
        if (!exerciseExists)
            throw new NotFoundException("Exercise", exerciseId);

        var substitutions = await _context.ExerciseSubstitutionsDbSet
            .AsNoTracking()
            .Where(es => es.ExerciseId == exerciseId)
            .Include(es => es.SubstituteExercise)
                .ThenInclude(se => se.MovementPattern)
            .ToListAsync(ct);

        return substitutions.Select(es => new ExerciseSubstitutionDto(
            es.SubstituteExerciseId,
            es.SubstituteExercise.Name,
            es.SubstituteExercise.Category,
            es.SubstituteExercise.MovementPattern.Name,
            es.SubstituteExercise.ResistanceProfile,
            es.SubstituteExercise.StabilityRequirement,
            es.IntentPreservationNotes
        )).ToList();
    }

    public async Task<IReadOnlyList<MovementPatternDto>> GetMovementPatternsAsync(CancellationToken ct = default)
    {
        var patterns = await _context.MovementPatternsDbSet
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return patterns.Select(p => new MovementPatternDto(p.Id, p.Name, p.Description)).ToList();
    }

    public async Task<IReadOnlyList<MuscleDto>> GetMusclesAsync(CancellationToken ct = default)
    {
        var muscles = await _context.MusclesDbSet
            .AsNoTracking()
            .OrderBy(m => m.BodyPart).ThenBy(m => m.Name)
            .ToListAsync(ct);

        return muscles.Select(m => new MuscleDto(m.Id, m.Name, m.CommonName, m.BodyPart)).ToList();
    }

    public async Task<IReadOnlyList<EquipmentDto>> GetEquipmentAsync(CancellationToken ct = default)
    {
        var items = await _context.EquipmentDbSet
            .AsNoTracking()
            .OrderBy(eq => eq.Name)
            .ToListAsync(ct);

        return items.Select(eq => new EquipmentDto(eq.Id, eq.Name, eq.Category)).ToList();
    }
}
