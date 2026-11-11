// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Template.Library.Common.Exceptions;
using Template.Library.WebApi;

namespace Template.Library.UnitTests.WebApi;

#pragma warning disable CA2201 // Do not raise reserved exception types
public class HttpResponseExceptionFilterTests
{
    private ExceptionContext _context = default!;
    private HttpResponseExceptionFilter _sut = default!;
    private FakeLogger<HttpResponseExceptionFilter> _logger = default!;

    [SetUp]
    public void SetUp()
    {
        _context = new ExceptionContext(
            new ActionContext(Substitute.ForPartsOf<HttpContext>(), new RouteData(), new ActionDescriptor()),
            []);

        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();

        problemDetailsFactory.CreateProblemDetails(default!)
            .ReturnsForAnyArgs(ci => new ProblemDetails
            {
                Status = ci.Arg<int?>(),
                Detail = ci.ArgAt<string>(4)
            });

        problemDetailsFactory.CreateValidationProblemDetails(default!, default!)
            .ReturnsForAnyArgs(ci => new ValidationProblemDetails(ci.Arg<ModelStateDictionary>())
            {
                Status = ci.Arg<int?>()
            });

        _logger = new FakeLogger<HttpResponseExceptionFilter>();
        _sut = new HttpResponseExceptionFilter(problemDetailsFactory, _logger);
    }

    [Test]
    public void Order_Always_100LessThanMax()
    {
        _sut.Order.Should().Be(int.MaxValue - 100);
    }

    [TestCase(null, "ups")]
    [TestCase("internal", "ups")]
    public void OnException_ConflictExceptionWithPublicMessage_IsHandled(
        string? internalMessage, string publicMessage)
    {
        // Arrange
        _context.Exception = new ConflictException(internalMessage, publicMessage);

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Detail.Should().Be("ups");

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_ConflictExceptionWithoutPublicMessage_IsHandled()
    {
        // Arrange
        _context.Exception = new ConflictException("ups");

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Detail.Should().BeNull();

        _context.ExceptionHandled.Should().BeTrue();
    }

    [TestCase(null, "ups")]
    [TestCase("internal", "ups")]
    public void OnException_InnerConflictExceptionWithPublicMessage_IsHandled(
        string? internalMessage, string publicMessage)
    {
        // Arrange
        _context.Exception = new Exception("parent ups", new ConflictException(internalMessage, publicMessage));

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Detail.Should().Be("ups");

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_InnerConflictExceptionWithoutPublicMessage_IsHandled()
    {
        // Arrange
        _context.Exception = new Exception("parent ups", new ConflictException("ups"));

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Detail.Should().BeNull();

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_ETagMismatchException_IsHandledAndMessageNotExposed()
    {
        // Arrange
        _context.Exception = new ETagMismatchException();

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.PreconditionFailed);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.PreconditionFailed);
        problemDetails.Detail.Should().BeNull();
    }

    [Test]
    public void OnException_InnerETagMismatchException_IsHandledAndMessageNotExposed()
    {
        // Arrange
        _context.Exception = new Exception("parent ups", new ETagMismatchException("ups"));

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.PreconditionFailed);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.PreconditionFailed);
        problemDetails.Detail.Should().BeNull();
    }

    [Test]
    public void OnException_ModelValidationException_IsHandled()
    {
        // Arrange
        _context.Exception = new ModelValidationException("prop1", "value ups");

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var problemDetails = result.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["prop1"] =
            [
                "value ups"
            ]
        });

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_ModelValidationExceptionWithMultipleErrors_IsHandled()
    {
        // Arrange
        _context.Exception = new ModelValidationException(new Dictionary<string, string>
        {
            ["prop1"] = "value1",
            ["prop2"] = "value2"
        });

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var problemDetails = result.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["prop1"] =
            [
                "value1"
            ],
            ["prop2"] =
            [
                "value2"
            ]
        });

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_InnerModelValidationException_IsHandled()
    {
        // Arrange
        _context.Exception = new Exception("parent ups", new ModelValidationException(null, "value ups"));

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var problemDetails = result.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            [string.Empty] =
            [
                "value ups"
            ]
        });

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_InnerModelValidationExceptionWithMultipleErrors_IsHandled()
    {
        // Arrange
        _context.Exception = new Exception("parent ups", new ModelValidationException(new Dictionary<string, string>
        {
            ["prop1"] = "value1",
            ["prop2"] = "value2"
        }));

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var problemDetails = result.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["prop1"] =
            [
                "value1"
            ],
            ["prop2"] =
            [
                "value2"
            ]
        });

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_OperationCanceledExceptionNotFromRequestToken_IsNotHandled()
    {
        // Arrange
        _context.Exception = new OperationCanceledException("Ups");

        // Act
        _sut.OnException(_context);

        // Assert
        _context.Result.Should().BeNull();
        _context.ExceptionHandled.Should().BeFalse();
    }

    [Test]
    public void OnException_TaskCanceledExceptionFromRequestToken_IsHandled()
    {
        // Arrange
        _context.HttpContext.RequestAborted.Returns(new CancellationToken(true));
        _context.Exception = new TaskCanceledException("Ups");

        // Act
        _sut.OnException(_context);

        // Assert
        _logger.ShouldContainMessage(LogLevel.Information, "Client closed request after 00:00:00.*", _ => true);

        _context.Result.Should().BeEquivalentTo(new StatusCodeResult(499));
        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_InnerTaskCanceledExceptionFromRequestToken_IsHandled()
    {
        // Arrange
        _context.HttpContext.RequestAborted.Returns(new CancellationToken(true));
        _context.Exception = new Exception("parent ups", new TaskCanceledException("Ups"));

        // Act
        _sut.OnException(_context);

        // Assert
        _logger.ShouldContainMessage(LogLevel.Information, "Client closed request after 00:00:00.*", _ => true);

        _context.Result.Should().BeEquivalentTo(new StatusCodeResult(499));
        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_TaskCanceledExceptionNotFromRequestToken_IsHandled()
    {
        // Arrange
        _context.HttpContext.RequestAborted.Returns(new CancellationToken(false));
        _context.Exception = new TaskCanceledException("Ups");

        // Act
        _sut.OnException(_context);

        // Assert
        _logger.ShouldContainMessage(LogLevel.Warning, "Failed request duration: 00:00:00.*", _ => true);

        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.GatewayTimeout);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.GatewayTimeout);
        problemDetails.Detail.Should().Be("Dependency Gateway Timeout");

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_InnerTaskCanceledExceptionNotFromRequestToken_IsNotHandled()
    {
        // Arrange
        _context.HttpContext.RequestAborted.Returns(new CancellationToken(false));
        _context.Exception = new Exception("parent ups", new TaskCanceledException("Ups"));

        // Act
        _sut.OnException(_context);

        // Assert
        _logger.ShouldContainMessage(LogLevel.Warning, "Failed request duration: 00:00:00.*", _ => true);

        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.GatewayTimeout);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.GatewayTimeout);
        problemDetails.Detail.Should().Be("Dependency Gateway Timeout");

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_ExceptionMessageContainsExceptionWord_MessageIsSanitized()
    {
        // Arrange
        _context.Exception = new ConflictException("Some exception happened");

        // Act
        _sut.OnException(_context);

        // Assert
        var result = _context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Detail.Should().BeNull();

        _context.ExceptionHandled.Should().BeTrue();
    }

    [Test]
    public void OnException_GenericException_IsNotHandled()
    {
        // Arrange
        _context.Exception = new Exception("ups");

        // Act
        _sut.OnException(_context);

        // Assert
        _context.Result.Should().BeNull();
        _context.ExceptionHandled.Should().BeFalse();
    }
}
