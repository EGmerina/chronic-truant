namespace ChronicTruant.History;

/// <summary>Одна пара одного дня: был ли студент и спросили ли его (null — прогулял и не спросили).</summary>
public record LessonRecord(Subject Subject, bool Attended, bool? WasAsked);
