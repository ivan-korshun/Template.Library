// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Annotations;

namespace Template.Library.UnitTests.Common.Annotations;

[TestFixture]
public class ElementNotDuplicatedStringAttributeTests
{
    [Test]
    public void IsValid_NotStringCollection_ReturnTrue()
    {
        new ElementNotDuplicatedStringAttribute(true)
            .IsValid(new object())
            .Should().BeTrue();
    }

    [TestCase(null, false)]
    [TestCase(new string[0], false)]
    [TestCase(new[] { (string?)null }, false)]
    [TestCase(new[] { "TEST", "test" }, false)]
    [TestCase(new[] { "test", "test2" }, true)]
    [TestCase(new[] { "test", "test2" }, false)]
    [TestCase(new[] { "test", "test2", null }, true)]
    public void IsValid_ValidElements_ReturnTrue(string?[]? value, bool ignoreCase)
    {
        new ElementNotDuplicatedStringAttribute(ignoreCase)
            .IsValid(value)
            .Should().BeTrue();
    }

    [TestCase(new[] { "test", "test" }, false)]
    [TestCase(new[] { "test", "test" }, true)]
    [TestCase(new[] { "test", "TEST" }, true)]
    [TestCase(new[] { "test", null, "TEST" }, true)]
    public void IsValid_DuplicatedElements_ReturnFalse(string?[]? value, bool ignoreCase)
    {
        new ElementNotDuplicatedStringAttribute(ignoreCase)
            .IsValid(value)
            .Should().BeFalse();
    }

    [Test]
    public void GetValidationResult_DuplicatedElements_ReturnErrorMessage()
    {
        // Arrange
        var sut = new ElementNotDuplicatedStringAttribute(true);

        var context = new ValidationContext(new object())
        {
            DisplayName = "MyName"
        };

        var value = new[]
        {
            "test",
            "test"
        };

        // Act
        ValidationResult? result = sut.GetValidationResult(value, context);

        // Assert
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("The MyName field must be a collection of non-duplicating elements.");
    }
}
