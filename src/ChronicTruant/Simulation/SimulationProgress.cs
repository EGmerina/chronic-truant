using ChronicTruant.Writer;

namespace ChronicTruant.Simulation;

public record SimulationProgress(
    int Day,
    DayResult Result,
    int TotalPleasure,
    double AveragePleasure);
