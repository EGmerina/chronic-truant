using ChronicTruant.Writer;

namespace ChronicTruant.Simulation;

/// <summary>Хранит прогресс бесконечной симуляции и запускает очередной день.</summary>
public class SimulationRunner 
{
    private readonly DaySimulator _daySimulator;
    private int _day;
    private int _totalPleasure;

    public SimulationRunner(DaySimulator daySimulator)
    {
        _daySimulator = daySimulator;
    }

    public SimulationProgress SimulateNextDay()
    {
        _day++;
        DayResult result = _daySimulator.Simulate(_day);

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
