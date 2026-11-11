// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Http;

namespace Template.Library.IntegrationTests;

[StackTraceHidden]
public class HttpResponseMessageAssertions(HttpResponseMessage response)
    : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>(response)
{
    public string ContentAsString => Subject.Content.ReadAsStringAsync().GetAwaiter().GetResult();

    public T ContentAs<T>() => Subject.Content.ReadFromJsonAsync<T>().GetAwaiter().GetResult()!;

    protected override string Identifier => typeof(HttpResponseMessage).FullName!;

    public HttpResponseMessageAssertions HaveStatusCode(
        HttpStatusCode statusCode, string because = "", params object[] becauseArgs)
    {
        return Assertion(
            () => Subject.StatusCode == statusCode,
            () => $"Expected status code to be '{statusCode}', but received '{Subject.StatusCode}'",
            because,
            becauseArgs);
    }

    public HttpResponseMessageAssertions HaveSuccessStatusCode(string because = "", params object[] becauseArgs)
    {
        return Assertion(
            () => Subject.IsSuccessStatusCode,
            () => $"Expected successful status code but received '{Subject.StatusCode}'",
            because,
            becauseArgs);
    }

    public HttpResponseMessageAssertions HaveAnyOfStatusCodes(
        HttpStatusCode[] statusCodes, string because = "", params object[] becauseArgs)
    {
        return Assertion(
            () => statusCodes.Contains(Subject.StatusCode),
            () => $"Expected status code to be any of '{string.Join(", ", statusCodes)}', but received '{Subject.StatusCode}')",
            because,
            becauseArgs);
    }

    private HttpResponseMessageAssertions Assertion(
        Func<bool> condition, Func<string> failMessageFactory, string because, object[] becauseArgs)
    {
        Execute.Assertion
            .ForCondition(condition())
            .BecauseOf(because, becauseArgs)
            .FailWith(() =>
            {
                var wwwAuthenticate = string.Join(Environment.NewLine, Subject.Headers.WwwAuthenticate);

                return new FailReason($"{{0}}. {Environment.NewLine}, Content: {{1}}. WWW-Authenticate: {{2}}",
                    failMessageFactory(),
                    ContentAsString,
                    wwwAuthenticate);
            });

        return this;
    }
}
