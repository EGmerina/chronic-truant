using ChronicTruant.History;
using ChronicTruant.Persistence.Entities;
using ChronicTruant.Simulation;
using ChronicTruant.Teachers;
using Microsoft.EntityFrameworkCore;

namespace ChronicTruant.Persistence;

public class EfSemesterStore : ISemesterStore
{
    private readonly IDbContextFactory<ChronicTruantDbContext> _contextFactory;

    public EfSemesterStore(IDbContextFactory<ChronicTruantDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public int CreateSemester(IReadOnlyDictionary<Subject, TeacherRuleSettings> rules)
    {
        using ChronicTruantDbContext db = _contextFactory.CreateDbContext();

        SemesterEntity semester = new()
        {
            StartedAt = DateTimeOffset.UtcNow,
            TeacherRules = rules
                .Select(pair => new TeacherRuleEntity
                {
                    Subject = pair.Key,
                    RuleType = pair.Value.Type,
                    SubjectA = pair.Value.SubjectA,
                    SubjectB = pair.Value.SubjectB,
                    Seed = pair.Value.Seed
                })
                .ToList()
        };

        db.Semesters.Add(semester);
        db.SaveChanges();

        return semester.Id;
    }

    public void SaveDay(int semesterId, SimulationProgress progress, IReadOnlyList<LessonRecord> lessons, bool finished)
    {
        using ChronicTruantDbContext db = _contextFactory.CreateDbContext();

        SemesterEntity semester = db.Semesters.Single(s => s.Id == semesterId);
        semester.CompletedDays = progress.Day;
        semester.TotalPleasure = progress.TotalPleasure;
        semester.IsExpelled = progress.Result.Expelled;
        semester.IsFinished = finished;

        db.SemesterDays.Add(new SemesterDayEntity
        {
            SemesterId = semesterId,
            Day = progress.Day,
            Pleasure = progress.Result.Pleasure,
            TotalPleasure = progress.TotalPleasure,
            Expelled = progress.Result.Expelled,
            ExpelledSubject = progress.Result.ExpelledSubject,
            Lessons = lessons
                .Select(lesson => new LessonEntity
                {
                    Subject = lesson.Subject,
                    Attended = lesson.Attended,
                    WasAsked = lesson.WasAsked
                })
                .ToList()
        });

        // Прогресс семестра и запись дня сохраняются одной транзакцией.
        db.SaveChanges();
    }

    public SavedSemester? Load(int semesterId)
    {
        using ChronicTruantDbContext db = _contextFactory.CreateDbContext();

        SemesterEntity? semester = WithDetails(db).FirstOrDefault(s => s.Id == semesterId);

        return semester is null ? null : ToSaved(semester);
    }

    public SavedSemester? LoadLatestUnfinished()
    {
        using ChronicTruantDbContext db = _contextFactory.CreateDbContext();

        SemesterEntity? semester = WithDetails(db)
            .Where(s => !s.IsFinished)
            .OrderByDescending(s => s.Id)
            .FirstOrDefault();

        return semester is null ? null : ToSaved(semester);
    }

    private static IQueryable<SemesterEntity> WithDetails(ChronicTruantDbContext db)
    {
        return db.Semesters
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.TeacherRules)
            .Include(s => s.Days)
            .ThenInclude(d => d.Lessons);
    }

    private static SavedSemester ToSaved(SemesterEntity semester)
    {
        Dictionary<Subject, TeacherRuleSettings> rules = semester.TeacherRules.ToDictionary(
            rule => rule.Subject,
            rule => new TeacherRuleSettings(rule.RuleType, rule.SubjectA, rule.SubjectB, rule.Seed));

        List<SavedDay> days = semester.Days
            .OrderBy(d => d.Day)
            .Select(d => new SavedDay(
                d.Day,
                d.Pleasure,
                d.TotalPleasure,
                d.Expelled,
                d.ExpelledSubject,
                d.Lessons
                    .OrderBy(l => l.Subject)
                    .Select(l => new LessonRecord(l.Subject, l.Attended, l.WasAsked))
                    .ToList()))
            .ToList();

        return new SavedSemester(
            semester.Id,
            rules,
            semester.CompletedDays,
            semester.TotalPleasure,
            semester.IsExpelled,
            semester.IsFinished,
            days);
    }
}
