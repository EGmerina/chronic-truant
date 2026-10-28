using ChronicTruant.Writer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Simulation;

/// <summary>Запускает один учебный день с интервалом из настроек симуляции.</summary>
public class SimulationWorker : BackgroundService
{
    private readonly SimulationRunner _runner;
    private readonly IWriter _writer;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly TimeSpan _dayInterval;

    public SimulationWorker(
        SimulationRunner runner,
        IWriter writer,
        IHostApplicationLifetime applicationLifetime,
        IOptions<SimulationOptions> options)
    {
        _runner = runner;
        _writer = writer;
        _applicationLifetime = applicationLifetime;
        _dayInterval = TimeSpan.FromSeconds(options.Value.DayIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_dayInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            SimulationProgress progress = _runner.SimulateNextDay();
            _writer.WriteDayResult(progress.Day, progress.Result);
            _writer.WriteProgress(progress);

            if (progress.Result.Expelled)
            {
                _applicationLifetime.StopApplication();
                return;
            }
        }
    }
}
