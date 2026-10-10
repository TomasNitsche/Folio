using Folio.Application.DTOs.Users;
using Folio.Domain.Enums;

namespace Folio.Application;

public static class RoleDefinitions
{
    public static IReadOnlyList<RoleDefinitionDto> All { get; } =
    [
        new(
            UserRole.Administrator,
            "Administrator",
            "Full access to all reporting areas and administration.",
            ["Manage users, roles and view all entities", "Open and manage periods", "Upload and confirm packages"]),
        new(
            UserRole.ReportingLead,
            "Reporting lead",
            "Folio role — opens reporting periods and selects companies from the Registry snapshot.",
            ["Open periods and select snapshot entities", "View all entities in a period and dashboards", "Bulk upload static files to multiple entities", "Assign confirmer and pack filler" ]),
        new(
            UserRole.Contributor,
            "Contributor",
            "Folio role — maintains static reference documents and coordinates company users per entity.",
            ["Upload static reference documents (budget pack)", "Bulk upload static files to multiple entities", "Assign confirmer and pack filler to entities", "View all entities in a period"]),
        new(
            UserRole.Confirmer,
            "Confirmer",
            "Reviews and confirms club packages.",
            ["View assigned entities", "Upload signed confirmations", "Confirm packages"]),
        new(
            UserRole.PackFiller,
            "Pack filler",
            "Uploads Excel reporting packages for assigned entities.",
            ["View assigned entities", "Upload Excel packages", "Mark package as final"])
    ];
}
