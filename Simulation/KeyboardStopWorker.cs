using ChronicTruant.Writer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Simulation;

/// <summary>Останавливает хост после нажатия любой клавиши в интерактивной консоли.</summary>
public class KeyboardStopWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWriter _writer;
    private readonly TimeSpan _pollInterval;

    public KeyboardStopWorker(
        IHostApplicationLifetime applicationLifetime,
        IWriter writer,
        IOptions<SimulationOptions> options)
    {
        _applicationLifetime = applicationLifetime;
        _writer = writer;
        _pollInterval = TimeSpan.FromMilliseconds(options.Value.KeyboardPollMilliseconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Console.IsInputRedirected)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                _writer.WriteStoppedByUser();
                _applicationLifetime.StopApplication();
                return;
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}
