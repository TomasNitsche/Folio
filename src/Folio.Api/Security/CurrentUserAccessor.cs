using Folio.Application.Interfaces.Security;
using Folio.Domain.Enums;

namespace Folio.Api.Security;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private CurrentUserContext? _current;

    public CurrentUserContext? Current => _current;

    public CurrentUserContext GetRequired() =>
        _current ?? throw new UnauthorizedAccessException("User context is required.");

    internal void SetUser(Guid userId, string userName, UserRole role) =>
        _current = new CurrentUserContext(userId, userName, role);
}
