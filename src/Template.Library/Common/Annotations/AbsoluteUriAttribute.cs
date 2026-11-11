// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class AbsoluteUriAttribute : ValidationAttribute
{
    public AbsoluteUriAttribute()
    {
        ErrorMessage = "The {0} field is not a valid fully-qualified URL.";
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        Uri? uri = value as Uri;
        if (uri == null && value is string stringValue)
        {
            if (stringValue.Length == 0)
            {
                return true;
            }

            Uri.TryCreate(stringValue, UriKind.Absolute, out uri);
        }

        if (uri?.IsAbsoluteUri != true)
        {
            return false;
        }

        string scheme = uri.Scheme.ToUpperInvariant();
        return scheme is "HTTP" or "HTTPS";
    }
}
