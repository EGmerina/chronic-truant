using ChronicTruant.History;
namespace ChronicTruant.Simulation;

public record DayResult(
    int Pleasure,
    bool Expelled,
    Subject? ExpelledSubject);