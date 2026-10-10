using Folio.Domain.Enums;

namespace Folio.Domain.Entities;

public class StoredDocument
{
    public Guid Id { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? PeriodEntityId { get; set; }
    public DocumentKind Kind { get; set; }
    public required string FileName { get; set; }
    public required string StoragePath { get; set; }
    public int? PackageVersion { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public EntityPackage? Package { get; set; }
}
