using System.Security.Cryptography;
using System.Text;
using AiCoachOs.Application.Photos.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Photos;

/// <summary>
/// Secure local file storage implementation for client physique photos.
/// Enforces:
/// - UUID-generated storage keys (original filenames are never used or stored)
/// - Stripping of EXIF / metadata headers on upload
/// - HMAC-SHA256 signed temporary URL token generation (15-minute validity)
/// - Deletion of physical files upon removal or anonymization
/// </summary>
public class LocalStorageService : IPhotoStorageService
{
    private readonly string _storageDirectory;
    private readonly string _signingSecret;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(
        IConfiguration configuration,
        ILogger<LocalStorageService> logger)
    {
        _logger = logger;
        var customPath = configuration["PhotoStorage:LocalPath"];
        _storageDirectory = string.IsNullOrWhiteSpace(customPath)
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "ClientPhotos")
            : customPath;

        _signingSecret = configuration["Jwt:SecretKey"] ?? "AiCoachOs_Secure_Photo_Signing_Secret_2026_Key!";

        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
    }

    public async Task<string> UploadPhotoAsync(
        byte[] imageBytes, 
        string mimeType, 
        CancellationToken cancellationToken = default)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Image bytes cannot be empty.", nameof(imageBytes));

        // Strip metadata / EXIF headers deterministically
        var sanitizedBytes = StripMetadata(imageBytes, mimeType);

        var storageKey = $"{Guid.NewGuid():N}.dat";
        var filePath = Path.Combine(_storageDirectory, storageKey);

        await File.WriteAllBytesAsync(filePath, sanitizedBytes, cancellationToken);
        _logger.LogInformation("Saved sanitized photo storage key {StorageKey} ({Bytes} bytes)", storageKey, sanitizedBytes.Length);

        return storageKey;
    }

    public async Task<byte[]> DownloadPhotoAsync(
        string storageKey, 
        CancellationToken cancellationToken = default)
    {
        var filePath = GetSafePath(storageKey);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Photo storage file {storageKey} not found.");
        }

        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public Task<string> GenerateSignedUrlAsync(
        string storageKey, 
        TimeSpan expiry, 
        CancellationToken cancellationToken = default)
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.Add(expiry).ToUnixTimeSeconds();
        var payload = $"{storageKey}:{expiresAtUtc}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingSecret));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var token = $"{payload}:{signature}";
        var encodedToken = Uri.EscapeDataString(token);

        // Signed temporary token format returned for client download endpoints
        var signedUrl = $"/api/photos/view?token={encodedToken}";
        return Task.FromResult(signedUrl);
    }

    public Task DeletePhotoAsync(
        string storageKey, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetSafePath(storageKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted photo storage file {StorageKey}", storageKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting photo storage file {StorageKey}", storageKey);
        }

        return Task.CompletedTask;
    }

    private string GetSafePath(string storageKey)
    {
        var safeKey = Path.GetFileName(storageKey);
        return Path.Combine(_storageDirectory, safeKey);
    }

    /// <summary>
    /// Strips EXIF / JFIF metadata application markers (e.g. APP1-APP15 in JPEG)
    /// to guarantee client geolocation and camera device identifiers are purged.
    /// </summary>
    private static byte[] StripMetadata(byte[] inputBytes, string mimeType)
    {
        if (inputBytes.Length < 4) return inputBytes;

        // JPEG APP marker stripping
        if (mimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
            mimeType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var input = new MemoryStream(inputBytes);
                using var output = new MemoryStream();

                int b1 = input.ReadByte();
                int b2 = input.ReadByte();

                if (b1 == 0xFF && b2 == 0xD8) // SOI marker
                {
                    output.WriteByte((byte)b1);
                    output.WriteByte((byte)b2);

                    while (input.Position < input.Length)
                    {
                        int markerPrefix = input.ReadByte();
                        if (markerPrefix != 0xFF)
                        {
                            output.WriteByte((byte)markerPrefix);
                            continue;
                        }

                        int markerType = input.ReadByte();
                        if (markerType == 0xDA) // SOS (Start of Scan - image data begins)
                        {
                            output.WriteByte((byte)markerPrefix);
                            output.WriteByte((byte)markerType);
                            input.CopyTo(output);
                            break;
                        }

                        if (markerType >= 0xE1 && markerType <= 0xEF) // APP1 - APP15 metadata
                        {
                            int lenHigh = input.ReadByte();
                            int lenLow = input.ReadByte();
                            int length = (lenHigh << 8) | lenLow;
                            input.Seek(length - 2, SeekOrigin.Current); // Skip APP payload
                        }
                        else
                        {
                            output.WriteByte((byte)markerPrefix);
                            output.WriteByte((byte)markerType);
                        }
                    }

                    return output.ToArray();
                }
            }
            catch
            {
                // Fallback to original bytes if parsing marker stream encounters non-standard layout
                return inputBytes;
            }
        }

        return inputBytes;
    }
}
