
using ChronicTruant.Simulation;

namespace ChronicTruant.Writer;

public interface IWriter
{
    public void WriteDayResult(int day, DayResult result);

    public void WriteProgress(SimulationProgress progress);

    public void WriteStartMessage();

    public void WriteStoppedByUser();

    public void WriteSemesterStarted(int semesterId);

    public void WriteSemesterResumed(int semesterId, int completedDays, int totalPleasure);

    public void WriteNothingToResume();

    public void WriteSemesterFinished(SimulationProgress progress);

    public void WriteDatabaseUnavailable(string error);
}
