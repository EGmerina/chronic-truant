using ChronicTruant.History;
using ChronicTruant.Persistence;
using ChronicTruant.Simulation;
using ChronicTruant.Teachers;

namespace ChronicTruant.Tests.Fakes;

/// <summary>Хранилище без базы: просто запоминает, что в него сохраняли.</summary>
public class InMemorySemesterStore : ISemesterStore
{
    public List<(int SemesterId, SimulationProgress Progress, IReadOnlyList<LessonRecord> Lessons, bool Finished)> SavedDays { get; } = [];

    public int CreateSemester(IReadOnlyDictionary<Subject, TeacherRuleSettings> rules)
    {
        return 1;
    }

    public void SaveDay(int semesterId, SimulationProgress progress, IReadOnlyList<LessonRecord> lessons, bool finished)
    {
        SavedDays.Add((semesterId, progress, lessons, finished));
    }

    public SavedSemester? Load(int semesterId)
    {
        return null;
    }

    public SavedSemester? LoadLatestUnfinished()
    {
        return null;
    }
}
