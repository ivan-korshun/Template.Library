// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Redaction;

public static class MaskingExtensions
{
    private const char MaskChar = '*';
    private const string MaskString = "***";

    public static string Mask(this Guid value, int keepLeft = 4, int keepRight = 4)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keepLeft);
        ArgumentOutOfRangeException.ThrowIfNegative(keepRight);

        return string.Create(32, (value, keepLeft, keepRight), static (dist, state) =>
        {
            state.value.TryFormat(dist, out _, "N");

            if (state.keepLeft + state.keepRight < 32)
            {
                dist[state.keepLeft..^state.keepRight].Fill(MaskChar);
            }
        });
    }

    public static string? Mask(this string? value, int keepLeft = 2, int keepRight = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keepLeft);
        ArgumentOutOfRangeException.ThrowIfNegative(keepRight);

        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (keepLeft + keepRight >= value.Length)
        {
            return MaskString;
        }

        return string.Concat(value.AsSpan(0, keepLeft), MaskString, value.AsSpan(value.Length - keepRight));
    }

    public static string? MaskProportional(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        int keepLeft = 0;
        int keepRight = 0;

        if (value.Length > 16)
        {
            keepLeft = keepRight = 4;
        }
        else if (value.Length >= 4)
        {
            keepLeft = keepRight = value.Length / 4;
        }

        return string.Concat(value.AsSpan(0, keepLeft), MaskString, value.AsSpan(value.Length - keepRight));
    }

    public static string? MaskEmail(this string? email, int keepLeft = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keepLeft);

        if (string.IsNullOrEmpty(email))
        {
            return email;
        }

        int at = email.LastIndexOf('@');
        if (at <= 0)
        {
            return MaskString;
        }

        int keep = Math.Min(keepLeft, at - 1);
        return string.Concat(email.AsSpan(0, keep), MaskString, email.AsSpan(at));
    }
}
