using ChronicTruant.History;
using ChronicTruant.Persistence;
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
    public void Runner_RestoredFromProgress_ContinuesNumberingAndPleasure()
    {
        List<Teacher> teachers = CreateTeachers(_ => new AskOnDaysRule());
        SimulationRunner runner = new(
            CreateSimulator(teachers, new FixedStrategy(AllSubjects(true)), new StudentHistory()),
            completedDays: 10,
            totalPleasure: 25);

        SimulationProgress progress = runner.SimulateNextDay();

        Assert.Equal(11, progress.Day);
        Assert.Equal(26, progress.TotalPleasure);
    }

    [Fact]
    public async Task Worker_StopsApplicationAfterExpulsion_AndProcessesNoMoreDays()
    {
        AskOnDaysRule english = new(3);
        List<Teacher> teachers = CreateTeachers(subject =>
            subject == Subject.English ? english : new AskOnDaysRule());
        SemesterSession session = CreateSession(teachers, AllSubjects(attend: false), semesterDays: 100);

        RecordingWriter writer = new();
        FakeLifetime lifetime = new();
        await RunWorkerAsync(session, writer, lifetime);

        Assert.Equal([1, 2, 3], writer.Days.Select(d => d.Day));
        Assert.True(writer.Days[^1].Result.Expelled);
        Assert.Null(writer.Finished);
        Assert.Equal(1, lifetime.StopCalls);
        Assert.Equal(3, english.Calls);
    }

    [Fact]
    public async Task Worker_StopsApplicationAtEndOfSemester()
    {
        List<Teacher> teachers = CreateTeachers(_ => new AskOnDaysRule());
        SemesterSession session = CreateSession(teachers, AllSubjects(attend: true), semesterDays: 4);

        RecordingWriter writer = new();
        FakeLifetime lifetime = new();
        await RunWorkerAsync(session, writer, lifetime);

        Assert.Equal([1, 2, 3, 4], writer.Days.Select(d => d.Day));
        Assert.Equal(4, writer.Finished?.Day);
        Assert.Equal(1, lifetime.StopCalls);
    }

    private static SemesterSession CreateSession(
        List<Teacher> teachers,
        bool[] decisions,
        int semesterDays,
        ISemesterStore? store = null)
    {
        StudentHistory history = new();
        SimulationRunner runner = new(CreateSimulator(teachers, new FixedStrategy(decisions), history));

        return new SemesterSession(
            id: 1,
            rules: new Dictionary<Subject, TeacherRuleSettings>(),
            runner,
            history,
            store ?? new InMemorySemesterStore(),
            semesterDays);
    }

    private static async Task RunWorkerAsync(SemesterSession session, RecordingWriter writer, FakeLifetime lifetime)
    {
        SimulationWorker worker = new(
            session,
            writer,
            lifetime,
            Options.Create(new SimulationOptions { DayIntervalSeconds = 0.001 }));

        await worker.StartAsync(CancellationToken.None);
        await worker.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Session_SavesEveryDayWithItsLessons()
    {
        InMemorySemesterStore store = new();
        bool[] decisions = AllSubjects(attend: true);
        decisions[(int)Subject.Physics] = false;
        List<Teacher> teachers = CreateTeachers(subject =>
            subject == Subject.Calculus ? new AskOnDaysRule(2) : new AskOnDaysRule());
        SemesterSession session = CreateSession(teachers, decisions, semesterDays: 3, store);

        session.SimulateNextDay();
        session.SimulateNextDay();
        session.SimulateNextDay();

        Assert.Equal([1, 2, 3], store.SavedDays.Select(d => d.Progress.Day));
        Assert.Equal([false, false, true], store.SavedDays.Select(d => d.Finished));
        Assert.All(store.SavedDays, day => Assert.Equal(Subjects.Length, day.Lessons.Count));

        LessonRecord calculusDay2 = store.SavedDays[1].Lessons.Single(l => l.Subject == Subject.Calculus);
        LessonRecord physicsDay2 = store.SavedDays[1].Lessons.Single(l => l.Subject == Subject.Physics);
        Assert.Equal(new LessonRecord(Subject.Calculus, Attended: true, WasAsked: true), calculusDay2);
        Assert.Equal(new LessonRecord(Subject.Physics, Attended: false, WasAsked: null), physicsDay2);

        Assert.True(session.IsFinished);
        Assert.Throws<InvalidOperationException>(() => session.SimulateNextDay());
    }

    [Fact]
    public void Session_OnExpulsion_SavesFinishedDayWithLessonsUpToExpulsion()
    {
        InMemorySemesterStore store = new();
        List<Teacher> teachers = CreateTeachers(subject =>
            subject == Subject.DiscreteMath ? new AskOnDaysRule(1) : new AskOnDaysRule());
        SemesterSession session = CreateSession(teachers, AllSubjects(attend: false), semesterDays: 100, store);

        session.SimulateNextDay();

        var saved = Assert.Single(store.SavedDays);
        Assert.True(saved.Finished);
        Assert.Equal(0, saved.Progress.TotalPleasure);
        Assert.Equal(
            [Subject.Calculus, Subject.LinearAlgebra, Subject.DiscreteMath],
            saved.Lessons.Select(l => l.Subject));
    }
}
