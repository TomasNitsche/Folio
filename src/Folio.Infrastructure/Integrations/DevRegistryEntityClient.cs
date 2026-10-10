using Dapper;
using Folio.Application.Interfaces.Integrations;
using Folio.Application.Mappings;
using Folio.Infrastructure.Persistence.Sql;
using System.Data;

namespace Folio.Infrastructure.Integrations;

/// <summary>
/// Development substitute for Registry — reads companies from dbo.DevCompanies via stored procedure.
/// </summary>
public sealed class DevRegistryEntityClient(DbConnectionFactory factory) : IRegistryEntityClient
{
    public async Task<IReadOnlyList<RegistryEntitySnapshot>> GetEntitiesValidOnAsync(DateOnly date, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<RegistryRow>(
            "Folio_DevCompany_SELECT_ValidOn",
            new { ValidOn = date },
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => new RegistryEntitySnapshot(r.RegistryEntityId, r.Name, r.Code, r.ValidOn)).ToList();
    }

    private sealed class RegistryRow
    {
        public int RegistryEntityId { get; init; }
        public string Name { get; init; } = "";
        public string Code { get; init; } = "";
        public DateOnly ValidOn { get; init; }
    }
}
