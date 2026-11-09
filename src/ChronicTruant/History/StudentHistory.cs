
namespace ChronicTruant.History;

public class StudentHistory : IReadOnlyStudentHistory
{
    private readonly Dictionary<(int Day, Subject Subject), bool> _attendance = new();

    private readonly Dictionary<(int Day, Subject Subject), bool?> _asked = new();

    public bool Attended(int day, Subject subject)
    {
        return _attendance[(day, subject)];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        return _asked[(day, subject)];
    }

    public void AddDay(int day, Subject subject, bool attended, bool? wasAsked)
    {
        _attendance[(day, subject)] = attended;
        _asked[(day, subject)] = wasAsked;
    }
}