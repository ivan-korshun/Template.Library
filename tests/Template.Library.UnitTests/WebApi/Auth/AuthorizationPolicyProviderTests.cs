// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Template.Library.WebApi.Auth;

namespace Template.Library.UnitTests.WebApi.Auth;

#pragma warning disable CA1001 // AuthorizationPolicyProvider is disposed in TearDown
[TestFixture]
public class AuthorizationPolicyProviderTests
{
    private AuthorizationOptions _options = default!;
    private AuthorizationPolicyProvider _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _options = new AuthorizationOptions();
        _sut = new AuthorizationPolicyProvider(new OptionsWrapper<AuthorizationOptions>(_options));
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task GetPolicy_Concurrency_NoExceptions()
    {
        // Arrange
        await Task.WhenAll(Enumerable.Repeat(true, 100000).Select(_ => Task.Run(async () =>
        {
            var policyName = Guid.NewGuid().ToString();

            // Act
            AuthorizationPolicy? policy = await _sut.GetPolicyAsync(policyName);

            // Assert
            policy.Should().NotBeNull();
            policy!.Requirements.Should().BeEquivalentTo([new HasAnyPermissionRequirement(policyName)]);
        })));
    }

    [Test]
    public async Task GetPolicy_NotExistingPolicyName_CreatesNew()
    {
        // Arrange
        var policyName = Guid.NewGuid().ToString();

        // Act
        AuthorizationPolicy? policy = await _sut.GetPolicyAsync(policyName);

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements.Should().BeEquivalentTo([new HasAnyPermissionRequirement(policyName)]);
    }

    [Test]
    public async Task GetPolicy_ExistingPolicyName_ReturnedInstance()
    {
        // Arrange
        var policyName = Guid.NewGuid().ToString();
        var permissionRequirement = new HasAnyPermissionRequirement(policyName);

        _options.AddPolicy(policyName, builder => builder.Requirements.Add(permissionRequirement));

        // Act
        AuthorizationPolicy? policy = await _sut.GetPolicyAsync(policyName);

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements.Should().HaveCount(1);
        policy.Requirements[0].Should().BeSameAs(permissionRequirement);
    }
}
