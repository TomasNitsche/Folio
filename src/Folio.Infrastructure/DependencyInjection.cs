using Microsoft.AspNetCore.StaticFiles;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Options;

using Folio.Application.Interfaces.Integrations;

using Folio.Application.Interfaces.Repositories;

using Folio.Application.Interfaces.Services;

using Folio.Infrastructure.Integrations;

using Folio.Infrastructure.Persistence;

using Folio.Infrastructure.Persistence.InMemory;

using Folio.Infrastructure.Persistence.Sql;

using Folio.Infrastructure.Storage;



namespace Folio.Infrastructure;



public static class DependencyInjection

{

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)

    {

        services.AddSingleton<FileExtensionContentTypeProvider>();

        services.AddSingleton<IFileStorage, FileStorageRepository>();

        services.AddScoped<IDevSeedService, DevSeedService>();



        var provider = configuration["Persistence:Provider"] ?? "InMemory";



        if (string.Equals(provider, "Sql", StringComparison.OrdinalIgnoreCase))

        {

            AddSqlPersistence(services, configuration);

        }

        else

        {

            AddInMemoryPersistence(services);

        }



        AddRegistryClient(services, configuration, provider);



        return services;

    }



    private static void AddRegistryClient(IServiceCollection services, IConfiguration configuration, string persistenceProvider)

    {

        var registryProvider = configuration["Registry:Provider"]

            ?? (string.Equals(persistenceProvider, "Sql", StringComparison.OrdinalIgnoreCase) ? "Dev" : "InMemory");



        if (string.Equals(registryProvider, "Http", StringComparison.OrdinalIgnoreCase))

        {

            services.AddOptions<RegistryOptions>()
                .Bind(configuration.GetSection(RegistryOptions.SectionName))
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<RegistryOptions>, RegistryOptionsValidator>();

            services.AddHttpClient<IRegistryEntityClient, HttpRegistryEntityClient>((sp, client) =>

                {

                    var registryOptions = sp.GetRequiredService<IOptions<RegistryOptions>>().Value;

                    client.BaseAddress = new Uri(registryOptions.BaseUrl);

                    client.Timeout = TimeSpan.FromSeconds(registryOptions.TimeoutSeconds);

                })

                // Windows auth (Negotiate) as the process identity — must match Registry' FolioIntegration:AllowedUser.
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true })

                .AddStandardResilienceHandler();

        }

        else if (string.Equals(registryProvider, "Dev", StringComparison.OrdinalIgnoreCase))

        {

            services.AddScoped<IRegistryEntityClient, DevRegistryEntityClient>();

        }

        else

        {

            services.AddSingleton<IRegistryEntityClient, InMemoryRegistryEntityClient>();

        }

    }



    private static void AddSqlPersistence(IServiceCollection services, IConfiguration configuration)

    {
        DapperTypeHandlers.Register();

        var connectionString = configuration.GetConnectionString("Folio")

            ?? throw new InvalidOperationException("ConnectionStrings:Folio is required when Persistence:Provider=Sql.");



        services.AddSingleton(new DbConnectionFactory(connectionString));



        services.AddScoped<SqlPackageRepository>();

        services.AddScoped<IPackageRepository>(sp => sp.GetRequiredService<SqlPackageRepository>());

        services.AddScoped<SqlAssignmentRepository>();

        services.AddScoped<IAssignmentRepository>(sp => sp.GetRequiredService<SqlAssignmentRepository>());

        services.AddScoped<IPeriodRepository, SqlPeriodRepository>();

        services.AddScoped<IPeriodEntityRepository, SqlPeriodEntityRepository>();

        services.AddScoped<IDocumentRepository, SqlDocumentRepository>();

        services.AddScoped<IAuditRepository, SqlAuditRepository>();

        services.AddScoped<IUserRepository, SqlUserRepository>();

        services.AddScoped<IDevCompanyRepository, SqlDevCompanyRepository>();

    }



    private static void AddInMemoryPersistence(IServiceCollection services)

    {

        services.AddSingleton<InMemoryStore>();

        services.AddSingleton<IPeriodRepository, InMemoryPeriodRepository>();

        services.AddSingleton<IPeriodEntityRepository, InMemoryPeriodEntityRepository>();

        services.AddSingleton<IPackageRepository, InMemoryPackageRepository>();

        services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();

        services.AddSingleton<IAuditRepository, InMemoryAuditRepository>();

        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        services.AddSingleton<IAssignmentRepository, InMemoryAssignmentRepository>();

        services.AddSingleton<IDevCompanyRepository, InMemoryDevCompanyRepository>();

    }

}


