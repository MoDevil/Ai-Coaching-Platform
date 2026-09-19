using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Knowledge.DTOs;
using AiCoachOs.Application.Knowledge.Services;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeSourceDto> CreateSourceAsync(CreateKnowledgeSourceDto dto, CancellationToken ct = default)
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            dto.SourceType,
            dto.Title,
            dto.Authors,
            dto.Year,
            dto.EvidenceLevel,
            dto.Doi,
            dto.Url,
            dto.Notes
        );

        await _context.AddKnowledgeSourceAsync(source, ct);
        await _context.SaveChangesAsync(ct);

        return MapToSourceDto(source);
    }

    public async Task<IReadOnlyList<KnowledgeSourceSummaryDto>> GetSourcesAsync(CancellationToken ct = default)
    {
        var sources = await _context.KnowledgeSourcesDbSet
            .AsNoTracking()
            .OrderByDescending(s => s.Year)
            .ThenBy(s => s.Title)
            .ToListAsync(ct);

        return sources.Select(s => new KnowledgeSourceSummaryDto(
            s.Id,
            s.SourceType,
            s.Title,
            s.Authors,
            s.Year,
            s.EvidenceLevel,
            s.Doi
        )).ToList();
    }

    public async Task<KnowledgeSourceDto> GetSourceByIdAsync(Guid id, CancellationToken ct = default)
    {
        var source = await _context.FindKnowledgeSourceByIdAsync(id, ct);
        if (source == null)
            throw new NotFoundException("KnowledgeSource", id);

        return MapToSourceDto(source);
    }

    public async Task<KnowledgeClaimDto> CreateClaimAsync(CreateKnowledgeClaimDto dto, CancellationToken ct = default)
    {
        if (dto.ExerciseId.HasValue)
        {
            var exerciseExists = await _context.ExercisesDbSet.AnyAsync(e => e.Id == dto.ExerciseId.Value, ct);
            if (!exerciseExists)
                throw new NotFoundException("Exercise", dto.ExerciseId.Value);
        }

        var claim = new KnowledgeClaim(
            Guid.NewGuid(),
            dto.Topic,
            dto.Question,
            dto.ClaimText,
            dto.EvidenceLevel,
            dto.Status,
            dto.ExerciseId,
            dto.Population,
            dto.Limitations,
            dto.PracticalApplication
        );

        if (dto.InitialSourceIds != null)
        {
            foreach (var sourceId in dto.InitialSourceIds.Distinct())
            {
                var sourceExists = await _context.KnowledgeSourcesDbSet.AnyAsync(s => s.Id == sourceId, ct);
                if (!sourceExists)
                    throw new NotFoundException("KnowledgeSource", sourceId);

                claim.AddSource(sourceId);
            }
        }

        await _context.AddKnowledgeClaimAsync(claim, ct);
        await _context.SaveChangesAsync(ct);

        return await GetClaimByIdAsync(claim.Id, ct);
    }

    public async Task<IReadOnlyList<KnowledgeClaimSummaryDto>> GetClaimsAsync(KnowledgeFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.KnowledgeClaimsDbSet
            .AsNoTracking()
            .Include(kc => kc.Exercise)
            .Include(kc => kc.Sources)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Topic.ToLower().Contains(search) ||
                c.Question.ToLower().Contains(search) ||
                c.ClaimText.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Topic))
        {
            var topic = filter.Topic.Trim().ToLower();
            query = query.Where(c => c.Topic.ToLower() == topic);
        }

        if (filter.ExerciseId.HasValue)
        {
            query = query.Where(c => c.ExerciseId == filter.ExerciseId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.MinEvidenceLevel.HasValue)
        {
            query = query.Where(c => c.EvidenceLevel >= filter.MinEvidenceLevel.Value);
        }

        var claims = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(ct);

        return claims.Select(c => new KnowledgeClaimSummaryDto(
            c.Id,
            c.Topic,
            c.Question,
            c.ClaimText,
            c.EvidenceLevel,
            c.Status,
            c.ExerciseId,
            c.Exercise?.Name,
            c.Sources.Count,
            c.CreatedAtUtc
        )).ToList();
    }

    public async Task<KnowledgeClaimDto> GetClaimByIdAsync(Guid id, CancellationToken ct = default)
    {
        var claim = await _context.FindKnowledgeClaimByIdAsync(id, ct);
        if (claim == null)
            throw new NotFoundException("KnowledgeClaim", id);

        return MapToClaimDto(claim);
    }

    public async Task<IReadOnlyList<KnowledgeClaimSummaryDto>> GetClaimsByExerciseIdAsync(Guid exerciseId, CancellationToken ct = default)
    {
        var exerciseExists = await _context.ExercisesDbSet.AnyAsync(e => e.Id == exerciseId, ct);
        if (!exerciseExists)
            throw new NotFoundException("Exercise", exerciseId);

        var claims = await _context.KnowledgeClaimsDbSet
            .AsNoTracking()
            .Include(kc => kc.Exercise)
            .Include(kc => kc.Sources)
            .Where(kc => kc.ExerciseId == exerciseId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(ct);

        return claims.Select(c => new KnowledgeClaimSummaryDto(
            c.Id,
            c.Topic,
            c.Question,
            c.ClaimText,
            c.EvidenceLevel,
            c.Status,
            c.ExerciseId,
            c.Exercise?.Name,
            c.Sources.Count,
            c.CreatedAtUtc
        )).ToList();
    }

    public async Task<KnowledgeClaimDto> AddSourceToClaimAsync(Guid claimId, AddClaimSourceDto dto, CancellationToken ct = default)
    {
        var claim = await _context.KnowledgeClaimsDbSet
            .Include(c => c.Sources)
            .FirstOrDefaultAsync(c => c.Id == claimId, ct);

        if (claim == null)
            throw new NotFoundException("KnowledgeClaim", claimId);

        var sourceExists = await _context.KnowledgeSourcesDbSet.AnyAsync(s => s.Id == dto.SourceId, ct);
        if (!sourceExists)
            throw new NotFoundException("KnowledgeSource", dto.SourceId);

        claim.AddSource(dto.SourceId, dto.RelevanceNote);
        await _context.SaveChangesAsync(ct);

        return await GetClaimByIdAsync(claimId, ct);
    }

    public async Task<KnowledgeClaimDto> SupersedeClaimAsync(Guid claimId, SupersedeClaimDto dto, CancellationToken ct = default)
    {
        var claim = await _context.KnowledgeClaimsDbSet
            .FirstOrDefaultAsync(c => c.Id == claimId, ct);

        if (claim == null)
            throw new NotFoundException("KnowledgeClaim", claimId);

        if (claim.Id == dto.ReplacementClaimId)
            throw new InvalidOperationException("A knowledge claim cannot supersede itself.");

        var replacement = await _context.KnowledgeClaimsDbSet
            .FirstOrDefaultAsync(c => c.Id == dto.ReplacementClaimId, ct);

        if (replacement == null)
            throw new NotFoundException("KnowledgeClaim", dto.ReplacementClaimId);

        claim.Supersede(replacement, dto.Reason);
        await _context.SaveChangesAsync(ct);

        return await GetClaimByIdAsync(claimId, ct);
    }

    private static KnowledgeSourceDto MapToSourceDto(KnowledgeSource s) => new(
        s.Id,
        s.SourceType,
        s.Title,
        s.Authors,
        s.Year,
        s.EvidenceLevel,
        s.Doi,
        s.Url,
        s.Notes,
        s.CreatedAtUtc
    );

    private static KnowledgeClaimDto MapToClaimDto(KnowledgeClaim c) => new(
        c.Id,
        c.Topic,
        c.Question,
        c.ClaimText,
        c.EvidenceLevel,
        c.Status,
        c.Population,
        c.Limitations,
        c.PracticalApplication,
        c.ExerciseId,
        c.Exercise?.Name,
        c.ReviewedAtUtc,
        c.ReviewedBy,
        c.SupersededByClaimId,
        c.SupersededAtUtc,
        c.SupersessionReason,
        c.Sources.Select(s => new KnowledgeClaimSourceDto(
            s.SourceId,
            s.Source.SourceType,
            s.Source.Title,
            s.Source.Authors,
            s.Source.Year,
            s.Source.EvidenceLevel,
            s.Source.Doi,
            s.RelevanceNote
        )).ToList(),
        c.CreatedAtUtc
    );
}
