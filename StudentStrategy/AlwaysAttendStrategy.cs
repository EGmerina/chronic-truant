using ChronicTruant.History;

namespace ChronicTruant.StudentStrategy;

public class AlwaysAttendStrategy : ISkipStrategy
{
    public string Name => "Always Attend Strategy";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        return
        [
            true,
            true,
            true,
            true,
            true,
            true
        ];
    }
}
