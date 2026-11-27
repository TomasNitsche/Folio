using System.Reflection;

namespace Folio.Web.Services;

/// <summary>The version of the running application (set by the build, see azure-pipelines.yml).</summary>
public static class AppVersion
{
    public static string Current { get; } = Read();

    private static string Read()
    {
        var info = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(info)) return "unknown";

        // The SDK appends "+<commit hash>" to the informational version; the version number is what people need.
        var plus = info.IndexOf('+');
        return plus > 0 ? info[..plus] : info;
    }
}
