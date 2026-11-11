// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using Asp.Versioning;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Template.Library.Common.Exceptions;
using Template.Library.DataSource.Abstraction;
using Template.Library.Models;
using Template.Library.WebApi.Controllers;
using Template.Library.WebApi.Models;

namespace Template.Library.UnitTests.WebApi.Controllers;

[TestFixture]
public class BooksControllerTests
{
    private IBookRepository _bookRepository = null!;
    private BooksController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = Substitute.For<IBookRepository>();

        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();

        problemDetailsFactory.CreateValidationProblemDetails(null!, null!)
            .ReturnsForAnyArgs(ci => new ValidationProblemDetails(ci.Arg<ModelStateDictionary>()));

        problemDetailsFactory.CreateProblemDetails(null!)
            .ReturnsForAnyArgs(ci => new ProblemDetails
            {
                Status = ci.ArgAt<int>(1),
                Detail = ci.ArgAt<string>(4)
            });

        _sut = new BooksController(_bookRepository)
        {
            Url = Substitute.For<IUrlHelper>(),
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = "Books"
                }
            },
            ProblemDetailsFactory = problemDetailsFactory
        };
    }

    [Test, AutoData]
    public async Task GetOffsetPage_Always_BooksPage(
        OffsetPaging paging, DataFragment<Book> books, CancellationToken cancellationToken)
    {
        // Arrange
        _bookRepository.GetBooks(paging.PageNumber, paging.PageSize, paging.IncludeTotal, cancellationToken)
            .Returns(books);

        // Act
        var result = await _sut.GetOffsetPage(paging, cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(new OffsetPage<Book>(books, paging));
    }

    [Test, AutoData]
    public async Task QueryCheckpointPage_WithoutFrom_BooksPage(
        int limit, string title, DataFragment<Book> books, CancellationToken cancellationToken)
    {
        // Arrange
        var query = new BookCheckpointQuery() { Limit = limit, Title = title };

        _bookRepository.GetBooks(null, limit, title, cancellationToken).Returns(books);

        // Act
        var result = await _sut.QueryCheckpointPage(query, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(new CheckpointPage<Book>(books, query));
    }

    [Test, AutoData]
    public async Task QueryCheckpointPage_WithFrom_BooksPage(
        int from, int limit, string title, DataFragment<Book> books, CancellationToken cancellationToken)
    {
        // Arrange
        var query = new BookCheckpointQuery() { From = $"{from}", Limit = limit, Title = title };

        _bookRepository.GetBooks(from, limit, title, cancellationToken).Returns(books);

        // Act
        var result = await _sut.QueryCheckpointPage(query, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(new CheckpointPage<Book>(books, query));
    }

    [Test, AutoData]
    public async Task QueryCheckpointPage_NotIntFrom_400BadRequest(
        int limit, string title, DataFragment<Book> books, CancellationToken cancellationToken)
    {
        // Arrange
        var query = new BookCheckpointQuery() { From = "non-int", Limit = limit, Title = title };

        // Act
        var result = await _sut.QueryCheckpointPage(query, cancellationToken);

        // Assert
        result.Value.Should().BeNull();
        result.Result.Should().BeEquivalentTo(new ObjectResult(
            new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(CheckpointPaging.From)] = ["The value is invalid."]
            })));
    }

    [Test, AutoData]
    public async Task Get_BookExist_BookWithETag(Book book, CancellationToken cancellationToken)
    {
        // Arrange
        _bookRepository.GetBook(book.Id, cancellationToken).Returns(book);

        // Act
        ActionResult<Book> result = await _sut.Get(book.Id, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(book);
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{book.Version}\"");
    }

    [Test]
    public async Task Get_BookNotExist_404Notfound()
    {
        // Arrange
        _bookRepository.GetBook(default, CancellationToken.None).ReturnsNullForAnyArgs();

        // Act
        var result = await _sut.Get(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Result.Should().BeEquivalentTo(
            new ObjectResult(new ProblemDetails { Status = (int)HttpStatusCode.NotFound })
            {
                StatusCode = (int)HttpStatusCode.NotFound
            });

        _sut.Response.Headers.ETag.Should().BeEmpty();
    }

    [Test, AutoData]
    public async Task Create_Always_CreatedBookWithETag(
        BookCreateDto bookRequest, Book book, string resultLocation, CancellationToken cancellationToken)
    {
        // Arrange
        _sut.Url = Substitute.For<IUrlHelper>();
        _sut.Url.Action(Arg.Any<UrlActionContext>()).Returns(resultLocation);

        _bookRepository.CreateBook(bookRequest, cancellationToken).Returns(book);

        // Act
        var result = await _sut.Create(new ApiVersion(1, 0), bookRequest, cancellationToken);

        // Assert
        result.Result.Should().BeEquivalentTo(new CreatedResult(resultLocation, book));
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{book.Version}\"");
    }

    [Test, AutoData]
    public async Task UpdateAsync_WithETag_UpdatedBookWithNewETag(
        BookUpdateDto bookRequest, string etag, Book book, CancellationToken cancellationToken)
    {
        // Arrange
        _sut.ControllerContext.HttpContext.Request.Headers["If-Match"] = $"\"{etag}\"";

        _bookRepository.UpdateBook(book.Id, bookRequest, etag, cancellationToken).Returns(book);

        // Act
        var result = await _sut.Update(book.Id, bookRequest, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(book);
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{book.Version}\"");
    }

    [Test, AutoData]
    public async Task UpdateAsync_WithoutETag_UpdatedBookWithNewETag(
        BookUpdateDto bookRequest, string etag, Book book, CancellationToken cancellationToken)
    {
        // Arrange
        _bookRepository.UpdateBook(book.Id, bookRequest, null, cancellationToken).Returns(book);

        // Act
        var result = await _sut.Update(book.Id, bookRequest, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(book);
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{book.Version}\"");
    }

    [Test, AutoData]
    public async Task UpdateAsync_ETagWithoutQuotes_ModelValidationException(
        Guid id, BookUpdateDto bookRequest, string etag)
    {
        // Arrange
        _sut.ControllerContext.HttpContext.Request.Headers["If-Match"] = etag;

        // Act
        var exception = Assert.ThrowsAsync<ModelValidationException>(() =>
            _sut.Update(id, bookRequest, CancellationToken.None));

        // Assert
        exception!.Errors.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            [""] = "The If-Match header is invalid."
        });
    }

    [Test, AutoData]
    public async Task UpdateAsync_BookNotExist_404NotFound(BookUpdateDto bookRequest, Guid id)
    {
        // Arrange2
        _bookRepository.UpdateBook(default, null!, null, CancellationToken.None).ReturnsNullForAnyArgs();

        // Act
        var result = await _sut.Update(id, bookRequest, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Result.Should().BeEquivalentTo(
            new ObjectResult(new ProblemDetails { Status = (int)HttpStatusCode.NotFound })
            {
                StatusCode = (int)HttpStatusCode.NotFound
            });

        _sut.Response.Headers.ETag.Should().BeEmpty();
    }

    [Test, AutoData]
    public async Task Delete_BookExist_204NoContent(Guid id, CancellationToken cancellationToken)
    {
        // Arrange
        _bookRepository.DeleteBook(id, cancellationToken).Returns(true);

        // Act
        var result = await _sut.Delete(id, cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(new NoContentResult());
    }

    [Test, AutoData]
    public async Task Delete_BookNotExist_404NotFound(Guid id)
    {
        // Arrange
        _bookRepository.DeleteBook(id, CancellationToken.None).Returns(false);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(
            new ObjectResult(new ProblemDetails { Status = (int)HttpStatusCode.NotFound })
            {
                StatusCode = (int)HttpStatusCode.NotFound
            });
    }
}
