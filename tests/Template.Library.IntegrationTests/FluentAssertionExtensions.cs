// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;

namespace Template.Library.IntegrationTests;

public static class FluentAssertionsExtensions
{
    public static HttpResponseMessageAssertions Should(this HttpResponseMessage response) => new(response);
}
