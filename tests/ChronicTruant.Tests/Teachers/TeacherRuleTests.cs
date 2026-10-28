using ChronicTruant.History;
using ChronicTruant.Teachers;
using ChronicTruant.Tests.Fakes;

namespace ChronicTruant.Tests;

public class TeacherRuleTests
{
    private const Subject Own = Subject.English;
    private const Subject A = Subject.Calculus;
    private const Subject B = Subject.Physics;

    [Fact]
    public void RandomRule_FollowsRandomSource()
    {
        QueueRandom random = new(doubles: [0.1, 0.9, 0.49, 0.5, 0.0]);
        RandomRule rule = new(random, askProbability: 0.5);
        StudentHistory history = new();

        bool[] asked = Enumerable.Range(1, 5)
            .Select(day => rule.WillAsk(day, Own, history))
            .ToArray();

        Assert.Equal([true, false, true, false, true], asked);
    }

    [Theory]
    [InlineData(0.0, false)]
    [InlineData(1.0, true)]
    public void RandomRule_RespectsProbabilityBounds(double probability, bool expected)
    {
        RandomRule rule = new(new QueueRandom(doubles: [0.0, 0.999]), probability);
        StudentHistory history = new();

        Assert.Equal(expected, rule.WillAsk(1, Own, history));
        Assert.Equal(expected, rule.WillAsk(2, Own, history));
    }

    [Fact]
    public void YesterdayAskedRule_NeverAsksOnFirstDay()
    {
        YesterdayAskedRule rule = new(A);

        Assert.False(rule.WillAsk(1, Own, new StudentHistory()));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    public void YesterdayAskedRule_RepeatsYesterdayForSubjectA(bool? askedYesterday, bool expected)
    {
        StudentHistory history = new();
        history.AddDay(1, A, attended: askedYesterday is not null, askedYesterday);

        Assert.Equal(expected, new YesterdayAskedRule(A).WillAsk(2, Own, history));
    }

    [Fact]
    public void YesterdayAskedRule_FollowsSequence()
    {
        bool[] aSequence = [true, false, false, true, true, false];
        StudentHistory history = HistoryBuilder.Attended(
            aSequence.Length,
            (day, subject) => subject == A && aSequence[day - 1]);
        YesterdayAskedRule rule = new(A);

        bool[] asked = Enumerable.Range(2, aSequence.Length - 1)
            .Select(day => rule.WillAsk(day, Own, history))
            .ToArray();

        Assert.Equal(aSequence[..^1], asked);
    }

    [Fact]
    public void XorSubjectsRule_NeverAsksOnFirstDay()
    {
        XorSubjectsRule rule = new(A, B);

        Assert.False(rule.WillAsk(1, Own, new StudentHistory()));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(null, true, true)]
    [InlineData(true, null, true)]
    [InlineData(null, null, false)]
    public void XorSubjectsRule_AsksWhenExactlyOneWasAsked(bool? askedA, bool? askedB, bool expected)
    {
        StudentHistory history = new();
        history.AddDay(1, A, attended: askedA is not null, askedA);
        history.AddDay(1, B, attended: askedB is not null, askedB);

        Assert.Equal(expected, new XorSubjectsRule(A, B).WillAsk(2, Own, history));
    }

    [Fact]
    public void Teacher_DelegatesToItsRuleWithOwnSubject()
    {
        StudentHistory history = new();
        history.AddDay(1, A, attended: true, wasAsked: true);
        Teacher teacher = new(Own, new YesterdayAskedRule(A));

        Assert.Equal(Own, teacher.Subject);
        Assert.True(teacher.WillAsk(2, history));
    }
}
