using ChronicTruant.Writer;
using Microsoft.Extensions.Hosting;

namespace ChronicTruant.Simulation;

/// <summary>Запускает один учебный день каждые пять секунд.</summary>
public class SimulationWorker : BackgroundService
{
    private static readonly TimeSpan DayInterval = TimeSpan.FromSeconds(5);

    private readonly SimulationRunner _runner;
    private readonly IWriter _writer;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public SimulationWorker(
        SimulationRunner runner,
        IWriter writer,
        IHostApplicationLifetime applicationLifetime)
    {
        _runner = runner;
        _writer = writer;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(DayInterval);

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
