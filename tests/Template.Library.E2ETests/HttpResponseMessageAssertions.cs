// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Template.Library.E2ETests;

namespace Template.Library.E2ETests;

[StackTraceHidden]
public class HttpResponseMessageAssertions(HttpResponseMessage response)
    : ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>(response)
{
    private static readonly Func<EquivalencyAssertionOptions<ProblemDetails>, EquivalencyAssertionOptions<ProblemDetails>> s_problemAssertionConfig =
            config => config.Excluding(x => x.Extensions);

    private static readonly Func<EquivalencyAssertionOptions<ValidationProblemDetails>, EquivalencyAssertionOptions<ValidationProblemDetails>> s_validationProblemAssertionConfig =
            config => config.Excluding(x => x.Extensions);

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

    public HttpResponseMessageAssertions HaveAnyOfStatusCodes(
        HttpStatusCode[] statusCodes, string because = "", params object[] becauseArgs)
    {
        return Assertion(
            () => statusCodes.Contains(Subject.StatusCode),
            () => $"Expected status code to be any of '{string.Join(", ", statusCodes)}', but received '{Subject.StatusCode}')",
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

    public HttpResponseMessageAssertions HaveOkStatusCode(string because = "", params object[] becauseArgs)
    {
        return HaveStatusCode(HttpStatusCode.OK, because, becauseArgs);
    }

    public HttpResponseMessageAssertions HaveCreatedStatusCode(string because = "", params object[] becauseArgs)
    {
        return HaveStatusCode(HttpStatusCode.Created, because, becauseArgs);
    }

    public HttpResponseMessageAssertions HaveNoContentStatusCode(string because = "", params object[] becauseArgs)
    {
        return HaveStatusCode(HttpStatusCode.NoContent, because, becauseArgs);
    }

    public HttpResponseMessageAssertions HaveForbiddenStatusCode(string because = "", params object[] becauseArgs)
    {
        return HaveStatusCode(HttpStatusCode.Forbidden, because, becauseArgs);
    }

    public HttpResponseMessageAssertions HaveUnauthorizedStatusCode(string because = "", params object[] becauseArgs)
    {
        return HaveStatusCode(HttpStatusCode.Unauthorized, because, becauseArgs);
    }

    public AndConstraint<HttpResponseMessageAssertions> NotHaveHeader(
        string name, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject.Headers.Where(x => x.Key == name))
            .ForCondition(x => x != null && !x.Any())
            .FailWith("Expected {context:header} {0} to not be present, but it presents and has value {1}.",
                _ => name,
                headers => string.Join(",", headers!.Select(x => x.Value)));

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndConstraint<HttpResponseMessageAssertions> HaveHeader(
        string name, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject.Headers.Where(x => x.Key == name).SelectMany(x => x.Value))
            .ForCondition(v => v != null && v.Any())
            .FailWith("Expected {0} {context:header} to be present in the response, but it is not.", _ => name)
            .Then
            .ForCondition(v => v != null && v.SelectMany(x => x.Split(',')).Any(x => !string.IsNullOrEmpty(x)))
            .FailWith("Expected {context:header} {0} to have value{reason}.",
                _ => name,
                values => values);

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndConstraint<HttpResponseMessageAssertions> HaveHeaderContainingValue(
        string name, string value, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject.Headers.Where(x => x.Key == name).SelectMany(x => x.Value))
            .ForCondition(v => v != null && v.Any())
            .FailWith("Expected {0} {context:header} to be present in the response, but it is not.", _ => name)
            .Then
            .ForCondition(v => v != null && v.SelectMany(x => x.Split(',')).Contains(value))
            .FailWith("Expected {context:header} {0} to contain {1} value{reason}, but found {2}.",
                _ => name,
                _ => value,
                values => values);

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndConstraint<HttpResponseMessageAssertions> HaveLocation(
        string location, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .ForCondition(Subject.Headers.Location != null
                && Subject.Headers.Location.ToString().Equals(location, StringComparison.OrdinalIgnoreCase))
            .BecauseOf(because, becauseArgs)
            .FailWith(() => new FailReason("Expected location header to be '{0}', but received '{1}'",
                location,
                Subject.Headers.Location
            ));

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndConstraint<HttpResponseMessageAssertions> NotHaveEtagHeader(
        string because = "", params object[] becauseArgs)
    {
        return NotHaveHeader("ETag", because, becauseArgs);
    }

    public string HaveEtagHeader(string because = "", params object[] becauseArgs)
    {
        return HaveHeader("ETag", because, becauseArgs).And.Subject.Headers.ETag!.Tag;
    }

    public void HaveEtagHeaderSameAs(string etag)
    {
        HaveEtagHeader().Should().Be(etag);
    }

    public string HaveEtagHeaderDifferentFrom(string etag)
    {
        return HaveEtagHeader().Should().NotBe(etag).And.Subject;
    }

    public void HaveForbiddenProblem(string? detail = null)
    {
        HaveStatusCode(HttpStatusCode.Forbidden)
            .ContentAs<ProblemDetails>()
            .Should().BeEquivalentTo(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                Title = "Forbidden",
                Status = 403,
                Detail = detail
            }, s_problemAssertionConfig);
    }

    public void HaveNotFoundProblem(string? detail = null)
    {
        HaveStatusCode(HttpStatusCode.NotFound)
            .ContentAs<ProblemDetails>()
            .Should().BeEquivalentTo(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = "Not Found",
                    Status = 404,
                    Detail = detail
                },
                s_problemAssertionConfig);
    }

    public void HaveConflictProblem(string? detail = null)
    {
        HaveStatusCode(HttpStatusCode.Conflict)
            .ContentAs<ProblemDetails>()
            .Should().BeEquivalentTo(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "Conflict",
                    Status = 409,
                    Detail = detail
                },
                s_problemAssertionConfig);
    }

    public void HavePreconditionFailedProblem(string? detail = null)
    {
        HaveStatusCode(HttpStatusCode.PreconditionFailed)
            .ContentAs<ProblemDetails>()
            .Should().BeEquivalentTo(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.13",
                    Title = "Precondition Failed",
                    Status = 412,
                    Detail = detail
                },
                s_problemAssertionConfig);
    }

    public void HaveUnsupportedMediaTypeProblem(string? detail = null)
    {
        HaveStatusCode(HttpStatusCode.UnsupportedMediaType)
            .ContentAs<ProblemDetails>()
            .Should().BeEquivalentTo(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.16",
                    Title = "Unsupported Media Type",
                    Status = 415,
                    Detail = detail
                },
                s_problemAssertionConfig);
    }

    public void HaveServiceUnavailableProblem(string? detail)
    {
        HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAs<ProblemDetails>()
            .Should().BeEquivalentTo(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
                    Title = "Service Unavailable",
                    Status = 503,
                    Detail = detail
                },
                s_problemAssertionConfig);
    }

    public void HaveBadRequestProblem()
    {
        HaveBadRequestProblem(new ValidationProblemDetails
        {
            Title = "Bad Request"
        });
    }

    public void HaveBadRequestProblem(string detail)
    {
        HaveBadRequestProblem(new ValidationProblemDetails
        {
            Title = "Bad Request",
            Detail = detail
        });
    }

    public void HaveBadRequestProblem(string key, params string[] errorMessages)
    {
        HaveBadRequestProblem(new Dictionary<string, string[]>
        {
            [key] = errorMessages
        });
    }

    public void HaveBadRequestProblem(Dictionary<string, string[]> errors)
    {
        HaveBadRequestProblem(new ValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred."
        });
    }

    public void HaveBadRequestProblem(ValidationProblemDetails expectedProblem)
    {
        expectedProblem.Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
        expectedProblem.Status = 400;

        HaveStatusCode(HttpStatusCode.BadRequest)
            .ContentAs<ValidationProblemDetails>()
            .Should().BeEquivalentTo(expectedProblem, s_validationProblemAssertionConfig);
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
