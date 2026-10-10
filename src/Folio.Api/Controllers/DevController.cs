using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Integrations;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

/// <summary>
/// Development-only endpoints for seeding data.
/// </summary>
[ApiController]
[Route("api/dev")]
public class DevController(IDevSeedService devSeedService, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("seed")]
    public async Task<ActionResult<DevSeedResultDto>> Seed(
        [FromQuery] bool includeDemoPeriod = true,
        CancellationToken ct = default)
    {
        if (!env.IsDevelopment())
            return NotFound();

        var result = await devSeedService.SeedAllAsync(includeDemoPeriod, ct);
        return Ok(result);
    }

    [HttpGet("storage")]
    public ActionResult<object> GetStorageInfo([FromServices] IFileStorage fileStorage)
    {
        if (!env.IsDevelopment())
            return NotFound();

        return Ok(new { rootPath = fileStorage.RootPath });
    }
}
