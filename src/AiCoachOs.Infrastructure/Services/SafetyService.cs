using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Safety.Dtos;
using AiCoachOs.Application.Safety.Engine;
using AiCoachOs.Application.Safety.Interfaces;
using AiCoachOs.Domain.Safety;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class SafetyService : ISafetyService
{
    private readonly IApplicationDbContext _context;
    private readonly ISafetyScreener _screener;

    public SafetyService(IApplicationDbContext context, ISafetyScreener screener)
    {
        _context = context;
        _screener = screener;
    }

    public async Task<SafetyScreeningDto> ScreenReportAsync(
        Guid coachId,
        CreateSafetyReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{request.ClientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access this client.");

        // Load active red flag rules
        var activeRules = await _context.RedFlagRules
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        // Load historical reported signals for client
        var pastScreenings = await _context.SafetyScreenings
            .Where(s => s.ClientId == request.ClientId)
            .OrderByDescending(s => s.GeneratedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        var historicalSignals = pastScreenings
            .SelectMany(s => s.ReportedSignals)
            .ToList();

        var domainSignals = request.Signals.Select(s => new ReportedSignal(
            bodyRegion: s.BodyRegion,
            signalType: s.SignalType,
            onset: s.Onset,
            timing: s.Timing,
            severity: s.Severity,
            worsening: s.Worsening,
            duration: s.Duration,
            associatedWithExerciseId: s.AssociatedWithExerciseId,
            freeText: s.FreeText)).ToList();

        var screening = _screener.Screen(
            clientId: request.ClientId,
            triggeredByType: request.TriggeredByType,
            currentSignals: domainSignals,
            historicalSignals: historicalSignals,
            activeRules: activeRules,
            triggeredByEntityId: request.TriggeredByEntityId);

        await _context.AddSafetyScreeningAsync(screening, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(screening);
    }

    public async Task<SafetyScreeningDto> AcknowledgeScreeningAsync(
        Guid coachId,
        Guid screeningId,
        AcknowledgeSafetyScreeningRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var screening = await _context.FindSafetyScreeningByIdAsync(screeningId, cancellationToken);
        if (screening == null)
            throw new KeyNotFoundException($"Safety screening '{screeningId}' was not found.");

        if (screening.Client == null)
        {
            var client = await _context.FindClientByIdAsync(screening.ClientId, cancellationToken);
            if (client == null || client.CoachId != coachId)
                throw new UnauthorizedAccessException("Coach is not authorized to acknowledge this screening.");
        }
        else if (screening.Client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach is not authorized to acknowledge this screening.");
        }

        screening.AcknowledgeByCoach(DateTime.UtcNow, request.CoachNote);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(screening);
    }

    public async Task<IReadOnlyList<SafetyScreeningDto>> GetClientScreeningsAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to view safety screenings for this client.");

        var screenings = await _context.SafetyScreenings
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.GeneratedAtUtc)
            .ToListAsync(cancellationToken);

        return screenings.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<RedFlagRuleDto>> GetActiveRulesAsync(
        CancellationToken cancellationToken = default)
    {
        var rules = await _context.RedFlagRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.SafetyCategoryTriggered)
            .ToListAsync(cancellationToken);

        return rules.Select(r => new RedFlagRuleDto(
            r.Id,
            r.Name,
            r.Description,
            r.SignalPattern,
            r.SafetyCategoryTriggered,
            r.RecommendedAction,
            r.EvidenceBasis,
            r.RequiresClinicalReview,
            r.IsActive,
            r.LastReviewedAtUtc,
            r.ReviewedBy,
            r.KnowledgeClaimId)).ToList();
    }

    private static SafetyScreeningDto MapToDto(SafetyScreening s)
    {
        var signals = s.ReportedSignals.Select(sig => new ReportedSignalDto(
            sig.BodyRegion,
            sig.SignalType,
            sig.Onset,
            sig.Timing,
            sig.Severity,
            sig.Worsening,
            sig.Duration,
            sig.AssociatedWithExerciseId,
            sig.FreeText)).ToList();

        return new SafetyScreeningDto(
            s.Id,
            s.ClientId,
            s.TriggeredByType,
            s.TriggeredByEntityId,
            s.ScreeningResult,
            s.RecommendedAction,
            s.SummaryRationale,
            s.Disclaimer,
            s.GeneratedAtUtc,
            s.RequiresCoachAcknowledgment,
            s.CoachAcknowledgedAtUtc,
            s.CoachNote,
            signals,
            s.RedFlagsMatched.ToList());
    }
}
