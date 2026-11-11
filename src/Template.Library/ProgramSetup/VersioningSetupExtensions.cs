// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class VersioningSetupExtensions
{
    public static void ConfigureVersioningAndOpenApi(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                // Reporting API versions will return the headers "api-supported-versions" and "api-deprecated-versions".
                options.ReportApiVersions = true;

                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.DefaultApiVersion = new ApiVersion(1, 0);
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstitutionFormat = "V";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi();
    }
}
