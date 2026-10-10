using Folio.Application.Interfaces.Integrations;
using Folio.Application.Mappings;
using Folio.Infrastructure.Persistence;

namespace Folio.Infrastructure.Integrations;

/// <summary>
/// Stub registry client: simulates a call to the external registry and returns the fictional hockey clubs
/// from <see cref="FakeClubDirectory"/> (valid from the requested date).
/// </summary>
public sealed class RegistryEntityClientStub : IRegistryEntityClient
{
    public async Task<IReadOnlyList<RegistryEntitySnapshot>> GetEntitiesValidOnAsync(DateOnly date, CancellationToken ct = default)
    {
        await Task.Delay(150, ct); // simulated network latency

        return FakeClubDirectory.All
            .Select(c => new RegistryEntitySnapshot(c.RegistryEntityId, c.Name, c.Code, date))
            .ToList();
    }
}
