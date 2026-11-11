// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Exceptions;

/// <summary>
/// Represents an error occurring when ETags does not match.
/// </summary>
public class ETagMismatchException : Exception
{
    private const string DefaultMessage = "ETag mismatch";

    /// <summary>Initializes a new instance of the <see cref="ETagMismatchException" /> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public ETagMismatchException(string? message) : base(message ?? DefaultMessage)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ETagMismatchException" /> class.</summary>
    public ETagMismatchException() : this(DefaultMessage)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ETagMismatchException" /> class.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null reference (<span class="keyword"><span class="languageSpecificText"><span class="cs">null</span><span class="vb">Nothing</span><span class="cpp">nullptr</span></span></span><span class="nu">a null reference (<span class="keyword">Nothing</span> in Visual Basic)</span> in Visual Basic) if no inner exception is specified.
    /// </param>
    public ETagMismatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
