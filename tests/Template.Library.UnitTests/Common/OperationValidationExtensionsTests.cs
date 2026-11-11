// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common;

namespace Template.Library.UnitTests.Common;

[TestFixture]
public class OperationValidationExtensionsTests
{
    [Test]
    public void EnsureNotNull_ClassArgumentNull_Throws()
    {
        object? instance = null;

        Action action = () => instance.EnsureNotNull(nameof(instance));

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'instance' must not be null.");
    }

    [Test]
    public void EnsureNotNull_StructArgumentNull_Throws()
    {
        TestStruct? instance = null;

        Action action = () => instance.EnsureNotNull(nameof(instance));

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'instance' must not be null.");
    }

    [Test]
    public void EnsureNotNull_ClassArgumentNotNull_ReturnsTheSameObject()
    {
        var instance = new object();

        object result = instance.EnsureNotNull(nameof(instance));

        result.Should().BeSameAs(instance);
    }

    [Test]
    public void EnsureNotNull_StructArgumentNotNull_ReturnsTheSameObject()
    {
        var instance = new TestStruct();

        TestStruct result = instance.EnsureNotNull(nameof(instance));

        result.Id.Should().Be(instance.Id);
    }

    [Test]
    public void EnsureNotNull_ClassAndArgumentNameNotProvided_UsesCallerArgumentName()
    {
        object? myInstance = null;

        Action action = () => myInstance.EnsureNotNull();

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'myInstance' must not be null.");
    }

    [Test]
    public void EnsureNotNull_StructAndArgumentNameNotProvided_UsesCallerArgumentName()
    {
        TestStruct? myInstance = null;

        Action action = () => myInstance.EnsureNotNull();

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'myInstance' must not be null.");
    }

    [Test]
    public void EnsureNotEmpty_WhenArgumentNull_Throws()
    {
        string? instance = null;

        Action action = () => instance.EnsureNotEmpty(nameof(instance));

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'instance' must not be null or empty.");
    }

    [Test]
    public void EnsureNotEmpty_WhenArgumentEmpty_Throws()
    {
        string? instance = string.Empty;

        Action action = () => instance.EnsureNotEmpty(nameof(instance));

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'instance' must not be null or empty.");
    }

    [Test]
    public void EnsureNotEmpty_WhenArgumentIsValid_ReturnsSameValue()
    {
        string instance = Guid.NewGuid().ToString();

        string result = instance.EnsureNotNull(nameof(instance));

        result.Should().Be(instance);
    }

    [Test]
    public void EnsureNotEmpty_WhenArgumentNameNotProvided_UsesCallerArgumentName()
    {
        string? instance = null;

        Action action = () => instance.EnsureNotEmpty();

        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("'instance' must not be null or empty.");
    }

    private readonly struct TestStruct
    {
        public TestStruct()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}
