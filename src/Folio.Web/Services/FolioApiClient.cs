using System.Net.Http.Json;
using Folio.Application.DTOs.Documents;
using Folio.Application.DTOs.Packages;
using Folio.Application.DTOs.Periods;
using Folio.Application.DTOs.Users;
using Folio.Domain.Enums;

namespace Folio.Web.Services;

public sealed class FolioApiClient(HttpClient http, CurrentUserOptions currentUser)
{
    public string CurrentUserName => currentUser.UserName;

    public async Task<IReadOnlyList<PeriodDto>> GetPeriodsAsync(ConfirmationTypeKind type, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<PeriodDto>>($"api/periods?type={type}", ct) ?? [];

    public async Task<IReadOnlyList<PackageGridRowDto>> GetPackageGridAsync(Guid periodId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<PackageGridRowDto>>($"api/periods/{periodId}/packages", ct) ?? [];

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid periodId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<DashboardSummaryDto>($"api/periods/{periodId}/packages/summary", ct)
        ?? new DashboardSummaryDto(0, 0, 0, 0, 0);

    public async Task<IReadOnlyList<RegistryEntityDto>> GetRegistryEntitiesAsync(DateOnly snapshotDate, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<RegistryEntityDto>>($"api/entities?snapshotDate={snapshotDate:yyyy-MM-dd}", ct) ?? [];

    public async Task<IReadOnlyList<DocumentDto>> GetDocumentsAsync(Guid periodEntityId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<DocumentDto>>($"api/period-entities/{periodEntityId}/documents", ct) ?? [];

    public async Task<PeriodDto?> OpenPeriodAsync(OpenPeriodRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/periods/open", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Failed to open period: {error}");
        }
        return await response.Content.ReadFromJsonAsync<PeriodDto>(ct);
    }

    public async Task<PeriodDto> SetDefaultPeriodAsync(Guid periodId, CancellationToken ct = default)
    {
        var response = await http.PostAsync(
            $"api/periods/{periodId}/default?performedBy={Uri.EscapeDataString(CurrentUserName)}", null, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));
        return (await response.Content.ReadFromJsonAsync<PeriodDto>(ct))!;
    }

    public async Task<PackageDto> UploadPackageAsync(Guid periodEntityId, Stream content, string fileName, CancellationToken ct = default)
    {
        await using var buffer = await BufferUploadStreamAsync(content, ct);
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(buffer);
        form.Add(streamContent, "file", fileName);
        var response = await http.PostAsync(
            $"api/period-entities/{periodEntityId}/packages/upload?uploadedBy={Uri.EscapeDataString(CurrentUserName)}",
            form, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PackageDto>(ct))!;
    }

    public async Task<DocumentDto> UploadSignedAsync(Guid periodEntityId, Stream content, string fileName, CancellationToken ct = default)
    {
        await using var buffer = await BufferUploadStreamAsync(content, ct);
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(buffer);
        form.Add(streamContent, "file", fileName);
        var response = await http.PostAsync(
            $"api/period-entities/{periodEntityId}/documents/signed?uploadedBy={Uri.EscapeDataString(CurrentUserName)}",
            form, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DocumentDto>(ct))!;
    }

    public async Task<DocumentDto> UploadStaticAsync(Guid periodEntityId, Stream content, string fileName, CancellationToken ct = default)
    {
        await using var buffer = await BufferUploadStreamAsync(content, ct);
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(buffer);
        form.Add(streamContent, "file", fileName);
        var response = await http.PostAsync(
            $"api/period-entities/{periodEntityId}/documents/static?uploadedBy={Uri.EscapeDataString(CurrentUserName)}",
            form, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DocumentDto>(ct))!;
    }

    public async Task<DevSeedResultDto> SeedDevDataAsync(bool includeDemoPeriod = true, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/dev/seed?includeDemoPeriod={includeDemoPeriod}", null, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DevSeedResultDto>(ct))!;
    }

    public async Task<PackageDto> MarkPackageFinalAsync(Guid periodEntityId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/period-entities/{periodEntityId}/packages/final",
            new MarkPackageFinalRequest(CurrentUserName), ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PackageDto>(ct))!;
    }

    public string GetDocumentDownloadUrl(Guid periodEntityId, Guid documentId) =>
        new Uri(http.BaseAddress!, $"api/period-entities/{periodEntityId}/documents/{documentId}/download").ToString();

    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadDocumentAsync(
        Guid periodEntityId,
        Guid documentId,
        CancellationToken ct = default)
    {
        var response = await http.GetAsync(
            $"api/period-entities/{periodEntityId}/documents/{documentId}/download", ct);
        response.EnsureSuccessStatusCode();

        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? $"document-{documentId}";
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var content = await response.Content.ReadAsByteArrayAsync(ct);
        return (content, contentType, fileName);
    }

    public async Task<StorageInfoDto?> GetStorageInfoAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<StorageInfoDto>("api/dev/storage", ct);

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<UserDto>>("api/users", ct) ?? [];

    public async Task<EntityAssignmentsDto> GetEntityAssignmentsAsync(Guid periodEntityId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<EntityAssignmentsDto>($"api/period-entities/{periodEntityId}/assignments", ct)
        ?? new EntityAssignmentsDto(null, null);

    public async Task SetEntityAssignmentsAsync(Guid periodEntityId, SetEntityAssignmentsRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync(
            $"api/period-entities/{periodEntityId}/assignments?performedBy={Uri.EscapeDataString(CurrentUserName)}",
            request,
            ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersForAdminAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<UserDto>>("api/users?includeInactive=true", ct) ?? [];

    public async Task<IReadOnlyList<RoleDefinitionDto>> GetRolesAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IReadOnlyList<RoleDefinitionDto>>("api/users/roles", ct) ?? [];

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/users", request, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));
        return (await response.Content.ReadFromJsonAsync<UserDto>(ct))!;
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"api/users/{id}", request, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));
        return (await response.Content.ReadFromJsonAsync<UserDto>(ct))!;
    }

    public async Task DeactivateUserAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/users/{id}/deactivate", null, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));
    }

    public async Task ReactivateUserAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/users/{id}/reactivate", null, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));
    }

    public async Task<BulkUploadResultDto> BulkUploadAsync(
        Guid periodId,
        BulkUploadKind kind,
        IReadOnlyList<Guid> periodEntityIds,
        Stream content,
        string fileName,
        CancellationToken ct = default)
    {
        await using var buffer = await BufferUploadStreamAsync(content, ct);
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(buffer);
        form.Add(streamContent, "file", fileName);
        form.Add(new StringContent(string.Join(',', periodEntityIds)), "periodEntityIds");

        var endpoint = kind switch
        {
            BulkUploadKind.StaticReference => $"api/periods/{periodId}/documents/static/bulk",
            BulkUploadKind.ExcelPackage => $"api/periods/{periodId}/packages/bulk",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        var response = await http.PostAsync(
            $"{endpoint}?uploadedBy={Uri.EscapeDataString(CurrentUserName)}",
            form,
            ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));

        return (await response.Content.ReadFromJsonAsync<BulkUploadResultDto>(ct))!;
    }

    public async Task<BulkUploadResultDto> BulkUploadStaticAsync(
        Guid periodId,
        IReadOnlyList<Guid> periodEntityIds,
        Stream content,
        string fileName,
        CancellationToken ct = default) =>
        await BulkUploadAsync(periodId, BulkUploadKind.StaticReference, periodEntityIds, content, fileName, ct);

    private static async Task<MemoryStream> BufferUploadStreamAsync(Stream content, CancellationToken ct)
    {
        var buffer = new MemoryStream();
        if (content.CanSeek)
            content.Position = 0;

        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;
        return buffer;
    }
}

public record StorageInfoDto(string RootPath);
