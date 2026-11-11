// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;

namespace Template.Library.Common.Http;

public static class HttpResponseMessageExtensions
{
    public static async ValueTask<HttpResponseMessage> EnsureSuccessAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        HttpRequestMessage? request = response.RequestMessage;
        string content = await response.Content.ReadAsStringAsync();

        throw new HttpOperationException(
            $"{request?.Method} {request?.RequestUri} has failed with {response.StatusCode} code. Content: {content}.");
    }
}
