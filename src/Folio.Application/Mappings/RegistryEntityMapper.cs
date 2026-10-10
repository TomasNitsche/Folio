namespace Folio.Application.Mappings;

/// <summary>
/// Raw shape returned by Registry' GET folio-integration/entities endpoint.
/// </summary>
public record RegistryApiEntityDto(int EntityId, string DisplayValue, string? RegistrationNumber, string? CompanyCode);

public static class RegistryEntityMapper
{
    /// <summary>
    /// Registry' dedicated folio-integration/entities endpoint returns CompanyCode and RegistrationNumber 
    /// directly — no LookupDisplayValues lookup needed. CompanyCode becomes the entity's short display Code (shown
    /// in the entity list/detail), falling back to DisplayValue when Registry hasn't set it for that entity;
    /// RegistrationNumber is carried through unchanged for later use.
    /// </summary>
    public static RegistryEntitySnapshot ToSnapshot(RegistryApiEntityDto entity, DateOnly validOn)
    {
        var displayValue = entity.DisplayValue ?? "";
        var code = !string.IsNullOrEmpty(entity.CompanyCode) ? entity.CompanyCode : displayValue;

        return new RegistryEntitySnapshot(entity.EntityId, displayValue, code, validOn, entity.RegistrationNumber);
    }
}
