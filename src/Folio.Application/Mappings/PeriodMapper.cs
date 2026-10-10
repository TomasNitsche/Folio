using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Packages;
using Folio.Application.DTOs.Periods;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Mappings;

public static class PeriodMapper
{
    public static PeriodDto ToDto(ReportingPeriod period) =>
        new(
            period.Id,
            period.ConfirmationType,
            period.Code,
            period.StartDate,
            period.EndDate,
            period.Status,
            period.SnapshotDate,
            period.OpenedBy,
            period.CreatedAt,
            period.Entities.Count,
            period.IsDefault);

    public static RegistryEntityDto ToDto(RegistryEntitySnapshot entity) =>
        new(entity.RegistryEntityId, entity.Name, entity.Code, entity.ValidOn, entity.RegistrationNumber);

    public static PeriodEntityDto ToDto(PeriodEntity entity)
    {
        var latest = entity.Packages.OrderByDescending(p => p.Version).FirstOrDefault();
        var responsible = entity.Assignments.FirstOrDefault(a => a.CanConfirm)?.DisplayName
            ?? entity.Assignments.FirstOrDefault(a => a.CanConfirm)?.UserName;

        return new PeriodEntityDto(
            entity.Id,
            entity.PeriodId,
            entity.RegistryEntityId,
            entity.Name,
            entity.Code,
            entity.SnapshotDate,
            latest?.Version ?? 0,
            latest?.IsFinal ?? false,
            latest?.Status ?? PackageStatus.Draft,
            responsible,
            latest?.UpdatedAt ?? DateTime.MinValue);
    }

    public static PackageGridRowDto ToGridRow(
        PeriodEntity entity,
        EntityPackage? latestPackage,
        IReadOnlyList<StoredDocument> documents,
        int documentsExpected = DocumentWorkflowProgress.ExpectedSteps)
    {
        var responsible = FormatResponsiblePerson(entity.Assignments);

        return new PackageGridRowDto(
            entity.Id,
            entity.RegistryEntityId,
            entity.Name,
            entity.Code,
            latestPackage?.Version ?? 0,
            latestPackage is not null,
            latestPackage?.IsFinal ?? false,
            EntityWorkflowState.DeriveStatus(documents, latestPackage),
            responsible,
            DocumentWorkflowProgress.CountCompleted(documents, latestPackage),
            documentsExpected,
            EntityWorkflowState.LastActivityAt(documents, latestPackage));
    }

    private static string? FormatResponsiblePerson(IEnumerable<EntityAssignment> assignments)
    {
        var confirmer = assignments.FirstOrDefault(a => a.CanConfirm);
        var packFiller = assignments.FirstOrDefault(a => a.CanFillPackage);
        var parts = new List<string>();

        if (confirmer is not null)
            parts.Add(confirmer.DisplayName ?? confirmer.UserName ?? "Confirmer");

        if (packFiller is not null)
            parts.Add(packFiller.DisplayName ?? packFiller.UserName ?? "Pack filler");

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }
}

public static class PackageMapper
{
    public static PackageDto ToDto(EntityPackage package) =>
        new(
            package.Id,
            package.PeriodEntityId,
            package.Version,
            package.IsFinal,
            package.Status,
            package.CreatedAt,
            package.UpdatedAt,
            package.CreatedBy);
}

public static class DocumentMapper
{
    public static DocumentDto ToDto(StoredDocument document) =>
        new(
            document.Id,
            document.Kind,
            document.FileName,
            document.StoragePath,
            document.PackageVersion,
            document.UploadedBy,
            document.UploadedAt);
}

/// <summary>
/// Lightweight Registry entity snapshot used before persisting to period. RegistrationNumber  is not persisted onto
/// PeriodEntity yet — it's carried through up to entity-selection time (RegistryEntityDto) for future detail/list use.
/// </summary>
public record RegistryEntitySnapshot(int RegistryEntityId, string Name, string Code, DateOnly ValidOn, string? RegistrationNumber = null);
