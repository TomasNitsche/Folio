using Folio.Application;
using Folio.Domain.Enums;
using Xunit;

namespace Folio.Application.Tests;

public class RolePermissionsTests
{
    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.Contributor, true)]
    [InlineData(UserRole.ReportingLead, true)]
    [InlineData(UserRole.Confirmer, false)]
    [InlineData(UserRole.PackFiller, false)]
    public void CanUploadStaticDocuments_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanUploadStaticDocuments(role));

    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.ReportingLead, true)]
    [InlineData(UserRole.Contributor, true)]
    [InlineData(UserRole.Confirmer, false)]
    public void CanAssignEntityUsers_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanAssignEntityUsers(role));

    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.ReportingLead, true)]
    [InlineData(UserRole.Contributor, true)]
    [InlineData(UserRole.Confirmer, false)]
    [InlineData(UserRole.PackFiller, false)]
    public void CanViewAllEntities_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanViewAllEntities(role));

    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.ReportingLead, true)]
    [InlineData(UserRole.Contributor, false)]
    public void CanOpenPeriod_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanOpenPeriod(role));

    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.ReportingLead, false)]
    public void CanAccessAdmin_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanAccessAdmin(role));

    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.Confirmer, true)]
    [InlineData(UserRole.PackFiller, false)]
    public void CanUploadSignedAtRoleLevel_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanUploadSignedAtRoleLevel(role));

    [Theory]
    [InlineData(UserRole.Administrator, true)]
    [InlineData(UserRole.PackFiller, true)]
    [InlineData(UserRole.Confirmer, false)]
    public void CanUploadPackageAtRoleLevel_returns_expected(UserRole role, bool expected) =>
        Assert.Equal(expected, RolePermissions.CanUploadPackageAtRoleLevel(role));
}
