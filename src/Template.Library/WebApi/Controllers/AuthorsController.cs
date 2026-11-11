// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
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
/// Authors endpoint.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthorsController(IAuthorRepository authorRepository) : ControllerBase
{
    private readonly IAuthorRepository _authorRepository = authorRepository;

    /// <summary>TBD</summary>
    [HttpGet]
    [Authorize(Policy = "authors:r, authors:w")]
    [RequireQueryParameter(nameof(OffsetPaging.PageSize))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<OffsetPage<Author>> GetOffsetPage([FromQuery] OffsetPaging paging, CancellationToken token)
    {
        DataFragment<Author> authors = await _authorRepository.GetAuthors(
            paging.PageNumber, paging.PageSize, paging.IncludeTotal, token);

        return new OffsetPage<Author>(authors, paging);
    }

    /// <summary>TBD</summary>
    [HttpGet]
    [Authorize(Policy = "authors:r, authors:w")]
    [RequireQueryParameter(nameof(CheckpointPaging.Limit))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CheckpointPage<Author>>> GetCheckpointPage(
        [FromQuery] CheckpointPaging paging, [FromQuery][Length(1, 255)] string? fullName, CancellationToken token)
    {
        var from = -1;
        if (!string.IsNullOrEmpty(paging.From) && !int.TryParse(paging.From, out from))
        {
            return this.ModelValidationProblem(nameof(paging.From), "The value is invalid.");
        }

        DataFragment<Author> authors = await _authorRepository.GetAuthors(
            from > -1 ? from : null, paging.Limit, fullName, token);

        return new CheckpointPage<Author>(authors, paging);
    }

    /// <summary>TBD</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "authors:r, authors:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Author>> Get(Guid id, CancellationToken token)
    {
        Author? result = await _authorRepository.GetAuthor(id, token);
        if (result == null)
        {
            return this.NotFoundProblem();
        }

        this.SetETagHeader(result.Version.EnsureNotEmpty());
        return result;
    }

    /// <summary>TBD</summary>
    [HttpPost]
    [Authorize(Policy = "authors:w")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Author>> Create(
        ApiVersion apiVersion, [FromBody] AuthorCreateDto author, CancellationToken token)
    {
        Author result = await _authorRepository.CreateAuthor(author, token);

        this.SetETagHeader(result.Version.EnsureNotEmpty());
        return this.CreatedResult(
            nameof(Get),
            new { version = apiVersion.ToString(), id = result.Id },
            result);
    }

    /// <summary>TBD</summary>
    [HttpPatch("{id}")]
    [Authorize(Policy = "authors:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Author>> Update(Guid id, [FromBody] AuthorUpdateDto author, CancellationToken token)
    {
        Author? result = await _authorRepository.UpdateAuthor(id, author, this.GetETagFromHeader(), token);
        if (result == null)
        {
            return this.NotFoundProblem();
        }

        this.SetETagHeader(result.Version.EnsureNotEmpty());
        return result;
    }

    /// <summary>TBD</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "authors:w")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        bool isDeleted = await _authorRepository.DeleteAuthor(id, token);
        return isDeleted ? NoContent() : this.NotFoundProblem();
    }
}
