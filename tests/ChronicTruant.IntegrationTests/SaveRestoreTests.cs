using ChronicTruant.History;
using ChronicTruant.IntegrationTests.Infrastructure;
using ChronicTruant.Persistence;
using ChronicTruant.Simulation;

namespace ChronicTruant.IntegrationTests;

public class SaveRestoreTests : IClassFixture<PostgresFixture>
{
    private const int SemesterLength = 60;

    private readonly PostgresFixture _postgres;
    private readonly int _seed = TestSemesters.SeedWithAllRuleTypes();

    public SaveRestoreTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(25)]
    public void StateSavedOnDayD_IsRestoredIdentically(int d)
    {
        EfSemesterStore store = _postgres.CreateStore();
        SemesterSession original = TestSemesters.CreateLoader(store, _seed).StartNew();
        List<SimulationProgress> progress = TestSemesters.Run(original, d);

        SavedSemester saved = store.Load(original.Id)!;
        // Другой seed доказывает, что всё нужное берётся из базы, а не генерируется заново.
        SemesterSession restored = TestSemesters.CreateLoader(store, seed: _seed + 1000).Restore(saved);

        Assert.Equivalent(original.Rules, saved.Rules, strict: true);
        Assert.Equivalent(original.Rules, restored.Rules, strict: true);

        Assert.Equal(d, saved.CompletedDays);
        Assert.Equal(original.TotalPleasure, saved.TotalPleasure);
        Assert.False(saved.IsExpelled);
        Assert.False(saved.IsFinished);

        Assert.Equal(original.CompletedDays, restored.CompletedDays);
        Assert.Equal(original.TotalPleasure, restored.TotalPleasure);

        Assert.Equal(
            progress.Select(p => (p.Day, p.Result.Pleasure, p.TotalPleasure, p.Result.Expelled)),
            saved.Days.Select(day => (day.Day, day.Pleasure, day.TotalPleasure, day.Expelled)));

        for (int day = 1; day <= d; day++)
        {
            Assert.Equal(original.History.GetDay(day), saved.Days[day - 1].Lessons);
            Assert.Equal(original.History.GetDay(day), restored.History.GetDay(day));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(25)]
    [InlineData(59)]
    public void ContinuingAfterRestore_GivesSameResultAsUninterruptedRun(int d)
    {
        EfSemesterStore store = _postgres.CreateStore();

        SemesterSession uninterrupted = TestSemesters.CreateLoader(store, _seed).StartNew();
        List<SimulationProgress> expected = TestSemesters.Run(uninterrupted, SemesterLength);

        SemesterSession beforeRestart = TestSemesters.CreateLoader(store, _seed).StartNew();
        List<SimulationProgress> actual = TestSemesters.Run(beforeRestart, d);

        // «Перезапуск приложения»: новые объекты, другой seed, всё состояние — только из базы.
        SemesterSession afterRestart = TestSemesters
            .CreateLoader(store, seed: _seed + 1000)
            .Restore(store.Load(beforeRestart.Id)!);
        actual.AddRange(TestSemesters.Run(afterRestart, SemesterLength - d));

        Assert.Equal(expected, actual);
        Assert.True(expected[^1].TotalPleasure > SemesterLength, "Стратегия должна что-то прогуливать, иначе тест вырожден.");

        SavedSemester expectedSaved = store.Load(uninterrupted.Id)!;
        SavedSemester actualSaved = store.Load(afterRestart.Id)!;
        Assert.Equal(expectedSaved.CompletedDays, actualSaved.CompletedDays);
        Assert.Equal(expectedSaved.TotalPleasure, actualSaved.TotalPleasure);
        Assert.Equivalent(expectedSaved.Days, actualSaved.Days, strict: true);
    }

    [Fact]
    public void Expulsion_IsSavedAndSemesterCannotBeContinued()
    {
        EfSemesterStore store = _postgres.CreateStore();
        SemesterSession session = TestSemesters
            .CreateLoader(store, _seed, strategy: new AlwaysSkipStrategy())
            .StartNew();

        List<SimulationProgress> progress = TestSemesters.Run(session, SemesterLength);

        SavedSemester saved = store.Load(session.Id)!;
        Assert.True(progress[^1].Result.Expelled);
        Assert.True(saved.IsExpelled);
        Assert.True(saved.IsFinished);
        Assert.Equal(0, saved.TotalPleasure);
        Assert.True(saved.Days[^1].Expelled);
        Assert.NotNull(saved.Days[^1].ExpelledSubject);
        Assert.Null(store.LoadLatestUnfinished());
        Assert.Throws<InvalidOperationException>(() =>
            TestSemesters.CreateLoader(store, _seed).Restore(saved));
    }

    [Fact]
    public void ContinueMode_ResumesLatestUnfinishedSemester()
    {
        EfSemesterStore store = _postgres.CreateStore();
        SimulationOptions continueMode = new() { StartMode = StartMode.Continue, SemesterDays = 5 };

        SemesterSession finished = TestSemesters.CreateLoader(store, _seed, continueMode).StartNew();
        TestSemesters.Run(finished, 5);
        SemesterSession older = TestSemesters.CreateLoader(store, _seed, continueMode).StartNew();
        TestSemesters.Run(older, 1);
        SemesterSession latest = TestSemesters.CreateLoader(store, _seed, continueMode).StartNew();
        TestSemesters.Run(latest, 3);

        SemesterSession resumed = TestSemesters.CreateLoader(store, _seed, continueMode).Start();

        Assert.True(finished.IsFinished);
        Assert.Equal(latest.Id, resumed.Id);
        Assert.Equal(3, resumed.CompletedDays);
        Assert.Equal(4, resumed.SimulateNextDay().Day);
    }

    [Fact]
    public void ContinueMode_WithoutUnfinishedSemesters_StartsNewOne()
    {
        EfSemesterStore store = _postgres.CreateStore();
        SimulationOptions continueMode = new() { StartMode = StartMode.Continue, SemesterDays = 2 };

        SemesterSession finished = TestSemesters.CreateLoader(store, _seed, continueMode).StartNew();
        TestSemesters.Run(finished, 2);

        SemesterSession started = TestSemesters.CreateLoader(store, _seed, continueMode).Start();

        Assert.NotEqual(finished.Id, started.Id);
        Assert.Equal(0, started.CompletedDays);
    }

    [Fact]
    public void NewMode_AlwaysStartsNewSemester()
    {
        EfSemesterStore store = _postgres.CreateStore();
        SemesterSession unfinished = TestSemesters.CreateLoader(store, _seed).StartNew();
        TestSemesters.Run(unfinished, 3);

        SemesterSession started = TestSemesters
            .CreateLoader(store, _seed, new SimulationOptions { StartMode = StartMode.New })
            .Start();

        Assert.NotEqual(unfinished.Id, started.Id);
        Assert.Equal(0, started.CompletedDays);
        Assert.Equal(unfinished.Id, store.Load(unfinished.Id)!.Id);
    }
}
