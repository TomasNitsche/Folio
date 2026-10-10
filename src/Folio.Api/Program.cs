using Folio.Application;
using Folio.Application.Interfaces.Security;
using Folio.Application.Interfaces.Services;
using Folio.Api.Middleware;
using Folio.Api.Security;
using Folio.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserAccessor>();
builder.Services.AddScoped<ICurrentUserAccessor>(sp => sp.GetRequiredService<CurrentUserAccessor>());

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Folio API";
        document.Info.Version = "v1";
        document.Info.Description = "Club reporting and document management";
        return Task.CompletedTask;
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["https://localhost:7299", "http://localhost:5299"])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (builder.Configuration.GetValue("DevSeedOnStartup", false))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDevSeedService>();
        var result = await seeder.SeedAllAsync(includeDemoPeriod: true);
        app.Logger.LogInformation("Dev seed: {Message}", result.Message);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Dev seed skipped. If using Sql persistence, run database\\Setup-Database.bat first.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseMiddleware<UnauthorizedExceptionMiddleware>();
app.UseMiddleware<DevUserContextMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
