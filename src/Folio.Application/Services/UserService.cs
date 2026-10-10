using Folio.Application;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Services;
using Folio.Domain.Entities;

namespace Folio.Application.Services;

public sealed class UserService(
    IEntityAccessService entityAccessService,
    IUserRepository userRepository) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        await entityAccessService.EnsureRoleCapabilityAsync(EntityAccessAction.AssignUsers, ct);
        var users = await userRepository.GetAllActiveAsync(ct);
        return users.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersForAdminAsync(CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessAdminAsync(ct);
        var users = await userRepository.GetAllForAdminAsync(ct);
        return users.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<RoleDefinitionDto>> GetRolesAsync(CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessAdminAsync(ct);
        return RoleDefinitions.All;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessAdminAsync(ct);
        ValidateUserInput(request.UserName, request.DisplayName, request.Email);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            DisplayName = request.DisplayName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.CreateAsync(user, ct);
        return ToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessAdminAsync(ct);
        ValidateUserInput(null, request.DisplayName, request.Email);

        var existing = await userRepository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("User not found.");

        existing.DisplayName = request.DisplayName.Trim();
        existing.Email = request.Email.Trim();
        existing.Role = request.Role;

        await userRepository.UpdateAsync(existing, ct);
        return ToDto(existing);
    }

    public async Task DeactivateUserAsync(Guid id, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessAdminAsync(ct);
        await userRepository.SetActiveAsync(id, false, ct);
    }

    public async Task ReactivateUserAsync(Guid id, CancellationToken ct = default)
    {
        await entityAccessService.EnsureCanAccessAdminAsync(ct);
        await userRepository.SetActiveAsync(id, true, ct);
    }

    private static UserDto ToDto(AppUser user) =>
        new(user.Id, user.UserName, user.DisplayName, user.Email, user.Role, user.IsActive, user.CreatedAt);

    private static void ValidateUserInput(string? userName, string displayName, string email)
    {
        if (userName is not null && string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username is required.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.");
    }
}
