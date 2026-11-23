using ChronicTruant.History;

namespace ChronicTruant.Persistence.Entities;

/// <summary>Одна пара дня: была ли посещена и спросили ли (null — прогулял и не спросили).</summary>
public class LessonEntity
{
    public int SemesterId { get; set; }
    public int Day { get; set; }
    public Subject Subject { get; set; }
    public bool Attended { get; set; }
    public bool? WasAsked { get; set; }
}
