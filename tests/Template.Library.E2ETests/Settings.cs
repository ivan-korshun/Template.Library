// Copyright © Korshun Ivan. All rights reserved.

using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Template.Library.E2ETests;

public static class Settings
{
    private static IConfiguration Config { get; } = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .AddUserSecrets(Assembly.GetCallingAssembly())
        .Build()
        .GetSection("LibraryTests");

    public static string LibraryHost { get; } = GetRequired("LibraryHost").TrimEnd('/');

    public static class ClientCredentials
    {
        public static string Audience { get; } = GetRequired("ClientCredentials:Audience");

        public static string ClientId { get; } = GetRequired("ClientCredentials:ClientId");

        public static string ClientSecret { get; } = GetRequired("ClientCredentials:ClientSecret");

        public static string TokenEndpoint { get; } = GetRequired("ClientCredentials:TokenEndpoint");
    }

    private static string GetRequired(string key)
    {
        return GetRequired<string>(key);
    }

    private static T GetRequired<T>(string key) where T : class
    {
        var value = Config.GetValue<T>(key);
        return value != null ? value : throw new InvalidOperationException($"Key {key} is not configured.");
    }
}
