// Copyright © Korshun Ivan. All rights reserved.

using System.Data;
using AutoFixture.NUnit4;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Template.Library.DataSource.Dapper;

namespace Template.Library.UnitTests.DataSource.Dapper;

[TestFixture]
public sealed class UtcDateTimeHandlerTests
{
    private UtcDateTimeHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new UtcDateTimeHandler();
    }

    [Test, AutoData]
    public void Parse_DateTime_UtcKindAdded(DateTime value)
    {
        // Arrange
        value = DateTime.SpecifyKind(value, DateTimeKind.Local);

        // Act
        DateTime result = _sut.Parse(value);

        // Assert
        result.Should().Be(value);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test, AutoData]
    public void Parse_NonDateTime_DateException(string text)
    {
        Action action = () => _sut.Parse(text);

        action.Should().Throw<DataException>().WithMessage("Cannot map System.String to DateTime.");
    }

    [Test, AutoData]
    public void SetValue_Always_SetParameterValue(DateTime value)
    {
        // Arrange
        var parameter = Substitute.For<IDbDataParameter>();

        // Act
        _sut.SetValue(parameter, value);

        // Assert
        parameter.Value.Should().Be(value);
    }
}
