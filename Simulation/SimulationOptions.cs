namespace ChronicTruant.Simulation;

public class SimulationOptions
{
    public const string SectionName = "Simulation";

    public double DayIntervalSeconds { get; init; } = 5;
    public int PiePleasure { get; init; } = 1;
    public int SkipPleasure { get; init; } = 1;
    public double RandomRuleAskProbability { get; init; } = 0.5;
    public int KeyboardPollMilliseconds { get; init; } = 100;
}
