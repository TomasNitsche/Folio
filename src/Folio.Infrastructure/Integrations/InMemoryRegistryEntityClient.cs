using Folio.Application.Interfaces.Integrations;
using Folio.Application.Mappings;
using Folio.Infrastructure.Persistence;
using Folio.Infrastructure.Persistence.InMemory;

namespace Folio.Infrastructure.Integrations;

/// <summary>
/// In-memory Registry substitute — reads from DevCompanies seeded via DevSeedService.
/// </summary>
public sealed class InMemoryRegistryEntityClient : IRegistryEntityClient
{
    public Task<IReadOnlyList<RegistryEntitySnapshot>> GetEntitiesValidOnAsync(DateOnly date, CancellationToken ct = default)
    {
        var companies = InMemoryDevCompanyRepository.GetValidOn(date);
        if (companies.Count == 0)
        {
            companies = DevSeedData.Companies
                .Where(c => c.ValidFrom <= date && (c.ValidTo is null || c.ValidTo >= date))
                .Select(c => (c.RegistryEntityId, c.Name, c.Code, date))
                .ToList();
        }

        return Task.FromResult<IReadOnlyList<RegistryEntitySnapshot>>(
            companies.Select(c => new RegistryEntitySnapshot(c.RegistryEntityId, c.Name, c.Code, c.ValidOn)).ToList());
    }
}
