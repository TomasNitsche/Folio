namespace Folio.Application;

public static class PackageFileNaming
{
    public static string ForVersion(int version, string originalFileName, bool isFinal = false) =>
        isFinal
            ? $"v{version}_Final_{originalFileName}"
            : $"v{version}_{originalFileName}";

    public static string ToFinalName(string currentFileName, int version)
    {
        var prefix = $"v{version}_";
        if (currentFileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && !currentFileName.StartsWith($"v{version}_Final_", StringComparison.OrdinalIgnoreCase))
        {
            return prefix + "Final_" + currentFileName[prefix.Length..];
        }

        return ForVersion(version, currentFileName, isFinal: true);
    }
}
