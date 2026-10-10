using Folio.Domain.Entities;
using Folio.Domain.Enums;
using Folio.Infrastructure.Persistence.InMemory;

namespace Folio.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void Seed(InMemoryStore store)
    {
        if (store.Periods.Count > 0) return;

        var transfersPeriod = new ReportingPeriod
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ConfirmationType = ConfirmationTypeKind.Transfers,
            Code = "2026M01",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 31),
            SnapshotDate = new DateOnly(2026, 1, 1),
            Status = PeriodStatus.Active,
            OpenedBy = "jan.novak",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        store.Periods.Add(transfersPeriod);

        var entities = new[]
        {
            ("HC Sparta Praha", "SPA", 101, PackageStatus.Pending, 0, false),
            ("HC Slavia Praha", "SLA", 102, PackageStatus.Pending, 2, false),
            ("HC Oceláři Třinec", "TRI", 103, PackageStatus.Confirmed, 1, true),
            ("HC Kometa Brno", "KOM", 104, PackageStatus.Draft, -1, false),
            ("HC Energie Karlovy Vary", "KVA", 105, PackageStatus.Rejected, 1, false)
        };

        foreach (var (name, code, registryId, status, version, isFinal) in entities)
        {
            var entityId = Guid.NewGuid();

            var entity = new PeriodEntity
            {
                Id = entityId,
                PeriodId = transfersPeriod.Id,
                RegistryEntityId = registryId,
                Name = name,
                Code = code,
                SnapshotDate = transfersPeriod.SnapshotDate,
                Assignments =
                [
                    new EntityAssignment
                    {
                        Id = Guid.NewGuid(),
                        PeriodEntityId = entityId,
                        UserName = "Karel Gott",
                        Email = "karel.gott@example.com",
                        CanConfirm = true,
                        CanFillPackage = false
                    }
                ]
            };

            store.PeriodEntities.Add(entity);

            if (version >= 0)
            {
                store.Packages.Add(new EntityPackage
                {
                    Id = Guid.NewGuid(),
                    PeriodEntityId = entityId,
                    Version = version,
                    IsFinal = isFinal,
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddHours(-Random.Shared.Next(1, 48)),
                    CreatedBy = "folio.user"
                });
            }
        }
    }
}
