// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Template.Library.Common;
using Template.Library.Common.Options;
using Template.Library.WebApi.Auth;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class AuthSetupExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        AuthOptions authOptions = config.GetValidOptions<AuthOptions>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = authOptions.Audiences.SplitWithTrim().EnsureNotEmpty()
                };
            });

        services
            .AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>()
            .AddAuthorization();

        return services;
    }

    private sealed class AuthOptions : IOptionsBase
    {
        public static string Key => "JwtBearer";

        [Required]
        public required string Authority { get; set; }

        [Required]
        public required string Audiences { get; set; }

    }
}
