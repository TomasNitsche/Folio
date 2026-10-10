using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Infrastructure.Persistence;

namespace Folio.Infrastructure.Persistence.InMemory;

public sealed class InMemoryUserRepository(InMemoryStore store) : IUserRepository
{
    public Task<IReadOnlyList<AppUser>> GetAllActiveAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AppUser>>(store.Users.Where(u => u.IsActive).OrderBy(u => u.DisplayName).ToList());

    public Task<IReadOnlyList<AppUser>> GetAllForAdminAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AppUser>>(store.Users.OrderByDescending(u => u.IsActive).ThenBy(u => u.DisplayName).ToList());

    public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(store.Users.FirstOrDefault(u => u.Id == id));

    public Task AddIfNotExistsAsync(AppUser user, CancellationToken ct = default)
    {
        if (store.Users.All(u => u.Id != user.Id))
            store.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task CreateAsync(AppUser user, CancellationToken ct = default)
    {
        if (store.Users.Any(u => u.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Username already exists.");
        if (store.Users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Email already exists.");

        store.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AppUser user, CancellationToken ct = default)
    {
        var existing = store.Users.FirstOrDefault(u => u.Id == user.Id)
            ?? throw new InvalidOperationException("User not found.");

        if (store.Users.Any(u => u.Id != user.Id && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Email already exists.");

        existing.DisplayName = user.DisplayName;
        existing.Email = user.Email;
        existing.Role = user.Role;
        return Task.CompletedTask;
    }

    public Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var existing = store.Users.FirstOrDefault(u => u.Id == id)
            ?? throw new InvalidOperationException("User not found.");

        existing.IsActive = isActive;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryAssignmentRepository(InMemoryStore store) : IAssignmentRepository
{
    public Task<IReadOnlyList<EntityAssignment>> GetByPeriodEntityIdAsync(Guid periodEntityId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<EntityAssignment>>(
            store.Assignments.Where(a => a.PeriodEntityId == periodEntityId).ToList());

    public Task AddAsync(EntityAssignment assignment, CancellationToken ct = default)
    {
        store.Assignments.Add(assignment);
        return Task.CompletedTask;
    }

    public Task ReplaceRoleAssignmentsAsync(
        Guid periodEntityId,
        Guid? confirmerUserId,
        Guid? packFillerUserId,
        CancellationToken ct = default)
    {
        store.Assignments.RemoveAll(a => a.PeriodEntityId == periodEntityId);

        if (confirmerUserId is Guid confirmerId)
        {
            var user = store.Users.FirstOrDefault(u => u.Id == confirmerId);
            store.Assignments.Add(new EntityAssignment
            {
                Id = Guid.NewGuid(),
                PeriodEntityId = periodEntityId,
                UserId = confirmerId,
                CanConfirm = true,
                CanFillPackage = false,
                UserName = user?.UserName,
                DisplayName = user?.DisplayName,
                Email = user?.Email
            });
        }

        if (packFillerUserId is Guid packFillerId)
        {
            var user = store.Users.FirstOrDefault(u => u.Id == packFillerId);
            store.Assignments.Add(new EntityAssignment
            {
                Id = Guid.NewGuid(),
                PeriodEntityId = periodEntityId,
                UserId = packFillerId,
                CanConfirm = false,
                CanFillPackage = true,
                UserName = user?.UserName,
                DisplayName = user?.DisplayName,
                Email = user?.Email
            });
        }

        return Task.CompletedTask;
    }
}

public sealed class InMemoryDevCompanyRepository : IDevCompanyRepository
{
    private static readonly List<(int Id, string Name, string Code, DateOnly ValidFrom, DateOnly? ValidTo)> Companies = [];

    static InMemoryDevCompanyRepository()
    {
        foreach (var c in DevSeedData.Companies)
            Companies.Add((c.RegistryEntityId, c.Name, c.Code, c.ValidFrom, c.ValidTo));
    }

    public Task MergeAsync(int registryEntityId, string name, string code, DateOnly validFrom, DateOnly? validTo = null, CancellationToken ct = default)
    {
        var idx = Companies.FindIndex(c => c.Id == registryEntityId);
        if (idx >= 0)
            Companies[idx] = (registryEntityId, name, code, validFrom, validTo);
        else
            Companies.Add((registryEntityId, name, code, validFrom, validTo));
        return Task.CompletedTask;
    }

    public static IReadOnlyList<(int RegistryEntityId, string Name, string Code, DateOnly ValidOn)> GetValidOn(DateOnly date) =>
        Companies
            .Where(c => c.ValidFrom <= date && (c.ValidTo is null || c.ValidTo >= date))
            .Select(c => (c.Id, c.Name, c.Code, date))
            .ToList();
}
