using ChronicTruant.History;

namespace ChronicTruant.StudentStrategy;

public class StudentStrategy : ISkipStrategy
{
    public string Name => "My First Strategy";

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
