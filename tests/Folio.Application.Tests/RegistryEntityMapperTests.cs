using Folio.Application.Mappings;

namespace Folio.Application.Tests;

public sealed class RegistryEntityMapperTests
{
    private static readonly DateOnly ValidOn = new(2026, 1, 1);

    [Fact]
    public void ToSnapshot_uses_CompanyCode_as_Code_when_present()
    {
        var entity = new RegistryApiEntityDto(42, "Acme HC", "12345678", "ACM");

        var snapshot = RegistryEntityMapper.ToSnapshot(entity, ValidOn);

        Assert.Equal("ACM", snapshot.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToSnapshot_falls_back_to_DisplayValue_when_CompanyCode_is_missing(string? companyCode)
    {
        var entity = new RegistryApiEntityDto(42, "Acme HC", "12345678", companyCode);

        var snapshot = RegistryEntityMapper.ToSnapshot(entity, ValidOn);

        Assert.Equal("Acme HC", snapshot.Code);
    }

    [Fact]
    public void ToSnapshot_carries_RegistrationNumber_through_unchanged()
    {
        var entity = new RegistryApiEntityDto(42, "Acme HC", "12345678", "ACM");

        var snapshot = RegistryEntityMapper.ToSnapshot(entity, ValidOn);

        Assert.Equal("12345678", snapshot.RegistrationNumber);
    }

    [Fact]
    public void ToSnapshot_carries_null_RegistrationNumber_through()
    {
        var entity = new RegistryApiEntityDto(42, "Acme HC", null, "ACM");

        var snapshot = RegistryEntityMapper.ToSnapshot(entity, ValidOn);

        Assert.Null(snapshot.RegistrationNumber);
    }

    [Fact]
    public void ToSnapshot_coalesces_null_DisplayValue_to_empty_string()
    {
        // Registry is external and its response shape isn't enforced by the C# compiler at the wire boundary —
        // a null DisplayValue must not become a literal "null" Name/Code.
        var entity = new RegistryApiEntityDto(42, null!, null, null);

        var snapshot = RegistryEntityMapper.ToSnapshot(entity, ValidOn);

        Assert.Equal("", snapshot.Name);
        Assert.Equal("", snapshot.Code);
    }

    [Fact]
    public void ToSnapshot_preserves_EntityId_and_ValidOn()
    {
        var entity = new RegistryApiEntityDto(42, "Acme HC", "12345678", "ACM");

        var snapshot = RegistryEntityMapper.ToSnapshot(entity, ValidOn);

        Assert.Equal(42, snapshot.RegistryEntityId);
        Assert.Equal(ValidOn, snapshot.ValidOn);
    }
}
