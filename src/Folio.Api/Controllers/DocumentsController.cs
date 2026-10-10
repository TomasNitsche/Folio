using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

[ApiController]
[Route("api/period-entities/{periodEntityId:guid}/[controller]")]
public class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetAll(Guid periodEntityId, CancellationToken ct) =>
        Ok(await documentService.GetDocumentsAsync(periodEntityId, ct));

    [HttpPost("static")]
    public async Task<ActionResult<DocumentDto>> UploadStatic(
        Guid periodEntityId,
        IFormFile file,
        [FromQuery] string uploadedBy,
        CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await documentService.UploadStaticAsync(periodEntityId, file.FileName, stream, uploadedBy, ct));
    }

    [HttpPost("signed")]
    public async Task<ActionResult<DocumentDto>> UploadSigned(
        Guid periodEntityId,
        IFormFile file,
        [FromQuery] string uploadedBy,
        CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await documentService.UploadSignedAsync(periodEntityId, file.FileName, stream, uploadedBy, ct));
    }

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(Guid periodEntityId, Guid documentId, CancellationToken ct)
    {
        var file = await documentService.DownloadAsync(documentId, ct);
        if (file is null) return NotFound();
        return File(file.Value.Content, file.Value.ContentType, file.Value.FileName);
    }

    [HttpPost("assignments")]
    public async Task<ActionResult> AssignUser(
        Guid periodEntityId,
        [FromBody] AssignUserRequest request,
        CancellationToken ct)
    {
        await documentService.AssignUserAsync(periodEntityId, request, ct);
        return NoContent();
    }
}
