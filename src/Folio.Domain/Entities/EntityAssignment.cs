namespace Folio.Domain.Entities;

public class EntityAssignment
{
    public Guid Id { get; set; }
    public Guid PeriodEntityId { get; set; }
    public Guid UserId { get; set; }
    public bool CanConfirm { get; set; }
    public bool CanFillPackage { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public PeriodEntity? PeriodEntity { get; set; }
}
