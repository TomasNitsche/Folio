using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

[ApiController]
[Route("api/period-entities/{periodEntityId:guid}/assignments")]
public class AssignmentsController(IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<EntityAssignmentsDto>> Get(Guid periodEntityId, CancellationToken ct) =>
        Ok(await documentService.GetAssignmentsAsync(periodEntityId, ct));

    [HttpPut]
    public async Task<IActionResult> Set(
        Guid periodEntityId,
        [FromBody] SetEntityAssignmentsRequest request,
        [FromQuery] string performedBy,
        CancellationToken ct)
    {
        try
        {
            await documentService.SetAssignmentsAsync(periodEntityId, request, performedBy, ct);
            return NoContent();
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or ArgumentException)
        {
            return BadRequest(ex.Message);
        }
    }
}
