// Copyright © Korshun Ivan. All rights reserved.

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Template.Library.DataSource.Abstraction;
using Template.Library.Models;
using Template.Library.WebApi.Models;

namespace Template.Library.WebApi.Controllers;

/// <summary>TBD</summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class BranchesController(IBranchRepository branchRepository) : ControllerBase
{
    private readonly IBranchRepository _branchRepository = branchRepository;

    /// <summary>TBD</summary>
    [HttpGet]
    [Authorize(Policy = "branches:r, branches:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<Page<Branch>> GetAll(CancellationToken token)
    {
        ICollection<Branch> branches = await _branchRepository.GetAllAsync(token);
        return new Page<Branch>(branches);
    }

    /// <summary>TBD</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "branches:r, branches:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Branch>> Get(int id, CancellationToken token)
    {
        Branch? result = await _branchRepository.GetAsync(id, token);
        if (result == null)
        {
            return this.NotFoundProblem();
        }

        return result;
    }

    /// <summary>TBD</summary>
    [HttpPost]
    [Authorize(Policy = "branches:w")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Branch>> Create(
        ApiVersion apiVersion, [FromBody] BranchCreateDto branch, CancellationToken token)
    {
        Branch result = await _branchRepository.CreateAsync(branch, token);

        return this.CreatedResult(
            nameof(Get),
            new { version = apiVersion.ToString(), id = result.Id },
            result);
    }

    /// <summary>TBD</summary>
    [HttpPatch("{id}")]
    [Authorize(Policy = "branches:w")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Branch>> Update(int id, [FromBody] BranchUpdateDto branch, CancellationToken token)
    {
        Branch? result = await _branchRepository.UpdateAsync(id, branch, token);
        if (result == null)
        {
            return this.NotFoundProblem();
        }

        return result;
    }

    /// <summary>TBD</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "branches:w")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        bool isDeleted = await _branchRepository.DeleteAsync(id, token);
        return isDeleted ? NoContent() : this.NotFoundProblem();
    }
}
