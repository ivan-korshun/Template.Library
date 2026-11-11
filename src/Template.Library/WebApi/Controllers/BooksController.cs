// Copyright © Korshun Ivan. All rights reserved.

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Template.Library.Common;
using Template.Library.DataSource.Abstraction;
using Template.Library.Models;
using Template.Library.WebApi.Attributes;
using Template.Library.WebApi.Models;

namespace Template.Library.WebApi.Controllers;

/// <summary>
/// Books endpoint.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class BooksController(IBookRepository bookRepository) : ControllerBase
{
    private readonly IBookRepository _bookRepository = bookRepository;

    /// <summary>TBD</summary>
    [HttpGet]
    [Authorize(Policy = "books:r, books:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<OffsetPage<Book>> GetOffsetPage([FromQuery] OffsetPaging paging, CancellationToken token)
    {
        DataFragment<Book> books = await _bookRepository.GetBooks(
            paging.PageNumber, paging.PageSize, paging.IncludeTotal, token);

        return new OffsetPage<Book>(books, paging);
    }

    /// <summary>
    /// Example of checkpoint paging together with new QUERY HTTP method, where body is used instead of query string.
    /// </summary>
    [HttpQuery]
    [Authorize(Policy = "books:r, books:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CheckpointPage<Book>>> QueryCheckpointPage(
        [FromBody] BookCheckpointQuery query, CancellationToken token)
    {
        var from = -1;
        if (!string.IsNullOrEmpty(query.From) && !int.TryParse(query.From, out from))
        {
            return this.ModelValidationProblem(nameof(query.From), "The value is invalid.");
        }

        DataFragment<Book> books = await _bookRepository.GetBooks(
            from > -1 ? from : null, query.Limit, query.Title, token);

        return new CheckpointPage<Book>(books, query);
    }

    /// <summary>TBD</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "books:r, books:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Book>> Get(Guid id, CancellationToken token)
    {
        Book? result = await _bookRepository.GetBook(id, token);
        if (result == null)
        {
            return this.NotFoundProblem();
        }

        this.SetETagHeader(result.Version.EnsureNotEmpty());
        return result;
    }

    /// <summary>TBD</summary>
    [HttpPost]
    [Authorize(Policy = "books:w")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Book>> Create(
        ApiVersion apiVersion, [FromBody] BookCreateDto book, CancellationToken token)
    {
        Book result = await _bookRepository.CreateBook(book, token);

        this.SetETagHeader(result.Version.EnsureNotEmpty());
        return this.CreatedResult(
            nameof(Get),
            new { version = apiVersion.ToString(), id = result.Id },
            result);
    }

    /// <summary>TBD</summary>
    [HttpPatch("{id}")]
    [Authorize(Policy = "books:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Book>> Update(Guid id, [FromBody] BookUpdateDto book, CancellationToken token)
    {
        Book? result = await _bookRepository.UpdateBook(id, book, this.GetETagFromHeader(), token);
        if (result == null)
        {
            return this.NotFoundProblem();
        }

        this.SetETagHeader(result.Version.EnsureNotEmpty());
        return result;
    }

    /// <summary>TBD</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "books:w")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        bool isDeleted = await _bookRepository.DeleteBook(id, token);
        return isDeleted ? NoContent() : this.NotFoundProblem();
    }
}
