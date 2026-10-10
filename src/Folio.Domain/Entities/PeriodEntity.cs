namespace Folio.Domain.Entities;

public class PeriodEntity
{
    public Guid Id { get; set; }
    public Guid PeriodId { get; set; }
    public int RegistryEntityId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public ReportingPeriod? Period { get; set; }
    public ICollection<EntityPackage> Packages { get; set; } = [];
    public ICollection<EntityAssignment> Assignments { get; set; } = [];
}
