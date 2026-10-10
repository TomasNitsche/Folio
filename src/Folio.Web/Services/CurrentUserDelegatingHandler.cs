using Folio.Web.Services;

namespace Folio.Web.Services;

public sealed class CurrentUserDelegatingHandler(CurrentUserOptions currentUser) : DelegatingHandler
{
    public const string UserIdHeader = "X-Folio-User-Id";
    public const string UserRoleHeader = "X-Folio-User-Role";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId != Guid.Empty)
            request.Headers.TryAddWithoutValidation(UserIdHeader, currentUser.UserId.ToString());

        request.Headers.TryAddWithoutValidation(UserRoleHeader, ((int)currentUser.AppRole).ToString());
        return base.SendAsync(request, cancellationToken);
    }
}
