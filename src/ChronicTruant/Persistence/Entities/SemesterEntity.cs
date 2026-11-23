namespace ChronicTruant.Persistence.Entities;

public class SemesterEntity
{
    public int Id { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public int CompletedDays { get; set; }
    public int TotalPleasure { get; set; }
    public bool IsExpelled { get; set; }
    public bool IsFinished { get; set; }

    public List<TeacherRuleEntity> TeacherRules { get; set; } = [];
    public List<SemesterDayEntity> Days { get; set; } = [];
}
