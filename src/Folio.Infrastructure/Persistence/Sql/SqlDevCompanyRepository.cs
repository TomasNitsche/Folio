using Dapper;
using Folio.Application.Interfaces.Repositories;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlDevCompanyRepository(DbConnectionFactory factory) : IDevCompanyRepository
{
    public async Task MergeAsync(int registryEntityId, string name, string code, DateOnly validFrom, DateOnly? validTo = null, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_DevCompany_MERGE",
            new { RegistryEntityId = registryEntityId, Name = name, Code = code, ValidFrom = validFrom, ValidTo = validTo },
            commandType: CommandType.StoredProcedure);
    }
}
