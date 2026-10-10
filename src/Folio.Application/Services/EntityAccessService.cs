using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Security;
using Folio.Application.Interfaces.Services;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Application.Services;

public sealed class EntityAccessService(
    ICurrentUserAccessor currentUserAccessor,
    IPeriodEntityRepository periodEntityRepository,
    IPeriodRepository periodRepository) : IEntityAccessService
{
    public IReadOnlyList<PeriodEntity> FilterEntitiesForUser(
        IReadOnlyList<PeriodEntity> entities,
        Guid userId,
        UserRole role)
    {
        if (RolePermissions.CanViewAllEntities(role))
            return entities;

        return role switch
        {
            UserRole.Confirmer => entities.Where(e => IsAssignedConfirmer(e, userId)).ToList(),
            UserRole.PackFiller => entities.Where(e => IsAssignedPackFiller(e, userId)).ToList(),
            _ => []
        };
    }

    public async Task EnsureCanAccessEntityAsync(
        Guid periodEntityId,
        EntityAccessAction action,
        CancellationToken ct = default)
    {
        var user = currentUserAccessor.GetRequired();

        if (IsInternalOnlyAction(action))
        {
            EnsureInternalRoleAction(user.Role, action);
            return;
        }

        var entity = await periodEntityRepository.GetByIdAsync(periodEntityId, ct)
            ?? throw new KeyNotFoundException("Period entity not found.");

        if (RolePermissions.CanViewAllEntities(user.Role))
        {
            EnsureInternalRoleAction(user.Role, action);
            return;
        }

        switch (action)
        {
            case EntityAccessAction.View:
                EnsureEntityViewAccess(entity, user.UserId, user.Role);
                break;
            case EntityAccessAction.UploadSigned:
                if (!RolePermissions.CanUploadSignedAtRoleLevel(user.Role) || !IsAssignedConfirmer(entity, user.UserId))
                    throw new UnauthorizedAccessException("You are not allowed to upload signed confirmations for this entity.");
                await EnsurePeriodAcceptsCompanyInputAsync(entity, ct);
                break;
            case EntityAccessAction.UploadPackage:
            case EntityAccessAction.MarkFinal:
                if (!RolePermissions.CanUploadPackageAtRoleLevel(user.Role) || !IsAssignedPackFiller(entity, user.UserId))
                    throw new UnauthorizedAccessException("You are not allowed to manage packages for this entity.");
                await EnsurePeriodAcceptsCompanyInputAsync(entity, ct);
                break;
            default:
                throw new UnauthorizedAccessException("You are not allowed to perform this action.");
        }
    }

    public Task EnsureRoleCapabilityAsync(EntityAccessAction action, CancellationToken ct = default)
    {
        var user = currentUserAccessor.GetRequired();
        EnsureInternalRoleAction(user.Role, action);
        return Task.CompletedTask;
    }

    public Task EnsureCanAccessAdminAsync(CancellationToken ct = default)
    {
        var user = currentUserAccessor.GetRequired();
        if (!RolePermissions.CanAccessAdmin(user.Role))
            throw new UnauthorizedAccessException("Administrator access is required.");

        return Task.CompletedTask;
    }

    public bool IsAssignedConfirmer(PeriodEntity entity, Guid userId) =>
        entity.Assignments.Any(a => a.UserId == userId && a.CanConfirm);

    public bool IsAssignedPackFiller(PeriodEntity entity, Guid userId) =>
        entity.Assignments.Any(a => a.UserId == userId && a.CanFillPackage);

    /// <summary>
    /// Company users may browse any period, but they only upload into the period the reporting team marked as
    /// default. When no period of that type is marked yet, nothing is locked.
    /// </summary>
    private async Task EnsurePeriodAcceptsCompanyInputAsync(PeriodEntity entity, CancellationToken ct)
    {
        var period = entity.Period ?? await periodRepository.GetByIdAsync(entity.PeriodId, ct);
        if (period is null || period.IsDefault)
            return;

        var periodsOfType = await periodRepository.GetByTypeAsync(period.ConfirmationType, ct);
        var defaultPeriod = periodsOfType.FirstOrDefault(p => p.IsDefault);
        if (defaultPeriod is null)
            return;

        throw new UnauthorizedAccessException(
            $"Period {period.Code} is read-only. The reporting team is currently collecting {defaultPeriod.Code}.");
    }

    private static bool IsInternalOnlyAction(EntityAccessAction action) =>
        action is EntityAccessAction.UploadStatic or EntityAccessAction.AssignUsers;

    private static void EnsureInternalRoleAction(UserRole role, EntityAccessAction action)
    {
        var allowed = action switch
        {
            EntityAccessAction.UploadStatic => RolePermissions.CanUploadStaticDocuments(role),
            EntityAccessAction.AssignUsers => RolePermissions.CanAssignEntityUsers(role),
            EntityAccessAction.OpenPeriod => RolePermissions.CanOpenPeriod(role),
            EntityAccessAction.UploadSigned => RolePermissions.CanUploadSignedAtRoleLevel(role),
            EntityAccessAction.UploadPackage or EntityAccessAction.MarkFinal =>
                RolePermissions.CanUploadPackageAtRoleLevel(role),
            EntityAccessAction.View => RolePermissions.CanViewAllEntities(role)
                || RolePermissions.CanUploadSignedAtRoleLevel(role)
                || RolePermissions.CanUploadPackageAtRoleLevel(role),
            _ => false
        };

        if (!allowed)
            throw new UnauthorizedAccessException("You are not allowed to perform this action.");
    }

    private static void EnsureEntityViewAccess(PeriodEntity entity, Guid userId, UserRole role)
    {
        var allowed = role switch
        {
            UserRole.Confirmer => entity.Assignments.Any(a => a.UserId == userId && a.CanConfirm),
            UserRole.PackFiller => entity.Assignments.Any(a => a.UserId == userId && a.CanFillPackage),
            _ => false
        };

        if (!allowed)
            throw new UnauthorizedAccessException("You are not allowed to view this entity.");
    }
}
