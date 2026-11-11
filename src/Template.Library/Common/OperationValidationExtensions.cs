// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Template.Library.Common;

public static class OperationValidationExtensions
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T EnsureNotNull<T>(this T? instance, [CallerArgumentExpression("instance")] string? instanceName = null)
    {
        if (instance == null)
        {
            throw new InvalidOperationException($"'{instanceName}' must not be null.");
        }

        return instance;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EnsureNotEmpty(
        this string? instance, [CallerArgumentExpression("instance")] string? instanceName = null)
    {
        if (string.IsNullOrEmpty(instance))
        {
            throw new InvalidOperationException($"'{instanceName}' must not be null or empty.");
        }

        return instance;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyCollection<T> EnsureNotEmpty<T>(
        this IReadOnlyCollection<T>? instance, [CallerArgumentExpression("instance")] string? instanceName = null)
    {
        if (instance == null || instance.Count == 0)
        {
            throw new InvalidOperationException($"'{instanceName}' must not be null or empty.");
        }

        return instance;
    }
}
