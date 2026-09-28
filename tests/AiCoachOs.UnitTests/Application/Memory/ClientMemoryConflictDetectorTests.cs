using System.Text.Json;
using AiCoachOs.Application.Memory.Engine;
using AiCoachOs.Domain.Memory;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application.Memory;

public class ClientMemoryConflictDetectorTests
{
    private readonly ClientMemoryConflictDetector _detector = new();

    [Fact]
    public void DetectConflict_Preference_SameSubjectSameSentiment_ReturnsNoConflict()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var existingPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-bench-press",
            subjectLabel: "Barbell Bench Press",
            sentiment: PreferenceSentiment.Prefers);

        var existingRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Preference,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(existingPayload));

        var newPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-bench-press",
            subjectLabel: "Barbell Bench Press",
            sentiment: PreferenceSentiment.Prefers);

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Preference,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(newPayload));

        var result = _detector.DetectConflict(newRecord, new[] { existingRecord });

        result.HasConflict.Should().BeFalse();
        result.ConflictedWithRecord.Should().BeNull();
    }

    [Fact]
    public void DetectConflict_Preference_SameSubjectContradictorySentiment_ReturnsConflict()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var existingPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-deadlift",
            subjectLabel: "Conventional Deadlift",
            sentiment: PreferenceSentiment.Prefers);

        var existingRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Preference,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(existingPayload));

        var newPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-deadlift",
            subjectLabel: "Conventional Deadlift",
            sentiment: PreferenceSentiment.Avoids);

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Aversion,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(newPayload));

        var result = _detector.DetectConflict(newRecord, new[] { existingRecord });

        result.HasConflict.Should().BeTrue();
        result.ConflictedWithRecord.Should().Be(existingRecord);
        result.ConflictDescription.Should().Contain("Contradictory sentiment");
        result.ConflictDescription.Should().Contain("Conventional Deadlift");
    }

    [Fact]
    public void DetectConflict_Preference_DifferentSubject_ReturnsNoConflict()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var existingPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-squat",
            subjectLabel: "Barbell Back Squat",
            sentiment: PreferenceSentiment.Prefers);

        var existingRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Preference,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(existingPayload));

        var newPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-leg-press",
            subjectLabel: "Leg Press",
            sentiment: PreferenceSentiment.Avoids);

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Aversion,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(newPayload));

        var result = _detector.DetectConflict(newRecord, new[] { existingRecord });

        result.HasConflict.Should().BeFalse();
    }

    [Fact]
    public void DetectConflict_PainObservation_SameRegionWithin30Days_ReturnsProximityConflict()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var existingPayload = new PainObservationContent(
            anatomicalRegion: "Lumbar Spine",
            anatomicalRegionKey: "lumbar_spine",
            painCharacter: "Aching",
            severity: 4);

        var existingRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.PainObservation,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(existingPayload),
            observedAt: now.AddDays(-10),
            recordedAt: now.AddDays(-10));

        var newPayload = new PainObservationContent(
            anatomicalRegion: "Lower Back",
            anatomicalRegionKey: "lumbar_spine",
            painCharacter: "Sharp twinge on flexion",
            severity: 6);

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.PainObservation,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(newPayload),
            observedAt: now,
            recordedAt: now);

        var result = _detector.DetectConflict(newRecord, new[] { existingRecord });

        result.HasConflict.Should().BeTrue();
        result.ConflictedWithRecord.Should().Be(existingRecord);
        result.ConflictDescription.Should().Contain("Pain observation proximity flag");
        result.ConflictDescription.Should().Contain("lumbar_spine");
    }

    [Fact]
    public void DetectConflict_PainObservation_SameRegionOutside30Days_ReturnsNoConflict()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var existingPayload = new PainObservationContent(
            anatomicalRegion: "Right Shoulder",
            anatomicalRegionKey: "shoulder_right",
            severity: 5);

        var existingRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.PainObservation,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(existingPayload),
            observedAt: now.AddDays(-45),
            recordedAt: now.AddDays(-45));

        var newPayload = new PainObservationContent(
            anatomicalRegion: "Right Shoulder",
            anatomicalRegionKey: "shoulder_right",
            severity: 2);

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.PainObservation,
            sourceType: MemorySourceType.CoachRecorded,
            content: JsonSerializer.Serialize(newPayload),
            observedAt: now,
            recordedAt: now);

        var result = _detector.DetectConflict(newRecord, new[] { existingRecord });

        result.HasConflict.Should().BeFalse();
    }

    [Fact]
    public void DetectConflict_NonAutomaticCategory_ReturnsNoConflict()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var existingRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.ScheduleConstraint,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Available MWF");

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.ScheduleConstraint,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Available TTS");

        var result = _detector.DetectConflict(newRecord, new[] { existingRecord });

        result.HasConflict.Should().BeFalse();
    }
}
