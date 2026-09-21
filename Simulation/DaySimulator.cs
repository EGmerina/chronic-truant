using ChronicTruant.History;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Writer;

namespace ChronicTruant.Simulation;

public class DaySimulator
{
    private const int PiePleasure = 1;
    private const int SkipPleasure = 1;

    private readonly IReadOnlyList<Teacher> _teachers;
    private readonly ISkipStrategy _strategy;
    private readonly StudentHistory _history;

    public DaySimulator(
        IReadOnlyList<Teacher> teachers,
        ISkipStrategy strategy,
        StudentHistory history)
    {
        _teachers = teachers;
        _strategy = strategy;
        _history = history;
    }

    public DayResult Simulate(int day)
    {
        bool[] decisions = _strategy.DecideDay(day, _history);

        int pleasure = PiePleasure;

        for (int i = 0; i < _teachers.Count; i++)
        {
            Teacher teacher = _teachers[i];

            bool attended = decisions[i];
            bool wasAsked = teacher.WillAsk(day, _history);

            if (!attended && wasAsked)
            {
                _history.AddDay(
                    day,
                    teacher.Subject,
                    attended: false,
                    wasAsked: null);

                return new DayResult(
                    Pleasure: 0,
                    Expelled: true,
                    ExpelledSubject: teacher.Subject);
            }

            bool? askedResult = attended
                ? wasAsked
                : null;

            _history.AddDay(
                day,
                teacher.Subject,
                attended,
                askedResult);

            if (!attended)
            {
                pleasure += SkipPleasure;
            }
        }

        return new DayResult(
            Pleasure: pleasure,
            Expelled: false,
            ExpelledSubject: null);
    }
}