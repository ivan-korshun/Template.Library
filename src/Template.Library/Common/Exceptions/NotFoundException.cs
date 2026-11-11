// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Exceptions;

public class NotFoundException : OperationException
{
    private const string DefaultMessage = "Not found";

    public NotFoundException() : this(DefaultMessage)
    {
    }

    public NotFoundException(string? internalMessage) : base(internalMessage ?? DefaultMessage)
    {
    }

    public NotFoundException(string internalMessage, Exception innerException) : base(internalMessage, innerException)
    {
    }

    public NotFoundException(string? internalMessage, string publicMessage) : base(internalMessage, publicMessage)
    {
    }

    public NotFoundException(string? internalMessage, string publicMessage, Exception? innerException)
        : base(internalMessage, publicMessage, innerException)
    {
    }
}
