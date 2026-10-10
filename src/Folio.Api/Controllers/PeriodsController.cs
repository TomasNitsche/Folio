using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Packages;
using Folio.Application.DTOs.Periods;
using Folio.Application.Interfaces.Services;
using Folio.Domain.Enums;

namespace Folio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeriodsController(IPeriodService periodService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PeriodDto>>> GetByType(
        [FromQuery] ConfirmationTypeKind type,
        CancellationToken ct) =>
        Ok(await periodService.GetPeriodsAsync(type, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PeriodDto>> GetById(Guid id, CancellationToken ct)
    {
        var period = await periodService.GetPeriodAsync(id, ct);
        return period is null ? NotFound() : Ok(period);
    }

    [HttpGet("registry-entities")]
    public async Task<ActionResult<IReadOnlyList<RegistryEntityDto>>> GetRegistryEntities(
        [FromQuery] DateOnly snapshotDate,
        CancellationToken ct) =>
        Ok(await periodService.FetchRegistryEntitiesAsync(snapshotDate, ct));

    [HttpPost("open")]
    public async Task<ActionResult<PeriodDto>> OpenPeriod(
        [FromBody] OpenPeriodRequest request,
        CancellationToken ct)
    {
        try
        {
            var period = await periodService.OpenPeriodAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = period.Id }, period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/default")]
    public async Task<ActionResult<PeriodDto>> SetDefault(
        Guid id,
        [FromQuery] string performedBy,
        CancellationToken ct)
    {
        try
        {
            return Ok(await periodService.SetDefaultPeriodAsync(id, performedBy, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
