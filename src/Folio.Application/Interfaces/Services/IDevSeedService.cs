using Folio.Application.DTOs.Users;

namespace Folio.Application.Interfaces.Services;

public interface IDevSeedService
{
    Task<DevSeedResultDto> SeedAllAsync(bool includeDemoPeriod = true, CancellationToken ct = default);
}

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetUsersForAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RoleDefinitionDto>> GetRolesAsync(CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeactivateUserAsync(Guid id, CancellationToken ct = default);
    Task ReactivateUserAsync(Guid id, CancellationToken ct = default);
}
