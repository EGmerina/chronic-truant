using ChronicTruant.History;

namespace ChronicTruant.StudentStrategy;

public class AlwaysAttendStrategy : ISkipStrategy
{
    public string Name => "Always Attend Strategy";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        bool[] decisions = new bool[Enum.GetValues<Subject>().Length];
        Array.Fill(decisions, true);

        return decisions;
    }
}
