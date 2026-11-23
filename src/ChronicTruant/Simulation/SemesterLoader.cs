using ChronicTruant.History;
using ChronicTruant.Persistence;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;
using ChronicTruant.Writer;
using Microsoft.Extensions.Options;

namespace ChronicTruant.Simulation;

/// <summary>Начинает новый семестр или восстанавливает сохранённый из хранилища.</summary>
public class SemesterLoader
{
    private readonly TeacherFactory _teacherFactory;
    private readonly ISkipStrategy _strategy;
    private readonly ISemesterStore _store;
    private readonly IWriter _writer;
    private readonly IOptions<SimulationOptions> _options;

    public SemesterLoader(
        TeacherFactory teacherFactory,
        ISkipStrategy strategy,
        ISemesterStore store,
        IWriter writer,
        IOptions<SimulationOptions> options)
    {
        _teacherFactory = teacherFactory;
        _strategy = strategy;
        _store = store;
        _writer = writer;
        _options = options;
    }

    /// <summary>Запуск в режиме из настроек: продолжить незавершённый семестр, если он есть, иначе начать новый.</summary>
    public SemesterSession Start()
    {
        if (_options.Value.StartMode == StartMode.Continue)
        {
            SavedSemester? saved = _store.LoadLatestUnfinished();
            if (saved is not null)
            {
                _writer.WriteSemesterResumed(saved.Id, saved.CompletedDays, saved.TotalPleasure);
                return Restore(saved);
            }

            _writer.WriteNothingToResume();
        }

        SemesterSession session = StartNew();
        _writer.WriteSemesterStarted(session.Id);

        return session;
    }

    public SemesterSession StartNew()
    {
        Dictionary<Subject, TeacherRuleSettings> rules = Enum.GetValues<Subject>()
            .ToDictionary(subject => subject, _ => _teacherFactory.GenerateRule());

        int id = _store.CreateSemester(rules);

        return Build(id, rules, new StudentHistory(), completedDays: 0, totalPleasure: 0);
    }

    public SemesterSession Restore(SavedSemester saved)
    {
        if (saved.IsFinished)
        {
            throw new InvalidOperationException($"Семестр {saved.Id} уже закончен, продолжить его нельзя.");
        }

        StudentHistory history = new();
        foreach (SavedDay day in saved.Days)
        {
            foreach (LessonRecord lesson in day.Lessons)
            {
                history.AddDay(day.Day, lesson.Subject, lesson.Attended, lesson.WasAsked);
            }
        }

        return Build(saved.Id, saved.Rules, history, saved.CompletedDays, saved.TotalPleasure);
    }

    private SemesterSession Build(
        int id,
        IReadOnlyDictionary<Subject, TeacherRuleSettings> rules,
        StudentHistory history,
        int completedDays,
        int totalPleasure)
    {
        // Порядок преподавателей совпадает с порядком предметов в enum — по нему стратегия раскладывает решения.
        List<Teacher> teachers = Enum.GetValues<Subject>()
            .Select(subject => _teacherFactory.Create(subject, rules[subject], completedDays))
            .ToList();

        DaySimulator simulator = new(teachers, _strategy, history, _options);
        SimulationRunner runner = new(simulator, completedDays, totalPleasure);

        return new SemesterSession(id, rules, runner, history, _store, _options.Value.SemesterDays);
    }
}
