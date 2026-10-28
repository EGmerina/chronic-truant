using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Tests.Fakes;
using ChronicTruant.Writer;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Tests;

public class SimulationTests
{
    private static readonly Subject[] Subjects = HistoryBuilder.Subjects;
    private static readonly SimulationOptions DefaultOptions = new();

    private static List<Teacher> CreateTeachers(Func<Subject, ITeacherRule> ruleFor)
    {
        return Subjects.Select(subject => new Teacher(subject, ruleFor(subject))).ToList();
    }

    private static bool[] AllSubjects(bool attend)
    {
        return Enumerable.Repeat(attend, Subjects.Length).ToArray();
    }

    private static DaySimulator CreateSimulator(
        IReadOnlyList<Teacher> teachers,
        ISkipStrategy strategy,
        StudentHistory history)
    {
        return new DaySimulator(teachers, strategy, history, Options.Create(DefaultOptions));
    }

    [Theory]
    [InlineData(true, false, 1, false, false)]
    [InlineData(true, true, 1, false, true)]
    [InlineData(false, false, 2, false, null)]
    [InlineData(false, true, 0, true, true)]
    public void SingleLesson_AllAttendanceAndQuestionCombinations(
        bool attended,
        bool asked,
        int expectedPleasure,
        bool expectedExpelled,
        bool? expectedHistory)
    {
        // На одну пару (матанализ) влияет решение, на остальные студент ходит и его не спрашивают.
        bool[] decisions = AllSubjects(attend: true);
        decisions[(int)Subject.Calculus] = attended;

        List<Teacher> teachers = CreateTeachers(subject =>
            subject == Subject.Calculus && asked ? new AskOnDaysRule(1) : new AskOnDaysRule());
        StudentHistory history = new();

        DayResult result = CreateSimulator(teachers, new FixedStrategy(decisions), history).Simulate(1);

        Assert.Equal(expectedPleasure, result.Pleasure);
        Assert.Equal(expectedExpelled, result.Expelled);
        Assert.Equal(expectedExpelled ? Subject.Calculus : null, result.ExpelledSubject);
        Assert.Equal(attended, history.Attended(1, Subject.Calculus));
        Assert.Equal(expectedHistory, history.WasAsked(1, Subject.Calculus));
    }

    [Fact]
    public void SkippingEverythingSafely_GivesPiePlusSixSkips()
    {
        List<Teacher> teachers = CreateTeachers(_ => new AskOnDaysRule());

        DayResult result = CreateSimulator(teachers, new FixedStrategy(AllSubjects(false)), new StudentHistory())
            .Simulate(1);

        Assert.Equal(DefaultOptions.PiePleasure + Subjects.Length * DefaultOptions.SkipPleasure, result.Pleasure);
    }

    [Fact]
    public void Expulsion_StopsProcessingRemainingLessonsOfTheDay()
    {
        AskOnDaysRule calculus = new(1);
        AskOnDaysRule linearAlgebra = new();
        List<Teacher> teachers = CreateTeachers(subject => subject switch
        {
            Subject.Calculus => calculus,
            Subject.LinearAlgebra => linearAlgebra,
            _ => new AskOnDaysRule()
        });

        DayResult result = CreateSimulator(teachers, new FixedStrategy(AllSubjects(false)), new StudentHistory())
            .Simulate(1);

        Assert.True(result.Expelled);
        Assert.Equal(0, linearAlgebra.Calls);
    }

    [Fact]
    public void YesterdayState_IsUpdatedBetweenDays()
    {
        // Матанализ спрашивают в 1-й и 3-й день; остальные повторяют вчерашний матанализ.
        List<Teacher> teachers = CreateTeachers(subject => subject == Subject.Calculus
            ? new AskOnDaysRule(1, 3)
            : new YesterdayAskedRule(Subject.Calculus));
        StudentHistory history = new();
        DaySimulator simulator = CreateSimulator(teachers, new FixedStrategy(AllSubjects(true)), history);

        for (int day = 1; day <= 4; day++)
        {
            simulator.Simulate(day);
        }

        Assert.Equal([true, false, true, false], Enumerable.Range(1, 4).Select(d => history.WasAsked(d, Subject.Calculus)));
        Assert.Equal([false, true, false, true], Enumerable.Range(1, 4).Select(d => history.WasAsked(d, Subject.Physics)));
    }

    [Fact]
    public void Runner_AccumulatesPleasureAndResetsOnExpulsion()
    {
        // Студент прогуливает только физику; на третий день по ней спрашивают.
        bool[] decisions = AllSubjects(attend: true);
        decisions[(int)Subject.Physics] = false;
        List<Teacher> teachers = CreateTeachers(subject =>
            subject == Subject.Physics ? new AskOnDaysRule(3) : new AskOnDaysRule());
        SimulationRunner runner = new(CreateSimulator(teachers, new FixedStrategy(decisions), new StudentHistory()));

        Assert.Equal(2, runner.SimulateNextDay().TotalPleasure);
        Assert.Equal(4, runner.SimulateNextDay().TotalPleasure);

        SimulationProgress expelled = runner.SimulateNextDay();

        Assert.True(expelled.Result.Expelled);
        Assert.Equal(0, expelled.TotalPleasure);
        Assert.True(runner.IsExpelled);
        Assert.Throws<InvalidOperationException>(() => runner.SimulateNextDay());
    }

    [Fact]
    public async Task Worker_StopsApplicationAfterExpulsion_AndProcessesNoMoreDays()
    {
        bool[] decisions = AllSubjects(attend: false);
        AskOnDaysRule english = new(3);
        List<Teacher> teachers = CreateTeachers(subject =>
            subject == Subject.English ? english : new AskOnDaysRule());
        SimulationRunner runner = new(CreateSimulator(teachers, new FixedStrategy(decisions), new StudentHistory()));

        RecordingWriter writer = new();
        FakeLifetime lifetime = new();
        SimulationWorker worker = new(
            runner,
            writer,
            lifetime,
            Options.Create(new SimulationOptions { DayIntervalSeconds = 0.001 }));

        await worker.StartAsync(CancellationToken.None);
        await worker.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal([1, 2, 3], writer.Days.Select(d => d.Day));
        Assert.True(writer.Days[^1].Result.Expelled);
        Assert.Equal(1, lifetime.StopCalls);
        Assert.Equal(3, english.Calls);
    }
}
