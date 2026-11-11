// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ElementRegularExpressionAttribute(string pattern) : ValidationAttribute(
    $"The {{0}} field must be a collection of elements that match the regular expression '{pattern.Replace("{", "{{").Replace("}", "}}")}'.")
{
    private readonly RegularExpressionAttribute _regularExpressionAttribute = new(pattern);

    public override bool IsValid(object? value)
    {
        return value is not IEnumerable<string?> enumerable || enumerable.All(_regularExpressionAttribute.IsValid);
    }
}
