using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Byway.Infrastructure.Authorization
{
    public class SameUserHandler : AuthorizationHandler<SameUserRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameUserRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                var routeValues = mvcContext.RouteData.Values;
                if (!routeValues.TryGetValue("id", out var routeIdObj)) return Task.CompletedTask;

                if (!int.TryParse(routeIdObj?.ToString(), out var routeId)) return Task.CompletedTask;

                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null && int.Parse(userIdClaim) == routeId)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
