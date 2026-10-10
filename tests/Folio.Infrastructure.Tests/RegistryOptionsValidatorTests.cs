using Microsoft.Extensions.Options;
using Folio.Infrastructure.Integrations;

namespace Folio.Infrastructure.Tests;

public sealed class RegistryOptionsValidatorTests
{
    private readonly RegistryOptionsValidator validator = new();

    private static RegistryOptions ValidOptions() => new()
    {
        BaseUrl = "https://registry-dev.example.com/api/",
    };

    [Fact]
    public void Validate_succeeds_when_all_required_settings_are_present()
    {
        var result = validator.Validate(null, ValidOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_fails_when_BaseUrl_is_missing()
    {
        var options = ValidOptions();
        options.BaseUrl = "";

        var result = validator.Validate(null, options);

        AssertFailureContains(result, "Registry:BaseUrl is required");
    }

    [Fact]
    public void Validate_fails_when_BaseUrl_has_no_trailing_slash()
    {
        var options = ValidOptions();
        options.BaseUrl = "https://registry-dev.example.com/api";

        var result = validator.Validate(null, options);

        AssertFailureContains(result, "must end with '/'");
    }

    [Fact]
    public void Validate_collects_every_failure_at_once()
    {
        var options = new RegistryOptions();

        var result = validator.Validate(null, options);

        Assert.Equal(1, result.Failures?.Count() ?? 0);
    }

    private static void AssertFailureContains(ValidateOptionsResult result, string expectedMessageFragment) =>
        Assert.Contains(result.Failures ?? [], f => f.Contains(expectedMessageFragment));
}
