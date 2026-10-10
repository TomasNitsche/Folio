using Folio.Domain.Enums;

namespace Folio.Domain.Entities;

public class EntityPackage
{
    public Guid Id { get; set; }
    public Guid PeriodEntityId { get; set; }
    public int Version { get; set; }
    public bool IsFinal { get; set; }
    public PackageStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public PeriodEntity? PeriodEntity { get; set; }
    public ICollection<StoredDocument> Documents { get; set; } = [];
}
