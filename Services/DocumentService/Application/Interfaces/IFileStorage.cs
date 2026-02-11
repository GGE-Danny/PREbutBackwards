namespace DocumentService.Application.Interfaces;

public interface IFileStorage
{
    Task<(string storagePath, string checksumSha256, long sizeBytes)> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct);

    Task DeleteAsync(string storagePath, CancellationToken ct);
}
