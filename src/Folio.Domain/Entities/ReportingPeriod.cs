using Folio.Domain.Enums;

namespace Folio.Domain.Entities;

public class ReportingPeriod
{
    public Guid Id { get; set; }
    public ConfirmationTypeKind ConfirmationType { get; set; }
    public required string Code { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public PeriodStatus Status { get; set; }
    /// <summary>The period the reporting team currently wants confirmed. At most one per confirmation type.</summary>
    public bool IsDefault { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public required string OpenedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public ICollection<PeriodEntity> Entities { get; set; } = [];
}
