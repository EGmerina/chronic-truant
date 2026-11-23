using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChronicTruant.Persistence;

/// <summary>Нужна только инструменту dotnet ef для создания миграций; к базе он при этом не подключается.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChronicTruantDbContext>
{
    public ChronicTruantDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ChronicTruantDbContext> options = new();
        PersistenceSetup.Configure(options, "Host=localhost;Database=chronic_truant");

        return new ChronicTruantDbContext(options.Options);
    }
}
