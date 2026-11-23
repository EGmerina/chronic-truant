using ChronicTruant.History;
using ChronicTruant.Teachers;

namespace ChronicTruant.Persistence;

/// <summary>Состояние семестра, прочитанное из базы.</summary>
public record SavedSemester(
    int Id,
    IReadOnlyDictionary<Subject, TeacherRuleSettings> Rules,
    int CompletedDays,
    int TotalPleasure,
    bool IsExpelled,
    bool IsFinished,
    IReadOnlyList<SavedDay> Days);

public record SavedDay(
    int Day,
    int Pleasure,
    int TotalPleasure,
    bool Expelled,
    Subject? ExpelledSubject,
    IReadOnlyList<LessonRecord> Lessons);
