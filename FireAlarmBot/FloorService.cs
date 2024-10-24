using Microsoft.Extensions.Logging;

public class FloorService
{
    private readonly ILogger<FloorService> _logger;
    private readonly List<int> _floorLogs = new();
    private readonly System.Timers.Timer _timer;

    public FloorService(ILogger<FloorService> logger)
    {
        _logger = logger;
        _timer = new System.Timers.Timer(TimeSpan.FromHours(1));
        _timer.Elapsed += (s, e) => ClearLogs();
    }

    public void AddFloor(int floor)
    {
        if (floor <= 1 || floor > 25) return;
        _floorLogs.Add(floor);
        _logger.LogInformation("FloorService.AddFloor({0})", floor);
        _timer.Stop();
        _timer.Start();
    }

    public IEnumerable<int> GetUniqueFloors()
    {
        return _floorLogs.Distinct().OrderBy(x => x);
    }

    private void ClearLogs()
    {
        _logger.LogInformation("FloorService.ClearLogs()");
        _floorLogs.Clear();
    }

    public int GetTotalFloorsChecked()
    {
        return _floorLogs.Count;  // Общее количество проверок этажей
    }

    public int GetUniqueFloorCount()
    {
        return _floorLogs.Distinct().Count();  // Количество уникальных этажей
    }

    // Новый метод для получения списка непроверенных этажей
    public IEnumerable<int> GetUncheckedFloors()
    {
        var allFloors = Enumerable.Range(2, 24);  // Этажи с 2 по 25
        var checkedFloors = GetUniqueFloors();
        return allFloors.Except(checkedFloors);  // Возвращаем те, которые ещё не были проверены
    }

    public int GetMostCheckedFloor()
    {
        return _floorLogs.GroupBy(f => f)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();  // Самый часто проверяемый этаж
    }

    public int GetLastCheckedFloor()
    {
        return _floorLogs.LastOrDefault();  // Последний проверенный этаж
    }

    public double GetCheckedPercentage()
    {
        float uniqueFloors = GetUniqueFloorCount();
        return uniqueFloors / 24f * 100f;  // Процент проверенных этажей
    }
}
