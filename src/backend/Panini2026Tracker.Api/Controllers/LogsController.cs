using Microsoft.AspNetCore.Mvc;
using Panini2026Tracker.Api.Contracts;
using Panini2026Tracker.Application.Logs;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SystemLogDto>>> GetAsync([FromQuery] string? category, [FromQuery] string? level, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _logService.GetAllAsync(new LogFilter(category, level, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost("delete")]
    public async Task<ActionResult<int>> DeleteAsync([FromBody] DeleteLogsRequest request, CancellationToken cancellationToken)
    {
        var deleted = await _logService.DeleteAsync(new DeleteLogsCommand(request.Category, request.Level, request.Search), cancellationToken);
        return Ok(deleted);
    }
}
