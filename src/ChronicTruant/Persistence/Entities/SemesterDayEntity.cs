using ChronicTruant.History;

namespace ChronicTruant.Persistence.Entities;

/// <summary>Итог одного дня: удовольствие за день, накопленное удовольствие и вылет.</summary>
public class SemesterDayEntity
{
    public int SemesterId { get; set; }
    public int Day { get; set; }
    public int Pleasure { get; set; }
    public int TotalPleasure { get; set; }
    public bool Expelled { get; set; }
    public Subject? ExpelledSubject { get; set; }

    public List<LessonEntity> Lessons { get; set; } = [];
}
