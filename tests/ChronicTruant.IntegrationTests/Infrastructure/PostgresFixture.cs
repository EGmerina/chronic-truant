using ChronicTruant.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ChronicTruant.IntegrationTests.Infrastructure;

/// <summary>
/// Один контейнер PostgreSQL на класс тестов. Каждый тест получает свою чистую базу,
/// поэтому тесты не видят данных друг друга.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public EfSemesterStore CreateStore()
    {
        NpgsqlConnectionStringBuilder connection = new(_container.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}"
        };

        DbContextOptionsBuilder<ChronicTruantDbContext> options = new();
        PersistenceSetup.Configure(options, connection.ConnectionString);

        PooledDbContextFactory<ChronicTruantDbContext> factory = new(options.Options);
        using (ChronicTruantDbContext db = factory.CreateDbContext())
        {
            db.Database.Migrate();
        }

        return new EfSemesterStore(factory);
    }
}
