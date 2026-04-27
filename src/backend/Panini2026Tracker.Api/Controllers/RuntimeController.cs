using Microsoft.AspNetCore.Mvc;
using Panini2026Tracker.Api.Runtime;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/runtime/session")]
public sealed class RuntimeController : ControllerBase
{
    private readonly DesktopSessionTracker _tracker;

    public RuntimeController(DesktopSessionTracker tracker)
    {
        _tracker = tracker;
    }

    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromQuery] string token)
    {
        return _tracker.TryRecordHeartbeat(token)
            ? Accepted()
            : NotFound();
    }

    [HttpPost("closed")]
    public IActionResult Closed([FromQuery] string token)
    {
        return _tracker.TryClose(token)
            ? Accepted()
            : NotFound();
    }
}
