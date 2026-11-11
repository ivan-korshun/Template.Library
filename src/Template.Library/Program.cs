// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Prometheus;
using Prometheus.DotNetRuntime;
using Template.Library.Common;
using Template.Library.Common.Http;
using Template.Library.Common.Json;
using Template.Library.Common.Options;
using Template.Library.ProgramSetup;
using Template.Library.WebApi;
using Template.Library.WebApi.HealthCheck;

namespace Template.Library;

[ExcludeFromCodeCoverage]
public class Program
{
    private const string AppKey = "Library";

    private static readonly string s_appName = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyTitleAttribute>().EnsureNotNull().Title;

    private static readonly string s_userAgent =
        $"{s_appName}/{Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)}";

    public static void Main(string[] args)
    {
        Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });

            DotNetRuntimeStatsBuilder.Customize().StartCollectingMetrics();

            IConfigurationSection appSection = builder.Configuration.GetSection(AppKey);

            ConfigureServices(builder, appSection);

            WebApplication app = builder.Build();

            ConfigureApp(app, appSection);

            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Application stopped because of exception");
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    private static void ConfigureServices(WebApplicationBuilder builder, IConfiguration appSection)
    {
        IServiceCollection services = builder.Services;

        services.AddAuth(appSection);

        services
            .AddControllers(o =>
            {
                o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; // correct error on empty body
                o.Filters.Add<HttpResponseExceptionFilter>();
            })
            .AddJsonOptions(o =>
            {
                o.AllowInputFormatterExceptionMessages = false; // do not expose internal details
                o.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.Converters.Add(new ShortGuidConverter());
                o.SerializeEnumAsString();
            });

        services.AddProblemDetails(o => o.TrimFieldNames());

        services.ConfigureVersioningAndOpenApi();

        // For Open API
        services.ConfigureHttpJsonOptions(options => options.SerializeEnumAsString());

        services.AddCorsPolicy(appSection); // if API is called from browsers

        IConfigurationSection resilienceSection = appSection.GetSection("Resilience:Default");

        services.AddHttpClientBasics(resilienceSection, s_userAgent)
            .UseHttpClientMetrics();

        services.AddHttpRepositories(appSection);

        services.AddDbRepositories(appSection, resilienceSection);

        services
            .AddValidOptions<HealthChecksOptions>(appSection)
            .AddHealthChecks()
            .AddAuthCheck()
            .AddCredentialsCheck()
            .AddDbCheck()
            .AddOrganizationServiceChecks();
    }

    private static void ConfigureApp(WebApplication app, IConfiguration appSection)
    {
        // if hidden behind a proxy and need original request data
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost
        });

        // app.UseRouting(); -> done implicitly in NET 8

        app.UseCors(); // if API is called from browsers

        // Note: neither UseHttpsRedirection nor UseHsts are useful for REST API

        app.UseHttpMetrics();

        // app.UseAuthentication(); -> done implicitly in NET 8
        // app.UseAuthorization();  -> done implicitly in NET 8

        if (app.Environment.IsDevelopment())
        {
            app.LogSqlQueries();
        }

        app.UseOpenApi(appSection, s_appName);

        app.MapControllers();
        app.MapAppMetrics(appSection);
        app.MapHealthChecks();
    }
}
