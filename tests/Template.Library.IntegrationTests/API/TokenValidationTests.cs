// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.IntegrationTests;

namespace Template.Library.IntegrationTests.API;

[TestFixture]
public class TokenValidationTests : ControllerTestsBase
{
    private const string Endpoint = "api/v1/books";

    private readonly Claim _validScope = new("scope", "books:r");

    [Test]
    public async Task WebApi_ValidToken_Authorized()
    {
        //Arrange
        SetupJwt(ValidIssuer, ValidAudience, DateTime.UtcNow.AddHours(1), _validScope);

        //Act
        using HttpResponseMessage response = await Client.GetAsync(Endpoint);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task WebApi_InvalidAudienceToken_Unauthorized()
    {
        //Arrange
        SetupJwt(ValidIssuer, "invalid", DateTime.UtcNow.AddHours(1), _validScope);

        //Act
        using HttpResponseMessage response = await Client.GetAsync(Endpoint);

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.Unauthorized)
            .ContentAsString.Should().BeEmpty();
    }

    [Test]
    public async Task WebApi_InvalidIssuerToken_Unauthorized()
    {
        //Arrange
        SetupJwt("https://invalid", ValidAudience, DateTime.UtcNow.AddHours(1), _validScope);

        //Act
        using HttpResponseMessage response = await Client.GetAsync(Endpoint);

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.Unauthorized)
            .ContentAsString.Should().BeEmpty();
    }

    [Test]
    public async Task WebApi_ExpiredToken_Unauthorized()
    {
        //Arrange
        SetupJwt(ValidIssuer, ValidAudience, DateTime.UtcNow.AddHours(-1), _validScope);

        //Act
        using HttpResponseMessage response = await Client.GetAsync(Endpoint);

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.Unauthorized)
            .ContentAsString.Should().BeEmpty();
    }
}
