// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Annotations;

namespace Template.Library.UnitTests.Common.Annotations;

[TestFixture]
public class ElementNotNullAttributeTests
{
    [TestCase("string")]
    [TestCase(1)]
    [TestCase(1.2)]
    [TestCase(null)]
    public void IsValid_PropertyIsNotEnumerable_ReturnsTrue(object property)
    {
        new ElementNotNullAttribute().IsValid(property).Should().BeTrue();
    }

    [Test]
    public void IsValid_PropertyIsEnumerableWithNulls_ReturnsFalse()
    {
        new ElementNotNullAttribute().IsValid(new[] { new object(), null }).Should().BeFalse();
    }

    [Test]
    public void IsValid_PropertyIsEnumerableWithoutNulls_ReturnsTrue()
    {
        new ElementNotNullAttribute().IsValid(new[] { new object(), new object() }).Should().BeTrue();
    }
}
