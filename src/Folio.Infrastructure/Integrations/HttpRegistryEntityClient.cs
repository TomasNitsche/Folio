using System.Net.Http.Json;
using Folio.Application.Interfaces.Integrations;
using Folio.Application.Mappings;

namespace Folio.Infrastructure.Integrations;

/// <summary>
/// Real Registry integration — calls folio-integration/entities on the production/dev Registry API. The agenda (Folio)
/// and entity type (Entity/company) are fixed on the Registry side; only approved values are returned.
/// </summary>
public sealed class HttpRegistryEntityClient(HttpClient httpClient) : IRegistryEntityClient
{
    public async Task<IReadOnlyList<RegistryEntitySnapshot>> GetEntitiesValidOnAsync(DateOnly date, CancellationToken ct = default)
    {
        var query = $"folio-integration/entities?asOfDate={date:yyyy-MM-dd}";

        var entities = await httpClient.GetFromJsonAsync<List<RegistryApiEntityDto>>(query, ct) ?? [];

        return entities
            .Select(e => RegistryEntityMapper.ToSnapshot(e, date))
            .ToList();
    }
}
