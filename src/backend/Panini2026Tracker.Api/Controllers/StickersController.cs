using Microsoft.AspNetCore.Mvc;
using Panini2026Tracker.Api.Contracts;
using Panini2026Tracker.Application.Stickers;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/stickers")]
public sealed class StickersController : ControllerBase
{
    private readonly IStickerService _stickerService;

    public StickersController(IStickerService stickerService)
    {
        _stickerService = stickerService;
    }

    [HttpGet("{stickerId:guid}")]
    public async Task<ActionResult<StickerDetailDto>> GetByIdAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        var result = await _stickerService.GetDetailAsync(stickerId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{stickerId:guid}/state")]
    public async Task<ActionResult<StickerDetailDto>> UpdateStateAsync(Guid stickerId, [FromBody] UpdateStickerStateRequest request, CancellationToken cancellationToken)
    {
        var result = await _stickerService.UpdateStateAsync(
            stickerId,
            new UpdateStickerStateCommand(
                request.IsOwned,
                request.DuplicateCount,
                request.Notes,
                request.Birthday,
                request.Height,
                request.Weight,
                request.Team),
            cancellationToken);
        return Ok(result);
    }
}
