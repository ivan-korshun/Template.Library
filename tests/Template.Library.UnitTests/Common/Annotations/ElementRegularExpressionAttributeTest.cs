// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Annotations;

namespace Template.Library.UnitTests.Common.Annotations;

[TestFixture]
public class ElementRegularExpressionAttributeTest
{
    private const string DatePattern = @"(\d{1,2}\/\d{1,2}\/\d{4})";

    [Test]
    public void IsValid_NotStringCollection_ReturnTrue()
    {
        new ElementRegularExpressionAttribute(DatePattern)
            .IsValid(new object())
            .Should().BeTrue();
    }

    // Second parameter is a workaround to array-related issue of TestCase attribute
    [TestCase(null, '_')]
    [TestCase(new string[0], '_')]
    [TestCase(new[] { (string?)null }, '_')]
    [TestCase(new[] { "21/3/2023", null }, '_')]
    [TestCase(new[] { "21/3/2023", "11/7/2023" }, '_')]
    public void IsValid_ValidElements_ReturnTrue(string?[]? value, char _)
    {
        new ElementRegularExpressionAttribute(DatePattern)
            .IsValid(value)
            .Should().BeTrue();
    }

    // Second parameter is a workaround to array-related issue of TestCase attribute
    [TestCase(new[] { "word" }, '_')]
    [TestCase(new[] { "21/3/2023", "word" }, '_')]
    public void IsValid_InvalidElements_ReturnFalse(string?[]? value, char _)
    {
        new ElementRegularExpressionAttribute(DatePattern)
            .IsValid(value)
            .Should().BeFalse();
    }

    [Test]
    public void GetValidationResult_InvalidElements_ReturnErrorMessage()
    {
        // Arrange
        var sut = new ElementRegularExpressionAttribute(DatePattern);

        var context = new ValidationContext(new object())
        {
            DisplayName = "MyName"
        };

        // Act
        ValidationResult? result = sut.GetValidationResult((string[])["word"], context);

        // Assert
        result.Should().NotBeNull();

        result!.ErrorMessage.Should().Be(
            @"The MyName field must be a collection of elements that match the regular expression '(\d{1,2}\/\d{1,2}\/\d{4})'.");
    }
}
