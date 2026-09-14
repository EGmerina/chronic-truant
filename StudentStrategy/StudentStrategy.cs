namespace Strategy;

public class Strategy : ISkipStrategy
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