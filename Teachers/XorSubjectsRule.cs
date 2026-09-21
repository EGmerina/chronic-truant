using ChronicTruant.History;

namespace ChronicTruant.Teachers;

public class XorSubjectsRule : ITeacherRule
{
    private readonly Subject _subjectA;
    private readonly Subject _subjectB;

    public XorSubjectsRule(Subject subjectA, Subject subjectB)
    {
        _subjectA = subjectA;
        _subjectB = subjectB;
    }

    public bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history)
    {
        if (day == 1)
        {
            return false;
        }

        bool askedA = history.WasAsked(day - 1, _subjectA) == true;
        bool askedB = history.WasAsked(day - 1, _subjectB) == true;

        return askedA != askedB;
    }
}