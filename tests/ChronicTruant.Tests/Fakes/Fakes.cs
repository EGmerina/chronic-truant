using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Writer;
using Microsoft.Extensions.Hosting;

namespace ChronicTruant.Tests.Fakes;

/// <summary>Правило, которое спрашивает в заданные дни.</summary>
public class AskOnDaysRule : ITeacherRule
{
    private readonly HashSet<int> _days;

    public AskOnDaysRule(params int[] days)
    {
        _days = [.. days];
    }

    public int Calls { get; private set; }

    public bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history)
    {
        Calls++;
        return _days.Contains(day);
    }
}

/// <summary>Стратегия с заранее заданным решением на каждый день.</summary>
public class FixedStrategy : ISkipStrategy
{
    private readonly bool[] _decisions;

    public FixedStrategy(params bool[] decisions)
    {
        _decisions = decisions;
    }

    public string Name => "Fixed";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        return _decisions;
    }
}

public class RecordingWriter : IWriter
{
    public List<(int Day, DayResult Result)> Days { get; } = [];

    public void WriteDayResult(int day, DayResult result)
    {
        Days.Add((day, result));
    }

    public void WriteProgress(SimulationProgress progress)
    {
    }

    public void WriteStartMessage()
    {
    }

    public void WriteStoppedByUser()
    {
    }
}

public class FakeLifetime : IHostApplicationLifetime
{
    public int StopCalls { get; private set; }

    public CancellationToken ApplicationStarted => CancellationToken.None;
    public CancellationToken ApplicationStopping => CancellationToken.None;
    public CancellationToken ApplicationStopped => CancellationToken.None;

    public void StopApplication()
    {
        StopCalls++;
    }
}
