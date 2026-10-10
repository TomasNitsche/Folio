using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Domain.Enums;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlPeriodRepository(DbConnectionFactory factory) : IPeriodRepository
{
    public async Task<IReadOnlyList<ReportingPeriod>> GetByTypeAsync(ConfirmationTypeKind type, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<SqlRows.PeriodRow>(
            "Folio_Period_SELECT_ByType",
            new { ConfirmationType = (byte)type },
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => r.ToEntity()).ToList();
    }

    public async Task<ReportingPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<SqlRows.PeriodRow>(
            "Folio_Period_SELECT_ById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        return row?.ToEntity();
    }

    public async Task<ReportingPeriod?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<SqlRows.PeriodRow>(
            "Folio_Period_SELECT_ByCode",
            new { Code = code },
            commandType: CommandType.StoredProcedure);

        return row?.ToEntity();
    }

    public async Task AddAsync(ReportingPeriod period, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Period_INSERT",
            new
            {
                period.Id,
                ConfirmationType = (byte)period.ConfirmationType,
                period.Code,
                period.StartDate,
                period.EndDate,
                Status = (byte)period.Status,
                period.SnapshotDate,
                period.OpenedBy,
                period.CreatedAt,
                period.IsDefault
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(ReportingPeriod period, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Period_UPDATE",
            new { period.Id, Status = (byte)period.Status, period.ClosedAt },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SetDefaultAsync(ConfirmationTypeKind type, Guid periodId, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Period_SET_Default",
            new { Id = periodId, ConfirmationType = (byte)type },
            commandType: CommandType.StoredProcedure);
    }
}
