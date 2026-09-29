using System.Text;
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

        var photo = new ClientPhoto(
            id: photoId,
            clientId: clientId,
            coachId: coachId,
            photoSetType: PhotoSetType.FrontRelaxed,
            storageKey: "uuid-photo-key.dat",
            mimeType: "image/jpeg",
            fileSizeBytes: 2048,
            notes: "Front relaxed posture");

        photo.Id.Should().Be(photoId);
        photo.ClientId.Should().Be(clientId);
        photo.CoachId.Should().Be(coachId);
        photo.PhotoSetType.Should().Be(PhotoSetType.FrontRelaxed);
        photo.StorageKey.Should().Be("uuid-photo-key.dat");
        photo.MimeType.Should().Be("image/jpeg");
        photo.FileSizeBytes.Should().Be(2048);
        photo.UploadedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        photo.Notes.Should().Be("Front relaxed posture");
        photo.IsAnonymized.Should().BeFalse();
        photo.ObservationRecordId.Should().BeNull();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void ClientPhoto_SupportedMimeTypes_Accepted(string mime)
    {
        var photo = new ClientPhoto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PhotoSetType.FrontRelaxed,
            "storage-key.dat",
            mime,
            1024);

        photo.MimeType.Should().Be(mime);
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
            PhotoSetType.FrontRelaxed,
            "storage-key.dat",
            mime,
            1024);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported image MIME type*");
    }

    [Fact]
    public void ClientPhoto_FileSizeBytesExceeds10MB_ThrowsArgumentException()
    {
        var tenMbPlusOne = 10 * 1024 * 1024 + 1;
        var act = () => new ClientPhoto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PhotoSetType.FrontRelaxed,
            "storage-key.dat",
            "image/jpeg",
            tenMbPlusOne);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*exceeds the maximum limit*");
    }

    [Fact]
    public void ClientPhoto_Anonymization_ClearsStorageKeyAndMarksAnonymized()
    {
        var photo = new ClientPhoto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PhotoSetType.FrontRelaxed,
            "secret-key.dat",
            "image/png",
            1024,
            notes: "Private client note");

        photo.MarkAnonymized();

        photo.IsAnonymized.Should().BeTrue();
        photo.AnonymizedAt.Should().NotBeNull();
        photo.StorageKey.Should().Be("ANONYMIZED");
        photo.Notes.Should().BeNull();
    }

    [Fact]
    public void PhotoVisionSafetySanitizer_AllForbiddenPatterns_ReplacedWithExactBoundaryToken()
    {
        var input = new PhysiqueObservationResult
        {
            GeneralObservations = "You have 15.5% body fat and this indicates superior conditioning with 100% certainty.",
            ApparentSymmetryNotes = "This is consistent with scoliosis in lumbar region.",
            PostureObservations = "We diagnose lordosis and clearly observable inflammation.",
            MuscularDevelopmentNotes = "Estimating approximately 42% skeletal muscle mass.",
            ComparisonNotes = "Definitely shows muscular hypertrophy disease with tendonitis.",
            LimitationsStatement = "" // Empty to test default injection
        };

        var sanitized = PhotoVisionSafetySanitizer.Sanitize(input);

        // Verify exact token replacement
        sanitized.GeneralObservations.Should().NotContain("15.5% body fat");
        sanitized.GeneralObservations.Should().NotContain("You have");
        sanitized.GeneralObservations.Should().NotContain("this indicates");
        sanitized.GeneralObservations.Should().NotContain("100%");
        sanitized.GeneralObservations.Should().Contain(PhotoVisionSafetySanitizer.RemovalSentinel);

        sanitized.ApparentSymmetryNotes.Should().NotContain("this is consistent with");
        sanitized.ApparentSymmetryNotes.Should().NotContain("scoliosis");
        sanitized.ApparentSymmetryNotes.Should().Contain(PhotoVisionSafetySanitizer.RemovalSentinel);

        sanitized.PostureObservations.Should().NotContain("diagnose");
        sanitized.PostureObservations.Should().NotContain("lordosis");
        sanitized.PostureObservations.Should().NotContain("inflammation");
        sanitized.PostureObservations.Should().Contain(PhotoVisionSafetySanitizer.RemovalSentinel);

        sanitized.MuscularDevelopmentNotes.Should().NotContain("42% skeletal muscle mass");
        sanitized.MuscularDevelopmentNotes.Should().Contain(PhotoVisionSafetySanitizer.RemovalSentinel);

        sanitized.ComparisonNotes.Should().NotContain("Definitely");
        sanitized.ComparisonNotes.Should().NotContain("tendonitis");
        sanitized.ComparisonNotes.Should().Contain(PhotoVisionSafetySanitizer.RemovalSentinel);

        sanitized.CoachActionRequired.Should().BeTrue();
        sanitized.LimitationsStatement.Should().NotBeEmpty();
        sanitized.ConfidenceStatement.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LocalStorageService_StripsExifMetadataFromJpeg()
    {
        // Build a JPEG byte stream containing an EXIF APP1 marker (0xFF, 0xE1)
        using var ms = new MemoryStream();
        ms.Write(new byte[] { 0xFF, 0xD8 }); // SOI
        // APP1 marker with dummy EXIF payload (length 8: 2 length bytes + 6 payload bytes)
        ms.Write(new byte[] { 0xFF, 0xE1, 0x00, 0x08, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00 });
        // SOS marker and image data
        ms.Write(new byte[] { 0xFF, 0xDA, 0x00, 0x02, 0x00, 0x00, 0xFF, 0xD9 }); // SOS + image data + EOI

        var rawJpegWithExif = ms.ToArray();

        var configMock = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var loggerMock = Microsoft.Extensions.Logging.Abstractions.NullLogger<LocalStorageService>.Instance;
        var storage = new LocalStorageService(configMock, loggerMock);

        var key = await storage.UploadPhotoAsync(rawJpegWithExif, "image/jpeg");
        var downloadedBytes = await storage.DownloadPhotoAsync(key);

        // Verify the APP1 (0xFF, 0xE1) marker was stripped out
        var hasApp1Marker = false;
        for (int i = 0; i < downloadedBytes.Length - 1; i++)
        {
            if (downloadedBytes[i] == 0xFF && downloadedBytes[i + 1] == 0xE1)
            {
                hasApp1Marker = true;
                break;
            }
        }

        hasApp1Marker.Should().BeFalse();

        // Cleanup
        await storage.DeletePhotoAsync(key);
    }
}
