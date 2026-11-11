// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Exceptions;

public class ForbiddenException : OperationException
{
    private const string DefaultMessage = "Forbidden";

    public ForbiddenException() : this(DefaultMessage)
    {
    }

    public ForbiddenException(string? internalMessage) : base(internalMessage ?? DefaultMessage)
    {
    }

    public ForbiddenException(string internalMessage, Exception innerException) : base(internalMessage, innerException)
    {
    }

    public ForbiddenException(string? internalMessage, string publicMessage) : base(internalMessage, publicMessage)
    {
    }

    public ForbiddenException(string? internalMessage, string publicMessage, Exception? innerException)
        : base(internalMessage, publicMessage, innerException)
    {
    }
}
