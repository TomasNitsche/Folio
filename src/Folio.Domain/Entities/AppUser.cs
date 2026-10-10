using Folio.Domain.Enums;

namespace Folio.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
