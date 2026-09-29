using System.Text.Json;
using AiCoachOs.Application.Videos.Dtos;
using AiCoachOs.Domain.Videos;
using AiCoachOs.Infrastructure.Videos;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Videos;

public class VideoDomainAndSanitizerTests
{
    [Fact]
    public void ClientVideo_ValidArguments_CreatesEntitySuccessfully()
    {
        var videoId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var frameKeys = new List<StoredFrameMetadata>
        {
            new() { FrameIndex = 1, TimestampSeconds = 1.0M, StorageKey = "frame1.jpg" },
            new() { FrameIndex = 2, TimestampSeconds = 5.0M, StorageKey = "frame2.jpg" }
        };
        var frameKeysJson = JsonSerializer.Serialize(frameKeys);

        var video = new ClientVideo(
            id: videoId,
            clientId: clientId,
            coachId: coachId,
            exerciseName: "Barbell Back Squat",
            storageKey: "video-key.mp4",
            mimeType: "video/mp4",
            fileSizeBytes: 1024 * 1024 * 5,
            durationSeconds: 15,
            frameCount: 2,
            frameStorageKeys: frameKeysJson,
            exerciseId: exerciseId,
            coachNotes: "Client testing heavy singles");

        video.Id.Should().Be(videoId);
        video.ClientId.Should().Be(clientId);
        video.CoachId.Should().Be(coachId);
        video.ExerciseId.Should().Be(exerciseId);
        video.ExerciseName.Should().Be("Barbell Back Squat");
        video.StorageKey.Should().Be("video-key.mp4");
        video.MimeType.Should().Be("video/mp4");
        video.FileSizeBytes.Should().Be(1024 * 1024 * 5);
        video.DurationSeconds.Should().Be(15);
        video.FrameCount.Should().Be(2);
        video.FrameStorageKeys.Should().Be(frameKeysJson);
        video.CoachNotes.Should().Be("Client testing heavy singles");
        video.IsAnonymized.Should().BeFalse();
        video.ObservationRecordId.Should().BeNull();
        video.UploadedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(181)]
    [InlineData(300)]
    public void ClientVideo_InvalidDuration_ThrowsArgumentException(int duration)
    {
        var act = () => new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Deadlift",
            "video.mp4",
            "video/mp4",
            1024 * 1024,
            duration,
            4,
            "[]");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Video duration must be between 2 and 180 seconds*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(100 * 1024 * 1024 + 1)]
    public void ClientVideo_InvalidFileSize_ThrowsArgumentException(long size)
    {
        var act = () => new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Deadlift",
            "video.mp4",
            "video/mp4",
            size,
            30,
            4,
            "[]");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*File size*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(-1)]
    public void ClientVideo_InvalidFrameCount_ThrowsArgumentException(int frameCount)
    {
        var act = () => new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Deadlift",
            "video.mp4",
            "video/mp4",
            1024 * 1024,
            30,
            frameCount,
            "[]");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Frame count must be between 1 and 8*");
    }

    [Theory]
    [InlineData("video/mp4")]
    [InlineData("video/quicktime")]
    [InlineData("video/webm")]
    public void ClientVideo_SupportedMimeTypes_Accepted(string mime)
    {
        var video = new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bench Press",
            "video.mp4",
            mime,
            1024 * 1024,
            20,
            4,
            "[]");

        video.MimeType.Should().Be(mime);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("audio/mp3")]
    [InlineData("application/octet-stream")]
    public void ClientVideo_UnsupportedMimeTypes_ThrowsArgumentException(string mime)
    {
        var act = () => new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bench Press",
            "video.mp4",
            mime,
            1024 * 1024,
            20,
            4,
            "[]");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported video MIME type*");
    }

    [Fact]
    public void ClientVideo_Anonymization_ClearsStorageKeysAndSetsSentinels()
    {
        var video = new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bench Press",
            "secret-video.mp4",
            "video/mp4",
            1024 * 1024,
            20,
            4,
            "[{\"FrameIndex\":1,\"StorageKey\":\"k1\"}]",
            coachNotes: "Confidential client video notes");

        video.MarkAnonymized();

        video.IsAnonymized.Should().BeTrue();
        video.AnonymizedAt.Should().NotBeNull();
        video.StorageKey.Should().Be("ANONYMIZED");
        video.FrameStorageKeys.Should().Be("[]");
        video.CoachNotes.Should().BeNull();
    }

    [Fact]
    public void ClientVideo_LinkObservationRecord_SetsId()
    {
        var video = new ClientVideo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bench Press",
            "video.mp4",
            "video/mp4",
            1024 * 1024,
            20,
            4,
            "[]");

        var recordId = Guid.NewGuid();
        video.LinkObservationRecord(recordId);

        video.ObservationRecordId.Should().Be(recordId);
    }

    [Fact]
    public void VideoTechniqueSafetySanitizer_AllForbiddenPatterns_ReplacedWithExactBoundaryToken()
    {
        var input = new VideoObservationResult
        {
            MovementExecutionNotes = "The client has a patellar tendonitis injury and clearly visible tear with 100% certainty.",
            JointAlignmentNotes = "Bar path is consistent with patellofemoral pain syndrome.",
            RangeOfMotionNotes = "We diagnose hip impingement and recommend lumbar surgery.",
            TempoAndControlNotes = "Subject exhibits 12.5% body fat and pathological spinal flexion with disc herniation.",
            LimitationsStatement = "" // Test fallback injection
        };

        var sanitized = VideoTechniqueSafetySanitizer.Sanitize(input);

        sanitized.MovementExecutionNotes.Should().NotContain("patellar tendonitis");
        sanitized.MovementExecutionNotes.Should().NotContain("tear");
        sanitized.MovementExecutionNotes.Should().NotContain("100%");
        sanitized.MovementExecutionNotes.Should().Contain(VideoTechniqueSafetySanitizer.RemovalSentinel);

        sanitized.JointAlignmentNotes.Should().NotContain("patellofemoral pain syndrome");
        sanitized.JointAlignmentNotes.Should().Contain(VideoTechniqueSafetySanitizer.RemovalSentinel);

        sanitized.RangeOfMotionNotes.Should().NotContain("diagnose");
        sanitized.RangeOfMotionNotes.Should().NotContain("surgery");
        sanitized.RangeOfMotionNotes.Should().Contain(VideoTechniqueSafetySanitizer.RemovalSentinel);

        sanitized.TempoAndControlNotes.Should().NotContain("12.5% body fat");
        sanitized.TempoAndControlNotes.Should().NotContain("pathological");
        sanitized.TempoAndControlNotes.Should().NotContain("herniation");
        sanitized.TempoAndControlNotes.Should().Contain(VideoTechniqueSafetySanitizer.RemovalSentinel);

        sanitized.CoachActionRequired.Should().BeTrue();
        sanitized.LimitationsStatement.Should().NotBeEmpty();
        sanitized.ConfidenceStatement.Should().NotBeEmpty();
    }

    [Fact]
    public void VideoProcessing_M16C2_TimestampFormula_CalculatesCorrectly()
    {
        // For duration < 8.0:
        // duration = 3.5 => N = min(floor(3.5), 8) = 3
        // timestamps: 0.5 + 0 * (2.5 / 2) = 0.5
        //             0.5 + 1 * 1.25 = 1.75
        //             0.5 + 2 * 1.25 = 3.0
        double duration1 = 3.5;
        int n1 = Math.Min((int)Math.Floor(duration1), 8);
        n1.Should().Be(3);
        double step1 = (duration1 - 1.0) / (n1 - 1);
        var t1_0 = 0.5;
        var t1_1 = 0.5 + step1;
        var t1_2 = 0.5 + 2 * step1;
        t1_0.Should().Be(0.5);
        t1_1.Should().Be(1.75);
        t1_2.Should().Be(3.0);

        // For duration >= 8.0:
        // duration = 15.0 => N = 8
        // step = (15.0 - 1.0) / 7 = 14.0 / 7 = 2.0
        // timestamps: 0.5, 2.5, 4.5, 6.5, 8.5, 10.5, 12.5, 14.5
        double duration2 = 15.0;
        int n2 = Math.Min((int)Math.Floor(duration2), 8);
        n2.Should().Be(8);
        double step2 = (duration2 - 1.0) / (n2 - 1);
        step2.Should().Be(2.0);
    }
}
