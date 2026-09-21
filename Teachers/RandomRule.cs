using ChronicTruant.History;

namespace ChronicTruant.Teachers;


public class RandomRule : ITeacherRule
{
    private readonly Random _random;

    public RandomRule(Random random)
    {
        _random = random;
    }

    public bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history)
    {
        return _random.NextDouble() < 0.5;
    }
}