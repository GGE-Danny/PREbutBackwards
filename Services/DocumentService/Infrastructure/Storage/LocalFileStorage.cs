using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DocumentService.Application.Interfaces;

namespace DocumentService.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IConfiguration configuration, ILogger<LocalFileStorage> logger)
    {
        _rootPath = configuration["Storage:RootPath"] ?? @"C:\erp_uploads\documents";
        _logger = logger;

        // Ensure root directory exists
        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
            _logger.LogInformation("Created storage root directory: {RootPath}", _rootPath);
        }
    }

    public async Task<(string storagePath, string checksumSha256, long sizeBytes)> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct)
    {
        // Create date-based subdirectory
        var now = DateTime.UtcNow;
        var subDir = Path.Combine(_rootPath, now.Year.ToString(), now.Month.ToString("D2"));

        if (!Directory.Exists(subDir))
        {
            Directory.CreateDirectory(subDir);
        }

        // Generate safe filename
        var safeFileName = SanitizeFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var fullPath = Path.Combine(subDir, uniqueFileName);

        // Write file and compute checksum simultaneously
        long sizeBytes;
        string checksumSha256;

        using (var sha256 = SHA256.Create())
        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
        using (var cryptoStream = new CryptoStream(fileStream, sha256, CryptoStreamMode.Write))
        {
            await stream.CopyToAsync(cryptoStream, ct);
            await cryptoStream.FlushFinalBlockAsync(ct);

            sizeBytes = fileStream.Length;
            checksumSha256 = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
        }

        _logger.LogInformation(
            "Saved file {FileName} to {Path}, Size={Size}, Checksum={Checksum}",
            fileName, fullPath, sizeBytes, checksumSha256);

        return (fullPath, checksumSha256, sizeBytes);
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct)
    {
        if (!File.Exists(storagePath))
        {
            throw new FileNotFoundException($"File not found: {storagePath}");
        }

        var stream = new FileStream(storagePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
            _logger.LogInformation("Deleted file: {Path}", storagePath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var invalidRegex = new Regex($"[{invalidChars}]");
        var sanitized = invalidRegex.Replace(fileName, "_");

        // Limit length
        if (sanitized.Length > 100)
        {
            var ext = Path.GetExtension(sanitized);
            var name = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = name[..Math.Min(name.Length, 90)] + ext;
        }

        return sanitized;
    }
}
