using Folio.Domain.Enums;

namespace Folio.Application.DTOs.Documents;

public record DocumentDto(
    Guid Id,
    DocumentKind Kind,
    string FileName,
    string StoragePath,
    int? PackageVersion,
    string? UploadedBy,
    DateTime UploadedAt);

public record UploadDocumentRequest(
    Guid PeriodEntityId,
    DocumentKind Kind,
    string FileName,
    string UploadedBy);

public record DashboardSummaryDto(
    int TotalEntities,
    int PendingCount,
    int ConfirmedCount,
    int DraftCount,
    int OverdueCount);

public record BulkUploadItemResult(
    Guid PeriodEntityId,
    string? EntityName,
    string? Error);

public record BulkUploadResultDto(
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<BulkUploadItemResult> Succeeded,
    IReadOnlyList<BulkUploadItemResult> Failed);

public enum BulkUploadKind
{
    StaticReference,
    ExcelPackage
}
