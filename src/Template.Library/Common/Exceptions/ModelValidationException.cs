// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Exceptions;

public class ModelValidationException : Exception
{
    public ModelValidationException(string message) : this(string.Empty, message)
    {
    }

    public ModelValidationException(string? property, string message)
        : this(new Dictionary<string, string> { [property ?? string.Empty] = message })
    {
    }

    public ModelValidationException(Dictionary<string, string> errors)
    {
        Errors = errors;
    }

    public Dictionary<string, string> Errors { get; }
}
