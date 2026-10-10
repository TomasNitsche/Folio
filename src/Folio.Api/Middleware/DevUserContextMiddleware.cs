using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Enums;

namespace Folio.Api.Middleware;

public sealed class DevUserContextMiddleware(RequestDelegate next)
{
    public const string UserIdHeader = "X-Folio-User-Id";
    public const string UserRoleHeader = "X-Folio-User-Role";

    public async Task InvokeAsync(
        HttpContext context,
        Security.CurrentUserAccessor accessor,
        IUserRepository userRepository)
    {
        if (context.Request.Headers.TryGetValue(UserIdHeader, out var idHeader)
            && Guid.TryParse(idHeader.ToString(), out var userId)
            && context.Request.Headers.TryGetValue(UserRoleHeader, out var roleHeader)
            && Enum.TryParse(roleHeader.ToString(), ignoreCase: true, out UserRole role)
            && Enum.IsDefined(role))
        {
            var user = await userRepository.GetByIdAsync(userId, context.RequestAborted);
            if (user is not null && user.IsActive && user.Role == role)
                accessor.SetUser(user.Id, user.UserName, user.Role);
        }

        await next(context);
    }
}
