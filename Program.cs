using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Writer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Services.Configure<HostOptions>(options =>
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost);

builder.Services.AddSingleton<Random>();
builder.Services.AddSingleton<TeacherFactory>();
builder.Services.AddSingleton<IReadOnlyList<Teacher>>(serviceProvider =>
{
    TeacherFactory factory = serviceProvider.GetRequiredService<TeacherFactory>();

    return Enum.GetValues<Subject>()
        .Select(factory.Create)
        .ToList();
});

builder.Services.AddSingleton<StudentHistory>();
builder.Services.AddSingleton<IReadOnlyStudentHistory>(serviceProvider =>
    serviceProvider.GetRequiredService<StudentHistory>());
builder.Services.AddSingleton<ISkipStrategy, AlwaysAttendStrategy>();
builder.Services.AddSingleton<DaySimulator>();
builder.Services.AddSingleton<SimulationRunner>();
builder.Services.AddSingleton<IWriter, ConsoleWriter>();

builder.Services.AddHostedService<SimulationWorker>();
builder.Services.AddHostedService<KeyboardStopWorker>();

using IHost host = builder.Build();

IWriter writer = host.Services.GetRequiredService<IWriter>();
writer.WriteStartMessage();

await host.RunAsync();
