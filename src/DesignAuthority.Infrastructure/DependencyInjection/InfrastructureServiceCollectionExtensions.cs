using DesignAuthority.Application.Abstractions;
using DesignAuthority.Application.Features;
using DesignAuthority.Infrastructure.Configuration;
using DesignAuthority.Infrastructure.FileStorage;
using DesignAuthority.Infrastructure.Persistence;
using DesignAuthority.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesignAuthority.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddDesignAuthorityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DesignAuthority") ?? string.Empty;
        services.AddSingleton(new SqlConnectionFactory(connectionString));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.AddSingleton<LocalFileStorage>();
        services.AddSingleton<IFileStorage>(provider => provider.GetRequiredService<LocalFileStorage>());
        services.AddScoped<IConfiguredUserContext, ConfiguredUserContext>();
        services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
        services.AddScoped<IWorkTrackingRepository, WorkTrackingRepository>();
        services.AddScoped<WorkTrackingService>();
        services.AddHostedService<MinuteCleanupWorker>();
        return services;
    }
}
