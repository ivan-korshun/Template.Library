// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Exceptions;

public abstract class OperationException : Exception
{
    public OperationException() : base()
    {
    }

    public OperationException(string? internalMessage) : base(internalMessage)
    {
    }

    public OperationException(string? internalMessage, Exception? innerException)
        : base(internalMessage, innerException)
    {
    }

    public OperationException(string? internalMessage, string? publicMessage)
        : base(internalMessage ?? publicMessage)
    {
        PublicMessage = publicMessage;
    }

    public OperationException(string? internalMessage, string? publicMessage, Exception? innerException)
        : base(internalMessage ?? publicMessage, innerException)
    {
        PublicMessage = publicMessage;
    }

    public string? PublicMessage { get; }
}
