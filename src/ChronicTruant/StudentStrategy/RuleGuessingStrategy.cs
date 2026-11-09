using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.Teachers;
using Microsoft.Extensions.Options;

namespace ChronicTruant.StudentStrategy;

/// <summary>
/// Угадывает правило каждого преподавателя по истории и прогуливает пару,
/// только если вероятность вопроса сегодня ниже допустимого риска.
/// </summary>
public class RuleGuessingStrategy : ISkipStrategy
{
    private readonly double _randomAskProbability;
    private readonly double _maxSkipRisk;

    public RuleGuessingStrategy(IOptions<SimulationOptions> options)
    {
        _randomAskProbability = options.Value.RandomRuleAskProbability;
        _maxSkipRisk = options.Value.MaxSkipRisk;
    }

    public string Name => "Rule Guessing Strategy";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        Subject[] subjects = Enum.GetValues<Subject>();
        List<Hypothesis> hypotheses = CreateHypotheses(subjects);
        bool[] decisions = new bool[subjects.Length];

        for (int i = 0; i < subjects.Length; i++)
        {
            double askProbability = PredictAskProbability(day, subjects[i], history, hypotheses);
            decisions[i] = askProbability >= _maxSkipRisk;
        }

        return decisions;
    }

    /// <summary>
    /// Все правила, которые мог выбрать преподаватель, с априорными весами:
    /// каждый тип правила равновероятен, внутри типа все варианты предметов тоже.
    /// </summary>
    private static List<Hypothesis> CreateHypotheses(Subject[] subjects)
    {
        double typePrior = 1.0 / Enum.GetValues<RuleType>().Length;
        int pairCount = subjects.Length * (subjects.Length - 1) / 2;

        List<Hypothesis> hypotheses = [];

        foreach (Subject a in subjects)
        {
            hypotheses.Add(new Hypothesis(new YesterdayAskedRule(a), typePrior / subjects.Length));
        }

        for (int a = 0; a < subjects.Length; a++)
        {
            for (int b = a + 1; b < subjects.Length; b++)
            {
                hypotheses.Add(new Hypothesis(
                    new XorSubjectsRule(subjects[a], subjects[b]),
                    typePrior / pairCount));
            }
        }

        // Случайное правило не задаётся объектом: его правдоподобие считается отдельно.
        hypotheses.Add(new Hypothesis(Rule: null, typePrior));

        return hypotheses;
    }

    /// <summary>
    /// Байесовская оценка вероятности, что по предмету спросят сегодня.
    /// Прошлые дни известны полностью: если студент прогулял и не вылетел, значит не спрашивали.
    /// </summary>
    private double PredictAskProbability(
        int day,
        Subject subject,
        IReadOnlyStudentHistory history,
        List<Hypothesis> hypotheses)
    {
        double totalWeight = 0;
        double askWeight = 0;

        foreach (Hypothesis hypothesis in hypotheses)
        {
            double weight = hypothesis.Prior * Likelihood(hypothesis.Rule, day, subject, history);
            if (weight == 0)
            {
                continue;
            }

            double askToday = hypothesis.Rule is null
                ? _randomAskProbability
                : hypothesis.Rule.WillAsk(day, subject, history) ? 1 : 0;

            totalWeight += weight;
            askWeight += weight * askToday;
        }

        return askWeight / totalWeight;
    }

    /// <summary>Вероятность увидеть прошлые вопросы по предмету, если верна данная гипотеза.</summary>
    private double Likelihood(
        ITeacherRule? rule,
        int day,
        Subject subject,
        IReadOnlyStudentHistory history)
    {
        double likelihood = 1;

        for (int pastDay = 1; pastDay < day; pastDay++)
        {
            bool wasAsked = history.WasAsked(pastDay, subject) == true;

            if (rule is null)
            {
                likelihood *= wasAsked ? _randomAskProbability : 1 - _randomAskProbability;
            }
            else if (rule.WillAsk(pastDay, subject, history) != wasAsked)
            {
                return 0;
            }
        }

        return likelihood;
    }

    private record Hypothesis(ITeacherRule? Rule, double Prior);
}
