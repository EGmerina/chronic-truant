using ChronicTruant.History;

namespace ChronicTruant.Teachers;

public class YesterdayAskedRule : ITeacherRule
{
    private readonly Subject _subjectA;

    public YesterdayAskedRule(Subject subjectA)
    {
        _subjectA = subjectA;
    }

    public bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history)
    {
        if (day == 1)
        {
            return false;
        }

        return history.WasAsked(day - 1, _subjectA) == true;
    }
}