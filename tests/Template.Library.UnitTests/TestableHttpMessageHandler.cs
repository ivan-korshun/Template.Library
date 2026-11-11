// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;

namespace Template.Library.UnitTests;

/// <summary>
/// Allows substituting the transport layer of an <see cref="HttpClient"/>, since the
/// <see cref="HttpMessageHandler.SendAsync"/> method is protected and cannot be intercepted directly.
/// </summary>
public abstract class TestableHttpMessageHandler : HttpMessageHandler
{
    public abstract Task<HttpResponseMessage> DoSendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return DoSendAsync(request, cancellationToken);
    }
}
