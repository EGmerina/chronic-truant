using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Tests;

public class RuleGuessingStrategyTests
{
    private const bool Attend = true;
    private const bool Skip = false;

    private static readonly Subject[] Subjects = HistoryBuilder.Subjects;

    private readonly RuleGuessingStrategy _strategy = new(Options.Create(new SimulationOptions()));

    [Fact]
    public void EmptyHistory_AttendsEverything()
    {
        bool[] decisions = _strategy.DecideDay(1, new StudentHistory());

        Assert.Equal(Subjects.Length, decisions.Length);
        Assert.All(decisions, decision => Assert.Equal(Attend, decision));
    }

    [Fact]
    public void ShortQuietHistory_StillAttends_BecauseRandomTeacherIsNotRuledOut()
    {
        StudentHistory history = HistoryBuilder.Attended(3, (_, _) => false);

        bool[] decisions = _strategy.DecideDay(4, history);

        Assert.All(decisions, decision => Assert.Equal(Attend, decision));
    }

    [Fact]
    public void StableNotAsked_SkipsEverything()
    {
        StudentHistory history = HistoryBuilder.Attended(20, (_, _) => false);

        bool[] decisions = _strategy.DecideDay(21, history);

        Assert.All(decisions, decision => Assert.Equal(Skip, decision));
    }

    [Fact]
    public void StableAsked_AttendsEverything()
    {
        StudentHistory history = HistoryBuilder.Attended(20, (_, _) => true);

        bool[] decisions = _strategy.DecideDay(21, history);

        Assert.All(decisions, decision => Assert.Equal(Attend, decision));
    }

    [Fact]
    public void SkippedAndSurvivedDays_AreTreatedAsNotAsked()
    {
        StudentHistory history = new();
        for (int day = 1; day <= 20; day++)
        {
            foreach (Subject subject in Subjects)
            {
                history.AddDay(day, subject, attended: false, wasAsked: null);
            }
        }

        bool[] decisions = _strategy.DecideDay(21, history);

        Assert.All(decisions, decision => Assert.Equal(Skip, decision));
    }

    /// <summary>
    /// Физику ведёт «случайный» преподаватель, остальные — детерминированные правила,
    /// зависящие от физики. Стратегия должна ходить на физику и угадать остальные правила.
    /// </summary>
    [Fact]
    public void MixedHistory_PredictsDeterministicTeachersAndAttendsRandomOne()
    {
        bool[] physics =
        [
            true, false, true, true, false, false, true, false, true, true,
            true, false, false, false, true, false, true, true, false, true,
            false, true, false, false, true, true, true, false, true, false
        ];
        int days = physics.Length;

        Dictionary<Subject, ITeacherRule> rules = new()
        {
            [Subject.Calculus] = new YesterdayAskedRule(Subject.Physics),
            [Subject.LinearAlgebra] = new XorSubjectsRule(Subject.Physics, Subject.Calculus),
            [Subject.DiscreteMath] = new YesterdayAskedRule(Subject.DiscreteMath),
            [Subject.Programming] = new XorSubjectsRule(Subject.LinearAlgebra, Subject.Physics),
            [Subject.English] = new YesterdayAskedRule(Subject.Calculus)
        };

        StudentHistory history = new();
        for (int day = 1; day <= days; day++)
        {
            history.AddDay(day, Subject.Physics, attended: true, physics[day - 1]);
            foreach ((Subject subject, ITeacherRule rule) in rules)
            {
                history.AddDay(day, subject, attended: true, rule.WillAsk(day, subject, history));
            }
        }

        int today = days + 1;
        bool[] decisions = _strategy.DecideDay(today, history);

        Assert.Equal(Attend, decisions[(int)Subject.Physics]);
        // Проверка не вырождена: сегодня есть и пары, на которых спросят, и пары без вопроса.
        Assert.Contains(rules, pair => pair.Value.WillAsk(today, pair.Key, history));
        Assert.Contains(rules, pair => !pair.Value.WillAsk(today, pair.Key, history));
        foreach ((Subject subject, ITeacherRule rule) in rules)
        {
            bool willAsk = rule.WillAsk(today, subject, history);
            Assert.True(decisions[(int)subject] == willAsk, $"{subject}: спросят = {willAsk}");
        }
    }

    [Theory]
    [MemberData(nameof(AllSubjects))]
    public void DecisionsAreIndependentPerSubject(Subject noisySubject)
    {
        StudentHistory history = HistoryBuilder.Attended(20, (_, subject) => subject == noisySubject);

        bool[] decisions = _strategy.DecideDay(21, history);

        foreach (Subject subject in Subjects)
        {
            bool expected = subject == noisySubject ? Attend : Skip;
            Assert.True(decisions[(int)subject] == expected, $"{subject}");
        }
    }

    [Fact]
    public void SameHistory_GivesSameDecisions()
    {
        StudentHistory history = HistoryBuilder.Attended(15, (day, subject) => (day + (int)subject) % 3 == 0);

        Assert.Equal(_strategy.DecideDay(16, history), _strategy.DecideDay(16, history));
    }

    public static TheoryData<Subject> AllSubjects()
    {
        return new TheoryData<Subject>(Subjects);
    }
}
