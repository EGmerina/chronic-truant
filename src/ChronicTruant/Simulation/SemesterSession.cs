using ChronicTruant.History;
using ChronicTruant.Persistence;
using ChronicTruant.Teachers;

namespace ChronicTruant.Simulation;

/// <summary>Идущий семестр: моделирует очередной день и сразу сохраняет его в хранилище.</summary>
public class SemesterSession
{
    private readonly SimulationRunner _runner;
    private readonly ISemesterStore _store;
    private readonly int _semesterDays;

    public SemesterSession(
        int id,
        IReadOnlyDictionary<Subject, TeacherRuleSettings> rules,
        SimulationRunner runner,
        StudentHistory history,
        ISemesterStore store,
        int semesterDays)
    {
        Id = id;
        Rules = rules;
        History = history;
        _runner = runner;
        _store = store;
        _semesterDays = semesterDays;
    }

    public int Id { get; }
    public IReadOnlyDictionary<Subject, TeacherRuleSettings> Rules { get; }
    public StudentHistory History { get; }
    public int CompletedDays => _runner.CompletedDays;
    public int TotalPleasure => _runner.TotalPleasure;

    /// <summary>Семестр закончен: студент вылетел или прошёл последний день (SemesterDays = 0 — без ограничения).</summary>
    public bool IsFinished { get; private set; }

    public SimulationProgress SimulateNextDay()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("Семестр уже закончен.");
        }

        SimulationProgress progress = _runner.SimulateNextDay();
        IsFinished = progress.Result.Expelled
            || (_semesterDays > 0 && progress.Day >= _semesterDays);

        _store.SaveDay(Id, progress, History.GetDay(progress.Day), IsFinished);

        return progress;
    }
}
