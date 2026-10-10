using Microsoft.Extensions.DependencyInjection;

using Folio.Application.Interfaces.Services;

using Folio.Application.Services;



namespace Folio.Application;



public static class DependencyInjection

{

    public static IServiceCollection AddApplication(this IServiceCollection services)

    {

        services.AddScoped<IPeriodService, PeriodService>();

        services.AddScoped<IPackageService, PackageService>();

        services.AddScoped<IDocumentService, DocumentService>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IEntityAccessService, EntityAccessService>();

        return services;

    }

}


