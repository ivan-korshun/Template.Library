// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ElementMaxStringLengthAttribute(int maxLength)
    : ValidationAttribute($"The maximum length of collection element must be '{maxLength}' for the {{0}} field.")
{
    private readonly int _maxLength = maxLength;

    public override bool IsValid(object? value)
    {
        return value is not IEnumerable<string?> enumerable
            || enumerable.All(el => el == null || el.Length <= _maxLength);
    }
}
