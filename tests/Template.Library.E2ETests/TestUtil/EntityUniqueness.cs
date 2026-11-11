// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestUtil;

public static class EntityUniqueness
{
    public static string GenerateTitle(string? customPart = null)
    {
        return GenerateUniqueValue(customPart);
    }

    public static string GenerateFullName(string? customPart = null)
    {
        return GenerateUniqueValue(customPart);
    }

    public static string GenerateCity(string? customPart = null)
    {
        return GenerateUniqueValue(customPart);
    }

    public static bool IsLeftoverTitle(string title)
    {
        return title.Contains(Constants.UniquePrefix, StringComparison.OrdinalIgnoreCase)
            && IsLeftover(ExtractTimeStamp(title));
    }

    public static bool IsLeftoverFullName(string fullName)
    {
        return fullName.Contains(Constants.UniquePrefix, StringComparison.OrdinalIgnoreCase)
            && IsLeftover(ExtractTimeStamp(fullName));
    }

    public static bool IsLeftoverCity(string city)
    {
        return city.Contains(Constants.UniquePrefix, StringComparison.OrdinalIgnoreCase)
            && IsLeftover(ExtractTimeStamp(city));
    }

    private static string GenerateUniqueValue(string? customPart, char separator = '_')
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return $"{Constants.UniquePrefix}{separator}{timestamp}{separator}{Ensure(customPart)}";
    }

    private static string Ensure(string? customPart)
    {
        return customPart ?? Guid.NewGuid().ToString("N");
    }

    private static long ExtractTimeStamp(string name, char separator = '_')
    {
        string[] parts = name.Split(separator);
        if (parts.Length < 3 || !long.TryParse(parts[1], out long timeStamp))
        {
            return 1;
        }

        return timeStamp;
    }

    private static bool IsLeftover(long timeStamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timeStamp) < DateTimeOffset.UtcNow.AddDays(-1);
    }
}
