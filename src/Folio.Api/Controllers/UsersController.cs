using Microsoft.AspNetCore.Mvc;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Services;

namespace Folio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default) =>
        Ok(includeInactive
            ? await userService.GetUsersForAdminAsync(ct)
            : await userService.GetUsersAsync(ct));

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyList<RoleDefinitionDto>>> GetRoles(CancellationToken ct) =>
        Ok(await userService.GetRolesAsync(ct));

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await userService.CreateUserAsync(request, ct);
            return CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await userService.UpdateUserAsync(id, request, ct));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await userService.DeactivateUserAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await userService.ReactivateUserAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
