using Microsoft.Extensions.Logging;
using System.Threading;
using Telegram.Bot.Types;

public class FloorService
{
    private readonly ILogger<FloorService> _logger;
    private readonly List<int> _floorLogs = new();
    private readonly System.Timers.Timer _timer;

    public FloorService(ILogger<FloorService> logger)
    {
        _logger = logger;
        _timer = new System.Timers.Timer(TimeSpan.FromHours(1).TotalMilliseconds);
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
    /*
    public IEnumerable<int> AddFloor(string s_floor)
    {
        if (s_floor.Contains('-'))
        {
            var range = s_floor.Split('-');
            if (range.Length >= 2 && int.TryParse(range.FirstOrDefault(), out int start) && int.TryParse(range.LastOrDefault(), out int end))
            {
                for (int i = start; i <= end; i++)
                    AddFloor(i);

                //_logger.LogInformation($"id: {update.Message.From?.Id} added {start} to {end}");
                //await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Этажи с {start} по {end} добавлены.", cancellationToken: cancellationToken);
            }
            else
            {
                //await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный диапазон этажей. Использование: /check 1-10", cancellationToken: cancellationToken);
            }
        }
        else if (s_floor.Contains(','))
        {
            //1, 2
            var floors = s_floor.Split(',').Where(x => int.TryParse(x.Trim(), out int i))
                .Select(x => int.Parse(x.Trim())).ToList();
            if (!floors.Any()) return;

            floors.ForEach(x => _floorService.AddFloor(x));
            var sfloors = string.Join(", ", floors);
            _logger.LogInformation($"id: {update.Message.From?.Id} added {sfloors}");
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Этажи {sfloors} добавлены", cancellationToken: cancellationToken);
        }
        else if (int.TryParse(s_floor, out int floor))
        {
            _floorService.AddFloor(floor);
            _logger.LogInformation($"id: {update.Message.From?.Id} added {floor}");
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Этаж {floor} добавлен", cancellationToken: cancellationToken);
        }
    }
    */
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
