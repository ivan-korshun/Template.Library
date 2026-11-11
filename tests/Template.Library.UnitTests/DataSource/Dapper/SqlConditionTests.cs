// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NUnit.Framework;
using Template.Library.DataSource.Dapper;

namespace Template.Library.UnitTests.DataSource.Dapper;

[TestFixture]
public sealed class SqlConditionTests
{
    [Test]
    public void Ctor_NoOperator_EqualByDefault()
    {
        new SqlCondition("name", "value").Operator.Should().Be(SqlOperator.Equal);
    }

    [Test]
    [TestCase("My [_%] Value", SqlOperator.Contains, "%My /[/_/%] Value%")]
    [TestCase("My [_%] Value", SqlOperator.StartsWith, "My /[/_/%] Value%")]
    [TestCase("My [_%] Value", SqlOperator.EndsWith, "%My /[/_/%] Value")]
    [TestCase("My [_%] Value", SqlOperator.Like, "My [_%] Value")]
    public void Value_AdvancedStringComparison_Enriched(string value, SqlOperator sqlOperator, string result)
    {
        new SqlCondition("name", sqlOperator, value).Value.Should().Be(result);
    }

    [Test]
    [TestCase("My [_%] Value", SqlOperator.Like)]
    [TestCase("My [_%] Value", SqlOperator.Equal)]
    public void Value_NonAdvancedStringComparison_RemainUnchanged(string value, SqlOperator sqlOperator)
    {
        new SqlCondition("name", sqlOperator, value).Value.Should().Be(value);
    }

    [TestCase("MyName", SqlOperator.Equal, "[MyName] = @MyName")]
    [TestCase("MyName", SqlOperator.NotEqual, "[MyName] <> @MyName")]
    [TestCase("MyName", SqlOperator.Less, "[MyName] < @MyName")]
    [TestCase("MyName", SqlOperator.LessOrEqual, "[MyName] <= @MyName")]
    [TestCase("MyName", SqlOperator.Greater, "[MyName] > @MyName")]
    [TestCase("MyName", SqlOperator.GreaterOrEqual, "[MyName] >= @MyName")]
    [TestCase("MyName", SqlOperator.In, "[MyName] IN @MyName")]
    [TestCase("MyName", SqlOperator.NotIn, "[MyName] NOT IN @MyName")]
    [TestCase("MyName", SqlOperator.StartsWith, "[MyName] LIKE @MyName")]
    [TestCase("MyName", SqlOperator.EndsWith, "[MyName] LIKE @MyName")]
    [TestCase("MyName", SqlOperator.Contains, "[MyName] LIKE @MyName")]
    [TestCase("MyName", SqlOperator.Like, "[MyName] LIKE @MyName")]
    [TestCase("MyName", SqlOperator.IsNull, "[MyName] IS NULL")]
    [TestCase("MyName", SqlOperator.IsNotNull, "[MyName] IS NOT NULL")]
    public void ToStringCondition_Always_SqlString(string name, SqlOperator sqlOperator, string result)
    {
        new SqlCondition(name, sqlOperator, "value").ToStringCondition().Should().Be(result);
    }

    [Test]
    public void ToStringCondition_NonDefinedOperator_InvalidOperationException()
    {
        var sut = new SqlCondition("name", (SqlOperator)99, "value");

        Action act = () => sut.ToStringCondition();

        act.Should().Throw<InvalidOperationException>().WithMessage("Wrong SQL operator.");
    }
}
