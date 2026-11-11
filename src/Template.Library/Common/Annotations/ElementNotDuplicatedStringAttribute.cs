// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ElementNotDuplicatedStringAttribute(bool ignoreCase)
    : ValidationAttribute("The {0} field must be a collection of non-duplicating elements.")
{
    private readonly StringComparer _stringComparer =
        ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public override bool IsValid(object? value)
    {
        if (value is not IEnumerable<string?> enumerable)
        {
            return true;
        }

        enumerable = [.. enumerable.Where(s => s != null)];

        return enumerable.Distinct(_stringComparer).Count() == enumerable.Count();
    }
}
