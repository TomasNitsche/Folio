using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlDocumentRepository(DbConnectionFactory factory) : IDocumentRepository
{
    public async Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<StoredDocument>(
            "Folio_Document_SELECT_ById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<StoredDocument>> GetByPeriodEntityAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var docs = await conn.QueryAsync<StoredDocument>(
            "Folio_Document_SELECT_ByPeriodEntityId",
            new { PeriodEntityId = periodEntityId },
            commandType: CommandType.StoredProcedure);

        return docs.ToList();
    }

    public async Task AddAsync(StoredDocument document, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Document_INSERT",
            new
            {
                document.Id,
                document.PackageId,
                document.PeriodEntityId,
                Kind = (byte)document.Kind,
                document.FileName,
                document.StoragePath,
                document.PackageVersion,
                document.UploadedBy,
                document.UploadedAt
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateFileAsync(StoredDocument document, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Document_UPDATE_File",
            new
            {
                document.Id,
                document.FileName,
                document.StoragePath
            },
            commandType: CommandType.StoredProcedure);
    }
}
