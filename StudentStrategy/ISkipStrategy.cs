public interface ISkipStrategy
{
    string Name { get; }
   
    /// Решение на день: для каждого предмета — идти на пару (true) или прогулять (false).
    /// Вызывается один раз в начале каждого дня, до того как станут известны сегодняшние исходы.
    
    bool[] DecideDay(int day, IReadOnlyStudentHistory history);
}
