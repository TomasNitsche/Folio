using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application;

/// <summary>
/// Derives the status and the last-activity timestamp shown on the dashboard from everything that happened
/// for one period entity — not just from its package. An entity is Draft until the reporting team uploads the
/// static reference, Pending while the company side works on it, and Confirmed once the package is final.
/// </summary>
public static class EntityWorkflowState
{
    public static PackageStatus DeriveStatus(IReadOnlyList<StoredDocument> documents, EntityPackage? latestPackage)
    {
        if (latestPackage?.Status == PackageStatus.Rejected)
            return PackageStatus.Rejected;

        if (latestPackage is { IsFinal: true } || latestPackage?.Status == PackageStatus.Confirmed)
            return PackageStatus.Confirmed;

        if (documents.Count > 0 || latestPackage is not null)
            return PackageStatus.Pending;

        return PackageStatus.Draft;
    }

    /// <summary>Newest upload or package change, or null when nothing happened yet.</summary>
    public static DateTime? LastActivityAt(IReadOnlyList<StoredDocument> documents, EntityPackage? latestPackage)
    {
        DateTime? latest = null;

        foreach (var document in documents)
        {
            if (latest is null || document.UploadedAt > latest)
                latest = document.UploadedAt;
        }

        if (latestPackage is not null && (latest is null || latestPackage.UpdatedAt > latest))
            latest = latestPackage.UpdatedAt;

        return latest;
    }
}
