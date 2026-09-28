using AiCoachOs.Domain.Memory;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain.Memory;

public class ClientMemoryDomainTests
{
    [Fact]
    public void ClientMemoryRecord_WhenValid_SetsProvenanceAndInitialActiveState()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var recordedAt = DateTime.UtcNow;
        var observedAt = recordedAt.AddDays(-2);

        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.Preference,
            sourceType: MemorySourceType.CoachRecorded,
            content: "{\"subjectType\":1,\"subjectId\":\"bench_press\",\"sentiment\":1}",
            observedAt: observedAt,
            sourceReference: "Session #4",
            sourceDescription: "Observed during chest day",
            recordedAt: recordedAt);

        record.ClientId.Should().Be(clientId);
        record.CoachId.Should().Be(coachId);
        record.MemoryCategory.Should().Be(MemoryCategory.Preference);
        record.SourceType.Should().Be(MemorySourceType.CoachRecorded);
        record.RecordStatus.Should().Be(MemoryRecordStatus.Active);
        record.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Provisional);
        record.RecordedAt.Should().Be(recordedAt);
        record.ObservedAt.Should().Be(observedAt);
        record.IsConflicted.Should().BeFalse();
        record.IsAnonymized.Should().BeFalse();
    }

    [Fact]
    public void ClientMemoryRecord_ObservedAtInFutureOfRecordedAt_ThrowsArgumentException()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var recordedAt = DateTime.UtcNow;
        var futureObserved = recordedAt.AddDays(1);

        var act = () => new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.GoalContext,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Future goal",
            observedAt: futureObserved,
            recordedAt: recordedAt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ObservedAt cannot be in the future relative to RecordedAt*");
    }

    [Theory]
    [InlineData(MemorySourceType.SystemGenerated, MemoryConfidenceLevel.Confirmed)]
    [InlineData(MemorySourceType.CoachRecorded, MemoryConfidenceLevel.Provisional)]
    [InlineData(MemorySourceType.CoachCorrected, MemoryConfidenceLevel.Confirmed)]
    [InlineData(MemorySourceType.AIGenerated, MemoryConfidenceLevel.Provisional)]
    public void ClientMemoryRecord_CreationDefaults_MatchSourceTypeRules(MemorySourceType sourceType, MemoryConfidenceLevel expectedConfidence)
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.GeneralNote,
            sourceType: sourceType,
            content: "Test note");

        record.RecordStatus.Should().Be(MemoryRecordStatus.Active);
        record.ConfidenceLevel.Should().Be(expectedConfidence);
    }

    [Theory]
    [InlineData(MemoryConfidenceLevel.Uncertain)]
    [InlineData(MemoryConfidenceLevel.Conflicted)]
    [InlineData(MemoryConfidenceLevel.Superseded)]
    public void ClientMemoryRecord_DisallowedExplicitInitialConfidence_ThrowsInvalidOperationException(MemoryConfidenceLevel confidence)
    {
        var act = () => new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.GeneralNote,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Test note",
            explicitConfidence: confidence);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Cannot initialize a memory record with confidence {confidence}*");
    }

    [Fact]
    public void FlagUncertain_WhenActive_SetsConfidenceToUncertain()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.NutritionHabit,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Eats 3 meals daily");

        record.FlagUncertain();

        record.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Uncertain);
        record.RecordStatus.Should().Be(MemoryRecordStatus.Active);
    }

    [Fact]
    public void FlagUncertain_WhenNotActive_ThrowsInvalidOperationException()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.NutritionHabit,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Eats 3 meals daily");

        record.Archive();

        var act = () => record.FlagUncertain();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Active records can be flagged*");
    }

    [Fact]
    public void Supersede_WhenActive_UpdatesStatusAndPointsToNewRecord()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.ScheduleConstraint,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Available MWF mornings");

        var newRecordId = Guid.NewGuid();
        record.Supersede(newRecordId, "Client schedule shifted to evenings");

        record.RecordStatus.Should().Be(MemoryRecordStatus.Superseded);
        record.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Superseded);
        record.SupersededById.Should().Be(newRecordId);
        record.SupersededAt.Should().NotBeNull();
        record.SupersessionReason.Should().Be("Client schedule shifted to evenings");
    }

    [Fact]
    public void Archive_WhenActiveAndUnconflicted_SetsStatusArchived()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.LifeEvent,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Trip to Luxor");

        record.Archive();

        record.RecordStatus.Should().Be(MemoryRecordStatus.Archived);
        record.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Provisional); // Confidence unchanged
    }

    [Fact]
    public void Archive_WhenConflicted_ThrowsInvalidOperationException()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.Preference,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Squats");

        record.FlagConflicted("Conflicted with aversion");

        var act = () => record.Archive();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot archive a conflicted record*");
    }

    [Fact]
    public void Anonymize_CleansPersonalContentWhilePreservingAuditShellAndClientId()
    {
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.PainObservation,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Sharp pain in right shoulder at 90 deg abduction",
            sourceDescription: "Reported during overhead press",
            sourceReference: "CheckIn #12");

        record.Anonymize();

        record.RecordStatus.Should().Be(MemoryRecordStatus.Anonymized);
        record.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Confirmed);
        record.IsAnonymized.Should().BeTrue();
        record.AnonymizedAt.Should().NotBeNull();

        // Content cleaned
        record.Content.Should().Be(ClientMemoryRecord.AnonymizedContentSentinel);
        record.SourceDescription.Should().Be(ClientMemoryRecord.AnonymizedContentSentinel);
        record.SourceReference.Should().BeNull();
        record.ObservedAt.Should().BeNull();

        // Audit shell intact
        record.ClientId.Should().Be(clientId);
        record.CoachId.Should().Be(coachId);
        record.MemoryCategory.Should().Be(MemoryCategory.PainObservation);
    }

    [Fact]
    public void Anonymize_WhenAlreadyAnonymized_ThrowsInvalidOperationException()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.GeneralNote,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Confidential intake note");

        record.Anonymize();

        var act = () => record.Anonymize();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Record is already anonymized*");
    }

    [Fact]
    public void AnonymizedRecord_IsImmutableAgainstTransitions()
    {
        var record = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            memoryCategory: MemoryCategory.GeneralNote,
            sourceType: MemorySourceType.CoachRecorded,
            content: "Note");

        record.Anonymize();

        var actUncertain = () => record.FlagUncertain();
        actUncertain.Should().Throw<InvalidOperationException>();

        var actArchive = () => record.Archive();
        actArchive.Should().Throw<InvalidOperationException>();

        var actSupersede = () => record.Supersede(Guid.NewGuid(), "New reason");
        actSupersede.Should().Throw<InvalidOperationException>();
    }
}
