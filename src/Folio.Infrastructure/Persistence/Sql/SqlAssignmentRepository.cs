using Dapper;
using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using System.Data;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class SqlAssignmentRepository(DbConnectionFactory factory) : IAssignmentRepository
{
    public async Task<IReadOnlyList<EntityAssignment>> GetByPeriodEntityIdAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        var rows = await conn.QueryAsync<AssignmentRow>(
            "Folio_Assignment_SELECT_ByPeriodEntityId",
            new { PeriodEntityId = periodEntityId },
            commandType: CommandType.StoredProcedure);

        return rows.Select(r => r.ToEntity()).ToList();
    }

    public async Task AddAsync(EntityAssignment assignment, CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Assignment_INSERT",
            new
            {
                assignment.Id,
                assignment.PeriodEntityId,
                assignment.UserId,
                assignment.CanConfirm,
                assignment.CanFillPackage
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task ReplaceRoleAssignmentsAsync(
        Guid periodEntityId,
        Guid? confirmerUserId,
        Guid? packFillerUserId,
        CancellationToken ct = default)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "Folio_Assignment_DELETE_ByPeriodEntityId",
            new { PeriodEntityId = periodEntityId },
            commandType: CommandType.StoredProcedure);

        if (confirmerUserId is Guid confirmerId)
        {
            await conn.ExecuteAsync(
                "Folio_Assignment_INSERT",
                new
                {
                    Id = Guid.NewGuid(),
                    PeriodEntityId = periodEntityId,
                    UserId = confirmerId,
                    CanConfirm = true,
                    CanFillPackage = false
                },
                commandType: CommandType.StoredProcedure);
        }

        if (packFillerUserId is Guid packFillerId)
        {
            await conn.ExecuteAsync(
                "Folio_Assignment_INSERT",
                new
                {
                    Id = Guid.NewGuid(),
                    PeriodEntityId = periodEntityId,
                    UserId = packFillerId,
                    CanConfirm = false,
                    CanFillPackage = true
                },
                commandType: CommandType.StoredProcedure);
        }
    }

    private sealed class AssignmentRow
    {
        public Guid Id { get; init; }
        public Guid PeriodEntityId { get; init; }
        public Guid UserId { get; init; }
        public bool CanConfirm { get; init; }
        public bool CanFillPackage { get; init; }
        public string UserName { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public string Email { get; init; } = "";

        public EntityAssignment ToEntity() => new()
        {
            Id = Id,
            PeriodEntityId = PeriodEntityId,
            UserId = UserId,
            CanConfirm = CanConfirm,
            CanFillPackage = CanFillPackage,
            UserName = UserName,
            DisplayName = DisplayName,
            Email = Email
        };
    }
}
