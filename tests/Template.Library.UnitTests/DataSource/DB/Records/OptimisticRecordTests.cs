// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture.NUnit4;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.DataSource.DB.Records;

namespace Template.Library.UnitTests.DataSource.DB.Records;

[TestFixture]
public sealed class OptimisticRecordTests
{
    private static readonly string s_base64Value = "AQQGCAgEAQ==";
    private static readonly byte[] s_byteArrayValue = [1, 4, 6, 8, 8, 4, 1];

    [Test]
    public async Task GetVersionString_Always_ConvertFromBase64String()
    {
        OptimisticRecord.GetVersionString(s_byteArrayValue).Should().Be(s_base64Value);
    }

    [Test, AutoData]
    public async Task ParseVersionString_Always_ConvertFromBase64String(string value)
    {
        OptimisticRecord.ParseVersionString(s_base64Value)
            .Should().BeEquivalentTo(s_byteArrayValue, o => o.WithStrictOrdering());
    }
}
