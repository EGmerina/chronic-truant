
using ChronicTruant.Simulation;

namespace ChronicTruant.Writer;

public interface IWriter
{
    public void WriteDayResult(int day, DayResult result);

    public void WriteProgress(SimulationProgress progress);

    public void WriteStartMessage();

    public void WriteStoppedByUser();
}
