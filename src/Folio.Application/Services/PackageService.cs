using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Packages;
using Folio.Application.Interfaces.Integrations;
using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Security;
using Folio.Application.Interfaces.Services;
using Folio.Application.Mappings;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Services;

public sealed class PackageService(
    ICurrentUserAccessor currentUserAccessor,
    IEntityAccessService entityAccessService,
    IPeriodEntityRepository periodEntityRepository,
    IPackageRepository packageRepository,
    IDocumentRepository documentRepository,
    IFileStorage fileStorage,
    IAuditRepository auditRepository) : IPackageService
{
    public async Task<IReadOnlyList<PackageGridRowDto>> GetGridAsync(Guid periodId, CancellationToken ct = default)
    {
        var user = currentUserAccessor.GetRequired();
        var entities = await periodEntityRepository.GetByPeriodIdAsync(periodId, ct);
        entities = entityAccessService.FilterEntitiesForUser(entities, user.UserId, user.Role);

        var rows = new List<PackageGridRowDto>();
        foreach (var entity in entities)
        {
            var docs = await documentRepository.GetByPeriodEntityAsync(entity.Id, ct);
            var latest = await packageRepository.GetLatestAsync(entity.Id, ct);
            rows.Add(PeriodMapper.ToGridRow(entity, latest, docs));
        }
        return rows;
    }

    public async Task<PackageDto?> GetLatestPackageAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.View, ct);
        var package = await packageRepository.GetLatestAsync(periodEntityId, ct);
        return package is null ? null : PackageMapper.ToDto(package);
    }

    public async Task<PackageDto> UploadPackageAsync(
        Guid periodEntityId,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.UploadPackage, ct);

        var periodEntity = await periodEntityRepository.GetByIdAsync(periodEntityId, ct)
            ?? throw new KeyNotFoundException("Period entity not found.");

        var periodCode = periodEntity.Period?.Code
            ?? throw new InvalidOperationException("Period not loaded.");

        var latest = await packageRepository.GetLatestAsync(periodEntityId, ct);
        if (latest?.IsFinal == true)
            throw new InvalidOperationException("Package is final. Upload a new version is not allowed.");

        var nextVersion = (latest?.Version ?? -1) + 1;
        var package = new EntityPackage
        {
            Id = Guid.NewGuid(),
            PeriodEntityId = periodEntityId,
            Version = nextVersion,
            IsFinal = false,
            Status = PackageStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = uploadedBy
        };
        await packageRepository.AddAsync(package, ct);

        var versionedFileName = PackageFileNaming.ForVersion(package.Version, fileName);
        await fileStorage.SaveAsync(periodCode, periodEntity.Code, DocumentKind.ExcelPackage, versionedFileName, content, ct);

        var document = new StoredDocument
        {
            Id = Guid.NewGuid(),
            PackageId = package.Id,
            PeriodEntityId = periodEntityId,
            Kind = DocumentKind.ExcelPackage,
            FileName = versionedFileName,
            StoragePath = $"{periodCode}/{periodEntity.Code}/packages/{versionedFileName}",
            PackageVersion = package.Version,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };
        await documentRepository.AddAsync(document, ct);

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(EntityPackage),
            EntityId = package.Id,
            Action = "PackageUploaded",
            PerformedBy = uploadedBy,
            PerformedAt = DateTime.UtcNow,
            Details = $"Version {package.Version}: {versionedFileName}"
        }, ct);

        return PackageMapper.ToDto(package);
    }

    public async Task<PackageDto> MarkFinalAsync(Guid periodEntityId, MarkPackageFinalRequest request, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.MarkFinal, ct);

        var package = await packageRepository.GetLatestAsync(periodEntityId, ct)
            ?? throw new KeyNotFoundException("No package found.");

        if (package.IsFinal)
            throw new InvalidOperationException("Package is already final.");

        var documents = await documentRepository.GetByPeriodEntityAsync(periodEntityId, ct);
        var packageDocument = documents
            .Where(d => d.PackageId == package.Id && d.Kind == DocumentKind.ExcelPackage)
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No package file found for the current version.");

        var finalFileName = PackageFileNaming.ToFinalName(packageDocument.FileName, package.Version);
        var newStoragePath = await fileStorage.RenameFileAsync(packageDocument.StoragePath, finalFileName, ct);

        packageDocument.FileName = finalFileName;
        packageDocument.StoragePath = newStoragePath;
        await documentRepository.UpdateFileAsync(packageDocument, ct);

        package.IsFinal = true;
        package.Status = PackageStatus.Confirmed;
        package.UpdatedAt = DateTime.UtcNow;
        await packageRepository.UpdateAsync(package, ct);

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(EntityPackage),
            EntityId = package.Id,
            Action = "PackageMarkedFinal",
            PerformedBy = request.UpdatedBy,
            PerformedAt = DateTime.UtcNow,
            Details = $"Version {package.Version} marked final: {finalFileName}"
        }, ct);

        return PackageMapper.ToDto(package);
    }

    public async Task<BulkUploadResultDto> BulkUploadPackageAsync(
        Guid periodId,
        IReadOnlyList<Guid> periodEntityIds,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default)
    {
        await entityAccessService.EnsureRoleCapabilityAsync(EntityAccessAction.UploadPackage, ct);

        if (periodEntityIds.Count == 0)
            throw new ArgumentException("Select at least one entity.");

        var periodEntities = await periodEntityRepository.GetByPeriodIdAsync(periodId, ct);
        if (periodEntities.Count == 0)
            throw new KeyNotFoundException("Period not found or has no entities.");

        var entityMap = periodEntities.ToDictionary(e => e.Id);
        var succeeded = new List<BulkUploadItemResult>();
        var failed = new List<BulkUploadItemResult>();

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);

        foreach (var entityId in periodEntityIds.Distinct())
        {
            if (!entityMap.TryGetValue(entityId, out var entity))
            {
                failed.Add(new BulkUploadItemResult(entityId, null, "Entity not found in this period."));
                continue;
            }

            try
            {
                buffer.Position = 0;
                await UploadPackageAsync(entityId, fileName, buffer, uploadedBy, ct);
                succeeded.Add(new BulkUploadItemResult(entityId, entity.Name, null));
            }
            catch (Exception ex)
            {
                failed.Add(new BulkUploadItemResult(entityId, entity.Name, ex.Message));
            }
        }

        return new BulkUploadResultDto(succeeded.Count, failed.Count, succeeded, failed);
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid periodId, CancellationToken ct = default)
    {
        var rows = await GetGridAsync(periodId, ct);

        return new DashboardSummaryDto(
            rows.Count,
            rows.Count(r => r.Status == PackageStatus.Pending),
            rows.Count(r => r.Status == PackageStatus.Confirmed),
            rows.Count(r => r.Status == PackageStatus.Draft),
            0);
    }
}
