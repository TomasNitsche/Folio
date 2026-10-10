using Folio.Application.DTOs.Periods;
using Folio.Application.Interfaces.Integrations;
using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Services;
using Folio.Application.Mappings;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Services;

public sealed class PeriodService(
    IEntityAccessService entityAccessService,
    IPeriodRepository periodRepository,
    IPeriodEntityRepository periodEntityRepository,
    IRegistryEntityClient registryClient,
    IFileStorage fileStorage,
    IAuditRepository auditRepository) : IPeriodService
{
    public async Task<IReadOnlyList<PeriodDto>> GetPeriodsAsync(ConfirmationTypeKind type, CancellationToken ct = default)
    {
        var periods = await periodRepository.GetByTypeAsync(type, ct);
        return periods.Select(PeriodMapper.ToDto).ToList();
    }

    public async Task<PeriodDto?> GetPeriodAsync(Guid id, CancellationToken ct = default)
    {
        var period = await periodRepository.GetByIdAsync(id, ct);
        return period is null ? null : PeriodMapper.ToDto(period);
    }

    public async Task<IReadOnlyList<RegistryEntityDto>> FetchRegistryEntitiesAsync(DateOnly snapshotDate, CancellationToken ct = default)
    {
        var entities = await registryClient.GetEntitiesValidOnAsync(snapshotDate, ct);
        return entities.Select(PeriodMapper.ToDto).ToList();
    }

    public async Task<PeriodDto> OpenPeriodAsync(OpenPeriodRequest request, CancellationToken ct = default)
    {
        await entityAccessService.EnsureRoleCapabilityAsync(EntityAccessAction.OpenPeriod, ct);

        var existing = await periodRepository.GetByCodeAsync(request.Code, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Period '{request.Code}' already exists.");

        var registryEntities = await registryClient.GetEntitiesValidOnAsync(request.SnapshotDate, ct);
        var selected = registryEntities
            .Where(e => request.SelectedRegistryEntityIds.Contains(e.RegistryEntityId))
            .ToList();

        if (selected.Count == 0)
            throw new InvalidOperationException("At least one entity must be selected.");

        var period = new ReportingPeriod
        {
            Id = Guid.NewGuid(),
            ConfirmationType = request.ConfirmationType,
            Code = request.Code,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SnapshotDate = request.SnapshotDate,
            Status = PeriodStatus.Active,
            OpenedBy = request.OpenedBy,
            CreatedAt = DateTime.UtcNow
        };

        var existingOfType = await periodRepository.GetByTypeAsync(request.ConfirmationType, ct);
        // The first period of a type becomes the default so company users always have one period to work in.
        period.IsDefault = existingOfType.All(p => !p.IsDefault);

        await fileStorage.EnsurePeriodFolderAsync(period.Code, ct);
        await periodRepository.AddAsync(period, ct);

        if (period.IsDefault)
            await periodRepository.SetDefaultAsync(period.ConfirmationType, period.Id, ct);

        var periodEntities = selected.Select(e => new PeriodEntity
        {
            Id = Guid.NewGuid(),
            PeriodId = period.Id,
            RegistryEntityId = e.RegistryEntityId,
            Name = e.Name,
            Code = e.Code,
            SnapshotDate = request.SnapshotDate
        }).ToList();

        await periodEntityRepository.AddRangeAsync(periodEntities, ct);

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(ReportingPeriod),
            EntityId = period.Id,
            Action = "PeriodOpened",
            PerformedBy = request.OpenedBy,
            PerformedAt = DateTime.UtcNow,
            Details = $"Opened with {periodEntities.Count} entities"
        }, ct);

        period.Entities = periodEntities;
        return PeriodMapper.ToDto(period);
    }

    public async Task<PeriodDto> SetDefaultPeriodAsync(Guid periodId, string performedBy, CancellationToken ct = default)
    {
        await entityAccessService.EnsureRoleCapabilityAsync(EntityAccessAction.OpenPeriod, ct);

        var period = await periodRepository.GetByIdAsync(periodId, ct)
            ?? throw new KeyNotFoundException("Period not found.");

        await periodRepository.SetDefaultAsync(period.ConfirmationType, period.Id, ct);
        period.IsDefault = true;

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(ReportingPeriod),
            EntityId = period.Id,
            Action = "PeriodMarkedDefault",
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            Details = $"Period {period.Code} is now the default for {period.ConfirmationType}"
        }, ct);

        return PeriodMapper.ToDto(period);
    }
}
