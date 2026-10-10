using Folio.Application.Interfaces.Repositories;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Infrastructure.Persistence.Sql;

internal static class SqlRows
{
    internal sealed class PeriodRow
    {
        public Guid Id { get; init; }
        public byte ConfirmationType { get; init; }
        public string Code { get; init; } = "";
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public byte Status { get; init; }
        public bool IsDefault { get; init; }
        public DateOnly SnapshotDate { get; init; }
        public string OpenedBy { get; init; } = "";
        public DateTime CreatedAt { get; init; }
        public DateTime? ClosedAt { get; init; }
        public int EntityCount { get; init; }

        public ReportingPeriod ToEntity() => new()
        {
            Id = Id,
            ConfirmationType = (ConfirmationTypeKind)ConfirmationType,
            Code = Code,
            StartDate = StartDate,
            EndDate = EndDate,
            Status = (PeriodStatus)Status,
            IsDefault = IsDefault,
            SnapshotDate = SnapshotDate,
            OpenedBy = OpenedBy,
            CreatedAt = CreatedAt,
            ClosedAt = ClosedAt
        };
    }

    internal class PeriodEntityRow
    {
        public Guid Id { get; init; }
        public Guid PeriodId { get; init; }
        public int RegistryEntityId { get; init; }
        public string Name { get; init; } = "";
        public string Code { get; init; } = "";
        public DateOnly SnapshotDate { get; init; }
    }

    internal sealed class PeriodEntityDetailRow : PeriodEntityRow
    {
        public Guid Period_Id { get; init; }
        public byte Period_ConfirmationType { get; init; }
        public string Period_Code { get; init; } = "";
        public DateOnly Period_StartDate { get; init; }
        public DateOnly Period_EndDate { get; init; }
        public byte Period_Status { get; init; }
        public bool Period_IsDefault { get; init; }
        public DateOnly Period_SnapshotDate { get; init; }
        public string Period_OpenedBy { get; init; } = "";
        public DateTime Period_CreatedAt { get; init; }
        public DateTime? Period_ClosedAt { get; init; }

        public PeriodEntity ToEntity() => new()
        {
            Id = Id,
            PeriodId = PeriodId,
            RegistryEntityId = RegistryEntityId,
            Name = Name,
            Code = Code,
            SnapshotDate = SnapshotDate,
            Period = new ReportingPeriod
            {
                Id = Period_Id,
                ConfirmationType = (ConfirmationTypeKind)Period_ConfirmationType,
                Code = Period_Code,
                StartDate = Period_StartDate,
                EndDate = Period_EndDate,
                Status = (PeriodStatus)Period_Status,
                IsDefault = Period_IsDefault,
                SnapshotDate = Period_SnapshotDate,
                OpenedBy = Period_OpenedBy,
                CreatedAt = Period_CreatedAt,
                ClosedAt = Period_ClosedAt
            }
        };
    }
}
