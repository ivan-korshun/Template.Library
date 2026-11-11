// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Annotations;

namespace Template.Library.UnitTests.Common.Annotations;

public class AbsoluteUriAttributeTests
{
    [Test]
    public void IsValid_NullPassed_ReturnsTrue()
    {
        new AbsoluteUriAttribute().IsValid(null).Should().BeTrue();
    }

    [Test]
    public void IsValid_NonUriOrStringPassed_ReturnsFalse()
    {
        new AbsoluteUriAttribute().IsValid(new object()).Should().BeFalse();
    }

    [Test]
    public void IsValid_RelativeUriPassed_ReturnsFalse()
    {
        new AbsoluteUriAttribute()
            .IsValid(new Uri("my.dom", UriKind.RelativeOrAbsolute))
            .Should().BeFalse();
    }

    [TestCase("hTTp")]
    [TestCase("hTTps")]
    public void IsValid_AbsoluteUriWithExpectedSchemePassed_ReturnsTrue(string scheme)
    {
        new AbsoluteUriAttribute()
            .IsValid(new Uri($"{scheme}://my.dom", UriKind.RelativeOrAbsolute))
            .Should().BeTrue();
    }

    [Test]
    public void IsValid_AbsoluteUriWithUnexpectedSchemePassed_ReturnsFalse()
    {
        new AbsoluteUriAttribute()
            .IsValid(new Uri("data://my.dom", UriKind.RelativeOrAbsolute))
            .Should().BeFalse();
    }

    [TestCase("")]
    [TestCase("hTTp://my.dom")]
    [TestCase("hTTps://my.dom")]
    public void IsValid_CorrectStringPassed_ReturnsTrue(string value)
    {
        new AbsoluteUriAttribute()
            .IsValid(value)
            .Should().BeTrue();
    }

    [TestCase("data://my.dom")]
    [TestCase("xxx")]
    [TestCase("http")]
    [TestCase("http://")]
    public void IsValid_IncorrectStringPassed_ReturnsFalse(string value)
    {
        new AbsoluteUriAttribute()
            .IsValid(value)
            .Should().BeFalse();
    }

    [Test]
    public void FormatErrorMessage_FieldNamePassed_ReturnsFormattedErrorMessage()
    {
        new AbsoluteUriAttribute()
            .FormatErrorMessage("My")
            .Should().Be("The My field is not a valid fully-qualified URL.");
    }
}
