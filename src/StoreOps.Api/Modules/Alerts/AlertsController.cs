namespace StoreOps.Api.Modules.Alerts;

using Microsoft.AspNetCore.Mvc;
using StoreOps.Api.Shared.Errors;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertsController(IAlertService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetForUser([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationError("userId query parameter is required.");

        var notifications = await _service.GetForUserAsync(userId);
        return Ok(notifications);
    }
}
