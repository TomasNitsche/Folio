namespace Folio.Infrastructure.Integrations;

/// <summary>
/// Configuration for the real Registry HTTP integration ("Registry:Provider" = "Http"). Bound from the "Registry" config section.
/// No credentials live here: Registry' folio-integration endpoints use Windows auth (Negotiate) against the process
/// identity, so Folio.Api must run under the service account set in Registry' "FolioIntegration:AllowedUser"
/// (that account must also exist as a user in Registry).
/// </summary>
public sealed class RegistryOptions
{
    public const string SectionName = "Registry";

    /// <summary>
    /// Base URL of the Registry API. Hosted environments serve the API under an "/api" path segment:
    /// https://registry-dev.example.com/api/ (dev), https://registry-test.example.com/api/ (test), https://registry.example.com/api/ (prod).
    /// Only a local Registry instance (https://localhost:5000/) has no "/api" segment. Must end with "/" — otherwise
    /// HttpClient drops the last path segment ("api") when resolving the relative request URL.
    /// </summary>
    public string BaseUrl { get; set; } = "";

    public int TimeoutSeconds { get; set; } = 30;
}
