using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using AiCoachOs.Application.AnatomyAndBiomechanics.Services;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class AnatomyService : IAnatomyService
{
    private readonly ApplicationDbContext _context;

    public AnatomyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AnatomicalRegionSummaryDto>> GetRegionsAsync(CancellationToken ct = default)
    {
        return await _context.AnatomicalRegionsDbSet
            .AsNoTracking()
            .OrderBy(ar => ar.Name)
            .Select(ar => new AnatomicalRegionSummaryDto(
                ar.Id,
                ar.Name,
                ar.Description,
                ar.Joints.Count))
            .ToListAsync(ct);
    }

    public async Task<AnatomicalRegionDto> GetRegionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var region = await _context.AnatomicalRegionsDbSet
            .AsNoTracking()
            .Include(ar => ar.Joints)
            .FirstOrDefaultAsync(ar => ar.Id == id, ct);

        if (region == null)
            throw new NotFoundException($"AnatomicalRegion with ID '{id}' was not found.");

        return new AnatomicalRegionDto(
            region.Id,
            region.Name,
            region.Description,
            region.Joints.Select(j => new JointSummaryDto(
                j.Id,
                j.RegionId,
                region.Name,
                j.Name,
                j.CommonName,
                j.Description)).ToList()
        );
    }

    public async Task<IReadOnlyList<JointSummaryDto>> GetJointsAsync(Guid? regionId = null, CancellationToken ct = default)
    {
        var query = _context.JointsDbSet
            .AsNoTracking()
            .Include(j => j.Region)
            .AsQueryable();

        if (regionId.HasValue)
        {
            query = query.Where(j => j.RegionId == regionId.Value);
        }

        return await query
            .OrderBy(j => j.Region.Name).ThenBy(j => j.Name)
            .Select(j => new JointSummaryDto(
                j.Id,
                j.RegionId,
                j.Region.Name,
                j.Name,
                j.CommonName,
                j.Description))
            .ToListAsync(ct);
    }

    public async Task<JointDto> GetJointByIdAsync(Guid id, CancellationToken ct = default)
    {
        var joint = await _context.JointsDbSet
            .AsNoTracking()
            .Include(j => j.Region)
            .Include(j => j.Actions)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (joint == null)
            throw new NotFoundException($"Joint with ID '{id}' was not found.");

        return new JointDto(
            joint.Id,
            joint.RegionId,
            joint.Region.Name,
            joint.Name,
            joint.CommonName,
            joint.Description,
            joint.Actions.Select(a => new JointActionSummaryDto(
                a.Id,
                a.JointId,
                joint.Name,
                a.ActionType,
                a.ActionType.ToString(),
                a.PlaneOfMotion,
                a.Description)).ToList()
        );
    }

    public async Task<IReadOnlyList<JointActionSummaryDto>> GetJointActionsAsync(Guid? jointId = null, CancellationToken ct = default)
    {
        var query = _context.JointActionsDbSet
            .AsNoTracking()
            .Include(ja => ja.Joint)
            .AsQueryable();

        if (jointId.HasValue)
        {
            query = query.Where(ja => ja.JointId == jointId.Value);
        }

        return await query
            .OrderBy(ja => ja.Joint.Name).ThenBy(ja => ja.ActionType)
            .Select(ja => new JointActionSummaryDto(
                ja.Id,
                ja.JointId,
                ja.Joint.Name,
                ja.ActionType,
                ja.ActionType.ToString(),
                ja.PlaneOfMotion,
                ja.Description))
            .ToListAsync(ct);
    }

    public async Task<JointActionDto> GetJointActionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var action = await _context.JointActionsDbSet
            .AsNoTracking()
            .Include(ja => ja.Joint)
            .Include(ja => ja.Muscles).ThenInclude(mja => mja.Muscle)
            .FirstOrDefaultAsync(ja => ja.Id == id, ct);

        if (action == null)
            throw new NotFoundException($"JointAction with ID '{id}' was not found.");

        return new JointActionDto(
            action.Id,
            action.JointId,
            action.Joint.Name,
            action.ActionType,
            action.ActionType.ToString(),
            action.PlaneOfMotion,
            action.Description,
            action.Muscles
                .Where(m => m.IsPrimaryAction)
                .Select(m => new MuscleSummaryDto(
                    m.MuscleId,
                    m.Muscle.Name,
                    m.Muscle.CommonName,
                    m.Muscle.BodyPart,
                    m.IsPrimaryAction)).ToList()
        );
    }

    public async Task<MuscleAnatomyDto> GetMuscleAnatomyAsync(Guid muscleId, CancellationToken ct = default)
    {
        var muscle = await _context.MusclesDbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == muscleId, ct);

        if (muscle == null)
            throw new NotFoundException($"Muscle with ID '{muscleId}' was not found.");

        var muscleActions = await _context.MuscleJointActionsDbSet
            .AsNoTracking()
            .Include(mja => mja.JointAction).ThenInclude(ja => ja.Joint)
            .Where(mja => mja.MuscleId == muscleId)
            .ToListAsync(ct);

        var primaryActions = muscleActions
            .Where(mja => mja.IsPrimaryAction)
            .Select(mja => new JointActionSummaryDto(
                mja.JointAction.Id,
                mja.JointAction.JointId,
                mja.JointAction.Joint.Name,
                mja.JointAction.ActionType,
                mja.JointAction.ActionType.ToString(),
                mja.JointAction.PlaneOfMotion,
                mja.JointAction.Description))
            .ToList();

        var secondaryActions = muscleActions
            .Where(mja => !mja.IsPrimaryAction)
            .Select(mja => new JointActionSummaryDto(
                mja.JointAction.Id,
                mja.JointAction.JointId,
                mja.JointAction.Joint.Name,
                mja.JointAction.ActionType,
                mja.JointAction.ActionType.ToString(),
                mja.JointAction.PlaneOfMotion,
                mja.JointAction.Description))
            .ToList();

        return new MuscleAnatomyDto(
            muscle.Id,
            muscle.Name,
            muscle.CommonName,
            muscle.BodyPart,
            primaryActions,
            secondaryActions
        );
    }
}
