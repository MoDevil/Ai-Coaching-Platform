using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Application.Substances.Engine;
using AiCoachOs.Application.Substances.Interfaces;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Substances;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class SubstanceService : ISubstanceService
{
    private readonly IApplicationDbContext _context;
    private readonly ISubstanceSafetyEvaluator _safetyEvaluator;

    public SubstanceService(
        IApplicationDbContext context,
        ISubstanceSafetyEvaluator safetyEvaluator)
    {
        _context = context;
        _safetyEvaluator = safetyEvaluator;
    }

    public async Task<IReadOnlyList<SupplementKnowledgeSummaryDto>> GetSupplementsAsync(
        bool includeProvisional = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Supplements
            .Where(s => s.IsActive);

        if (!includeProvisional)
        {
            query = query.Where(s => s.PrimaryKnowledgeClaim == null || s.PrimaryKnowledgeClaim.Status != ClaimStatus.Provisional);
        }

        var list = await query
            .OrderBy(s => s.Name)
            .Select(s => new SupplementKnowledgeSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                SupplementCategory = s.SupplementCategory,
                EvidenceLevel = s.EvidenceLevel,
                Description = s.Description,
                IsEgyptianMarketAvailable = s.IsEgyptianMarketAvailable,
                SafetyFlagCount = s.SafetyFlags.Count,
                LastReviewedAtUtc = s.LastReviewedAtUtc
            })
            .ToListAsync(cancellationToken);

        return list;
    }

    public async Task<SupplementKnowledgeDto?> GetSupplementByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var supplement = await _context.FindSupplementByIdAsync(id, cancellationToken);
        if (supplement == null)
            return null;

        EvidenceCitationDto? evidenceDto = null;
        if (supplement.PrimaryKnowledgeClaim != null)
        {
            var claim = supplement.PrimaryKnowledgeClaim;
            var citations = claim.Sources?
                .Select(s => s.Source != null ? $"{s.Source.Authors} ({s.Source.Year}). {s.Source.Title}. {s.Source.Notes ?? string.Empty}" : "Unknown source")
                .ToList() ?? new List<string>();

            evidenceDto = new EvidenceCitationDto
            {
                ClaimId = claim.Id,
                Topic = claim.Topic,
                ClaimText = claim.ClaimText,
                EvidenceLevel = claim.EvidenceLevel,
                Status = claim.Status,
                SourceCitations = citations
            };
        }

        return new SupplementKnowledgeDto
        {
            Id = supplement.Id,
            Name = supplement.Name,
            Category = supplement.Category,
            SupplementCategory = supplement.SupplementCategory,
            EvidenceLevel = supplement.EvidenceLevel,
            Description = supplement.Description,
            EvidenceSummary = supplement.EvidenceSummary,
            UncertaintyStatement = supplement.UncertaintyStatement,
            CommonForms = supplement.CommonForms,
            TypicalDoseRange = supplement.TypicalDoseRange,
            TimingRecommendation = supplement.TimingRecommendation,
            InteractionsAndNotes = supplement.InteractionsAndNotes,
            IsEgyptianMarketAvailable = supplement.IsEgyptianMarketAvailable,
            LastReviewedAtUtc = supplement.LastReviewedAtUtc,
            ReviewedBy = supplement.ReviewedBy,
            IsActive = supplement.IsActive,
            SafetyFlags = supplement.SafetyFlags.Select(f => new SubstanceSafetyFlagDto
            {
                FlagType = f.FlagType,
                Severity = f.Severity,
                Message = f.Message,
                EvidenceBasis = f.EvidenceBasis
            }).ToList(),
            EvidenceClaim = evidenceDto
        };
    }

    public async Task<IReadOnlyList<HormoneKnowledgeSummaryDto>> GetHormonesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Hormones
            .Where(h => h.IsActive)
            .OrderBy(h => h.Name)
            .Select(h => new HormoneKnowledgeSummaryDto
            {
                Id = h.Id,
                Name = h.Name,
                HormoneAxis = h.HormoneAxis,
                Description = h.Description,
                LastReviewedAtUtc = h.LastReviewedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<HormoneKnowledgeDto?> GetHormoneByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var hormone = await _context.FindHormoneByIdAsync(id, cancellationToken);
        if (hormone == null)
            return null;

        EvidenceCitationDto? evidenceDto = null;
        if (hormone.PrimaryKnowledgeClaim != null)
        {
            var claim = hormone.PrimaryKnowledgeClaim;
            var citations = claim.Sources?
                .Select(s => s.Source != null ? $"{s.Source.Authors} ({s.Source.Year}). {s.Source.Title}. {s.Source.Notes ?? string.Empty}" : "Unknown source")
                .ToList() ?? new List<string>();

            evidenceDto = new EvidenceCitationDto
            {
                ClaimId = claim.Id,
                Topic = claim.Topic,
                ClaimText = claim.ClaimText,
                EvidenceLevel = claim.EvidenceLevel,
                Status = claim.Status,
                SourceCitations = citations
            };
        }

        return new HormoneKnowledgeDto
        {
            Id = hormone.Id,
            Name = hormone.Name,
            Category = hormone.Category,
            HormoneAxis = hormone.HormoneAxis,
            Description = hormone.Description,
            PhysiologicalRole = hormone.PhysiologicalRole,
            TrainingImpactSummary = hormone.TrainingImpactSummary,
            EvidenceSummary = hormone.EvidenceSummary,
            UncertaintyStatement = hormone.UncertaintyStatement,
            BiomarkerReferenceNotes = hormone.BiomarkerReferenceNotes,
            LastReviewedAtUtc = hormone.LastReviewedAtUtc,
            ReviewedBy = hormone.ReviewedBy,
            IsActive = hormone.IsActive,
            SafetyFlags = hormone.SafetyFlags.Select(f => new SubstanceSafetyFlagDto
            {
                FlagType = f.FlagType,
                Severity = f.Severity,
                Message = f.Message,
                EvidenceBasis = f.EvidenceBasis
            }).ToList(),
            EvidenceClaim = evidenceDto
        };
    }

    public async Task<IReadOnlyList<PEDSafetyRecordSummaryDto>> GetPEDSafetyRecordsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.PEDSafetyRecords
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new PEDSafetyRecordSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                PEDCategory = p.PEDCategory,
                Description = p.Description,
                RiskCount = p.Risks.Count,
                LastReviewedAtUtc = p.LastReviewedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PEDSafetyRecordDto?> GetPEDSafetyRecordByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ped = await _context.FindPEDSafetyRecordByIdAsync(id, cancellationToken);
        if (ped == null)
            return null;

        EvidenceCitationDto? evidenceDto = null;
        if (ped.PrimaryKnowledgeClaim != null)
        {
            var claim = ped.PrimaryKnowledgeClaim;
            var citations = claim.Sources?
                .Select(s => s.Source != null ? $"{s.Source.Authors} ({s.Source.Year}). {s.Source.Title}. {s.Source.Notes ?? string.Empty}" : "Unknown source")
                .ToList() ?? new List<string>();

            evidenceDto = new EvidenceCitationDto
            {
                ClaimId = claim.Id,
                Topic = claim.Topic,
                ClaimText = claim.ClaimText,
                EvidenceLevel = claim.EvidenceLevel,
                Status = claim.Status,
                SourceCitations = citations
            };
        }

        return new PEDSafetyRecordDto
        {
            Id = ped.Id,
            Name = ped.Name,
            Category = ped.Category,
            PEDCategory = ped.PEDCategory,
            Description = ped.Description,
            MechanismSummary = ped.MechanismSummary,
            HealthRisksSummary = ped.HealthRisksSummary,
            EvidenceSummary = ped.EvidenceSummary,
            SafetyDisclaimer = ped.SafetyDisclaimer,
            LastReviewedAtUtc = ped.LastReviewedAtUtc,
            ReviewedBy = ped.ReviewedBy,
            IsActive = ped.IsActive,
            Risks = ped.Risks.Select(r => new PEDRiskRecordDto
            {
                Id = r.Id,
                OrganSystem = r.OrganSystem,
                Severity = r.Severity,
                RiskDescription = r.RiskDescription,
                ReversibilityNotes = r.ReversibilityNotes,
                KnowledgeClaimId = r.KnowledgeClaimId
            }).ToList(),
            SafetyFlags = ped.SafetyFlags.Select(f => new SubstanceSafetyFlagDto
            {
                FlagType = f.FlagType,
                Severity = f.Severity,
                Message = f.Message,
                EvidenceBasis = f.EvidenceBasis
            }).ToList(),
            EvidenceClaim = evidenceDto
        };
    }

    public async Task<SubstanceSafetyEvaluationResultDto> EvaluateSafetyAsync(
        Guid coachId,
        EvaluateSubstanceSafetyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        SubstanceRecord? substance = null;
        if (request.SubstanceRecordId.HasValue)
        {
            substance = await _context.FindSubstanceByIdAsync(request.SubstanceRecordId.Value, cancellationToken);
        }

        var activeRules = await _context.PEDRedFlagRules
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        var eval = _safetyEvaluator.Evaluate(request.ReportedSignals, activeRules, substance);

        // Record Coach-Owned Escalation Log
        var escalationRecord = new SubstanceEscalationRecord(
            id: Guid.NewGuid(),
            coachId: coachId,
            escalationLevel: eval.EscalationLevel,
            summaryRationale: eval.SummaryRationale,
            recommendedAction: eval.RecommendedAction,
            substanceRecordId: request.SubstanceRecordId,
            createdAtUtc: DateTime.UtcNow,
            reportedSignals: request.ReportedSignals,
            matchedRedFlags: eval.MatchedRedFlags);

        await _context.AddSubstanceEscalationRecordAsync(escalationRecord, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SubstanceSafetyEvaluationResultDto
        {
            EscalationRecordId = escalationRecord.Id,
            EscalationLevel = eval.EscalationLevel,
            SummaryRationale = eval.SummaryRationale,
            RecommendedAction = eval.RecommendedAction,
            Disclaimer = eval.Disclaimer,
            MatchedRedFlags = eval.MatchedRedFlags,
            ReportedSignals = request.ReportedSignals,
            CreatedAtUtc = escalationRecord.CreatedAtUtc
        };
    }

    public async Task<IReadOnlyList<SubstanceEscalationRecordDto>> GetCoachEscalationsAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        return await _context.SubstanceEscalationRecords
            .Where(r => r.CoachId == coachId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new SubstanceEscalationRecordDto
            {
                Id = r.Id,
                CoachId = r.CoachId,
                SubstanceRecordId = r.SubstanceRecordId,
                SubstanceName = r.SubstanceRecord != null ? r.SubstanceRecord.Name : null,
                EscalationLevel = r.EscalationLevel,
                SummaryRationale = r.SummaryRationale,
                RecommendedAction = r.RecommendedAction,
                Disclaimer = r.Disclaimer,
                CreatedAtUtc = r.CreatedAtUtc,
                ReportedSignals = r.ReportedSignals.ToList(),
                MatchedRedFlags = r.MatchedRedFlags.ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PEDRedFlagRuleDto>> GetActivePEDRedFlagRulesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.PEDRedFlagRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .Select(r => new PEDRedFlagRuleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                SignalPattern = r.SignalPattern,
                EscalationLevel = r.EscalationLevel,
                RecommendedAction = r.RecommendedAction,
                EvidenceBasis = r.EvidenceBasis,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
