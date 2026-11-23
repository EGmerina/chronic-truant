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

    /// <summary>
    /// Создаёт правило с собственным генератором из seed и прокручивает его на completedDays дней:
    /// симулятор спрашивает преподавателя ровно один раз в день, поэтому после восстановления
    /// последовательность продолжается с того же места, что и при непрерывном прогоне.
    /// </summary>
    public static RandomRule FromSeed(int seed, double askProbability, int completedDays)
    {
        Random random = new(seed);
        for (int day = 0; day < completedDays; day++)
        {
            random.NextDouble();
        }

        return new RandomRule(random, askProbability);
    }

    public bool WillAsk(int day, Subject subject, IReadOnlyStudentHistory history)
    {
        return _random.NextDouble() < _askProbability;
    }
}