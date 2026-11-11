// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.DataSource.Dapper;

public class UniqueConstraintException : Exception
{
    private const string DefaultMessage = "Unique constraint is violated.";

    public UniqueConstraintException(Exception innerException) : this(DefaultMessage, innerException)
    {
    }

    public UniqueConstraintException(string message, Exception innerException)
        : base(message ?? DefaultMessage, innerException)
    {
    }

    public string? DuplicateValue { get; set; }

    public override string? StackTrace =>
        $"{base.StackTrace}{Environment.NewLine}{nameof(DuplicateValue)}: {DuplicateValue}";
}
