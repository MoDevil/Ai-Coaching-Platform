using System.Text.Json;
using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Application.Memory.Engine;

/// <summary>
/// Deterministic conflict detection engine for client memory.
/// ZERO AI, ZERO semantic similarity, ZERO embeddings.
/// Strictly applies locked deterministic rules on structured categories.
/// </summary>
public class ClientMemoryConflictDetector : IClientMemoryConflictDetector
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MemoryConflictDetectionResult DetectConflict(
        ClientMemoryRecord newRecord,
        IReadOnlyList<ClientMemoryRecord> activeRecords)
    {
        ArgumentNullException.ThrowIfNull(newRecord);
        if (activeRecords == null || activeRecords.Count == 0)
        {
            return new MemoryConflictDetectionResult(false, null, null);
        }

        if (newRecord.MemoryCategory == MemoryCategory.Preference || newRecord.MemoryCategory == MemoryCategory.Aversion)
        {
            return DetectPreferenceConflict(newRecord, activeRecords);
        }

        if (newRecord.MemoryCategory == MemoryCategory.PainObservation)
        {
            return DetectPainObservationConflict(newRecord, activeRecords);
        }

        // All other categories use manual conflict creation only.
        return new MemoryConflictDetectionResult(false, null, null);
    }

    private static MemoryConflictDetectionResult DetectPreferenceConflict(
        ClientMemoryRecord newRecord,
        IReadOnlyList<ClientMemoryRecord> activeRecords)
    {
        var newPreference = TryDeserializePreference(newRecord.Content);
        if (newPreference == null || string.IsNullOrWhiteSpace(newPreference.SubjectId))
        {
            return new MemoryConflictDetectionResult(false, null, null);
        }

        foreach (var existing in activeRecords)
        {
            if (existing.Id == newRecord.Id ||
                (existing.MemoryCategory != MemoryCategory.Preference && existing.MemoryCategory != MemoryCategory.Aversion) ||
                existing.RecordStatus != MemoryRecordStatus.Active ||
                existing.IsConflicted ||
                existing.IsAnonymized)
            {
                continue;
            }

            var existingPreference = TryDeserializePreference(existing.Content);
            if (existingPreference == null || string.IsNullOrWhiteSpace(existingPreference.SubjectId))
            {
                continue;
            }

            // Same SubjectType + SubjectId
            if (newPreference.SubjectType == existingPreference.SubjectType &&
                string.Equals(newPreference.SubjectId, existingPreference.SubjectId, StringComparison.OrdinalIgnoreCase))
            {
                if (newPreference.Sentiment != existingPreference.Sentiment)
                {
                    string desc = $"Contradictory sentiment detected for {newPreference.SubjectType} '{newPreference.SubjectLabel}' (Id: {newPreference.SubjectId}): existing sentiment is {existingPreference.Sentiment}, new sentiment is {newPreference.Sentiment}.";
                    return new MemoryConflictDetectionResult(true, existing, desc);
                }
                // Same sentiment -> duplicate, no conflict
            }
        }

        return new MemoryConflictDetectionResult(false, null, null);
    }

    private static MemoryConflictDetectionResult DetectPainObservationConflict(
        ClientMemoryRecord newRecord,
        IReadOnlyList<ClientMemoryRecord> activeRecords)
    {
        var newPain = TryDeserializePain(newRecord.Content);
        if (newPain == null || string.IsNullOrWhiteSpace(newPain.AnatomicalRegionKey))
        {
            return new MemoryConflictDetectionResult(false, null, null);
        }

        var newEffectiveDate = newRecord.ObservedAt ?? newRecord.RecordedAt;

        foreach (var existing in activeRecords)
        {
            if (existing.Id == newRecord.Id ||
                existing.MemoryCategory != MemoryCategory.PainObservation ||
                existing.RecordStatus != MemoryRecordStatus.Active ||
                existing.IsConflicted ||
                existing.IsAnonymized)
            {
                continue;
            }

            var existingPain = TryDeserializePain(existing.Content);
            if (existingPain == null || string.IsNullOrWhiteSpace(existingPain.AnatomicalRegionKey))
            {
                continue;
            }

            // Same AnatomicalRegionKey
            if (string.Equals(newPain.AnatomicalRegionKey, existingPain.AnatomicalRegionKey, StringComparison.OrdinalIgnoreCase))
            {
                var existingEffectiveDate = existing.ObservedAt ?? existing.RecordedAt;
                var daysDiff = Math.Abs((newEffectiveDate - existingEffectiveDate).TotalDays);

                if (daysDiff <= 30.0)
                {
                    string desc = $"Pain observation proximity flag: multiple active pain observations recorded for region '{newPain.AnatomicalRegion}' (Key: {newPain.AnatomicalRegionKey}) within {daysDiff:F0} days. Requires coach review.";
                    return new MemoryConflictDetectionResult(true, existing, desc);
                }
            }
        }

        return new MemoryConflictDetectionResult(false, null, null);
    }

    private static PreferenceContent? TryDeserializePreference(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<PreferenceContent>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static PainObservationContent? TryDeserializePain(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<PainObservationContent>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
