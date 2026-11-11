// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Redaction;

namespace Template.Library.UnitTests.Common.Redaction;

[TestFixture]
public class MaskingExtensionsTests
{
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("a@some.com", "***@some.com")]
    [TestCase("ab@some.com", "a***@some.com")]
    [TestCase("abc@some.com", "ab***@some.com")]
    [TestCase("abcd@some.com", "ab***@some.com")]
    [TestCase("abc.def@i.ua", "ab***@i.ua")]
    public void MaskEmail_DefaultKeep(string? email, string? resutl)
    {
        email.MaskEmail().Should().Be(resutl);
    }
}
