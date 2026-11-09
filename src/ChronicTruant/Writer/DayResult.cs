using ChronicTruant.History;
namespace ChronicTruant.Writer;

public record DayResult(
    int Pleasure,
    bool Expelled,
    Subject? ExpelledSubject);