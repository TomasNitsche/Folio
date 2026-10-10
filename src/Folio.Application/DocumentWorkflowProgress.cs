using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application;

/// <summary>
/// Workflow steps: 1 static directive, 2 signed confirmation, 3 excel package, 4 package marked final.
/// </summary>
public static class DocumentWorkflowProgress
{
    public const int ExpectedSteps = 4;

    public static int CountCompleted(IReadOnlyList<StoredDocument> documents, EntityPackage? latestPackage)
    {
        var count = 0;
        if (documents.Any(d => d.Kind == DocumentKind.StaticReference))
            count++;
        if (documents.Any(d => d.Kind == DocumentKind.SignedConfirmation))
            count++;
        if (documents.Any(d => d.Kind == DocumentKind.ExcelPackage))
            count++;
        if (latestPackage?.IsFinal == true)
            count++;
        return count;
    }
}
