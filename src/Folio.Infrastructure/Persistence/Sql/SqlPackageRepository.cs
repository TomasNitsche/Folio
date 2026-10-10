using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Domain.Enums;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlPackageRepository(DbConnectionFactory factory) : IPackageRepository
{
    internal async Task<IReadOnlyList<EntityPackage>> GetAllForEntityAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<PackageRow>(
            "Folio_Package_SELECT_ByPeriodEntityId",
            new { PeriodEntityId = periodEntityId },
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => r.ToEntity()).ToList();
    }

    public async Task<EntityPackage?> GetLatestAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<PackageRow>(
            "Folio_Package_SELECT_Latest",
            new { PeriodEntityId = periodEntityId },
            commandType: CommandType.StoredProcedure);

        return row?.ToEntity();
    }

    public async Task AddAsync(EntityPackage package, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Package_INSERT",
            new
            {
                package.Id,
                package.PeriodEntityId,
                package.Version,
                package.IsFinal,
                Status = (byte)package.Status,
                package.CreatedAt,
                package.UpdatedAt,
                package.CreatedBy
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(EntityPackage package, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Package_UPDATE",
            new
            {
                package.Id,
                package.IsFinal,
                Status = (byte)package.Status,
                package.UpdatedAt
            },
            commandType: CommandType.StoredProcedure);
    }

    private sealed class PackageRow
    {
        public Guid Id { get; init; }
        public Guid PeriodEntityId { get; init; }
        public int Version { get; init; }
        public bool IsFinal { get; init; }
        public byte Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string? CreatedBy { get; init; }

        public EntityPackage ToEntity() => new()
        {
            Id = Id,
            PeriodEntityId = PeriodEntityId,
            Version = Version,
            IsFinal = IsFinal,
            Status = (PackageStatus)Status,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            CreatedBy = CreatedBy
        };
    }
}
