using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using AiCoachOs.Application.AnatomyAndBiomechanics.Services;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Domain.AnatomyAndBiomechanics;
using AiCoachOs.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class BiomechanicsService : IBiomechanicsService
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<CreateBiomechanicalConsiderationDto> _considerationValidator;

    public BiomechanicsService(
        ApplicationDbContext context,
        IValidator<CreateBiomechanicalConsiderationDto> considerationValidator)
    {
        _context = context;
        _considerationValidator = considerationValidator;
    }

    public async Task<ExerciseBiomechanicsDto> GetExerciseBiomechanicsAsync(Guid exerciseId, CancellationToken ct = default)
    {
        var exercise = await _context.ExercisesDbSet
            .AsNoTracking()
            .Include(e => e.MovementPattern)
            .FirstOrDefaultAsync(e => e.Id == exerciseId, ct);

        if (exercise == null)
            throw new NotFoundException($"Exercise with ID '{exerciseId}' was not found.");

        var jointActions = await _context.ExerciseJointActionsDbSet
            .AsNoTracking()
            .Include(eja => eja.JointAction).ThenInclude(ja => ja.Joint)
            .Where(eja => eja.ExerciseId == exerciseId)
            .OrderBy(eja => eja.Role).ThenBy(eja => eja.JointAction.Joint.Name)
            .Select(eja => new ExerciseJointActionDto(
                eja.JointActionId,
                eja.JointAction.JointId,
                eja.JointAction.Joint.Name,
                eja.JointAction.ActionType,
                eja.JointAction.ActionType.ToString(),
                eja.JointAction.PlaneOfMotion,
                eja.Role))
            .ToListAsync(ct);

        var considerations = await _context.BiomechanicalConsiderationsDbSet
            .AsNoTracking()
            .Include(bc => bc.KnowledgeClaim)
            .Where(bc => bc.ExerciseId == exerciseId)
            .OrderBy(bc => bc.Aspect).ThenBy(bc => bc.Certainty)
            .Select(bc => new BiomechanicalConsiderationDto(
                bc.Id,
                bc.ExerciseId,
                bc.Aspect,
                bc.Certainty,
                bc.Summary,
                bc.Explanation,
                bc.PracticalCues,
                bc.KnowledgeClaimId,
                bc.KnowledgeClaim != null ? bc.KnowledgeClaim.Topic : null,
                bc.KnowledgeClaim != null ? bc.KnowledgeClaim.ClaimText : null,
                bc.CreatedAtUtc))
            .ToListAsync(ct);

        return new ExerciseBiomechanicsDto(
            exercise.Id,
            exercise.Name,
            exercise.MovementPattern.Name,
            exercise.ResistanceProfile.ToString(),
            jointActions,
            considerations
        );
    }

    public async Task<BiomechanicalConsiderationDto> AddConsiderationAsync(
        Guid exerciseId,
        CreateBiomechanicalConsiderationDto dto,
        CancellationToken ct = default)
    {
        var validationResult = await _considerationValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            throw new AiCoachOs.Application.Common.Exceptions.ValidationException(validationResult.Errors);

        var exerciseExists = await _context.ExercisesDbSet.AnyAsync(e => e.Id == exerciseId, ct);
        if (!exerciseExists)
            throw new NotFoundException($"Exercise with ID '{exerciseId}' was not found.");

        string? claimTopic = null;
        string? claimText = null;

        if (dto.KnowledgeClaimId.HasValue)
        {
            var claim = await _context.KnowledgeClaimsDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(kc => kc.Id == dto.KnowledgeClaimId.Value, ct);

            if (claim == null)
                throw new NotFoundException($"KnowledgeClaim with ID '{dto.KnowledgeClaimId.Value}' was not found.");

            claimTopic = claim.Topic;
            claimText = claim.ClaimText;
        }

        var consideration = new BiomechanicalConsideration(
            Guid.NewGuid(),
            exerciseId,
            dto.Aspect,
            dto.Certainty,
            dto.Summary,
            dto.Explanation,
            dto.PracticalCues,
            dto.KnowledgeClaimId
        );

        await _context.BiomechanicalConsiderationsDbSet.AddAsync(consideration, ct);
        await _context.SaveChangesAsync(ct);

        return new BiomechanicalConsiderationDto(
            consideration.Id,
            consideration.ExerciseId,
            consideration.Aspect,
            consideration.Certainty,
            consideration.Summary,
            consideration.Explanation,
            consideration.PracticalCues,
            consideration.KnowledgeClaimId,
            claimTopic,
            claimText,
            consideration.CreatedAtUtc
        );
    }
}
