// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Library.Common;

namespace Template.Library.IntegrationTests.API;

public class ControllerTestsBase : IntegrationTestsBase
{
    private JwtHelper _tokenHelper = null!;

    protected string ValidIssuer { get; private set; } = null!;

    protected string ValidAudience { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    public override async Task OnOneTimeSetUp()
    {
        await base.OnOneTimeSetUp();

        _tokenHelper = new JwtHelper();

        IConfiguration config = Factory.Services.GetRequiredService<IConfiguration>();
        ValidIssuer = config[$"{AppSection}:JwtBearer:Authority"].EnsureNotNull();
        ValidAudience = config[$"{AppSection}:JwtBearer:Audiences"].EnsureNotNull().SplitWithTrim()[0].EnsureNotNull();

        Client = Factory.CreateClient();
    }

    public override async Task OnOneTimeTearDown()
    {
        Client.Dispose();
        await base.OnOneTimeTearDown();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey = _tokenHelper.SecurityKey;
            });
        });
    }

    protected void SetupValidJwt(params Claim[] claims)
    {
        SetupJwt(ValidIssuer, ValidAudience, DateTime.UtcNow.AddMinutes(20), claims);
    }

    protected void SetupJwt(string issuer, string audience, DateTime expirationTime, params Claim[] claims)
    {
        string token = _tokenHelper.GenerateToken(issuer, audience, expirationTime, claims);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme, token);
    }

    protected async Task ShouldBeAuthorized(string scope, HttpMethod method, string url, object? body = null)
    {
        //Arrange
        SetupValidJwt(new Claim("scope", scope));

        using var request = new HttpRequestMessage(method, url);

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        //Act
        using HttpResponseMessage response = await Client.SendAsync(request);

        //Assert
        response.Should().HaveAnyOfStatusCodes([HttpStatusCode.BadRequest, HttpStatusCode.OK]);
    }

    protected async Task ShouldBeForbidden(string scope, HttpMethod method, string url, object? body = null)
    {
        //Arrange
        SetupValidJwt(new Claim("scope", scope));

        using var request = new HttpRequestMessage(method, url);

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        //Act
        using HttpResponseMessage response = await Client.SendAsync(request);

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.Forbidden)
            .ContentAsString.Should().BeEmpty();
    }
}
