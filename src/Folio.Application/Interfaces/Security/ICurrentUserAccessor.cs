using Folio.Domain.Enums;

namespace Folio.Application.Interfaces.Security;

public sealed record CurrentUserContext(Guid UserId, string UserName, UserRole Role);

public interface ICurrentUserAccessor
{
    CurrentUserContext? Current { get; }

    CurrentUserContext GetRequired();
}
