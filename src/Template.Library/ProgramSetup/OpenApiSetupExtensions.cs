// Copyright © Korshun Ivan. All rights reserved.

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Library.Common.Options;

namespace Template.Library.ProgramSetup;

public static class OpenApiSetupExtensions
{
    public static WebApplication UseOpenApi(this WebApplication app, IConfiguration config, string appName)
    {
        var options = config.GetValidOptions<OpenApiOptions>();
        if (!options.Enabled)
        {
            return app;
        }

        app.MapOpenApi().WithDocumentPerVersion();

        if (options.UiEnabled)
        {
            app.UseSwaggerUI(options =>
            {
                var versionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (ApiVersionDescription description in versionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/openapi/{description.GroupName}.json",
                        $"{appName} {description.GroupName.ToUpperInvariant()}");
                }
            });
        }

        return app;
    }

    private sealed class OpenApiOptions : IOptionsBase
    {
        public static string Key => "OpenApi";

        public bool Enabled { get; set; }

        public bool UiEnabled { get; set; }
    }
}
