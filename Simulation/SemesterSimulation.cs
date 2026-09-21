using ChronicTruant.Writer;
namespace ChronicTruant.Simulation;
public class SemesterSimulator
{
    private const int SemesterDays = 100;

    private readonly DaySimulator _daySimulator;
    private readonly ConsoleWriter _writer;

    public SemesterSimulator(
        DaySimulator daySimulator,
        ConsoleWriter writer)
    {
        _daySimulator = daySimulator;
        _writer = writer;
    }

    public void Run()
    {
        int totalPleasure = 0;

        for (int day = 1; day <= SemesterDays; day++)
        {
            DayResult result = _daySimulator.Simulate(day);

            _writer.WriteDayResult(day, result);

            if (result.Expelled)
            {
                return;
            }

            totalPleasure += result.Pleasure;
        }

        _writer.WriteSemesterResult(totalPleasure, SemesterDays);
    }
}