using System.Collections.Concurrent;

public class FloorService
{
    private readonly List<int> _floorLogs = new();
    private readonly Timer _timer;

    public FloorService()
    {
        _timer = new Timer(ClearLogs, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    }

    public void AddFloor(int floor)
    {
        _floorLogs.Add(floor);
    }

    public IEnumerable<int> GetUniqueFloors()
    {
        return _floorLogs.Distinct().OrderBy(x => x);
    }

    private void ClearLogs(object state)
    {
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
        int uniqueFloors = GetUniqueFloorCount();
        return uniqueFloors / 100 / 25;  // Процент проверенных этажей
    }
}
