
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

    /// <summary>Все пары указанного дня, которые были записаны (при вылете часть пар дня отсутствует).</summary>
    public IReadOnlyList<LessonRecord> GetDay(int day)
    {
        return Enum.GetValues<Subject>()
            .Where(subject => _attendance.ContainsKey((day, subject)))
            .Select(subject => new LessonRecord(subject, _attendance[(day, subject)], _asked[(day, subject)]))
            .ToList();
    }

    public void AddDay(int day, Subject subject, bool attended, bool? wasAsked)
    {
        _attendance[(day, subject)] = attended;
        _asked[(day, subject)] = wasAsked;
    }
}