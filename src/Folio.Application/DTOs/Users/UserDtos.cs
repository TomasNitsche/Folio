namespace Folio.Application.DTOs.Users;

using Folio.Domain.Enums;

public record UserDto(
    Guid Id,
    string UserName,
    string DisplayName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt);

public record CreateUserRequest(
    string UserName,
    string DisplayName,
    string Email,
    UserRole Role);

public record UpdateUserRequest(
    string DisplayName,
    string Email,
    UserRole Role);

public record RoleDefinitionDto(
    UserRole Role,
    string Name,
    string Description,
    IReadOnlyList<string> Permissions);

public record AssignUserRequest(
    Guid UserId,
    bool CanConfirm,
    bool CanFillPackage);

public record EntityAssignmentDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string DisplayName,
    string Email,
    bool CanConfirm,
    bool CanFillPackage);

public record EntityAssignmentsDto(
    EntityAssignmentDto? Confirmer,
    EntityAssignmentDto? PackFiller);

public record SetEntityAssignmentsRequest(
    Guid? ConfirmerUserId,
    Guid? PackFillerUserId);

public record DevSeedResultDto(
    int UsersSeeded,
    int CompaniesSeeded,
    int PeriodsSeeded,
    string Message);
