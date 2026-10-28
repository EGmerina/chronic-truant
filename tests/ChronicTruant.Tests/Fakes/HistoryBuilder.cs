using ChronicTruant.History;

namespace ChronicTruant.Tests.Fakes;

/// <summary>Собирает историю, в которой студент ходил на все пары.</summary>
public static class HistoryBuilder
{
    public static readonly Subject[] Subjects = Enum.GetValues<Subject>();

    /// <param name="wasAsked">Спросили ли в день (с 1) по предмету.</param>
    public static StudentHistory Attended(int days, Func<int, Subject, bool> wasAsked)
    {
        StudentHistory history = new();

        for (int day = 1; day <= days; day++)
        {
            foreach (Subject subject in Subjects)
            {
                history.AddDay(day, subject, attended: true, wasAsked(day, subject));
            }
        }

        return history;
    }
}
