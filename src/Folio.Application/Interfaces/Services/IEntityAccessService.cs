using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Interfaces.Services;

public interface IEntityAccessService
{
    IReadOnlyList<PeriodEntity> FilterEntitiesForUser(
        IReadOnlyList<PeriodEntity> entities,
        Guid userId,
        UserRole role);

    Task EnsureCanAccessEntityAsync(
        Guid periodEntityId,
        EntityAccessAction action,
        CancellationToken ct = default);

    Task EnsureRoleCapabilityAsync(EntityAccessAction action, CancellationToken ct = default);

    Task EnsureCanAccessAdminAsync(CancellationToken ct = default);

    bool IsAssignedConfirmer(PeriodEntity entity, Guid userId);

    bool IsAssignedPackFiller(PeriodEntity entity, Guid userId);
}
