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
        string? name = null,
        SupplementEvidenceStatus? evidenceStatus = null,
        bool includeProvisional = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Supplements.Where(s => s.IsActive);

        if (!includeProvisional)
        {
            query = query.Where(s => !s.IsProvisional && (s.PrimaryKnowledgeClaim == null || s.PrimaryKnowledgeClaim.Status != ClaimStatus.Provisional));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalized = name.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(normalized));
        }

        if (evidenceStatus.HasValue)
        {
            query = query.Where(s => s.EvidenceStatus == evidenceStatus.Value);
        }

        var entities = await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(s => new SupplementKnowledgeSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            CommonAliases = s.CommonAliases.ToList(),
            PrimaryClaimedBenefit = s.PrimaryClaimedBenefit,
            EvidenceStatus = s.EvidenceStatus,
            EffectMagnitude = s.EffectMagnitude,
            Description = s.Description,
            IsEgyptianMarketAvailable = s.IsEgyptianMarketAvailable,
            IsProvisional = s.IsProvisional,
            RequiresClinicalReview = s.RequiresClinicalReview,
            SafetyFlagCount = s.SafetyFlags.Count,
            LastReviewedAtUtc = s.LastReviewedAtUtc,
            ReviewDueAtUtc = s.ReviewDueAtUtc
        }).ToList();
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
            CommonAliases = supplement.CommonAliases.ToList(),
            SubstanceCategory = supplement.SubstanceCategory,
            PrimaryClaimedBenefit = supplement.PrimaryClaimedBenefit,
            EfficacyClaim = supplement.EfficacyClaim,
            EvidenceStatus = supplement.EvidenceStatus,
            EffectMagnitude = supplement.EffectMagnitude,
            PopulationNote = supplement.PopulationNote,
            Description = supplement.Description,
            UncertaintyStatement = supplement.UncertaintyStatement,
            TypicalDoseRangeMin = supplement.TypicalDoseRangeMin,
            TypicalDoseRangeMax = supplement.TypicalDoseRangeMax,
            DoseUnit = supplement.DoseUnit,
            DoseSourceClaimId = supplement.DoseSourceClaimId,
            TimingNote = supplement.TimingNote,
            CommonForms = supplement.CommonForms,
            InteractionsAndNotes = supplement.InteractionsAndNotes,
            IsEgyptianMarketAvailable = supplement.IsEgyptianMarketAvailable,
            IsProvisional = supplement.IsProvisional,
            RequiresClinicalReview = supplement.RequiresClinicalReview,
            ClaimStatus = supplement.ClaimStatus,
            LastReviewedAtUtc = supplement.LastReviewedAtUtc,
            ReviewDueAtUtc = supplement.ReviewDueAtUtc,
            ReviewedBy = supplement.ReviewedBy,
            IsActive = supplement.IsActive,
            SafetyFlags = supplement.SafetyFlags.Select(f => new SubstanceSafetyFlagDto
            {
                Category = f.Category,
                Description = f.Description,
                AffectedPopulation = f.AffectedPopulation,
                SourceClaimId = f.SourceClaimId,
                EscalationLevel = f.EscalationLevel,
                CoachNote = f.CoachNote
            }).ToList(),
            EvidenceClaim = evidenceDto
        };
    }

    public async Task<IReadOnlyList<HormoneKnowledgeSummaryDto>> GetHormonesAsync(
        HormoneCategory? category = null,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Hormones.Where(h => h.IsActive);

        if (category.HasValue)
        {
            query = query.Where(h => h.HormoneCategory == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalized = name.Trim().ToLower();
            query = query.Where(h => h.Name.ToLower().Contains(normalized));
        }

        var entities = await query
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(h => new HormoneKnowledgeSummaryDto
        {
            Id = h.Id,
            Name = h.Name,
            CommonAliases = h.CommonAliases.ToList(),
            HormoneCategory = h.HormoneCategory,
            PhysiologicalRole = h.PhysiologicalRole,
            Description = h.Description,
            IsProvisional = h.IsProvisional,
            RequiresClinicalReview = h.RequiresClinicalReview,
            LastReviewedAtUtc = h.LastReviewedAtUtc,
            ReviewDueAtUtc = h.ReviewDueAtUtc
        }).ToList();
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
            CommonAliases = hormone.CommonAliases.ToList(),
            SubstanceCategory = hormone.SubstanceCategory,
            HormoneCategory = hormone.HormoneCategory,
            Description = hormone.Description,
            PhysiologicalRole = hormone.PhysiologicalRole,
            TrainingRelevance = hormone.TrainingRelevance,
            UncertaintyStatement = hormone.UncertaintyStatement,
            BiomarkerReferenceNotes = hormone.BiomarkerReferenceNotes,
            EvidenceClaimIds = hormone.EvidenceClaimIds.ToList(),
            MedicalEvaluationTriggers = hormone.MedicalEvaluationTriggers.ToList(),
            IsProvisional = hormone.IsProvisional,
            RequiresClinicalReview = hormone.RequiresClinicalReview,
            ClaimStatus = hormone.ClaimStatus,
            LastReviewedAtUtc = hormone.LastReviewedAtUtc,
            ReviewDueAtUtc = hormone.ReviewDueAtUtc,
            ReviewedBy = hormone.ReviewedBy,
            IsActive = hormone.IsActive,
            SafetyFlags = hormone.SafetyFlags.Select(f => new SubstanceSafetyFlagDto
            {
                Category = f.Category,
                Description = f.Description,
                AffectedPopulation = f.AffectedPopulation,
                SourceClaimId = f.SourceClaimId,
                EscalationLevel = f.EscalationLevel,
                CoachNote = f.CoachNote
            }).ToList(),
            EvidenceClaim = evidenceDto
        };
    }

    public async Task<IReadOnlyList<PEDSafetyRecordSummaryDto>> GetPEDSafetyRecordsAsync(
        PEDCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PEDSafetyRecords.Where(p => p.IsActive);

        if (category.HasValue)
        {
            query = query.Where(p => p.PEDCategory == category.Value);
        }

        var entities = await query
            .Include(p => p.DocumentedRisks)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(p => new PEDSafetyRecordSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            CommonAliases = p.CommonAliases.ToList(),
            PEDCategory = p.PEDCategory,
            Description = p.Description,
            RiskCount = p.DocumentedRisks.Count,
            IsProvisional = p.IsProvisional,
            RequiresClinicalReview = p.RequiresClinicalReview,
            LastReviewedAtUtc = p.LastReviewedAtUtc,
            ReviewDueAtUtc = p.ReviewDueAtUtc
        }).ToList();
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
            CommonAliases = ped.CommonAliases.ToList(),
            SubstanceCategory = ped.SubstanceCategory,
            PEDCategory = ped.PEDCategory,
            Description = ped.Description,
            MechanismSummary = ped.MechanismSummary,
            SafetyDisclaimer = ped.SafetyDisclaimer,
            MonitoringConcepts = ped.MonitoringConcepts.ToList(),
            IsProvisional = ped.IsProvisional,
            RequiresClinicalReview = ped.RequiresClinicalReview,
            ClaimStatus = ped.ClaimStatus,
            LastReviewedAtUtc = ped.LastReviewedAtUtc,
            ReviewDueAtUtc = ped.ReviewDueAtUtc,
            ReviewedBy = ped.ReviewedBy,
            IsActive = ped.IsActive,
            Risks = ped.DocumentedRisks.Select(r => new PEDRiskRecordDto
            {
                Id = r.Id,
                PEDSafetyRecordId = r.PEDSafetyRecordId,
                RiskCategory = r.RiskCategory,
                Severity = r.Severity,
                Description = r.Description,
                EvidenceLevel = r.EvidenceLevel,
                ReversibilityNotes = r.ReversibilityNotes,
                EvidenceClaimId = r.EvidenceClaimId
            }).ToList(),
            SafetyFlags = ped.SafetyFlags.Select(f => new SubstanceSafetyFlagDto
            {
                Category = f.Category,
                Description = f.Description,
                AffectedPopulation = f.AffectedPopulation,
                SourceClaimId = f.SourceClaimId,
                EscalationLevel = f.EscalationLevel,
                CoachNote = f.CoachNote
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

        // Record Coach-Owned Escalation Log (Strictly NO ClientId)
        var escalationRecord = new SubstanceEscalationRecord(
            id: Guid.NewGuid(),
            coachId: coachId,
            escalationLevel: eval.EscalationLevel,
            summaryRationale: eval.SummaryRationale,
            recommendedAction: eval.RecommendedAction,
            substanceRecordId: request.SubstanceRecordId,
            coachNote: request.CoachNote,
            createdAtUtc: DateTime.UtcNow,
            reportedSignals: request.ReportedSignals,
            triggeredFlagIds: eval.MatchedRedFlags);

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

        var entities = await _context.SubstanceEscalationRecords
            .Include(r => r.SubstanceRecord)
            .Where(r => r.CoachId == coachId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => new SubstanceEscalationRecordDto
        {
            Id = r.Id,
            CoachId = r.CoachId,
            SubstanceRecordId = r.SubstanceRecordId,
            SubstanceName = r.SubstanceRecord != null ? r.SubstanceRecord.Name : null,
            EscalationLevel = r.EscalationLevel,
            CoachNote = r.CoachNote,
            SummaryRationale = r.SummaryRationale,
            RecommendedAction = r.RecommendedAction,
            Disclaimer = r.Disclaimer,
            CreatedAtUtc = r.CreatedAtUtc,
            ReportedSignals = r.ReportedSignals.ToList(),
            TriggeredFlagIds = r.TriggeredFlagIds.ToList()
        }).ToList();
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
                PEDCategory = r.PEDCategory,
                Description = r.Description,
                SignalPattern = r.SignalPattern,
                EscalationLevel = r.EscalationLevel,
                RequiresClinicalReview = r.RequiresClinicalReview,
                RecommendedAction = r.RecommendedAction,
                EvidenceBasis = r.EvidenceBasis,
                SourceClaimId = r.SourceClaimId,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
