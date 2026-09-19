using AiCoachOs.Application.Programs.DTOs;

namespace AiCoachOs.Application.Programs.Services;

public interface IProgramService
{
    Task<ProgramDto> GenerateProgramAsync(Guid coachId, GenerateProgramRequestDto request, CancellationToken cancellationToken = default);
    Task<ProgramDto?> GetProgramByIdAsync(Guid coachId, Guid programId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProgramSummaryDto>> GetProgramsByClientIdAsync(Guid coachId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ProgramDto?> GetActiveProgramForClientAsync(Guid coachId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ProgramDto> UpdateProgramStatusAsync(Guid coachId, Guid programId, UpdateProgramStatusDto request, CancellationToken cancellationToken = default);
}
