using Folio.Domain.Enums;

namespace Folio.Application;

public static class RolePermissions
{
    public static bool CanViewAllEntities(UserRole role) =>
        role is UserRole.Administrator or UserRole.ReportingLead or UserRole.Contributor;

    public static bool CanAssignEntityUsers(UserRole role) =>
        role is UserRole.Administrator or UserRole.ReportingLead or UserRole.Contributor;

    public static bool CanUploadStaticDocuments(UserRole role) =>
        role is UserRole.Administrator or UserRole.Contributor or UserRole.ReportingLead;

    public static bool CanOpenPeriod(UserRole role) =>
        role is UserRole.Administrator or UserRole.ReportingLead;

    public static bool CanAccessAdmin(UserRole role) =>
        role is UserRole.Administrator;

    public static bool CanUploadSignedAtRoleLevel(UserRole role) =>
        role is UserRole.Administrator or UserRole.Confirmer;

    public static bool CanUploadPackageAtRoleLevel(UserRole role) =>
        role is UserRole.Administrator or UserRole.PackFiller;

    /// <summary>Company-side roles — they only work in the period the reporting team marked as default.</summary>
    public static bool IsCompanyRole(UserRole role) =>
        role is UserRole.Confirmer or UserRole.PackFiller;

    public static bool CanMarkFinalAtRoleLevel(UserRole role) =>
        role is UserRole.Administrator or UserRole.PackFiller;
}
