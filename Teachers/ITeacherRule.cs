using ChronicTruant.History;
using ChronicTruant.Teachers;

namespace ChronicTruant.Teachers;

public interface ITeacherRule
{
    bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history);
}