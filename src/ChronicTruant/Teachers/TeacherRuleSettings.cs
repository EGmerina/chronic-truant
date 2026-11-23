using ChronicTruant.History;

namespace ChronicTruant.Teachers;

/// <summary>
/// Параметры сгенерированного правила преподавателя — всё, что нужно, чтобы воссоздать его заново.
/// SubjectA/SubjectB заполнены для правил, зависящих от предметов, Seed — для случайного правила.
/// </summary>
public record TeacherRuleSettings(
    RuleType Type,
    Subject? SubjectA = null,
    Subject? SubjectB = null,
    int? Seed = null);
