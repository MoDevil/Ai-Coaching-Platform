using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Application.Memory.Engine;

public record MemoryConflictDetectionResult(
    bool HasConflict,
    ClientMemoryRecord? ConflictedWithRecord,
    string? ConflictDescription);

public interface IClientMemoryConflictDetector
{
    MemoryConflictDetectionResult DetectConflict(
        ClientMemoryRecord newRecord,
        IReadOnlyList<ClientMemoryRecord> activeRecords);
}
