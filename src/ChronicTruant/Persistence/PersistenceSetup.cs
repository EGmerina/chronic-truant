using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChronicTruant.Persistence;

public static class PersistenceSetup
{
    public const string ConnectionStringName = "ChronicTruant";

    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<ChronicTruantDbContext>(options => Configure(options, connectionString));
        services.AddSingleton<ISemesterStore, EfSemesterStore>();

        return services;
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
    }
}
