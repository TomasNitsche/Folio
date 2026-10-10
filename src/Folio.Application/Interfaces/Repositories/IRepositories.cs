using Folio.Application.DTOs.Periods;
using Folio.Domain.Entities;

namespace Folio.Application.Interfaces.Repositories;

public interface IPeriodRepository
{
    Task<IReadOnlyList<ReportingPeriod>> GetByTypeAsync(Domain.Enums.ConfirmationTypeKind type, CancellationToken ct = default);
    Task<ReportingPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReportingPeriod?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(ReportingPeriod period, CancellationToken ct = default);
    Task UpdateAsync(ReportingPeriod period, CancellationToken ct = default);
    /// <summary>Marks one period as the default for its confirmation type and clears the flag on all others of that type.</summary>
    Task SetDefaultAsync(Domain.Enums.ConfirmationTypeKind type, Guid periodId, CancellationToken ct = default);
}

public interface IPeriodEntityRepository
{
    Task<IReadOnlyList<PeriodEntity>> GetByPeriodIdAsync(Guid periodId, CancellationToken ct = default);
    Task<PeriodEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PeriodEntity> entities, CancellationToken ct = default);
    Task UpdateAsync(PeriodEntity entity, CancellationToken ct = default);
}

public interface IPackageRepository
{
    Task<EntityPackage?> GetLatestAsync(Guid periodEntityId, CancellationToken ct = default);
    Task AddAsync(EntityPackage package, CancellationToken ct = default);
    Task UpdateAsync(EntityPackage package, CancellationToken ct = default);
}

public interface IDocumentRepository
{
    Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StoredDocument>> GetByPeriodEntityAsync(Guid periodEntityId, CancellationToken ct = default);
    Task AddAsync(StoredDocument document, CancellationToken ct = default);
    Task UpdateFileAsync(StoredDocument document, CancellationToken ct = default);
}

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken ct = default);
}
