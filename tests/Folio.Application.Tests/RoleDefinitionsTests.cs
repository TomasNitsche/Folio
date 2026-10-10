using Folio.Application;
using Folio.Domain.Enums;
using Xunit;

namespace Folio.Application.Tests;

public class RoleDefinitionsTests
{
    [Fact]
    public void All_roles_have_unique_ids() =>
        Assert.Equal(RoleDefinitions.All.Count, RoleDefinitions.All.Select(r => r.Role).Distinct().Count());

    [Fact]
    public void Contributor_can_upload_static_documents() =>
        Assert.Contains(
            RoleDefinitions.All.First(r => r.Role == UserRole.Contributor).Permissions,
            p => p.Contains("static", StringComparison.OrdinalIgnoreCase));
}
