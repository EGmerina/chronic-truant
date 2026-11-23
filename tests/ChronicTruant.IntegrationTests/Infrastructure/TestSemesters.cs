using ChronicTruant.History;
using ChronicTruant.Persistence;
using ChronicTruant.Simulation;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Writer;
using Microsoft.Extensions.Options;

namespace ChronicTruant.IntegrationTests.Infrastructure;

public static class TestSemesters
{
    public static SemesterLoader CreateLoader(
        ISemesterStore store,
        int seed,
        SimulationOptions? options = null,
        ISkipStrategy? strategy = null)
    {
        IOptions<SimulationOptions> wrapped = Options.Create(options ?? new SimulationOptions());

        return new SemesterLoader(
            new TeacherFactory(new Random(seed), wrapped),
            strategy ?? new RuleGuessingStrategy(wrapped),
            store,
            new SilentWriter(),
            wrapped);
    }

    /// <summary>
    /// Первый seed, при котором среди преподавателей есть все три типа правил.
    /// Так тесты гарантированно проверяют и случайное правило, и правила, зависящие от истории.
    /// </summary>
    public static int SeedWithAllRuleTypes()
    {
        IOptions<SimulationOptions> options = Options.Create(new SimulationOptions());

        for (int seed = 1; ; seed++)
        {
            TeacherFactory factory = new(new Random(seed), options);
            HashSet<RuleType> types = Enum.GetValues<Subject>()
                .Select(_ => factory.GenerateRule().Type)
                .ToHashSet();

            if (types.Count == Enum.GetValues<RuleType>().Length)
            {
                return seed;
            }
        }
    }

    /// <summary>Прогоняет до count дней (или до конца семестра) и возвращает прогресс по каждому дню.</summary>
    public static List<SimulationProgress> Run(SemesterSession session, int count)
    {
        List<SimulationProgress> progress = [];
        while (progress.Count < count && !session.IsFinished)
        {
            progress.Add(session.SimulateNextDay());
        }

        return progress;
    }

    private class SilentWriter : IWriter
    {
        public void WriteDayResult(int day, DayResult result) { }
        public void WriteProgress(SimulationProgress progress) { }
        public void WriteStartMessage() { }
        public void WriteStoppedByUser() { }
        public void WriteSemesterStarted(int semesterId) { }
        public void WriteSemesterResumed(int semesterId, int completedDays, int totalPleasure) { }
        public void WriteNothingToResume() { }
        public void WriteSemesterFinished(SimulationProgress progress) { }
        public void WriteDatabaseUnavailable(string error) { }
    }
}

/// <summary>Студент, который прогуливает всё подряд: быстро приводит к вылету.</summary>
public class AlwaysSkipStrategy : ISkipStrategy
{
    public string Name => "Always Skip";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        return new bool[Enum.GetValues<Subject>().Length];
    }
}
