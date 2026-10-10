using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Packages;
using Folio.Application.DTOs.Periods;
using Folio.Application.DTOs.Users;
using Folio.Domain.Enums;

namespace Folio.Application.Interfaces.Services;

public interface IPeriodService
{
    Task<IReadOnlyList<PeriodDto>> GetPeriodsAsync(ConfirmationTypeKind type, CancellationToken ct = default);
    Task<PeriodDto?> GetPeriodAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RegistryEntityDto>> FetchRegistryEntitiesAsync(DateOnly snapshotDate, CancellationToken ct = default);
    Task<PeriodDto> OpenPeriodAsync(OpenPeriodRequest request, CancellationToken ct = default);
    Task<PeriodDto> SetDefaultPeriodAsync(Guid periodId, string performedBy, CancellationToken ct = default);
}

public interface IPackageService
{
    Task<IReadOnlyList<PackageGridRowDto>> GetGridAsync(Guid periodId, CancellationToken ct = default);
    Task<PackageDto?> GetLatestPackageAsync(Guid periodEntityId, CancellationToken ct = default);
    Task<PackageDto> UploadPackageAsync(Guid periodEntityId, string fileName, Stream content, string uploadedBy, CancellationToken ct = default);
    Task<PackageDto> MarkFinalAsync(Guid periodEntityId, MarkPackageFinalRequest request, CancellationToken ct = default);
    Task<DashboardSummaryDto> GetSummaryAsync(Guid periodId, CancellationToken ct = default);
    Task<BulkUploadResultDto> BulkUploadPackageAsync(
        Guid periodId,
        IReadOnlyList<Guid> periodEntityIds,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default);
}

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentDto>> GetDocumentsAsync(Guid periodEntityId, CancellationToken ct = default);
    Task<DocumentDto> UploadStaticAsync(Guid periodEntityId, string fileName, Stream content, string uploadedBy, CancellationToken ct = default);
    Task<DocumentDto> UploadSignedAsync(Guid periodEntityId, string fileName, Stream content, string uploadedBy, CancellationToken ct = default);
    Task<(Stream Content, string ContentType, string FileName)?> DownloadAsync(Guid documentId, CancellationToken ct = default);
    Task AssignUserAsync(Guid periodEntityId, AssignUserRequest request, CancellationToken ct = default);
    Task<EntityAssignmentsDto> GetAssignmentsAsync(Guid periodEntityId, CancellationToken ct = default);
    Task SetAssignmentsAsync(Guid periodEntityId, SetEntityAssignmentsRequest request, string performedBy, CancellationToken ct = default);
    Task<BulkUploadResultDto> BulkUploadStaticAsync(
        Guid periodId,
        IReadOnlyList<Guid> periodEntityIds,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default);
}
