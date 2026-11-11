// Copyright © Korshun Ivan. All rights reserved.

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;
using Template.Library.WebApi.Auth;

namespace Template.Library.UnitTests.WebApi.Auth;

public class HasAnyPermissionRequirementTests
{
    [TestCase("scopeName1", true)]
    [TestCase(",scopeName2,", true)]
    [TestCase("scopeName1,scopeName2", true)]
    [TestCase("scopeName1,invalidScopeName", true)]
    [TestCase("scopeName1,scopeName2,invalidScopeName", true)]
    [TestCase("invalidScopeName", false)]
    [TestCase(",invalidScopeName,", false)]
    [TestCase("validPermissionName", true)]
    [TestCase("validPermissionName2,", true)]
    [TestCase("validPermissionName,validPermissionName2", true)]
    [TestCase("validPermissionName,invalidPermissionName", true)]
    [TestCase("validPermissionName,validPermissionName2,invalidPermissionName", true)]
    [TestCase("invalidPermissionName", false)]
    [TestCase("invalidPermissionName,", false)]
    [TestCase("", false)]
    [TestCase(",", false)]
    public async Task HandleAsync_HasCorrectPermission(string permissions, bool expected)
    {
        // Arrange
        var sut = new HasAnyPermissionRequirement(permissions);

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("scope", "scopeName1 scopeName2", null, "validIssuer"),
                new Claim("permissions", "validPermissionName validPermissionName2", null, "validIssuer")
            ],
            "Test"));

        var context = new AuthorizationHandlerContext([sut], claimsPrincipal, null);

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().Be(expected);
    }
}
