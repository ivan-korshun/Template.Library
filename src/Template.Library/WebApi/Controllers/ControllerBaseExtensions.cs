// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Template.Library.Common.Exceptions;

namespace Template.Library.WebApi.Controllers;

public static class ControllerBaseExtensions
{
    public static void SetETagHeader(this ControllerBase controller, string etag)
    {
        controller.Response.Headers.ETag = $"\"{etag.Trim('"')}\"";
    }

    public static string? GetETagFromHeader(this ControllerBase controller)
    {
        if (controller.Request.Headers.IfMatch.Count == 0)
        {
            return null;
        }

        var ifMatchHeaders = controller.Request.GetTypedHeaders().IfMatch;
        if (ifMatchHeaders.Count == 0)
        {
            throw new ModelValidationException($"The {HeaderNames.IfMatch} header is invalid.");
        }

        return ifMatchHeaders[0].Tag.Value?.Trim('"');
    }

    public static CreatedResult CreatedResult(
        this ControllerBase controller, string actionName, object urlParams, object result)
    {
        var detailsPath = controller.Url.Action(
            actionName, controller.ControllerContext.ActionDescriptor.ControllerName, urlParams);

        var url = new Uri(
            detailsPath ?? throw new InvalidOperationException("Unable to get route URL"), UriKind.Relative);

        return controller.Created(url, result);
    }

    public static ActionResult ModelValidationProblem(this ControllerBase controller, string message)
    {
        return ModelValidationProblem(controller, string.Empty, message);
    }

    public static ActionResult ModelValidationProblem(this ControllerBase controller, string? key, string message)
    {
        controller.ModelState.AddModelError(key ?? string.Empty, SanitizeMessage(message) ?? string.Empty);
        return controller.ValidationProblem();
    }

    public static ActionResult NotFoundProblem(this ControllerBase controller, string? message = null)
    {
        return controller.Problem(SanitizeMessage(message), statusCode: (int)HttpStatusCode.NotFound);
    }

    public static ActionResult ConflictProblem(this ControllerBase controller, string? message = null)
    {
        return controller.Problem(SanitizeMessage(message), statusCode: (int)HttpStatusCode.Conflict);
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
}
