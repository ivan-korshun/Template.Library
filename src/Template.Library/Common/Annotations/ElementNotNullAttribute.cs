// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ElementNotNullAttribute : ValidationAttribute
{
    public ElementNotNullAttribute()
        : base("The {0} field must be a collection which elements are not null.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is not IEnumerable<object?> enumerable || enumerable.All(e => e is not null);
    }
}
