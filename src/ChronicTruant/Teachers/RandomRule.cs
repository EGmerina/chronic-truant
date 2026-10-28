using ChronicTruant.History;

namespace ChronicTruant.Teachers;


public class RandomRule : ITeacherRule
{
    private readonly Random _random;
    private readonly double _askProbability;

    public RandomRule(Random random, double askProbability)
    {
        _random = random;
        _askProbability = askProbability;
    }

    public bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history)
    {
        return _random.NextDouble() < _askProbability;
    }
}