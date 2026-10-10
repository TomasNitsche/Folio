using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Packages;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

[ApiController]
[Route("api/periods/{periodId:guid}/[controller]")]
public class PackagesController(IPackageService packageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PackageGridRowDto>>> GetGrid(Guid periodId, CancellationToken ct) =>
        Ok(await packageService.GetGridAsync(periodId, ct));

    [HttpGet("~/api/period-entities/{periodEntityId:guid}/packages/latest")]
    public async Task<ActionResult<PackageDto>> GetLatest(Guid periodEntityId, CancellationToken ct)
    {
        var package = await packageService.GetLatestPackageAsync(periodEntityId, ct);
        return package is null ? NotFound() : Ok(package);
    }

    [HttpPost("~/api/period-entities/{periodEntityId:guid}/packages/upload")]
    public async Task<ActionResult<PackageDto>> Upload(
        Guid periodEntityId,
        IFormFile file,
        [FromQuery] string uploadedBy,
        CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest(new { error = "File is empty." });

        await using var stream = file.OpenReadStream();
        var package = await packageService.UploadPackageAsync(periodEntityId, file.FileName, stream, uploadedBy, ct);
        return Ok(package);
    }

    [HttpPost("~/api/period-entities/{periodEntityId:guid}/packages/final")]
    public async Task<ActionResult<PackageDto>> MarkFinal(
        Guid periodEntityId,
        [FromBody] MarkPackageFinalRequest request,
        CancellationToken ct)
    {
        try
        {
            return Ok(await packageService.MarkFinalAsync(periodEntityId, request, ct));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkUploadResultDto>> BulkUpload(
        Guid periodId,
        IFormFile file,
        [FromForm] string periodEntityIds,
        [FromQuery] string uploadedBy,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(periodEntityIds))
            return BadRequest("periodEntityIds is required.");

        var ids = periodEntityIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        if (ids.Count == 0)
            return BadRequest("No valid entity IDs provided.");

        if (file.Length == 0)
            return BadRequest("File is empty.");

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await packageService.BulkUploadPackageAsync(
                periodId, ids, file.FileName, stream, uploadedBy, ct);
            return Ok(result);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(Guid periodId, CancellationToken ct) =>
        Ok(await packageService.GetSummaryAsync(periodId, ct));
}
