// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ElementNotNullOrEmptyStringAttribute : ValidationAttribute
{
    public ElementNotNullOrEmptyStringAttribute()
        : base("The {0} field must be a collection which elements are not null or empty.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is not IEnumerable<string?> enumerable || enumerable.All(e => !string.IsNullOrWhiteSpace(e));
    }
}
