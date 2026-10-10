using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Infrastructure.Persistence.InMemory;

public sealed class InMemoryStore
{
    public List<AppUser> Users { get; } = [];
    public List<EntityAssignment> Assignments { get; } = [];
    public List<ReportingPeriod> Periods { get; } = [];
    public List<PeriodEntity> PeriodEntities { get; } = [];
    public List<EntityPackage> Packages { get; } = [];
    public List<StoredDocument> Documents { get; } = [];
    public List<AuditEntry> AuditEntries { get; } = [];
}

public sealed class InMemoryPeriodRepository(InMemoryStore store) : IPeriodRepository
{
    public Task<IReadOnlyList<ReportingPeriod>> GetByTypeAsync(ConfirmationTypeKind type, CancellationToken ct = default)
    {
        var periods = store.Periods.Where(p => p.ConfirmationType == type).ToList();
        AttachEntities(periods);
        return Task.FromResult<IReadOnlyList<ReportingPeriod>>(periods);
    }

    public Task<ReportingPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var period = store.Periods.FirstOrDefault(p => p.Id == id);
        if (period is not null) AttachEntities([period]);
        return Task.FromResult(period);
    }

    public Task<ReportingPeriod?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        Task.FromResult(store.Periods.FirstOrDefault(p => p.Code == code));

    public Task AddAsync(ReportingPeriod period, CancellationToken ct = default)
    {
        store.Periods.Add(period);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ReportingPeriod period, CancellationToken ct = default) => Task.CompletedTask;

    public Task SetDefaultAsync(ConfirmationTypeKind type, Guid periodId, CancellationToken ct = default)
    {
        foreach (var period in store.Periods.Where(p => p.ConfirmationType == type))
        {
            period.IsDefault = period.Id == periodId;
        }
        return Task.CompletedTask;
    }

    private void AttachEntities(List<ReportingPeriod> periods)
    {
        foreach (var period in periods)
        {
            period.Entities = store.PeriodEntities.Where(e => e.PeriodId == period.Id).ToList();
            foreach (var entity in period.Entities)
            {
                entity.Period = period;
                entity.Packages = store.Packages.Where(p => p.PeriodEntityId == entity.Id).ToList();
                foreach (var package in entity.Packages)
                {
                    package.Documents = store.Documents.Where(d => d.PackageId == package.Id).ToList();
                }
            }
        }
    }
}

public sealed class InMemoryPeriodEntityRepository(InMemoryStore store) : IPeriodEntityRepository
{
    public Task<IReadOnlyList<PeriodEntity>> GetByPeriodIdAsync(Guid periodId, CancellationToken ct = default)
    {
        var entities = store.PeriodEntities.Where(e => e.PeriodId == periodId).ToList();
        Attach(entities);
        return Task.FromResult<IReadOnlyList<PeriodEntity>>(entities);
    }

    public Task<PeriodEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = store.PeriodEntities.FirstOrDefault(e => e.Id == id);
        if (entity is not null) Attach([entity]);
        return Task.FromResult(entity);
    }

    public Task AddRangeAsync(IEnumerable<PeriodEntity> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            store.PeriodEntities.Add(entity);
            foreach (var package in entity.Packages)
            {
                store.Packages.Add(package);
            }
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PeriodEntity entity, CancellationToken ct = default) => Task.CompletedTask;

    private void Attach(List<PeriodEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.Period = store.Periods.FirstOrDefault(p => p.Id == entity.PeriodId);
            entity.Packages = store.Packages.Where(p => p.PeriodEntityId == entity.Id).ToList();
            entity.Assignments = store.Assignments.Where(a => a.PeriodEntityId == entity.Id).ToList();
            foreach (var package in entity.Packages)
            {
                package.Documents = store.Documents.Where(d => d.PackageId == package.Id).ToList();
            }
        }
    }
}

public sealed class InMemoryPackageRepository(InMemoryStore store) : IPackageRepository
{
    public Task<EntityPackage?> GetLatestAsync(Guid periodEntityId, CancellationToken ct = default) =>
        Task.FromResult(store.Packages
            .Where(p => p.PeriodEntityId == periodEntityId)
            .OrderByDescending(p => p.Version)
            .FirstOrDefault());

    public Task AddAsync(EntityPackage package, CancellationToken ct = default)
    {
        store.Packages.Add(package);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EntityPackage package, CancellationToken ct = default)
    {
        var index = store.Packages.FindIndex(p => p.Id == package.Id);
        if (index >= 0)
            store.Packages[index] = package;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryDocumentRepository(InMemoryStore store) : IDocumentRepository
{
    public Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(store.Documents.FirstOrDefault(d => d.Id == id));

    public Task<IReadOnlyList<StoredDocument>> GetByPeriodEntityAsync(Guid periodEntityId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StoredDocument>>(
            store.Documents.Where(d => d.PeriodEntityId == periodEntityId).ToList());

    public Task AddAsync(StoredDocument document, CancellationToken ct = default)
    {
        store.Documents.Add(document);
        return Task.CompletedTask;
    }

    public Task UpdateFileAsync(StoredDocument document, CancellationToken ct = default)
    {
        var index = store.Documents.FindIndex(d => d.Id == document.Id);
        if (index >= 0)
            store.Documents[index] = document;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryAuditRepository(InMemoryStore store) : IAuditRepository
{
    public Task AddAsync(AuditEntry entry, CancellationToken ct = default)
    {
        store.AuditEntries.Add(entry);
        return Task.CompletedTask;
    }
}
