// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using AutoFixture.NUnit4;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.Common.Http;

namespace Template.Library.UnitTests.Common.Http;

[TestFixture]
public class HttpResponseMessageExtensionsTests
{
    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.Created)]
    [TestCase(HttpStatusCode.Accepted)]
    [TestCase(HttpStatusCode.NonAuthoritativeInformation)]
    [TestCase(HttpStatusCode.NoContent)]
    [TestCase(HttpStatusCode.ResetContent)]
    [TestCase(HttpStatusCode.PartialContent)]
    [TestCase(HttpStatusCode.MultiStatus)]
    [TestCase(HttpStatusCode.AlreadyReported)]
    public async Task EnsureSuccessAsync_SuccessStatusCode_ReturnsExpectedResult(HttpStatusCode statusCode)
    {
        // Arrange
        using var response = new HttpResponseMessage(statusCode);

        // Act
        var result = await response.EnsureSuccessAsync();

        // Assert
        result.Should().Be(response);
    }

    [Test]
    [InlineAutoData(HttpStatusCode.Continue)]
    [InlineAutoData(HttpStatusCode.Ambiguous)]
    [InlineAutoData(HttpStatusCode.BadRequest)]
    [InlineAutoData(HttpStatusCode.Unauthorized)]
    [InlineAutoData(HttpStatusCode.NotFound)]
    [InlineAutoData(HttpStatusCode.InternalServerError)]
    public async Task EnsureSuccessAsync_NotSuccessStatusCode_HttpOperationException(
        HttpStatusCode statusCode, HttpMethod method, Uri uri, string responseContent)
    {
        // Arrange
        using var response = new HttpResponseMessage(statusCode)
        {
            RequestMessage = new HttpRequestMessage(method, uri),
            Content = JsonContent.Create(responseContent)
        };

        // Act
        Func<Task> act = () => response.EnsureSuccessAsync().AsTask();

        // Assert
        (await act.Should().ThrowAsync<HttpOperationException>())
            .WithMessage($"{method} {uri} has failed with {statusCode} code. Content: \"{responseContent}\".");
    }
}
