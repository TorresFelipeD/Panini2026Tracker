using Microsoft.AspNetCore.Mvc;
using Panini2026Tracker.Api.Contracts;
using Panini2026Tracker.Application.Duplicates;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/duplicates")]
public sealed class DuplicatesController : ControllerBase
{
    private readonly IDuplicateService _duplicateService;

    public DuplicatesController(IDuplicateService duplicateService)
    {
        _duplicateService = duplicateService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DuplicateItemDto>>> GetAsync([FromQuery] string? search, [FromQuery] string[]? countryCodes, CancellationToken cancellationToken)
    {
        var result = await _duplicateService.GetAllAsync(new DuplicateFilter(search, countryCodes ?? []), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{stickerId:guid}")]
    public async Task<ActionResult<DuplicateItemDto>> UpdateAsync(Guid stickerId, [FromBody] UpdateDuplicateRequest request, CancellationToken cancellationToken)
    {
        var result = await _duplicateService.UpdateAsync(stickerId, new UpdateDuplicateCommand(request.Quantity), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{stickerId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        await _duplicateService.DeleteAsync(stickerId, cancellationToken);
        return NoContent();
    }
}
