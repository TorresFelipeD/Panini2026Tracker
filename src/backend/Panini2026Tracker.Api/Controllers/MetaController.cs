using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panini2026Tracker.Api.Configuration;
using Panini2026Tracker.Application.Meta;

namespace Panini2026Tracker.Api.Controllers;

[ApiController]
[Route("api/meta")]
public sealed class MetaController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public MetaController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public ActionResult<AppMetaDto> Get()
    {
        var meta = _configuration.GetRequiredSection("AppMeta").Get<AppMetaConfiguration>()
            ?? throw new InvalidOperationException("La seccion AppMeta no pudo vincularse desde app-config.json.");

        return Ok(new AppMetaDto(
            meta.Name ?? throw new InvalidOperationException("AppMeta:Name es obligatorio en app-config.json."),
            meta.Version ?? throw new InvalidOperationException("AppMeta:Version es obligatorio en app-config.json."),
            meta.License ?? throw new InvalidOperationException("AppMeta:License es obligatorio en app-config.json."),
            meta.BackendFramework ?? throw new InvalidOperationException("AppMeta:BackendFramework es obligatorio en app-config.json."),
            meta.FrontendTarget ?? throw new InvalidOperationException("AppMeta:FrontendTarget es obligatorio en app-config.json.")));
    }
}
