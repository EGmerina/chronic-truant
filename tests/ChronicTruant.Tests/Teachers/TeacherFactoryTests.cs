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
    public void Create_RandomRule_UsesRandomSourceForEachQuestion()
    {
        QueueRandom random = new(doubles: [0.2, 0.8], ints: [(int)RuleType.RandomRule]);
        Teacher teacher = CreateFactory(random).Create(Subject.English);
        StudentHistory history = new();

        Assert.Equal(Subject.English, teacher.Subject);
        Assert.True(teacher.WillAsk(1, history));
        Assert.False(teacher.WillAsk(2, history));
    }

    [Fact]
    public void Create_YesterdayAskedRule_UsesRandomlyChosenSubject()
    {
        QueueRandom random = new(ints: [(int)RuleType.YesterdayAskedRule, (int)Subject.Physics]);
        Teacher teacher = CreateFactory(random).Create(Subject.English);

        StudentHistory physicsAsked = HistoryBuilder.Attended(1, (_, subject) => subject == Subject.Physics);
        StudentHistory calculusAsked = HistoryBuilder.Attended(1, (_, subject) => subject == Subject.Calculus);

        Assert.True(teacher.WillAsk(2, physicsAsked));
        Assert.False(teacher.WillAsk(2, calculusAsked));
    }

    [Fact]
    public void Create_XorRule_RerollsSecondSubjectWhenItMatchesFirst()
    {
        QueueRandom random = new(ints:
        [
            (int)RuleType.XorSubjectsRule,
            (int)Subject.Calculus,
            (int)Subject.Calculus,
            (int)Subject.Physics
        ]);
        Teacher teacher = CreateFactory(random).Create(Subject.English);

        StudentHistory onlyCalculus = HistoryBuilder.Attended(1, (_, subject) => subject == Subject.Calculus);
        StudentHistory both = HistoryBuilder.Attended(
            1,
            (_, subject) => subject is Subject.Calculus or Subject.Physics);

        Assert.True(teacher.WillAsk(2, onlyCalculus));
        Assert.False(teacher.WillAsk(2, both));
    }
}
