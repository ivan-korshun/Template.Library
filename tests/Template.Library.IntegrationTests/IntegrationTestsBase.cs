// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Template.Library.IntegrationTests;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Resources are cleaned up in OnOneTimeTearDown")]
public abstract class IntegrationTestsBase
{
    public const string AppSection = "Library";

    private WebApplicationFactory<Program>? _rootFactory;
    private WebApplicationFactory<Program>? _factory;

    protected WebApplicationFactory<Program> Factory => _factory!;

    [OneTimeSetUp]
    public virtual Task OnOneTimeSetUp()
    {
        //Refer issue https://github.com/dotnet/aspnetcore/issues/40715
        Environment.SetEnvironmentVariable("DOTNET_HOST_FACTORY_RESOLVER_DEFAULT_TIMEOUT_IN_SECONDS", "100");

        _rootFactory = new WebApplicationFactory<Program>();
        _factory = _rootFactory.WithWebHostBuilder(ConfigureWebHost);

        return Task.CompletedTask;
    }

    [OneTimeTearDown]
    public virtual Task OnOneTimeTearDown()
    {
        _factory?.Dispose();
        _rootFactory?.Dispose();
        return Task.CompletedTask;
    }

    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
    }
}
