using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Rehab.Dtos;
using AiCoachOs.Application.Rehab.Engine;
using AiCoachOs.Application.Rehab.Interfaces;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Rehab;
using AiCoachOs.Domain.Safety;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class RehabService : IRehabService
{
    private readonly IApplicationDbContext _context;
    private readonly IRehabAwarenessEngine _engine;

    public RehabService(IApplicationDbContext context, IRehabAwarenessEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    public async Task<TrainingLimitationDto> CreateLimitationAsync(
        Guid coachId,
        CreateTrainingLimitationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{request.ClientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access this client.");

        if (request.SafetyScreeningId.HasValue)
        {
            var screening = await _context.FindSafetyScreeningByIdAsync(request.SafetyScreeningId.Value, cancellationToken);
            if (screening == null || screening.ClientId != request.ClientId)
                throw new InvalidOperationException("Referenced safety screening does not belong to this client.");
        }

        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: request.ClientId,
            affectedBodyRegion: request.AffectedBodyRegion,
            limitationSource: request.LimitationSource,
            reportedAtUtc: DateTime.UtcNow,
            safetyScreeningId: request.SafetyScreeningId,
            description: request.Description);

        await _context.AddTrainingLimitationAsync(limitation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(limitation);
    }

    public async Task<TrainingLimitationDto> ActivateLimitationAsync(
        Guid coachId,
        Guid limitationId,
        ActivateLimitationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var limitation = await _context.FindTrainingLimitationByIdAsync(limitationId, cancellationToken);
        if (limitation == null)
            throw new KeyNotFoundException($"Training limitation '{limitationId}' was not found.");

        if (limitation.Client == null)
        {
            var client = await _context.FindClientByIdAsync(limitation.ClientId, cancellationToken);
            if (client == null || client.CoachId != coachId)
                throw new UnauthorizedAccessException("Coach is not authorized to activate this limitation.");
        }
        else if (limitation.Client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach is not authorized to activate this limitation.");
        }

        limitation.ActivateByCoach(DateTime.UtcNow, request.CoachNote);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(limitation);
    }

    public async Task<TrainingLimitationDto> UpdateLimitationStatusAsync(
        Guid coachId,
        Guid limitationId,
        UpdateLimitationStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var limitation = await _context.FindTrainingLimitationByIdAsync(limitationId, cancellationToken);
        if (limitation == null)
            throw new KeyNotFoundException($"Training limitation '{limitationId}' was not found.");

        if (limitation.Client == null)
        {
            var client = await _context.FindClientByIdAsync(limitation.ClientId, cancellationToken);
            if (client == null || client.CoachId != coachId)
                throw new UnauthorizedAccessException("Coach is not authorized to update this limitation.");
        }
        else if (limitation.Client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach is not authorized to update this limitation.");
        }

        switch (request.Status)
        {
            case LimitationStatus.Resolved:
                limitation.Resolve(DateTime.UtcNow);
                break;
            case LimitationStatus.OnHold:
                limitation.PutOnHold();
                break;
            case LimitationStatus.Active:
                limitation.Reactivate();
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(limitation);
    }

    public async Task<IReadOnlyList<RehabAwarenessConsiderationDto>> GenerateConsiderationsAsync(
        Guid coachId,
        Guid limitationId,
        GenerateConsiderationsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var limitation = await _context.FindTrainingLimitationByIdAsync(limitationId, cancellationToken);
        if (limitation == null)
            throw new KeyNotFoundException($"Training limitation '{limitationId}' was not found.");

        if (limitation.Client == null)
        {
            var client = await _context.FindClientByIdAsync(limitation.ClientId, cancellationToken);
            if (client == null || client.CoachId != coachId)
                throw new UnauthorizedAccessException("Coach is not authorized to generate considerations for this limitation.");
        }
        else if (limitation.Client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach is not authorized to generate considerations for this limitation.");
        }

        // Determine safety screening to evaluate gating
        SafetyScreening? screening = limitation.SafetyScreening;
        if (screening == null && limitation.SafetyScreeningId.HasValue)
        {
            screening = await _context.FindSafetyScreeningByIdAsync(limitation.SafetyScreeningId.Value, cancellationToken);
        }
        if (screening == null)
        {
            // Fall back to latest screening for client if any
            screening = await _context.SafetyScreenings
                .Where(s => s.ClientId == limitation.ClientId)
                .OrderByDescending(s => s.GeneratedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Check if gating blocks generation
        if (screening != null)
        {
            if (screening.ScreeningResult == SafetyCategory.UrgentMedicalAttention)
            {
                throw new InvalidOperationException("Cannot generate training considerations: client has an active Urgent Medical Attention safety screening.");
            }

            if (screening.ScreeningResult == SafetyCategory.ReferToHealthcareProfessional &&
                (limitation.CoachActivatedM9AtUtc == null || string.IsNullOrWhiteSpace(limitation.CoachActivationNote)))
            {
                throw new InvalidOperationException("Cannot generate training considerations: post-referral limitations require explicit coach activation note.");
            }

            if (screening.ScreeningResult == SafetyCategory.CautionCoachReview && screening.CoachAcknowledgedAtUtc == null)
            {
                throw new InvalidOperationException("Cannot generate training considerations: safety screening requires coach acknowledgment first.");
            }
        }

        Exercise? exercise = null;
        List<ExerciseSubstitution> substitutions = new();
        if (request.ExerciseId.HasValue)
        {
            exercise = await _context.FindExerciseByIdAsync(request.ExerciseId.Value, cancellationToken);
            if (exercise != null)
            {
                substitutions = await _context.ExerciseSubstitutions
                    .Include(s => s.SubstituteExercise)
                    .Where(s => s.ExerciseId == exercise.Id)
                    .ToListAsync(cancellationToken);
            }
        }

        var knowledgeClaims = await _context.KnowledgeClaims
            .Where(k => k.Status == ClaimStatus.Active)
            .ToListAsync(cancellationToken);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: screening,
            exercise: exercise,
            substitutions: substitutions,
            knowledgeClaims: knowledgeClaims,
            generatedAtUtc: DateTime.UtcNow);

        foreach (var consideration in considerations)
        {
            await _context.AddRehabAwarenessConsiderationAsync(consideration, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return considerations.Select(c => MapConsiderationToDto(c, exercise?.Name)).ToList();
    }

    public async Task<RehabAwarenessConsiderationDto> RecordDecisionAsync(
        Guid coachId,
        Guid considerationId,
        RecordConsiderationDecisionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var consideration = await _context.FindRehabAwarenessConsiderationByIdAsync(considerationId, cancellationToken);
        if (consideration == null)
            throw new KeyNotFoundException($"Consideration '{considerationId}' was not found.");

        var client = consideration.TrainingLimitation?.Client;
        if (client == null && consideration.TrainingLimitation != null)
        {
            client = await _context.FindClientByIdAsync(consideration.TrainingLimitation.ClientId, cancellationToken);
        }

        if (client == null || client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to record decision on this consideration.");

        switch (request.Decision)
        {
            case ConsiderationStatus.ApprovedByCoach:
                consideration.Approve(DateTime.UtcNow, request.Note);
                break;
            case ConsiderationStatus.RejectedByCoach:
                consideration.Reject(DateTime.UtcNow, request.Note);
                break;
            case ConsiderationStatus.Applied:
                consideration.Apply(DateTime.UtcNow, request.Note);
                break;
            default:
                throw new ArgumentException($"Unsupported consideration decision status: {request.Decision}");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapConsiderationToDto(consideration, consideration.Exercise?.Name);
    }

    public async Task<IReadOnlyList<TrainingLimitationDto>> GetClientLimitationsAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to view limitations for this client.");

        var limitations = await _context.TrainingLimitations
            .Include(t => t.Considerations)
                .ThenInclude(c => c.Exercise)
            .Include(t => t.Considerations)
                .ThenInclude(c => c.KnowledgeClaim)
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        return limitations.Select(MapToDto).ToList();
    }

    private static TrainingLimitationDto MapToDto(TrainingLimitation t)
    {
        var considerations = t.Considerations
            .Select(c => MapConsiderationToDto(c, c.Exercise?.Name))
            .ToList();

        return new TrainingLimitationDto(
            t.Id,
            t.ClientId,
            t.SafetyScreeningId,
            t.AffectedBodyRegion,
            t.LimitationSource,
            t.Status,
            t.Description,
            t.ReportedAtUtc,
            t.CoachActivatedM9AtUtc,
            t.CoachActivationNote,
            t.ResolvedAtUtc,
            considerations);
    }

    private static RehabAwarenessConsiderationDto MapConsiderationToDto(RehabAwarenessConsideration c, string? exerciseName)
    {
        return new RehabAwarenessConsiderationDto(
            c.Id,
            c.TrainingLimitationId,
            c.ExerciseId,
            exerciseName ?? c.Exercise?.Name,
            c.ConsiderationType,
            c.ConsiderationText,
            c.KnowledgeClaimId,
            c.EvidenceBasis,
            c.Disclaimer,
            c.Status,
            c.GeneratedAtUtc,
            c.CoachDecisionAtUtc,
            c.CoachDecisionNote);
    }
}
