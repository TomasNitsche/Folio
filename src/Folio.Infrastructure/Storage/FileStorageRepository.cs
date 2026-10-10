using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Folio.Application.Interfaces.Integrations;
using Folio.Domain.Enums;

namespace Folio.Infrastructure.Storage;

public sealed class FileStorageRepository : IFileStorage
{
    private readonly string _rootPath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public FileStorageRepository(IConfiguration configuration, FileExtensionContentTypeProvider contentTypeProvider)
    {
        _contentTypeProvider = contentTypeProvider;
        _rootPath = configuration["ReportFilesPath"] ?? Path.Combine(Path.GetTempPath(), "Folio");
        Directory.CreateDirectory(_rootPath);
    }

    public string RootPath => _rootPath;

    public Task EnsurePeriodFolderAsync(string periodCode, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.Combine(_rootPath, periodCode));
        return Task.CompletedTask;
    }

    public async Task SaveAsync(string periodCode, string entityCode, DocumentKind kind, string fileName, Stream content, CancellationToken ct = default)
    {
        var folder = Path.Combine(_rootPath, periodCode, entityCode, KindFolder(kind));
        Directory.CreateDirectory(folder);

        var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        var fullPath = Path.Combine(folder, safeName);

        content.Seek(0, SeekOrigin.Begin);
        await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(file, ct);
    }

    public Task<string> RenameFileAsync(string storagePath, string newFileName, CancellationToken ct = default)
    {
        var oldFullPath = Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(oldFullPath))
            throw new FileNotFoundException("File not found.", oldFullPath);

        var directory = Path.GetDirectoryName(oldFullPath)
            ?? throw new InvalidOperationException("Invalid storage path.");

        var safeName = string.Join("_", newFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        var newFullPath = Path.Combine(directory, safeName);

        if (File.Exists(newFullPath))
            throw new InvalidOperationException($"Target file already exists: {safeName}");

        File.Move(oldFullPath, newFullPath);
        var newStoragePath = Path.GetRelativePath(_rootPath, newFullPath).Replace(Path.DirectorySeparatorChar, '/');
        return Task.FromResult(newStoragePath);
    }

    public async Task<(Stream Content, string ContentType)?> GetAsync(string periodCode, string entityCode, DocumentKind kind, string fileName, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_rootPath, periodCode, entityCode, KindFolder(kind), fileName);
        if (!File.Exists(fullPath)) return null;

        if (!_contentTypeProvider.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        var memory = new MemoryStream();
        await using var file = File.OpenRead(fullPath);
        await file.CopyToAsync(memory, ct);
        memory.Seek(0, SeekOrigin.Begin);
        return (memory, contentType);
    }

    public async Task<(Stream Content, string ContentType, string FileName)?> GetByStoragePathAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath)) return null;

        if (!_contentTypeProvider.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        var memory = new MemoryStream();
        await using var file = File.OpenRead(fullPath);
        await file.CopyToAsync(memory, ct);
        memory.Seek(0, SeekOrigin.Begin);
        return (memory, contentType, Path.GetFileName(fullPath));
    }

    public IEnumerable<string> ListFiles(string periodCode, string entityCode, DocumentKind kind)
    {
        var folder = Path.Combine(_rootPath, periodCode, entityCode, KindFolder(kind));
        if (!Directory.Exists(folder)) yield break;

        foreach (var file in Directory.EnumerateFiles(folder))
            yield return Path.GetFileName(file);
    }

    private static string KindFolder(DocumentKind kind) => kind switch
    {
        DocumentKind.StaticReference => "static",
        DocumentKind.SignedConfirmation => "signed",
        DocumentKind.ExcelPackage => "packages",
        _ => "other"
    };
}
