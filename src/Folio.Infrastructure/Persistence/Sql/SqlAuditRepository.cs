using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlAuditRepository(DbConnectionFactory factory) : IAuditRepository
{
    public async Task AddAsync(AuditEntry entry, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Audit_INSERT",
            new
            {
                entry.Id,
                entry.EntityType,
                entry.EntityId,
                entry.Action,
                entry.PerformedBy,
                entry.PerformedAt,
                entry.Details
            },
            commandType: CommandType.StoredProcedure);
    }
}
