// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Dependency.Emulation.Models;

namespace Template.Dependency.Emulation.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public sealed class BranchesController : ControllerBase
{
    private static readonly List<Branch> s_data =
    [
        new Branch(1, "UA", "Dnipro", "Victory", 7, new DateTime(2012, 11, 17, 15, 43, 20))
    ];

    [HttpGet]
    public Page<Branch> List()
    {
        return new Page<Branch>(s_data);
    }

    [HttpGet("{id}")]
    public ActionResult<Branch> Get(int id)
    {
        Branch? branch = s_data.FirstOrDefault(b => b.Id == id);
        if (branch == null)
        {
            return NotFound();
        }

        return branch;
    }

    [HttpPost]
    public ActionResult<Branch> Create([FromBody] BranchCreateDto branchDto)
    {
        var branch = new Branch(
            (s_data.LastOrDefault()?.Id ?? 0) + 1,
            branchDto.Country,
            branchDto.City,
            branchDto.Street,
            branchDto.BuildingNumber,
            branchDto.AddedAt,
            branchDto.Phone);

        s_data.Add(branch);

        return Created(Url.Action(nameof(Get), new { id = branch.Id }), branch);
    }

    [HttpPatch("{id}")]
    public ActionResult<Branch> Update(int id, [FromBody] BranchUpdateDto branchDto)
    {
        Branch? branch = s_data.FirstOrDefault(b => b.Id == id);
        if (branch == null)
        {
            return NotFound();
        }

        branch.Country = branchDto.Country ?? branch.Country;
        branch.City = branchDto.City ?? branch.City;
        branch.Street = branchDto.Street ?? branch.Street;
        branch.BuildingNumber = branchDto.BuildingNumber ?? branch.BuildingNumber;
        branch.Phone = branchDto.Phone ?? branch.Phone;

        return branch;
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Branch? branch = s_data.FirstOrDefault(b => b.Id == id);
        if (branch == null)
        {
            return NotFound();
        }

        s_data.Remove(branch);
        return NoContent();
    }
}
