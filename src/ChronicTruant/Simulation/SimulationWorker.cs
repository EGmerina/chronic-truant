using ChronicTruant.Writer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Simulation;

/// <summary>Запускает один учебный день с интервалом из настроек, пока семестр не закончится.</summary>
public class SimulationWorker : BackgroundService
{
    private readonly SemesterSession _session;
    private readonly IWriter _writer;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly TimeSpan _dayInterval;

    public SimulationWorker(
        SemesterSession session,
        IWriter writer,
        IHostApplicationLifetime applicationLifetime,
        IOptions<SimulationOptions> options)
    {
        _session = session;
        _writer = writer;
        _applicationLifetime = applicationLifetime;
        _dayInterval = TimeSpan.FromSeconds(options.Value.DayIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_dayInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            SimulationProgress progress = _session.SimulateNextDay();
            _writer.WriteDayResult(progress.Day, progress.Result);
            _writer.WriteProgress(progress);

            if (_session.IsFinished)
            {
                if (!progress.Result.Expelled)
                {
                    _writer.WriteSemesterFinished(progress);
                }

                _applicationLifetime.StopApplication();
                return;
            }
        }
    }
}
