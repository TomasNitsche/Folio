using Folio.Application;
using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Security;
using Folio.Application.Services;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Tests;

public sealed class EntityAccessServiceTests
{
    private static readonly Guid ConfirmerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4");
    private static readonly Guid PackFillerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7");
    private static readonly Guid EntityA = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
    private static readonly Guid EntityB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

    [Fact]
    public void FilterEntitiesForUser_confirmer_sees_only_assigned_entities()
    {
        var entities = CreateEntities();
        var service = CreateService();

        var filtered = service.FilterEntitiesForUser(entities, ConfirmerId, UserRole.Confirmer);

        Assert.Single(filtered);
        Assert.Equal(EntityA, filtered[0].Id);
    }

    [Fact]
    public void FilterEntitiesForUser_packFiller_sees_only_assigned_entities()
    {
        var entities = CreateEntities();
        var service = CreateService();

        var filtered = service.FilterEntitiesForUser(entities, PackFillerId, UserRole.PackFiller);

        Assert.Single(filtered);
        Assert.Equal(EntityB, filtered[0].Id);
    }

    [Fact]
    public void FilterEntitiesForUser_administrator_sees_all_entities()
    {
        var entities = CreateEntities();
        var service = CreateService();

        var filtered = service.FilterEntitiesForUser(entities, Guid.NewGuid(), UserRole.Administrator);

        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_unassigned_confirmer_throws()
    {
        var service = CreateService(currentUserId: ConfirmerId, currentRole: UserRole.Confirmer);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.EnsureCanAccessEntityAsync(EntityB, EntityAccessAction.UploadSigned));
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_assigned_confirmer_can_upload_signed()
    {
        var service = CreateService(currentUserId: ConfirmerId, currentRole: UserRole.Confirmer);

        await service.EnsureCanAccessEntityAsync(EntityA, EntityAccessAction.UploadSigned);
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_confirmer_cannot_upload_package()
    {
        var service = CreateService(currentUserId: ConfirmerId, currentRole: UserRole.Confirmer);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.EnsureCanAccessEntityAsync(EntityA, EntityAccessAction.UploadPackage));
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_assigned_confirmer_cannot_upload_into_non_default_period()
    {
        var (entities, periods) = CreateEntitiesWithPeriods(entityAPeriodIsDefault: false);
        var service = CreateService(ConfirmerId, UserRole.Confirmer, entities, periods);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.EnsureCanAccessEntityAsync(EntityA, EntityAccessAction.UploadSigned));
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_assigned_confirmer_can_upload_into_default_period()
    {
        var (entities, periods) = CreateEntitiesWithPeriods(entityAPeriodIsDefault: true);
        var service = CreateService(ConfirmerId, UserRole.Confirmer, entities, periods);

        await service.EnsureCanAccessEntityAsync(EntityA, EntityAccessAction.UploadSigned);
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_allows_upload_when_no_period_is_marked_default()
    {
        var (entities, periods) = CreateEntitiesWithPeriods(entityAPeriodIsDefault: false, anyDefault: false);
        var service = CreateService(ConfirmerId, UserRole.Confirmer, entities, periods);

        await service.EnsureCanAccessEntityAsync(EntityA, EntityAccessAction.UploadSigned);
    }

    [Fact]
    public async Task EnsureCanAccessEntityAsync_internal_role_is_not_limited_to_the_default_period()
    {
        var (entities, periods) = CreateEntitiesWithPeriods(entityAPeriodIsDefault: false);
        var service = CreateService(Guid.NewGuid(), UserRole.ReportingLead, entities, periods);

        await service.EnsureCanAccessEntityAsync(EntityA, EntityAccessAction.UploadStatic);
    }

    private static EntityAccessService CreateService(
        Guid? currentUserId = null,
        UserRole currentRole = UserRole.Administrator,
        IReadOnlyList<PeriodEntity>? entities = null,
        IReadOnlyList<ReportingPeriod>? periods = null)
    {
        var accessor = new FakeCurrentUserAccessor(
            currentUserId ?? Guid.NewGuid(),
            "test.user",
            currentRole);

        return new EntityAccessService(
            accessor,
            new FakePeriodEntityRepository(entities ?? CreateEntities()),
            new FakePeriodRepository(periods ?? []));
    }

    /// <summary>Entity A sits in period 2026M06, the other period 2026M07 is the one being collected.</summary>
    private static (IReadOnlyList<PeriodEntity> Entities, IReadOnlyList<ReportingPeriod> Periods) CreateEntitiesWithPeriods(
        bool entityAPeriodIsDefault,
        bool anyDefault = true)
    {
        var entityPeriod = new ReportingPeriod
        {
            Id = Guid.NewGuid(),
            ConfirmationType = ConfirmationTypeKind.Transfers,
            Code = "2026M06",
            Status = PeriodStatus.Active,
            IsDefault = entityAPeriodIsDefault,
            OpenedBy = "folio.lead"
        };

        var otherPeriod = new ReportingPeriod
        {
            Id = Guid.NewGuid(),
            ConfirmationType = ConfirmationTypeKind.Transfers,
            Code = "2026M07",
            Status = PeriodStatus.Active,
            IsDefault = anyDefault && !entityAPeriodIsDefault,
            OpenedBy = "folio.lead"
        };

        var entities = CreateEntities();
        foreach (var entity in entities)
        {
            entity.PeriodId = entityPeriod.Id;
            entity.Period = entityPeriod;
        }

        return (entities, [entityPeriod, otherPeriod]);
    }

    private static IReadOnlyList<PeriodEntity> CreateEntities() =>
    [
        new PeriodEntity
        {
            Id = EntityA,
            PeriodId = Guid.NewGuid(),
            RegistryEntityId = 887,
            Name = "HC Energie Karlovy Vary",
            Code = "KVA",
            SnapshotDate = new DateOnly(2026, 7, 1),
            Assignments =
            [
                new EntityAssignment
                {
                    Id = Guid.NewGuid(),
                    PeriodEntityId = EntityA,
                    UserId = ConfirmerId,
                    CanConfirm = true,
                    CanFillPackage = false
                }
            ]
        },
        new PeriodEntity
        {
            Id = EntityB,
            PeriodId = Guid.NewGuid(),
            RegistryEntityId = 2105,
            Name = "HC Kometa Brno",
            Code = "KOM",
            SnapshotDate = new DateOnly(2026, 7, 1),
            Assignments =
            [
                new EntityAssignment
                {
                    Id = Guid.NewGuid(),
                    PeriodEntityId = EntityB,
                    UserId = PackFillerId,
                    CanConfirm = false,
                    CanFillPackage = true
                }
            ]
        }
    ];

    private sealed class FakeCurrentUserAccessor(Guid userId, string userName, UserRole role) : ICurrentUserAccessor
    {
        public CurrentUserContext? Current => new(userId, userName, role);

        public CurrentUserContext GetRequired() => Current!;
    }

    private sealed class FakePeriodEntityRepository(IReadOnlyList<PeriodEntity> entities) : IPeriodEntityRepository
    {
        public Task<IReadOnlyList<PeriodEntity>> GetByPeriodIdAsync(Guid periodId, CancellationToken ct = default) =>
            Task.FromResult(entities);

        public Task<PeriodEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(entities.FirstOrDefault(e => e.Id == id));

        public Task AddRangeAsync(IEnumerable<PeriodEntity> periodEntities, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(PeriodEntity entity, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class FakePeriodRepository(IReadOnlyList<ReportingPeriod> periods) : IPeriodRepository
    {
        public Task<IReadOnlyList<ReportingPeriod>> GetByTypeAsync(ConfirmationTypeKind type, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ReportingPeriod>>(periods.Where(p => p.ConfirmationType == type).ToList());

        public Task<ReportingPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(periods.FirstOrDefault(p => p.Id == id));

        public Task<ReportingPeriod?> GetByCodeAsync(string code, CancellationToken ct = default) =>
            Task.FromResult(periods.FirstOrDefault(p => p.Code == code));

        public Task AddAsync(ReportingPeriod period, CancellationToken ct = default) => Task.CompletedTask;

        public Task UpdateAsync(ReportingPeriod period, CancellationToken ct = default) => Task.CompletedTask;

        public Task SetDefaultAsync(ConfirmationTypeKind type, Guid periodId, CancellationToken ct = default)
        {
            foreach (var period in periods.Where(p => p.ConfirmationType == type))
            {
                period.IsDefault = period.Id == periodId;
            }
            return Task.CompletedTask;
        }
    }
}
