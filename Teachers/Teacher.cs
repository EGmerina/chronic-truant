using ChronicTruant.History;
namespace ChronicTruant.Teachers;

public class Teacher
{
    public Subject Subject { get; }

    private readonly ITeacherRule _rule;

    public Teacher(Subject subject, ITeacherRule rule)
    {
        Subject = subject;
        _rule = rule;
    }

    public bool WillAsk(int day, IReadOnlyStudentHistory history)
    {
        return _rule.WillAsk(day, Subject, history);
    }
}