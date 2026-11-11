// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Annotations;

namespace Template.Library.UnitTests.Common.Annotations;

[TestFixture]
public class ElementMaxStringLengthAttributeTests
{
    [Test]
    public void IsValid_NotStringCollection_ReturnTrue()
    {
        new ElementMaxStringLengthAttribute(50)
            .IsValid(new object())
            .Should().BeTrue();
    }

    // Second parameter is a workaround to array-related issue of TestCase attribute
    [TestCase(null, '_')]
    [TestCase(new string[0], '_')]
    [TestCase(new[] { (string?)null }, '_')]
    [TestCase(new[] { "ABC", null }, '_')]
    [TestCase(new[] { "short", "short" }, '_')]
    public void IsValid_ValueIsValid_ReturnTrue(string?[]? value, char _)
    {
        new ElementMaxStringLengthAttribute(50)
            .IsValid(value)
            .Should().BeTrue();
    }

    [Test]
    public void IsValid_OneStringIsLongerThanMaxElementLength_ReturnFalse()
    {
        new ElementMaxStringLengthAttribute(5)
            .IsValid((string[])["ABC", "o-lo-lo"])
            .Should().BeFalse();
    }

    [Test]
    public void GetValidationResult_OneStringIsLongerThanMaxElementLength_ReturnErrorMessage()
    {
        // Arrange
        var sut = new ElementMaxStringLengthAttribute(5);

        var context = new ValidationContext(new object())
        {
            DisplayName = "MyName"
        };

        // Act
        var result = sut.GetValidationResult((string[])["ABC", "o-lo-lo"], context);

        // Assert
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("The maximum length of collection element must be '5' for the MyName field.");
    }
}
