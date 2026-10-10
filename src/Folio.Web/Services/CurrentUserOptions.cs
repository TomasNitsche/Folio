namespace Folio.Web.Services;

using Folio.Domain.Enums;

public sealed class CurrentUserOptions
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "jan.novak";
    public string DisplayName { get; set; } = "Test Testovič";
    public string Role { get; set; } = "League office · Admin";
    public UserRole AppRole { get; set; } = UserRole.Administrator;
    public string Initials { get; set; } = "TT";
}
