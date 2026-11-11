// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Template.Library.Common;

public static class StringExtensions
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this string text, string value)
    {
        return text.StartsWith(value, StringComparison.Ordinal);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] SplitWithTrim(this string value, char separator = ',')
    {
        return value.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
