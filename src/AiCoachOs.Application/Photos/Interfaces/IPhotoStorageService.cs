namespace AiCoachOs.Application.Photos.Interfaces;

public interface IPhotoStorageService
{
    Task<string> UploadPhotoAsync(byte[] imageBytes, string mimeType, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadPhotoAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<string> GenerateSignedUrlAsync(string storageKey, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task DeletePhotoAsync(string storageKey, CancellationToken cancellationToken = default);
}
