using ChronicTruant.Writer;

namespace ChronicTruant.Simulation;

/// <summary>Хранит прогресс симуляции и запускает очередной день.</summary>
public class SimulationRunner 
{
    private readonly DaySimulator _daySimulator;
    private int _day;
    private int _totalPleasure;

    public bool IsExpelled { get; private set; }

    public SimulationRunner(DaySimulator daySimulator)
        : this(daySimulator, completedDays: 0, totalPleasure: 0)
    {
    }

    /// <summary>Продолжает симуляцию с сохранённого прогресса.</summary>
    public SimulationRunner(DaySimulator daySimulator, int completedDays, int totalPleasure)
    {
        _daySimulator = daySimulator;
        _day = completedDays;
        _totalPleasure = totalPleasure;
    }

    public int CompletedDays => _day;
    public int TotalPleasure => _totalPleasure;

    public SimulationProgress SimulateNextDay()
    {
        if (IsExpelled)
        {
            throw new InvalidOperationException("Студент отчислен, дальнейшие дни не моделируются.");
        }

        _day++;
        DayResult result = _daySimulator.Simulate(_day);
        IsExpelled = result.Expelled;

        _totalPleasure = result.Expelled
            ? 0
            : _totalPleasure + result.Pleasure;

        return new SimulationProgress(
            _day,
            result,
            _totalPleasure,
            (double)_totalPleasure / _day);
    }
}
