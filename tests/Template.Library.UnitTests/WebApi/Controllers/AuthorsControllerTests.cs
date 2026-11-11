// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using Asp.Versioning;
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
public class AuthorsControllerTests
{
    private IAuthorRepository _authorRepository = null!;
    private AuthorsController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepository = Substitute.For<IAuthorRepository>();

        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();

        problemDetailsFactory.CreateValidationProblemDetails(null!, null!)
            .ReturnsForAnyArgs(ci => new ValidationProblemDetails(ci.Arg<ModelStateDictionary>()));

        problemDetailsFactory.CreateProblemDetails(null!)
            .ReturnsForAnyArgs(ci => new ProblemDetails
            {
                Status = ci.ArgAt<int>(1),
                Detail = ci.ArgAt<string>(4)
            });

        _sut = new AuthorsController(_authorRepository)
        {
            Url = Substitute.For<IUrlHelper>(),
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = "Authors"
                }
            },
            ProblemDetailsFactory = problemDetailsFactory
        };
    }

    [Test, ExtendedAutoData]
    public async Task GetOffsetPage_Always_AuthorsPage(
        OffsetPaging paging, DataFragment<Author> authors, CancellationToken cancellationToken)
    {
        // Arrange
        _authorRepository.GetAuthors(paging.PageNumber, paging.PageSize, paging.IncludeTotal, cancellationToken)
            .Returns(authors);

        // Act
        var result = await _sut.GetOffsetPage(paging, cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(new OffsetPage<Author>(authors, paging));
    }

    [Test, ExtendedAutoData]
    public async Task GetCheckpointPage_WithoutFrom_AuthorsPage(
        int limit, string nameQuery, DataFragment<Author> authors, CancellationToken cancellationToken)
    {
        // Arrange
        var paging = new CheckpointPaging() { Limit = limit };

        _authorRepository.GetAuthors(null, limit, nameQuery, cancellationToken).Returns(authors);

        // Act
        var result = await _sut.GetCheckpointPage(paging, nameQuery, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(new CheckpointPage<Author>(authors, paging));
    }

    [Test, ExtendedAutoData]
    public async Task GetCheckpointPage_WithFrom_AuthorsPage(
        int from, int limit, string nameQuery, DataFragment<Author> authors, CancellationToken cancellationToken)
    {
        // Arrange
        var paging = new CheckpointPaging() { From = $"{from}", Limit = limit };

        _authorRepository.GetAuthors(from, limit, nameQuery, cancellationToken).Returns(authors);

        // Act
        var result = await _sut.GetCheckpointPage(paging, nameQuery, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(new CheckpointPage<Author>(authors, paging));
    }

    [Test, ExtendedAutoData]
    public async Task GetCheckpointPage_NotIntFrom_400BadRequest(
        int limit, string nameQuery, DataFragment<Author> authors, CancellationToken cancellationToken)
    {
        // Arrange
        var paging = new CheckpointPaging() { From = "non-int", Limit = limit };

        // Act
        var result = await _sut.GetCheckpointPage(paging, nameQuery, cancellationToken);

        // Assert
        result.Value.Should().BeNull();
        result.Result.Should().BeEquivalentTo(new ObjectResult(
            new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(CheckpointPaging.From)] = ["The value is invalid."]
            })));
    }

    [Test, ExtendedAutoData]
    public async Task Get_AuthorExist_AuthorWithETag(Author author, CancellationToken cancellationToken)
    {
        // Arrange
        _authorRepository.GetAuthor(author.Id, cancellationToken).Returns(author);

        // Act
        ActionResult<Author> result = await _sut.Get(author.Id, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(author);
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{author.Version}\"");
    }

    [Test]
    public async Task Get_AuthorNotExist_404Notfound()
    {
        // Arrange
        _authorRepository.GetAuthor(default, CancellationToken.None).ReturnsNullForAnyArgs();

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

    [Test, ExtendedAutoData]
    public async Task Create_Always_CreatedAuthorWithETag(
        AuthorCreateDto authorRequest, Author author, string resultLocation, CancellationToken cancellationToken)
    {
        // Arrange
        _sut.Url = Substitute.For<IUrlHelper>();
        _sut.Url.Action(Arg.Any<UrlActionContext>()).Returns(resultLocation);

        _authorRepository.CreateAuthor(authorRequest, cancellationToken).Returns(author);

        // Act
        var result = await _sut.Create(new ApiVersion(1, 0), authorRequest, cancellationToken);

        // Assert
        result.Result.Should().BeEquivalentTo(new CreatedResult(resultLocation, author));
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{author.Version}\"");
    }

    [Test, ExtendedAutoData]
    public async Task UpdateAsync_WithETag_UpdatedAuthorWithNewETag(
        AuthorUpdateDto authorRequest, string etag, Author author, CancellationToken cancellationToken)
    {
        // Arrange
        _sut.ControllerContext.HttpContext.Request.Headers["If-Match"] = $"\"{etag}\"";

        _authorRepository.UpdateAuthor(author.Id, authorRequest, etag, cancellationToken).Returns(author);

        // Act
        var result = await _sut.Update(author.Id, authorRequest, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(author);
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{author.Version}\"");
    }

    [Test, ExtendedAutoData]
    public async Task UpdateAsync_WithoutETag_UpdatedAuthorWithNewETag(
        AuthorUpdateDto authorRequest, string etag, Author author, CancellationToken cancellationToken)
    {
        // Arrange
        _authorRepository.UpdateAuthor(author.Id, authorRequest, null, cancellationToken).Returns(author);

        // Act
        var result = await _sut.Update(author.Id, authorRequest, cancellationToken);

        // Assert
        result.Value.Should().BeEquivalentTo(author);
        _sut.Response.Headers.ETag.ToString().Should().Be($"\"{author.Version}\"");
    }

    [Test, ExtendedAutoData]
    public async Task UpdateAsync_ETagWithoutQuotes_ModelValidationException(
        Guid id, AuthorUpdateDto authorRequest, string etag)
    {
        // Arrange
        _sut.ControllerContext.HttpContext.Request.Headers["If-Match"] = etag;

        // Act
        var exception = Assert.ThrowsAsync<ModelValidationException>(() =>
            _sut.Update(id, authorRequest, CancellationToken.None));

        // Assert
        exception!.Errors.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            [""] = "The If-Match header is invalid."
        });
    }

    [Test, ExtendedAutoData]
    public async Task UpdateAsync_AuthorNotExist_404NotFound(AuthorUpdateDto authorRequest, Guid id)
    {
        // Arrange2
        _authorRepository.UpdateAuthor(default, null!, null, CancellationToken.None).ReturnsNullForAnyArgs();

        // Act
        var result = await _sut.Update(id, authorRequest, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Result.Should().BeEquivalentTo(
            new ObjectResult(new ProblemDetails { Status = (int)HttpStatusCode.NotFound })
            {
                StatusCode = (int)HttpStatusCode.NotFound
            });

        _sut.Response.Headers.ETag.Should().BeEmpty();
    }

    [Test, ExtendedAutoData]
    public async Task Delete_AuthorExist_204NoContent(Guid id, CancellationToken cancellationToken)
    {
        // Arrange
        _authorRepository.DeleteAuthor(id, cancellationToken).Returns(true);

        // Act
        var result = await _sut.Delete(id, cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(new NoContentResult());
    }

    [Test, ExtendedAutoData]
    public async Task Delete_AuthorNotExist_404NotFound(Guid id)
    {
        // Arrange
        _authorRepository.DeleteAuthor(id, CancellationToken.None).Returns(false);

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
