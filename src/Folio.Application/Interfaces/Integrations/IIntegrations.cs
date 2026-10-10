using Folio.Application.Mappings;
using Folio.Domain.Enums;

namespace Folio.Application.Interfaces.Integrations;

public interface IRegistryEntityClient
{
    Task<IReadOnlyList<RegistryEntitySnapshot>> GetEntitiesValidOnAsync(DateOnly date, CancellationToken ct = default);
}

public interface IFileStorage
{
    string RootPath { get; }
    Task EnsurePeriodFolderAsync(string periodCode, CancellationToken ct = default);
    Task SaveAsync(string periodCode, string entityCode, DocumentKind kind, string fileName, Stream content, CancellationToken ct = default);
    Task<string> RenameFileAsync(string storagePath, string newFileName, CancellationToken ct = default);
    Task<(Stream Content, string ContentType)?> GetAsync(string periodCode, string entityCode, DocumentKind kind, string fileName, CancellationToken ct = default);
    Task<(Stream Content, string ContentType, string FileName)?> GetByStoragePathAsync(string storagePath, CancellationToken ct = default);
    IEnumerable<string> ListFiles(string periodCode, string entityCode, DocumentKind kind);
}
