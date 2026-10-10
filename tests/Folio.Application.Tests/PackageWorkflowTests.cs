using Folio.Application;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Tests;

public sealed class PackageWorkflowTests
{
    [Theory]
    [InlineData(0, "Filled_package.xlsx", "v0_Filled_package.xlsx")]
    [InlineData(2, "Filled_package.xlsx", "v2_Filled_package.xlsx")]
    [InlineData(1, "report.xlsx", "v1_Final_report.xlsx", true)]
    public void ForVersion_builds_expected_name(int version, string original, string expected, bool isFinal = false)
    {
        Assert.Equal(expected, PackageFileNaming.ForVersion(version, original, isFinal));
    }

    [Fact]
    public void ToFinalName_inserts_Final_after_version_prefix()
    {
        Assert.Equal("v3_Final_Filled_package.xlsx", PackageFileNaming.ToFinalName("v3_Filled_package.xlsx", 3));
    }

    [Fact]
    public void CountCompleted_counts_workflow_milestones_not_raw_files()
    {
        var docs = new List<StoredDocument>
        {
            Doc(DocumentKind.StaticReference),
            Doc(DocumentKind.SignedConfirmation),
            Doc(DocumentKind.ExcelPackage),
            Doc(DocumentKind.ExcelPackage),
            Doc(DocumentKind.ExcelPackage)
        };

        var latest = new EntityPackage { IsFinal = false };

        Assert.Equal(3, DocumentWorkflowProgress.CountCompleted(docs, latest));
    }

    [Fact]
    public void CountCompleted_includes_final_step_when_package_is_final()
    {
        var docs = new List<StoredDocument>
        {
            Doc(DocumentKind.StaticReference),
            Doc(DocumentKind.SignedConfirmation),
            Doc(DocumentKind.ExcelPackage)
        };

        var latest = new EntityPackage { IsFinal = true };

        Assert.Equal(4, DocumentWorkflowProgress.CountCompleted(docs, latest));
    }

    private static StoredDocument Doc(DocumentKind kind) =>
        new()
        {
            Kind = kind,
            FileName = "test.xlsx",
            StoragePath = "period/entity/packages/test.xlsx"
        };
}
