using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Periods;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

/// <summary>
/// Registry entity catalog — used when opening a period to build the company snapshot.
/// In Development reads from DevCompanies; production will call real Registry.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntitiesController(IPeriodService periodService) : ControllerBase
{
    /// <summary>
    /// Returns all entities valid on the snapshot date (before selecting subset for a period).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegistryEntityDto>>> GetValidOn(
        [FromQuery] DateOnly snapshotDate,
        CancellationToken ct) =>
        Ok(await periodService.FetchRegistryEntitiesAsync(snapshotDate, ct));
}
