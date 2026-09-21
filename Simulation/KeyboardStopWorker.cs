using ChronicTruant.Writer;
using Microsoft.Extensions.Hosting;

namespace ChronicTruant.Simulation;

/// <summary>Останавливает хост после нажатия любой клавиши в интерактивной консоли.</summary>
public class KeyboardStopWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWriter _writer;

    public KeyboardStopWorker(
        IHostApplicationLifetime applicationLifetime,
        IWriter writer)
    {
        _applicationLifetime = applicationLifetime;
        _writer = writer;
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

            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }
    }
}
