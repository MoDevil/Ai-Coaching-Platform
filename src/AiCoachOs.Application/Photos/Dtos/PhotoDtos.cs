using System.Text.Json.Serialization;
using AiCoachOs.Domain.Photos;

namespace AiCoachOs.Application.Photos.Dtos;

public class UploadPhotoRequestDto
{
    [JsonPropertyName("fileBytes")]
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("photoSetType")]
    public PhotoSetType PhotoSetType { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class ClientPhotoDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("coachId")]
    public Guid CoachId { get; set; }

    [JsonPropertyName("photoSetType")]
    public PhotoSetType PhotoSetType { get; set; }

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("uploadedAt")]
    public DateTimeOffset UploadedAt { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("observationRecordId")]
    public Guid? ObservationRecordId { get; set; }

    [JsonPropertyName("isAnonymized")]
    public bool IsAnonymized { get; set; }

    [JsonPropertyName("anonymizedAt")]
    public DateTimeOffset? AnonymizedAt { get; set; }
}

public class ClientPhotoSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("photoSetType")]
    public PhotoSetType PhotoSetType { get; set; }

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("uploadedAt")]
    public DateTimeOffset UploadedAt { get; set; }

    [JsonPropertyName("hasObservation")]
    public bool HasObservation { get; set; }

    [JsonPropertyName("observationRecordId")]
    public Guid? ObservationRecordId { get; set; }

    [JsonPropertyName("isAnonymized")]
    public bool IsAnonymized { get; set; }
}

public class SignedPhotoUrlDto
{
    [JsonPropertyName("photoId")]
    public Guid PhotoId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("expiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
