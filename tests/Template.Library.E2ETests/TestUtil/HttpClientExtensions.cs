// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Template.Library.E2ETests.TestUtil;

public static class HttpClientExtensions
{
    extension(HttpClient client)
    {
        public async Task<HttpResponseMessage> QueryAsJsonAsync<TValue>(
            string requestUri, TValue value, JsonSerializerOptions? options = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Query, requestUri)
            {
                Content = JsonContent.Create(value, options: options)
            };

            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
            string requestUri, string etag, TValue value, JsonSerializerOptions? options = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
            {
                Content = JsonContent.Create(value, options: options)
            };

            request.Headers.IfMatch.ParseAdd(etag);

            return await client.SendAsync(request);
        }
    }
}
