using AiCoachOs.Application.Photos.Dtos;
using AiCoachOs.Domain.Photos;
using AiCoachOs.Infrastructure.Photos;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Photos;

public class PhotoDomainAndSanitizerTests
{
    [Fact]
    public void ClientPhoto_ValidArguments_CreatesEntitySuccessfully()
    {
        var photoId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var takenAt = DateTime.UtcNow.AddDays(-1);

        var photo = new ClientPhoto(
            id: photoId,
            clientId: clientId,
            coachId: coachId,
            photoSetType: PhotoSetType.Front,
            storageKey: "uuid-photo-key.dat",
            mimeType: "image/jpeg",
            fileSizeBytes: 2048,
            takenAt: takenAt,
            notes: "Front relaxed posture");

        photo.Id.Should().Be(photoId);
        photo.ClientId.Should().Be(clientId);
        photo.CoachId.Should().Be(coachId);
        photo.PhotoSetType.Should().Be(PhotoSetType.Front);
        photo.StorageKey.Should().Be("uuid-photo-key.dat");
        photo.MimeType.Should().Be("image/jpeg");
        photo.FileSizeBytes.Should().Be(2048);
        photo.TakenAt.Should().Be(takenAt);
        photo.Notes.Should().Be("Front relaxed posture");
        photo.IsAnonymized.Should().BeFalse();
        photo.ObservationRecordId.Should().BeNull();
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("video/mp4")]
    public void ClientPhoto_UnsupportedMimeType_ThrowsArgumentException(string mime)
    {
        var act = () => new ClientPhoto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PhotoSetType.Front,
            "storage-key",
            mime,
            1024,
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported image MIME type*");
    }

    [Fact]
    public void ClientPhoto_Anonymization_ClearsStorageKeyAndMarksAnonymized()
    {
        var photo = new ClientPhoto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PhotoSetType.Front,
            "secret-key.dat",
            "image/png",
            1024,
            DateTime.UtcNow,
            notes: "Private client note");

        photo.MarkAnonymized();

        photo.IsAnonymized.Should().BeTrue();
        photo.AnonymizedAt.Should().NotBeNull();
        photo.StorageKey.Should().Be("ANONYMIZED");
        photo.Notes.Should().BeNull();
    }

    [Fact]
    public void PhotoVisionSafetySanitizer_RedactsBodyFatAndMuscleMassPercentages()
    {
        var input = new PhysiqueObservationResult
        {
            GeneralObservations = "Client appears to be at 14.5% body fat with visible abdominal definition.",
            MuscularDevelopmentNotes = "Estimating approximately 42% skeletal muscle mass.",
            ConfidenceStatement = "Visual evaluation."
        };

        var sanitized = PhotoVisionSafetySanitizer.Sanitize(input);

        sanitized.GeneralObservations.Should().NotContain("14.5% body fat");
        sanitized.GeneralObservations.Should().Contain("[body composition definition]");

        sanitized.MuscularDevelopmentNotes.Should().NotContain("42% skeletal muscle mass");
        sanitized.MuscularDevelopmentNotes.Should().Contain("[muscular development]");
        sanitized.CoachActionRequired.Should().BeTrue();
    }

    [Fact]
    public void PhotoVisionSafetySanitizer_NeutralizesMedicalDiagnosesAndCertaintyClaims()
    {
        var input = new PhysiqueObservationResult
        {
            PostureObservations = "We diagnose lordosis and scoliosis in sagittal alignment.",
            GeneralObservations = "This photo definitely proves superior conditioning.",
            LimitationsStatement = "" // Empty to test default injection
        };

        var sanitized = PhotoVisionSafetySanitizer.Sanitize(input);

        sanitized.PostureObservations.Should().NotContain("diagnose");
        sanitized.PostureObservations.Should().NotContain("scoliosis");
        sanitized.GeneralObservations.Should().NotContain("definitely proves");
        sanitized.GeneralObservations.Should().Contain("observations suggest");
        sanitized.LimitationsStatement.Should().NotBeEmpty();
        sanitized.LimitationsStatement.Should().Contain("do not constitute diagnostic");
    }
}
