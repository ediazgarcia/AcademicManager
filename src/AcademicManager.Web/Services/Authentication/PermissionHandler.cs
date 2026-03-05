using Microsoft.AspNetCore.Authorization;

namespace AcademicManager.Web.Services.Authentication;

public class PermissionHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.Requirements)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
