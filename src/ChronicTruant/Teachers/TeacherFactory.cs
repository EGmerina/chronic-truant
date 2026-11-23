using ChronicTruant.History;
using ChronicTruant.Simulation;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Teachers;

public class TeacherFactory
{
    private readonly Random _random;
    private readonly double _randomRuleAskProbability;

    public TeacherFactory(Random random, IOptions<SimulationOptions> options)
    {
        _random = random;
        _randomRuleAskProbability = options.Value.RandomRuleAskProbability;
    }

    public Teacher Create(Subject subject)
    {
        return Create(subject, GenerateRule());
    }

    /// <summary>Случайно выбирает тип правила и его параметры.</summary>
    public TeacherRuleSettings GenerateRule()
    {
        RuleType[] types = Enum.GetValues<RuleType>();
        RuleType ruleType = types[_random.Next(types.Length)];

        return ruleType switch
        {
            RuleType.RandomRule => new TeacherRuleSettings(ruleType, Seed: _random.Next()),
            RuleType.YesterdayAskedRule => new TeacherRuleSettings(ruleType, SubjectA: GetRandomSubject()),
            RuleType.XorSubjectsRule => GenerateXorRule(),
            _ => throw new InvalidOperationException($"Unknown rule: {ruleType}")
        };
    }

    /// <summary>
    /// Создаёт преподавателя по сохранённым параметрам правила.
    /// completedDays — сколько дней семестра уже прошло (нужно, чтобы продолжить случайную последовательность).
    /// </summary>
    public Teacher Create(Subject subject, TeacherRuleSettings settings, int completedDays = 0)
    {
        ITeacherRule rule = settings.Type switch
        {
            RuleType.RandomRule => RandomRule.FromSeed(
                Require(settings.Seed),
                _randomRuleAskProbability,
                completedDays),
            RuleType.YesterdayAskedRule => new YesterdayAskedRule(Require(settings.SubjectA)),
            RuleType.XorSubjectsRule => new XorSubjectsRule(Require(settings.SubjectA), Require(settings.SubjectB)),
            _ => throw new InvalidOperationException($"Unknown rule: {settings.Type}")
        };

        return new Teacher(subject, rule);
    }

    private static T Require<T>(T? value) where T : struct
    {
        return value ?? throw new InvalidOperationException("В параметрах правила не хватает значения.");
    }

    private Subject GetRandomSubject()
    {
        Subject[] subjects = Enum.GetValues<Subject>();
        return subjects[_random.Next(subjects.Length)];
    }

    private TeacherRuleSettings GenerateXorRule()
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

        return new TeacherRuleSettings(RuleType.XorSubjectsRule, SubjectA: a, SubjectB: b);
    }
}
