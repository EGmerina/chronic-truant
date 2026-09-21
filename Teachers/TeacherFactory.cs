using ChronicTruant.History;

namespace ChronicTruant.Teachers;

public class TeacherFactory
{
    private readonly Random _random;

    public TeacherFactory(Random random)
    {
        _random = random;
    }

    public Teacher Create(Subject subject)
    {
        RuleType[] types = Enum.GetValues<RuleType>();
        RuleType ruleType = types[_random.Next(types.Length)];

        ITeacherRule rule = ruleType switch
        {
            RuleType.RandomRule => new RandomRule(_random),
            RuleType.YesterdayAskedRule => new YesterdayAskedRule(GetRandomSubject()),
            RuleType.XorSubjectsRule => CreateXorRule(),
            _ => throw new InvalidOperationException($"Unknown rule: {ruleType}")
        };

        return new Teacher(subject, rule);
    }

    private Subject GetRandomSubject()
    {
        Subject[] subjects = Enum.GetValues<Subject>();
        return subjects[_random.Next(subjects.Length)];
    }

    private ITeacherRule CreateXorRule()
    {
        Subject[] subjects = Enum.GetValues<Subject>();

        Subject a = subjects[_random.Next(subjects.Length)];

        Subject b;
        int attempt = 0;
        do
        {
            b = subjects[_random.Next(subjects.Length)];
            if (attempt > subjects.Length)
            {
                b = subjects[((int)a + 1) % subjects.Length];
                break;
            }
            attempt++;
        }
        while (b == a);

        return new XorSubjectsRule(a, b);
    }
}