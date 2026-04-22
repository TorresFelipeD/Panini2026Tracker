using Microsoft.AspNetCore.Mvc;
using Panini2026Tracker.Application.Albums;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/album")]
public sealed class AlbumController : ControllerBase
{
    private readonly IAlbumService _albumService;

    public AlbumController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    [HttpGet]
    public async Task<ActionResult<AlbumOverviewDto>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] string[]? countryCodes,
        [FromQuery] bool? isOwned,
        [FromQuery] bool? hasImage,
        [FromQuery] bool? hasDuplicates,
        CancellationToken cancellationToken)
    {
        var result = await _albumService.GetOverviewAsync(new AlbumFilter(search, countryCodes ?? [], isOwned, hasImage, hasDuplicates), cancellationToken);
        return Ok(result);
    }
}
