using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.Teachers;
using ChronicTruant.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Tests;

public class TeacherFactoryTests
{
    private static TeacherFactory CreateFactory(QueueRandom random)
    {
        return new TeacherFactory(random, Options.Create(new SimulationOptions()));
    }

    [Fact]
    public void GenerateRule_RandomRule_TakesSeedFromRandomSource()
    {
        QueueRandom random = new(ints: [(int)RuleType.RandomRule, 42]);

        TeacherRuleSettings settings = CreateFactory(random).GenerateRule();

        Assert.Equal(new TeacherRuleSettings(RuleType.RandomRule, Seed: 42), settings);
    }

    [Fact]
    public void GenerateRule_YesterdayAskedRule_TakesSubjectFromRandomSource()
    {
        QueueRandom random = new(ints: [(int)RuleType.YesterdayAskedRule, (int)Subject.Physics]);

        TeacherRuleSettings settings = CreateFactory(random).GenerateRule();

        Assert.Equal(new TeacherRuleSettings(RuleType.YesterdayAskedRule, SubjectA: Subject.Physics), settings);
    }

    [Fact]
    public void GenerateRule_XorRule_RerollsSecondSubjectWhenItMatchesFirst()
    {
        QueueRandom random = new(ints:
        [
            (int)RuleType.XorSubjectsRule,
            (int)Subject.Calculus,
            (int)Subject.Calculus,
            (int)Subject.Physics
        ]);

        TeacherRuleSettings settings = CreateFactory(random).GenerateRule();

        Assert.Equal(
            new TeacherRuleSettings(RuleType.XorSubjectsRule, SubjectA: Subject.Calculus, SubjectB: Subject.Physics),
            settings);
    }

    [Fact]
    public void Create_FromYesterdayAskedSettings_AsksAfterSubjectA()
    {
        Teacher teacher = CreateFactory(new QueueRandom()).Create(
            Subject.English,
            new TeacherRuleSettings(RuleType.YesterdayAskedRule, SubjectA: Subject.Physics));

        StudentHistory physicsAsked = HistoryBuilder.Attended(1, (_, subject) => subject == Subject.Physics);
        StudentHistory calculusAsked = HistoryBuilder.Attended(1, (_, subject) => subject == Subject.Calculus);

        Assert.Equal(Subject.English, teacher.Subject);
        Assert.True(teacher.WillAsk(2, physicsAsked));
        Assert.False(teacher.WillAsk(2, calculusAsked));
    }

    [Fact]
    public void Create_FromXorSettings_AsksWhenExactlyOneSubjectWasAsked()
    {
        Teacher teacher = CreateFactory(new QueueRandom()).Create(
            Subject.English,
            new TeacherRuleSettings(RuleType.XorSubjectsRule, SubjectA: Subject.Calculus, SubjectB: Subject.Physics));

        StudentHistory onlyCalculus = HistoryBuilder.Attended(1, (_, subject) => subject == Subject.Calculus);
        StudentHistory both = HistoryBuilder.Attended(
            1,
            (_, subject) => subject is Subject.Calculus or Subject.Physics);

        Assert.True(teacher.WillAsk(2, onlyCalculus));
        Assert.False(teacher.WillAsk(2, both));
    }

    [Fact]
    public void Create_FromRandomSettings_RestoredTeacherContinuesSameSequence()
    {
        TeacherFactory factory = CreateFactory(new QueueRandom());
        TeacherRuleSettings settings = new(RuleType.RandomRule, Seed: 12345);
        StudentHistory history = new();

        Teacher fromStart = factory.Create(Subject.English, settings);
        bool[] continuous = Enumerable.Range(1, 20).Select(day => fromStart.WillAsk(day, history)).ToArray();

        Teacher restored = factory.Create(Subject.English, settings, completedDays: 8);
        bool[] afterRestore = Enumerable.Range(9, 12).Select(day => restored.WillAsk(day, history)).ToArray();

        Assert.Equal(continuous[8..], afterRestore);
    }

    [Fact]
    public void Create_WithMissingParameters_Throws()
    {
        TeacherFactory factory = CreateFactory(new QueueRandom());

        Assert.Throws<InvalidOperationException>(() =>
            factory.Create(Subject.English, new TeacherRuleSettings(RuleType.YesterdayAskedRule)));
    }
}
