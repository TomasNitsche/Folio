using Folio.Domain.Enums;

namespace Folio.Application.DTOs.Periods;

public record PeriodDto(
    Guid Id,
    ConfirmationTypeKind ConfirmationType,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    PeriodStatus Status,
    DateOnly SnapshotDate,
    string OpenedBy,
    DateTime CreatedAt,
    int EntityCount,
    bool IsDefault = false);

public record CreatePeriodRequest(
    ConfirmationTypeKind ConfirmationType,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly SnapshotDate,
    string OpenedBy);

public record OpenPeriodRequest(
    ConfirmationTypeKind ConfirmationType,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly SnapshotDate,
    string OpenedBy,
    IReadOnlyList<int> SelectedRegistryEntityIds);

public record RegistryEntityDto(
    int Id,
    string Name,
    string Code,
    DateOnly ValidOn,
    string? RegistrationNumber = null);

public record PeriodEntityDto(
    Guid Id,
    Guid PeriodId,
    int RegistryEntityId,
    string Name,
    string Code,
    DateOnly SnapshotDate,
    int CurrentVersion,
    bool IsFinal,
    PackageStatus Status,
    string? ResponsiblePerson,
    DateTime UpdatedAt);
