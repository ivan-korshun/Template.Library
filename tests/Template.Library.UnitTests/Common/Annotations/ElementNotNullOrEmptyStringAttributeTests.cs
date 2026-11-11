// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Annotations;

namespace Template.Library.UnitTests.Common.Annotations;

[TestFixture]
public class ElementNotNullOrEmptyStringAttributeTests
{
    private ElementNotNullOrEmptyStringAttribute _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ElementNotNullOrEmptyStringAttribute();
    }

    [Test]
    public void IsValid_NotStringCollection_ReturnTrue()
    {
        _sut.IsValid(new object()).Should().BeTrue();
    }

    // Second parameter is a workaround to array-related issue of TestCase attribute
    [TestCase(null, '_')]
    [TestCase(new string[0], '_')]
    [TestCase(new[] { "test", "test2" }, '_')]
    public void IsValid_ValidElements_ReturnTrue(string?[]? value, char _)
    {
        _sut.IsValid(value).Should().BeTrue();
    }

    // Second parameter is a workaround to array-related issue of TestCase attribute
    [TestCase(new[] { (string?)null }, '_')]
    [TestCase(new[] { "" }, '_')]
    [TestCase(new[] { "test", "" }, '_')]
    [TestCase(new[] { "test", "   " }, '_')]
    [TestCase(new[] { null, "test" }, '_')]
    public void IsValid_InvalidElements_ReturnFalse(string?[]? value, char _)
    {
        _sut.IsValid(value).Should().BeFalse();
    }

    [Test]
    public void GetValidationResult_InvalidElements_ReturnErrorMessage()
    {
        // Arrange
        var context = new ValidationContext(new object())
        {
            DisplayName = "MyName"
        };

        string?[] value = { null };

        // Act
        ValidationResult? result = _sut.GetValidationResult(value, context);

        // Assert
        result.Should().NotBeNull();

        result!.ErrorMessage.Should().Be(
            "The MyName field must be a collection which elements are not null or empty.");
    }
}
