// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Template.Library.Common.Threading;

namespace Template.Library.WebApi.Auth;

public sealed class AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options), IDisposable
{
    private readonly AsyncLock _lock = new();
    private readonly AuthorizationOptions _options = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check static policies first
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        // Policy storage in base class is not thread safe
        using (await _lock.LockAsync())
        {
            policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new HasAnyPermissionRequirement(policyName))
                .Build();

            // Add policy to the AuthorizationOptions, so we don't have to re-create it each time
            _options.AddPolicy(policyName, policy);

            return policy;
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
