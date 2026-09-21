
namespace ChronicTruant.Writer;

public interface IWriter
{
    public void WriteDayResult(int day, DayResult result);

    public void WriteSemesterResult(int pleasure, int days);
}