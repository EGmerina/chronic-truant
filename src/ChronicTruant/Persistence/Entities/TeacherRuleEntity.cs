using ChronicTruant.History;
using ChronicTruant.Teachers;

namespace ChronicTruant.Persistence.Entities;

/// <summary>Параметры правила преподавателя по одному предмету в конкретном семестре.</summary>
public class TeacherRuleEntity
{
    public int SemesterId { get; set; }
    public Subject Subject { get; set; }
    public RuleType RuleType { get; set; }
    public Subject? SubjectA { get; set; }
    public Subject? SubjectB { get; set; }
    public int? Seed { get; set; }
}
