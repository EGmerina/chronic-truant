
using ChronicTruant.Simulation;

namespace ChronicTruant.Writer;

public class ConsoleWriter : IWriter
{
    public void WriteDayResult(int day, DayResult result)
    {
        if (result.Expelled)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine($"║ День {day,-39}║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ ❌ ВЫЛЕТ                                      ║");
            Console.WriteLine($"║ Предмет: {result.ExpelledSubject,-35}║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");

            return;
        }

        Console.WriteLine();
        Console.WriteLine($"── День {day} ───────────────────────────────────");
        Console.WriteLine($"  ✓ Удовольствие за день: {result.Pleasure}");
    }

    public void WriteProgress(SimulationProgress progress)
    {
        Console.WriteLine("  ┌─ Текущий результат ─────────────────────────");
        Console.WriteLine($"  │ Общее удовольствие: {progress.TotalPleasure}");
        Console.WriteLine($"  │ Среднее за день:    {progress.AveragePleasure:F2}");
        Console.WriteLine("  └─────────────────────────────────────────────");
    }

    public void WriteStartMessage()
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║       МАСТЕР ПРОГУЛОВ 80-ГО УРОВНЯ            ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine("║ Симуляция запущена                            ║");
        Console.WriteLine("║ Новый учебный день — каждые 5 секунд          ║");
        Console.WriteLine("║ Нажмите любую клавишу для остановки           ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
    }

    public void WriteStoppedByUser()
    {
        Console.WriteLine();
        Console.WriteLine("Симуляция остановлена пользователем.");
    }
}
