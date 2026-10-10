using Folio.Application.Mappings;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Tests;

public sealed class PeriodMapperTests
{
    [Fact]
    public void ToDto_maps_RegistryEntitySnapshot_fields_including_RegistrationNumber()
    {
        var snapshot = new RegistryEntitySnapshot(42, "Acme HC", "ACM", new DateOnly(2026, 1, 1), "12345678");

        var dto = PeriodMapper.ToDto(snapshot);

        Assert.Equal(42, dto.Id);
        Assert.Equal("Acme HC", dto.Name);
        Assert.Equal("ACM", dto.Code);
        Assert.Equal(new DateOnly(2026, 1, 1), dto.ValidOn);
        Assert.Equal("12345678", dto.RegistrationNumber);
    }

    [Fact]
    public void ToDto_maps_null_RegistrationNumber_through()
    {
        var snapshot = new RegistryEntitySnapshot(42, "Acme HC", "ACM", new DateOnly(2026, 1, 1));

        var dto = PeriodMapper.ToDto(snapshot);

        Assert.Null(dto.RegistrationNumber);
    }

    [Fact]
    public void ToGridRow_without_documents_is_Draft_with_no_update_timestamp()
    {
        var row = PeriodMapper.ToGridRow(CreateEntity(), latestPackage: null, documents: []);

        Assert.Equal(PackageStatus.Draft, row.Status);
        Assert.Null(row.UpdatedAt);
        Assert.Equal(0, row.DocumentsUploaded);
    }

    [Fact]
    public void ToGridRow_after_static_upload_is_Pending_and_shows_the_upload_time()
    {
        var uploadedAt = new DateTime(2026, 9, 22, 9, 3, 0);
        var documents = new List<StoredDocument>
        {
            new()
            {
                Kind = DocumentKind.StaticReference,
                FileName = "directive.pdf",
                StoragePath = "2026M06/CEZ/static/directive.pdf",
                UploadedAt = uploadedAt
            }
        };

        var row = PeriodMapper.ToGridRow(CreateEntity(), latestPackage: null, documents: documents);

        Assert.Equal(PackageStatus.Pending, row.Status);
        Assert.Equal(uploadedAt, row.UpdatedAt);
        Assert.Equal(1, row.DocumentsUploaded);
    }

    private static PeriodEntity CreateEntity() => new()
    {
        Id = Guid.NewGuid(),
        PeriodId = Guid.NewGuid(),
        RegistryEntityId = 100,
        Name = "HC Dynamo Pardubice",
        Code = "CEZ",
        SnapshotDate = new DateOnly(2026, 6, 1)
    };
}
