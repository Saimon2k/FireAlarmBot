using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class FloorService : IFloorService
{
    private readonly ILogger<FloorService> _logger;
    private readonly List<int> _floorLogs = new();
    private readonly System.Timers.Timer _timer;
    private readonly BotOptions _options;
    private readonly object _lock = new();

    public FloorService(ILogger<FloorService> logger, IOptions<BotOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _timer = new System.Timers.Timer(_options.LogsClearInterval.TotalMilliseconds);
        _timer.Elapsed += (s, e) => ClearLogs();
        _timer.Start();
    }

    public void AddFloor(int floor)
    {
        if (floor < _options.MinFloor || floor > _options.MaxFloor) return;
        
        lock (_lock)
        {
            _floorLogs.Add(floor);
        }
        
        _logger.LogInformation("Floor {Floor} added", floor);
        _timer.Stop();
        _timer.Start();
    }

    public async Task<string> AddCheckedFloor(string input, long userId)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty", nameof(input));

        if (input.Contains('-'))
        {
            var range = input.Split('-');
            if (range.Length >= 2 && int.TryParse(range.FirstOrDefault(), out int start) && int.TryParse(range.LastOrDefault(), out int end))
            {
                for (int i = start; i <= end; i++)
                    AddFloor(i);

                _logger.LogInformation("User {UserId} added floors from {Start} to {End}", userId, start, end);
                return $"Этажи с {start} по {end} добавлены.";
            }
            throw new ArgumentException("Некорректный диапазон этажей");
        }
        
        if (input.Contains(','))
        {
            var floors = input.Split(',')
                .Where(x => int.TryParse(x.Trim(), out int i))
                .Select(x => int.Parse(x.Trim()))
                .ToList();

            if (!floors.Any())
                throw new ArgumentException("Не удалось распознать номера этажей");

            floors.ForEach(AddFloor);
            var sfloors = string.Join(", ", floors);
            _logger.LogInformation("User {UserId} added floors {Floors}", userId, sfloors);
            return $"Этажи {sfloors} добавлены";
        }
        
        if (int.TryParse(input, out int floor))
        {
            AddFloor(floor);
            _logger.LogInformation("User {UserId} added floor {Floor}", userId, floor);
            return $"Этаж {floor} добавлен";
        }

        throw new ArgumentException("Некорректный номер этажа");
    }

    public Task<IEnumerable<int>> GetUniqueFloorsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_floorLogs.Distinct().OrderBy(x => x).Select(x => x));
        }
    }

    public Task<IEnumerable<int>> GetUncheckedFloorsAsync()
    {
        lock (_lock)
        {
            var allFloors = Enumerable.Range(_options.MinFloor, _options.MaxFloor - _options.MinFloor + 1);
            var checkedFloors = _floorLogs.Distinct();
            return Task.FromResult(allFloors.Except(checkedFloors));
        }
    }

    public async Task<FloorStatistics> GetStatisticsAsync()
    {
        IEnumerable<int> uniqueFloors;
        IEnumerable<int> uncheckedFloors;
        int totalChecks;
        int mostCheckedFloor;
        int lastCheckedFloor;

        //lock (_lock)
        {
            uniqueFloors = await GetUniqueFloorsAsync();//_floorLogs.Distinct().OrderBy(x => x).ToList();
            uncheckedFloors = await GetUniqueFloorsAsync();
            totalChecks = _floorLogs.Count;
            mostCheckedFloor = _floorLogs.GroupBy(f => f)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            lastCheckedFloor = _floorLogs.LastOrDefault();
        }

        return new FloorStatistics(
            CheckedFloors: uniqueFloors.Any() ? string.Join(", ", uniqueFloors) : "Нет таких этажей",
            UncheckedFloors: uncheckedFloors.Any() ? string.Join(", ", uncheckedFloors) : "Нет таких этажей",
            TotalChecks: totalChecks,
            UniqueFloors: uniqueFloors.Count(),
            MostCheckedFloor: mostCheckedFloor,
            LastCheckedFloor: lastCheckedFloor,
            CheckedPercentage: (double)uniqueFloors.Count() / (_options.MaxFloor - _options.MinFloor + 1) * 100
        );
    }

    private void ClearLogs()
    {
        lock (_lock)
        {
            _logger.LogInformation("Clearing floor logs");
            _floorLogs.Clear();
        }
    }
}
