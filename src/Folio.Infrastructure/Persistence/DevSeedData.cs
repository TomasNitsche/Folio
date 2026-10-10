using Folio.Domain.Enums;

namespace Folio.Infrastructure.Persistence;

public static class DevSeedData
{
    public sealed record UserSeed(Guid Id, string UserName, string DisplayName, string Email, UserRole Role);

    public sealed record CompanySeed(int RegistryEntityId, string Name, string Code, DateOnly ValidFrom, DateOnly? ValidTo = null);

    public static readonly UserSeed[] Users =
    [
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "test.testovic",  "Test Testovič",   "test.testovic@folio.local",  UserRole.Administrator),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), "folio.admin",   "Folio Admin",    "admin@folio.local",          UserRole.Administrator),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), "folio.lead",    "Reporting Lead",  "lead@folio.local",           UserRole.ReportingLead),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), "karel.gott",     "Karel Gott",      "karel.gott@example.com",      UserRole.Confirmer),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"), "jan.novak",      "Jan Novák",       "jan.novak@example.com",       UserRole.Confirmer),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6"), "marie.vesela",   "Marie Veselá",    "marie.vesela@example.com",    UserRole.Confirmer),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7"), "petr.horak",     "Petr Horák",      "petr.horak@example.com",      UserRole.PackFiller),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa8"), "eva.kralova",    "Eva Králová",     "eva.kralova@example.com",     UserRole.PackFiller),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa9"), "folio.uploader","Folio Uploader", "uploader@folio.local",       UserRole.Contributor),
    ];

    public static readonly CompanySeed[] Companies = FakeClubDirectory.All
        .Select(c => new CompanySeed(c.RegistryEntityId, c.Name, c.Code, new DateOnly(2020, 1, 1)))
        .ToArray();

    /// <summary>Demo transfers period entity IDs and package states. Version -1 = no package yet.</summary>
    public static readonly (int RegistryEntityId, PackageStatus Status, int Version, bool IsFinal, Guid? AssignUserId)[] DemoPeriodEntities =
    [
        (101, PackageStatus.Pending,   0, false, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4")),
        (102, PackageStatus.Pending,   2, false, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5")),
        (103, PackageStatus.Confirmed, 1, true,  Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6")),
        (104, PackageStatus.Draft,    -1, false, null),
        (105, PackageStatus.Rejected,  1, false, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7")),
    ];

    public const string DemoPeriodCode = "2026M01";

    public static readonly DateOnly DemoPeriodStart = new(2026, 1, 1);

    public static readonly DateOnly DemoPeriodEnd = new(2026, 1, 31);

    public static readonly DateOnly DemoPeriodSnapshot = new(2026, 1, 1);
}
