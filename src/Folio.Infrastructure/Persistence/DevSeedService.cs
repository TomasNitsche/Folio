using Folio.Application.DTOs.Periods;
using Folio.Application.DTOs.Users;
using Folio.Application.Interfaces.Repositories;
using Folio.Application.Interfaces.Services;
using Folio.Domain.Entities;
using Folio.Domain.Enums;

namespace Folio.Infrastructure.Persistence;

public sealed class DevSeedService(
    IUserRepository userRepository,
    IDevCompanyRepository devCompanyRepository,
    IPeriodRepository periodRepository,
    IPeriodEntityRepository periodEntityRepository,
    IPackageRepository packageRepository,
    IAssignmentRepository assignmentRepository,
    IPeriodService periodService) : IDevSeedService
{
    public async Task<DevSeedResultDto> SeedAllAsync(bool includeDemoPeriod = true, CancellationToken ct = default)
    {
        var usersSeeded = 0;
        foreach (var seed in DevSeedData.Users)
        {
            var user = new AppUser
            {
                Id = seed.Id,
                UserName = seed.UserName,
                DisplayName = seed.DisplayName,
                Email = seed.Email,
                Role = seed.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var existing = await userRepository.GetByIdAsync(seed.Id, ct);
            if (existing is null)
                await userRepository.AddIfNotExistsAsync(user, ct);
            else
            {
                existing.DisplayName = seed.DisplayName;
                existing.Email = seed.Email;
                existing.Role = seed.Role;
                await userRepository.UpdateAsync(existing, ct);
            }

            usersSeeded++;
        }

        foreach (var company in DevSeedData.Companies)
        {
            await devCompanyRepository.MergeAsync(
                company.RegistryEntityId, company.Name, company.Code, company.ValidFrom, company.ValidTo, ct);
        }

        var periodsSeeded = 0;
        if (includeDemoPeriod)
        {
            var existing = await periodRepository.GetByCodeAsync(DevSeedData.DemoPeriodCode, ct);
            if (existing is null)
            {
                await periodService.OpenPeriodAsync(new OpenPeriodRequest(
                    ConfirmationTypeKind.Transfers,
                    DevSeedData.DemoPeriodCode,
                    DevSeedData.DemoPeriodStart,
                    DevSeedData.DemoPeriodEnd,
                    DevSeedData.DemoPeriodSnapshot,
                    "folio.lead",
                    DevSeedData.DemoPeriodEntities.Select(e => e.RegistryEntityId).ToList()), ct);

                var period = await periodRepository.GetByCodeAsync(DevSeedData.DemoPeriodCode, ct)
                    ?? throw new InvalidOperationException("Demo period was not created.");

                var entities = await periodEntityRepository.GetByPeriodIdAsync(period.Id, ct);
                foreach (var demo in DevSeedData.DemoPeriodEntities)
                {
                    var entity = entities.FirstOrDefault(e => e.RegistryEntityId == demo.RegistryEntityId);
                    if (entity is null) continue;

                    if (demo.Version >= 0)
                    {
                        await packageRepository.AddAsync(new EntityPackage
                        {
                            Id = Guid.NewGuid(),
                            PeriodEntityId = entity.Id,
                            Version = demo.Version,
                            Status = demo.Status,
                            IsFinal = demo.IsFinal,
                            CreatedAt = DateTime.UtcNow.AddDays(-2),
                            UpdatedAt = DateTime.UtcNow.AddHours(-Random.Shared.Next(1, 48)),
                            CreatedBy = "folio.lead"
                        }, ct);
                    }

                    if (demo.AssignUserId is Guid userId)
                    {
                        await assignmentRepository.AddAsync(new EntityAssignment
                        {
                            Id = Guid.NewGuid(),
                            PeriodEntityId = entity.Id,
                            UserId = userId,
                            CanConfirm = true,
                            CanFillPackage = false
                        }, ct);
                    }
                }

                periodsSeeded = 1;
            }
        }

        return new DevSeedResultDto(
            usersSeeded,
            DevSeedData.Companies.Length,
            periodsSeeded,
            periodsSeeded > 0
                ? $"Seeded users, companies, and demo period {DevSeedData.DemoPeriodCode}."
                : "Seeded users and companies. Demo period already exists or was skipped.");
    }
}
