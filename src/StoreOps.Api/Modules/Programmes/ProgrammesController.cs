namespace StoreOps.Api.Modules.Programmes;

using Microsoft.AspNetCore.Mvc;
using StoreOps.Api.Modules.Programmes.Dtos;
using StoreOps.Api.Shared.Errors;

[ApiController]
[Route("api/programmes")]
public sealed class ProgrammesController : ControllerBase
{
    private readonly IProgrammeService _service;

    public ProgrammesController(IProgrammeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid storeId)
    {
        if (storeId == Guid.Empty)
            throw new ValidationError("storeId query parameter is required.");

        var projects = await _service.ListAsync(storeId);
        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(List), new { storeId = created.StoreId }, created);
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest request)
    {
        await _service.AddMemberAsync(id, request);
        return NoContent();
    }
}
