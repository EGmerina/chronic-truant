
namespace ChronicTruant.Simulation;

public class ConsoleWriter
{
    public void WriteDayResult(int day, DayResult result)
    {
        if (result.Expelled)
        {
            Console.WriteLine(
                $"День {day}: вылет! " +
                $"Предмет: {result.ExpelledSubject}");

            return;
        }

        Console.WriteLine(
            $"День {day}: " +
            $"удовольствие за день = {result.Pleasure}");
    }

    public void WriteSemesterResult(int pleasure, int days)
    {
        Console.WriteLine();
        Console.WriteLine("Семестр завершён.");
        Console.WriteLine($"Общее удовольствие: {pleasure}");
        Console.WriteLine(
            $"Среднее в день: {(double)pleasure / days:F2}");
    }
}