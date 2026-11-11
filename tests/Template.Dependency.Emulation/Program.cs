// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace Template.Dependency.Emulation;

public static class Program
{
    private static void Main(string[] args)
    {
        bool appConfigured = false;

        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });

            ConfigureServices(builder);

            WebApplication app = builder.Build();

            ConfigureApp(app);

            appConfigured = true;
            app.Run();
        }
        catch (Exception e)
        {
            if (!appConfigured) // do not log twice exceptions from app.Run()
            {
                logger.Error(e); // log configuration exceptions in correct format instead of plain text
            }
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager config = builder.Configuration;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = config.GetValue<string>("Auth:Authority");
                options.Audience = config.GetValue<string>("Auth:Audience");
            });

        services.AddAuthorization();

        services.AddControllers();

        services.AddProblemDetails();
    }

    private static void ConfigureApp(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.Map("api/status/healthz/live", () => Results.Ok());
    }
}
