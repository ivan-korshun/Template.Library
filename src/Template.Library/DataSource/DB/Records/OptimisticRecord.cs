// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.DataSource.DB.Records;

/// <summary>
/// Optimistic concurrency
/// </summary>
public class OptimisticRecord
{
    public required byte[] RowVersion { get; init; }

    public static byte[] ParseVersionString(string version)
    {
        return Convert.FromBase64String(version);
    }

    public static string GetVersionString(byte[] version)
    {
        return Convert.ToBase64String(version);
    }
}
