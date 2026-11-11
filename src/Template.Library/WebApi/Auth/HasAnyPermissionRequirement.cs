// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Template.Library.Common;

namespace Template.Library.WebApi.Auth;

public sealed class HasAnyPermissionRequirement(string permissions)
    : AuthorizationHandler<HasAnyPermissionRequirement>, IAuthorizationRequirement
{
    private static readonly string[] s_claimTypes = ["permissions", "scope"];

    public IReadOnlyCollection<string> Permissions { get; } = permissions.SplitWithTrim();

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, HasAnyPermissionRequirement requirement)
    {
        foreach (string type in s_claimTypes)
        {
            if (context.User.HasClaim(c => c.Type == type && c.Value.Split(' ').Any(requirement.Permissions.Contains)))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
