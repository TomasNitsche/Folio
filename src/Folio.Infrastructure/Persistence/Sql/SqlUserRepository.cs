using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Domain.Enums;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlUserRepository(DbConnectionFactory factory) : IUserRepository
{
    public async Task<IReadOnlyList<AppUser>> GetAllActiveAsync(CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<UserRow>(
            "Folio_User_SELECT_All",
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => r.ToEntity()).ToList();
    }

    public async Task<IReadOnlyList<AppUser>> GetAllForAdminAsync(CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<UserRow>(
            "Folio_User_SELECT_AllAdmin",
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => r.ToEntity()).ToList();
    }

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<UserRow>(
            "Folio_User_SELECT_ById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        return row?.ToEntity();
    }

    public async Task AddIfNotExistsAsync(AppUser user, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_User_INSERT",
            new
            {
                user.Id,
                user.UserName,
                user.DisplayName,
                user.Email,
                Role = (byte)user.Role
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CreateAsync(AppUser user, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_User_CREATE",
            new
            {
                user.Id,
                user.UserName,
                user.DisplayName,
                user.Email,
                Role = (byte)user.Role
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(AppUser user, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_User_UPDATE",
            new
            {
                user.Id,
                user.DisplayName,
                user.Email,
                Role = (byte)user.Role
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            isActive ? "Folio_User_REACTIVATE" : "Folio_User_DEACTIVATE",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    private sealed class UserRow
    {
        public Guid Id { get; init; }
        public string UserName { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public string Email { get; init; } = "";
        public UserRole Role { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }

        public AppUser ToEntity() => new()
        {
            Id = Id,
            UserName = UserName,
            DisplayName = DisplayName,
            Email = Email,
            Role = Role,
            IsActive = IsActive,
            CreatedAt = CreatedAt
        };
    }
}
