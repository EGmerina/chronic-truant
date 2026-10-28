namespace ChronicTruant.Tests.Fakes;

/// <summary>Управляемый источник случайности: возвращает заранее заданные значения по очереди.</summary>
public class QueueRandom : Random
{
    private readonly Queue<double> _doubles;
    private readonly Queue<int> _ints;

    public QueueRandom(IEnumerable<double>? doubles = null, IEnumerable<int>? ints = null)
    {
        _doubles = new Queue<double>(doubles ?? []);
        _ints = new Queue<int>(ints ?? []);
    }

    public override double NextDouble()
    {
        return _doubles.Dequeue();
    }

    public override int Next(int maxValue)
    {
        int value = _ints.Dequeue();
        if (value < 0 || value >= maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), $"Значение {value} вне [0, {maxValue}).");
        }

        return value;
    }
}
