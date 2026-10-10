using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Documents;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

[ApiController]
[Route("api/periods/{periodId:guid}/documents")]
public class PeriodDocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpPost("static/bulk")]
    public async Task<ActionResult<BulkUploadResultDto>> BulkUploadStatic(
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

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await documentService.BulkUploadStaticAsync(
                periodId, ids, file.FileName, stream, uploadedBy, ct);
            return Ok(result);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return BadRequest(ex.Message);
        }
    }
}
