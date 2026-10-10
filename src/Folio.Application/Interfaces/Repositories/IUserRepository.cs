using Folio.Domain.Entities;

namespace Folio.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<AppUser>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AppUser>> GetAllForAdminAsync(CancellationToken ct = default);
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddIfNotExistsAsync(AppUser user, CancellationToken ct = default);
    Task CreateAsync(AppUser user, CancellationToken ct = default);
    Task UpdateAsync(AppUser user, CancellationToken ct = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
}

public interface IAssignmentRepository
{
    Task<IReadOnlyList<EntityAssignment>> GetByPeriodEntityIdAsync(Guid periodEntityId, CancellationToken ct = default);
    Task AddAsync(EntityAssignment assignment, CancellationToken ct = default);
    Task ReplaceRoleAssignmentsAsync(
        Guid periodEntityId,
        Guid? confirmerUserId,
        Guid? packFillerUserId,
        CancellationToken ct = default);
}

public interface IDevCompanyRepository
{
    Task MergeAsync(int registryEntityId, string name, string code, DateOnly validFrom, DateOnly? validTo = null, CancellationToken ct = default);
}
