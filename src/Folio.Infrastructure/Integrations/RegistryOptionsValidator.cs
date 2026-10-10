using Microsoft.Extensions.Options;

namespace Folio.Infrastructure.Integrations;

/// <summary>
/// Fails fast at startup when "Registry:Provider" is "Http" but the settings needed to actually reach Registry
/// are missing, instead of surfacing an opaque exception on the first period-open call.
/// </summary>
public sealed class RegistryOptionsValidator : IValidateOptions<RegistryOptions>
{
    public ValidateOptionsResult Validate(string? name, RegistryOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            errors.Add("Registry:BaseUrl is required.");
        else if (!options.BaseUrl.EndsWith('/'))
            errors.Add("Registry:BaseUrl must end with '/' — otherwise HttpClient drops its last path segment (e.g. \"api\").");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
