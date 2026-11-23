using ChronicTruant.History;
using ChronicTruant.Simulation;
using ChronicTruant.Teachers;

namespace ChronicTruant.Persistence;

/// <summary>Хранилище семестров: правила преподавателей и прогресс по каждому дню.</summary>
public interface ISemesterStore
{
    /// <summary>Создаёт новый семестр с правилами преподавателей и возвращает его идентификатор.</summary>
    int CreateSemester(IReadOnlyDictionary<Subject, TeacherRuleSettings> rules);

    /// <summary>Сохраняет итог дня, все его пары и обновляет прогресс семестра.</summary>
    void SaveDay(int semesterId, SimulationProgress progress, IReadOnlyList<LessonRecord> lessons, bool finished);

    SavedSemester? Load(int semesterId);

    /// <summary>Последний семестр, который не закончился вылетом или последним днём.</summary>
    SavedSemester? LoadLatestUnfinished();
}
