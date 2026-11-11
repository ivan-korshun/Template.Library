// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Exceptions;

/// <summary>
/// Represents an error occurring, when conflicts are happening.
/// </summary>
public class ConflictException : OperationException
{
    private const string DefaultMessage = "Conflict occurred";

    public ConflictException() : this(DefaultMessage)
    {
    }

    public ConflictException(string? internalMessage) : base(internalMessage ?? DefaultMessage)
    {
    }

    public ConflictException(string internalMessage, Exception innerException) : base(internalMessage, innerException)
    {
    }

    public ConflictException(string? internalMessage, string publicMessage) : base(internalMessage, publicMessage)
    {
    }

    public ConflictException(string? internalMessage, string publicMessage, Exception? innerException)
        : base(internalMessage, publicMessage, innerException)
    {
    }
}
