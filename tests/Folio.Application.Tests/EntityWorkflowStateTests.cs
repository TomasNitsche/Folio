using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Tests;

public sealed class EntityWorkflowStateTests
{
    [Fact]
    public void DeriveStatus_is_Draft_before_anything_is_uploaded()
    {
        Assert.Equal(PackageStatus.Draft, EntityWorkflowState.DeriveStatus([], null));
    }

    [Fact]
    public void DeriveStatus_is_Pending_once_the_static_reference_is_uploaded()
    {
        var docs = new List<StoredDocument> { Doc(DocumentKind.StaticReference, new DateTime(2026, 9, 1, 8, 0, 0)) };

        Assert.Equal(PackageStatus.Pending, EntityWorkflowState.DeriveStatus(docs, null));
    }

    [Fact]
    public void DeriveStatus_stays_Pending_while_the_package_is_not_final()
    {
        var docs = new List<StoredDocument>
        {
            Doc(DocumentKind.StaticReference, new DateTime(2026, 9, 1, 8, 0, 0)),
            Doc(DocumentKind.SignedConfirmation, new DateTime(2026, 9, 2, 8, 0, 0)),
            Doc(DocumentKind.ExcelPackage, new DateTime(2026, 9, 3, 8, 0, 0))
        };
        var package = new EntityPackage { Status = PackageStatus.Pending, IsFinal = false };

        Assert.Equal(PackageStatus.Pending, EntityWorkflowState.DeriveStatus(docs, package));
    }

    [Fact]
    public void DeriveStatus_is_Confirmed_once_the_package_is_final()
    {
        var docs = new List<StoredDocument> { Doc(DocumentKind.ExcelPackage, new DateTime(2026, 9, 3, 8, 0, 0)) };
        var package = new EntityPackage { Status = PackageStatus.Pending, IsFinal = true };

        Assert.Equal(PackageStatus.Confirmed, EntityWorkflowState.DeriveStatus(docs, package));
    }

    [Fact]
    public void DeriveStatus_keeps_a_rejected_package_rejected()
    {
        var docs = new List<StoredDocument> { Doc(DocumentKind.ExcelPackage, new DateTime(2026, 9, 3, 8, 0, 0)) };
        var package = new EntityPackage { Status = PackageStatus.Rejected, IsFinal = false };

        Assert.Equal(PackageStatus.Rejected, EntityWorkflowState.DeriveStatus(docs, package));
    }

    [Fact]
    public void LastActivityAt_is_null_when_nothing_happened()
    {
        Assert.Null(EntityWorkflowState.LastActivityAt([], null));
    }

    [Fact]
    public void LastActivityAt_returns_the_newest_upload_even_without_a_package()
    {
        var docs = new List<StoredDocument>
        {
            Doc(DocumentKind.StaticReference, new DateTime(2026, 9, 1, 8, 0, 0)),
            Doc(DocumentKind.SignedConfirmation, new DateTime(2026, 9, 5, 14, 30, 0))
        };

        Assert.Equal(new DateTime(2026, 9, 5, 14, 30, 0), EntityWorkflowState.LastActivityAt(docs, null));
    }

    [Fact]
    public void LastActivityAt_prefers_the_package_when_it_changed_last()
    {
        var docs = new List<StoredDocument> { Doc(DocumentKind.StaticReference, new DateTime(2026, 9, 1, 8, 0, 0)) };
        var package = new EntityPackage { UpdatedAt = new DateTime(2026, 9, 6, 9, 0, 0) };

        Assert.Equal(new DateTime(2026, 9, 6, 9, 0, 0), EntityWorkflowState.LastActivityAt(docs, package));
    }

    private static StoredDocument Doc(DocumentKind kind, DateTime uploadedAt) =>
        new()
        {
            Kind = kind,
            FileName = "test.xlsx",
            StoragePath = "period/entity/static/test.xlsx",
            UploadedAt = uploadedAt
        };
}
