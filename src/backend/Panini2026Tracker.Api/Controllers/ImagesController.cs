using Microsoft.AspNetCore.Mvc;
using Panini2026Tracker.Application.Images;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<StickerImageDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var result = await _imageService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("{stickerId:guid}")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<StickerImageDto>> UploadAsync(Guid stickerId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A file is required.");
        }

        await using var stream = file.OpenReadStream();
        var result = await _imageService.UploadAsync(stickerId, stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{stickerId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        await _imageService.DeleteAsync(stickerId, cancellationToken);
        return NoContent();
    }
}
