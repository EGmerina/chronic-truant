using ChronicTruant.Persistence;
using ChronicTruant.Simulation;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Writer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

// appsettings.json копируется рядом со сборкой, поэтому конфиг читается оттуда, откуда бы ни запускали.
HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Короткий ключ для режима запуска: --mode new | --mode continue
builder.Configuration.AddCommandLine(args, new Dictionary<string, string>
{
    ["--mode"] = $"{SimulationOptions.SectionName}:{nameof(SimulationOptions.StartMode)}"
});

builder.Logging.ClearProviders();
builder.Services.Configure<HostOptions>(options =>
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost);

builder.Services.Configure<SimulationOptions>(
    builder.Configuration.GetSection(SimulationOptions.SectionName));

string connectionString = builder.Configuration.GetConnectionString(PersistenceSetup.ConnectionStringName)
    ?? throw new InvalidOperationException(
        $"Не задана строка подключения ConnectionStrings:{PersistenceSetup.ConnectionStringName}.");
builder.Services.AddPersistence(connectionString);

builder.Services.AddSingleton<Random>();
builder.Services.AddSingleton<TeacherFactory>();
builder.Services.AddSingleton<ISkipStrategy, RuleGuessingStrategy>();
builder.Services.AddSingleton<IWriter, ConsoleWriter>();
builder.Services.AddSingleton<SemesterLoader>();
builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<SemesterLoader>().Start());

builder.Services.AddHostedService<SimulationWorker>();
builder.Services.AddHostedService<KeyboardStopWorker>();

using IHost host = builder.Build();

IWriter writer = host.Services.GetRequiredService<IWriter>();
writer.WriteStartMessage();

try
{
    using (ChronicTruantDbContext db = host.Services
               .GetRequiredService<IDbContextFactory<ChronicTruantDbContext>>()
               .CreateDbContext())
    {
        db.Database.Migrate();
    }

    // Семестр создаётся или восстанавливается до старта воркеров, чтобы ошибки базы были видны сразу.
    host.Services.GetRequiredService<SemesterSession>();
}
catch (NpgsqlException exception)
{
    writer.WriteDatabaseUnavailable(exception.Message);
    return 1;
}

await host.RunAsync();
return 0;
