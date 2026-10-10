using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Folio.Web.Components;
using Folio.Web.Services;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Dev user switching, In Development, resolve the active user from DevUsers[ActiveDevUser]
// app (CurrentUserOptions, MainLayout) works without any other changes.
if (builder.Environment.IsDevelopment())
{
    var activeKey = builder.Configuration["ActiveDevUser"];
    if (!string.IsNullOrWhiteSpace(activeKey))
    {
        var section = builder.Configuration.GetSection($"DevUsers:{activeKey}");
        if (section.Exists())
        {
            var overrides = new Dictionary<string, string?>
            {
                ["CurrentUser:UserId"]      = section["UserId"],
                ["CurrentUser:UserName"]    = section["UserName"],
                ["CurrentUser:DisplayName"] = section["DisplayName"],
                ["CurrentUser:Role"]        = section["Role"],
                ["CurrentUser:AppRole"]     = section["AppRole"],
                ["CurrentUser:Initials"]    = section["Initials"],
            };
            builder.Configuration.AddInMemoryCollection(overrides);
        }
    }
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.Configure<CurrentUserOptions>(builder.Configuration.GetSection("CurrentUser"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<CurrentUserOptions>>().Value);
builder.Services.AddTransient<CurrentUserDelegatingHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7280/";
builder.Services.AddHttpClient<FolioApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<CurrentUserDelegatingHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
