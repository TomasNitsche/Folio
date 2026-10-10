using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Integrations;
using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Services;
using Folio.Application.Mappings;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Services;

public sealed class DocumentService(
    IEntityAccessService entityAccessService,
    IPeriodRepository periodRepository,
    IPeriodEntityRepository periodEntityRepository,
    IDocumentRepository documentRepository,
    IUserRepository userRepository,
    IAssignmentRepository assignmentRepository,
    IFileStorage fileStorage,
    IAuditRepository auditRepository) : IDocumentService
{
    public async Task<IReadOnlyList<DocumentDto>> GetDocumentsAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.View, ct);
        var docs = await documentRepository.GetByPeriodEntityAsync(periodEntityId, ct);
        return docs.Select(DocumentMapper.ToDto).ToList();
    }

    public async Task<DocumentDto> UploadStaticAsync(
        Guid periodEntityId,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.UploadStatic, ct);
        return await UploadAsync(periodEntityId, fileName, content, uploadedBy, DocumentKind.StaticReference, "StaticUploaded", ct);
    }

    public async Task<DocumentDto> UploadSignedAsync(
        Guid periodEntityId,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.UploadSigned, ct);
        return await UploadAsync(periodEntityId, fileName, content, uploadedBy, DocumentKind.SignedConfirmation, "SignedUploaded", ct);
    }

    public async Task<(Stream Content, string ContentType, string FileName)?> DownloadAsync(Guid documentId, CancellationToken ct = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, ct)
            ?? throw new KeyNotFoundException("Document not found.");

        if (document.PeriodEntityId is not { } periodEntityId)
            throw new InvalidOperationException("Document is not linked to a period entity.");

        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.View, ct);

        var file = await fileStorage.GetByStoragePathAsync(document.StoragePath, ct);
        return file;
    }

    public async Task AssignUserAsync(Guid periodEntityId, AssignUserRequest request, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.AssignUsers, ct);

        _ = await periodEntityRepository.GetByIdAsync(periodEntityId, ct)
            ?? throw new KeyNotFoundException("Period entity not found.");

        var user = await userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        await assignmentRepository.AddAsync(new EntityAssignment
        {
            Id = Guid.NewGuid(),
            PeriodEntityId = periodEntityId,
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            CanConfirm = request.CanConfirm,
            CanFillPackage = request.CanFillPackage
        }, ct);

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(PeriodEntity),
            EntityId = periodEntityId,
            Action = "UserAssigned",
            PerformedBy = user.UserName,
            PerformedAt = DateTime.UtcNow,
            Details = $"{user.Email} confirm={request.CanConfirm} pack={request.CanFillPackage}"
        }, ct);
    }

    public async Task<EntityAssignmentsDto> GetAssignmentsAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.View, ct);

        var assignments = await assignmentRepository.GetByPeriodEntityIdAsync(periodEntityId, ct);
        return ToAssignmentsDto(assignments);
    }

    public async Task SetAssignmentsAsync(
        Guid periodEntityId,
        SetEntityAssignmentsRequest request,
        string performedBy,
        CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessEntityAsync(periodEntityId, EntityAccessAction.AssignUsers, ct);

        _ = await periodEntityRepository.GetByIdAsync(periodEntityId, ct)
            ?? throw new KeyNotFoundException("Period entity not found.");

        if (request.ConfirmerUserId is Guid confirmerId)
            await ValidateAssignmentUserAsync(confirmerId, UserRole.Confirmer, "Confirmer", ct);

        if (request.PackFillerUserId is Guid packFillerId)
            await ValidateAssignmentUserAsync(packFillerId, UserRole.PackFiller, "Pack filler", ct);

        await assignmentRepository.ReplaceRoleAssignmentsAsync(
            periodEntityId,
            request.ConfirmerUserId,
            request.PackFillerUserId,
            ct);

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(PeriodEntity),
            EntityId = periodEntityId,
            Action = "AssignmentsUpdated",
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            Details = $"confirmer={request.ConfirmerUserId} packFiller={request.PackFillerUserId}"
        }, ct);
    }

    public async Task<BulkUploadResultDto> BulkUploadStaticAsync(
        Guid periodId,
        IReadOnlyList<Guid> periodEntityIds,
        string fileName,
        Stream content,
        string uploadedBy,
        CancellationToken ct = default)
    {
        await entityAccessService.EnsureRoleCapabilityAsync(EntityAccessAction.UploadStatic, ct);

        if (periodEntityIds.Count == 0)
            throw new ArgumentException("Select at least one entity.");

        _ = await periodRepository.GetByIdAsync(periodId, ct)
            ?? throw new KeyNotFoundException("Period not found.");

        var periodEntities = await periodEntityRepository.GetByPeriodIdAsync(periodId, ct);
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
                await UploadAsync(entityId, fileName, buffer, uploadedBy, DocumentKind.StaticReference, "StaticUploaded", ct);
                succeeded.Add(new BulkUploadItemResult(entityId, entity.Name, null));
            }
            catch (Exception ex)
            {
                failed.Add(new BulkUploadItemResult(entityId, entity.Name, ex.Message));
            }
        }

        return new BulkUploadResultDto(succeeded.Count, failed.Count, succeeded, failed);
    }

    private async Task ValidateAssignmentUserAsync(Guid userId, UserRole expectedRole, string roleLabel, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"{roleLabel} user not found.");

        if (!user.IsActive)
            throw new InvalidOperationException($"{roleLabel} user is inactive.");

        if (user.Role != expectedRole)
            throw new InvalidOperationException($"{user.DisplayName} is not a {roleLabel.ToLowerInvariant()} user.");
    }

    private static EntityAssignmentsDto ToAssignmentsDto(IReadOnlyList<EntityAssignment> assignments)
    {
        EntityAssignmentDto? ToDto(EntityAssignment a) =>
            new(a.Id, a.UserId, a.UserName ?? "", a.DisplayName ?? "", a.Email ?? "", a.CanConfirm, a.CanFillPackage);

        var confirmer = assignments.FirstOrDefault(a => a.CanConfirm);
        var packFiller = assignments.FirstOrDefault(a => a.CanFillPackage);

        return new EntityAssignmentsDto(
            confirmer is null ? null : ToDto(confirmer),
            packFiller is null ? null : ToDto(packFiller));
    }

    private async Task<DocumentDto> UploadAsync(
        Guid periodEntityId,
        string fileName,
        Stream content,
        string uploadedBy,
        DocumentKind kind,
        string auditAction,
        CancellationToken ct)
    {
        var entity = await periodEntityRepository.GetByIdAsync(periodEntityId, ct)
            ?? throw new KeyNotFoundException("Period entity not found.");

        var periodCode = entity.Period?.Code
            ?? throw new InvalidOperationException("Period not loaded.");

        await fileStorage.SaveAsync(periodCode, entity.Code, kind, fileName, content, ct);

        var document = new StoredDocument
        {
            Id = Guid.NewGuid(),
            PeriodEntityId = periodEntityId,
            Kind = kind,
            FileName = fileName,
            StoragePath = $"{periodCode}/{entity.Code}/{KindFolder(kind)}/{fileName}",
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        await documentRepository.AddAsync(document, ct);

        await auditRepository.AddAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(StoredDocument),
            EntityId = document.Id,
            Action = auditAction,
            PerformedBy = uploadedBy,
            PerformedAt = DateTime.UtcNow,
            Details = fileName
        }, ct);

        return DocumentMapper.ToDto(document);
    }

    private static string KindFolder(DocumentKind kind) => kind switch
    {
        DocumentKind.StaticReference => "static",
        DocumentKind.SignedConfirmation => "signed",
        DocumentKind.ExcelPackage => "packages",
        _ => "other"
    };
}
