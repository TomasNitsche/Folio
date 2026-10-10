using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Domain.Enums;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlPeriodEntityRepository(
    DbConnectionFactory factory,
    SqlPackageRepository packageRepository,
    SqlAssignmentRepository assignmentRepository) : IPeriodEntityRepository
{
    public async Task<IReadOnlyList<PeriodEntity>> GetByPeriodIdAsync(Guid periodId, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<SqlRows.PeriodEntityRow>(
            "Folio_PeriodEntity_SELECT_ByPeriodId",
            new { PeriodId = periodId },
            commandType: CommandType.StoredProcedure);

        var entities = rows.Select(r => new PeriodEntity
        {
            Id = r.Id,
            PeriodId = r.PeriodId,
            RegistryEntityId = r.RegistryEntityId,
            Name = r.Name,
            Code = r.Code,
            SnapshotDate = r.SnapshotDate
        }).ToList();

        foreach (var entity in entities)
        {
            entity.Packages = (await packageRepository.GetAllForEntityAsync(entity.Id, ct)).ToList();
            entity.Assignments = (await assignmentRepository.GetByPeriodEntityIdAsync(entity.Id, ct)).ToList();
        }

        return entities;
    }

    public async Task<PeriodEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<SqlRows.PeriodEntityDetailRow>(
            "Folio_PeriodEntity_SELECT_ById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        if (row is null) return null;

        var entity = row.ToEntity();
        entity.Packages = (await packageRepository.GetAllForEntityAsync(entity.Id, ct)).ToList();
        entity.Assignments = (await assignmentRepository.GetByPeriodEntityIdAsync(entity.Id, ct)).ToList();
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<PeriodEntity> entities, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        try
        {
            foreach (var entity in entities)
            {
                await conn.ExecuteAsync(
                    "Folio_PeriodEntity_INSERT",
                    new
                    {
                        entity.Id,
                        entity.PeriodId,
                        entity.RegistryEntityId,
                        entity.Name,
                        entity.Code,
                        entity.SnapshotDate
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure);

                foreach (var package in entity.Packages)
                {
                    package.PeriodEntityId = entity.Id;
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
                        transaction: tx,
                        commandType: CommandType.StoredProcedure);
                }
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public Task UpdateAsync(PeriodEntity entity, CancellationToken ct = default) =>
        Task.CompletedTask;
}
