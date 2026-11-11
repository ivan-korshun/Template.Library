// Copyright © Korshun Ivan. All rights reserved.

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Template.Library.UnitTests;

public class FakeLogger<T> : ILogger<T>
{
    private readonly bool _enabled;

    public FakeLogger(bool enabled = true)
    {
        _enabled = enabled;
    }

    public ConcurrentBag<(LogLevel logLevel, string message, Exception? exception)> Messages { get; } = [];

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return Substitute.For<IDisposable>();
    }

    public bool IsEnabled(LogLevel logLevel) => _enabled;

    public virtual void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Messages.Add((logLevel, formatter(state, exception), exception));
    }

    public void ShouldReceive(LogLevel logLevel, string message, Exception? exception = null)
    {
        Messages.Should().Contain((logLevel, message, exception));
    }

    public void ShouldNotReceive(LogLevel logLevel, string message, Exception? exception = null)
    {
        Messages.Should().NotContain((logLevel, message, exception));
    }

    public void ShouldContainMessage(LogLevel logLevel, string message)
    {
        Messages.Should().Contain(m => m.logLevel == logLevel && m.message == message);
    }

    public void ShouldContainMessage(LogLevel logLevel, string message, Func<Exception?, bool> exceptionValidator)
    {
        message = message.Replace("*", ".*?");
        Messages.Should().Contain(
            m => m.logLevel == logLevel && Regex.IsMatch(m.message, message) && exceptionValidator(m.exception));
    }
}
