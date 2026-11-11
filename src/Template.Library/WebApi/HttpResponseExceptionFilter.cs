// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Template.Library.Common.Exceptions;

namespace Template.Library.WebApi;

public sealed partial class HttpResponseExceptionFilter(
    ProblemDetailsFactory detailsFactory, ILogger<HttpResponseExceptionFilter> logger)
    : IExceptionFilter, IOrderedFilter
{
    private readonly ProblemDetailsFactory _detailsFactory = detailsFactory;
    private readonly ILogger<HttpResponseExceptionFilter> _logger = logger;

    private readonly DateTime _startTime = DateTime.UtcNow;

    public int Order => int.MaxValue - 100;

    public void OnException(ExceptionContext context)
    {
        IActionResult? result = GetActionResult(context.HttpContext, context.Exception);
        if (result == null)
        {
            return;
        }

        context.Result = result;
        context.ExceptionHandled = true;
    }

    private static string? SanitizeMessage(string? message)
    {
        // transform empty string to null
        if (string.IsNullOrEmpty(message) || message.Contains("Exception", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return message;
    }

    private IActionResult? GetActionResult(HttpContext httpContext, Exception? exception)
    {
        switch (exception)
        {
            case ModelValidationException:
            case { InnerException: ModelValidationException }:
                {
                    var modelException = exception as ModelValidationException
                        ?? exception.InnerException as ModelValidationException;

                    return CreateValidationProblemResult(httpContext, modelException!.Errors);
                }

            case ConflictException:
            case { InnerException: ConflictException }:
                {
                    var conflictException = exception as ConflictException
                        ?? exception.InnerException as ConflictException;

                    return CreateProblemResult(
                        httpContext, (int)HttpStatusCode.Conflict, conflictException!.PublicMessage);
                }

            case ETagMismatchException:
            case { InnerException: ETagMismatchException }:
                {
                    return CreateProblemResult(httpContext, (int)HttpStatusCode.PreconditionFailed);
                }

            case TaskCanceledException:
            case { InnerException: TaskCanceledException }:
                {
                    TimeSpan duration = DateTime.UtcNow - _startTime;

                    if (!httpContext.RequestAborted.IsCancellationRequested)
                    {
                        LogFailedRequestDuration(duration);

                        return CreateProblemResult(
                            httpContext, (int)HttpStatusCode.GatewayTimeout, "Dependency Gateway Timeout");
                    }

                    if (duration < TimeSpan.FromSeconds(30))
                    {
                        LogClientClosedRequestInformation(duration);
                    }
                    else
                    {
                        LogClientClosedRequestWarning(duration);
                    }

                    return new StatusCodeResult(499); // custom Nginx code: Client Closed Request
                }

            default:
                return null;
        }
    }

    private ObjectResult CreateProblemResult(HttpContext httpContext, int statusCode, string? message = null)
    {
        string? detail = SanitizeMessage(message);
        return new ObjectResult(_detailsFactory.CreateProblemDetails(httpContext, statusCode, detail: detail))
        {
            StatusCode = statusCode
        };
    }

    private ObjectResult CreateValidationProblemResult(HttpContext httpContext, Dictionary<string, string> errors)
    {
        var stateDictionary = new ModelStateDictionary();
        foreach ((string? key, string? message) in errors)
        {
            stateDictionary.AddModelError(key, SanitizeMessage(message) ?? string.Empty);
        }

        ValidationProblemDetails problemDetails = _detailsFactory.CreateValidationProblemDetails(
            httpContext, stateDictionary, (int)HttpStatusCode.BadRequest);

        return new ObjectResult(problemDetails)
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }

    [LoggerMessage(LogLevel.Warning, "Failed request duration: {RequestDuration}")]
    private partial void LogFailedRequestDuration(TimeSpan requestDuration);

    [LoggerMessage(LogLevel.Warning, "Client closed request after {RequestDuration}")]
    private partial void LogClientClosedRequestWarning(TimeSpan requestDuration);

    [LoggerMessage(LogLevel.Information, "Client closed request after {RequestDuration}")]
    private partial void LogClientClosedRequestInformation(TimeSpan requestDuration);
}
