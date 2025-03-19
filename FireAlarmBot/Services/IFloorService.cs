public interface IFloorService
{
    Task<string> AddCheckedFloor(string input, long userId);
    Task<IEnumerable<int>> GetUniqueFloorsAsync();
    Task<IEnumerable<int>> GetUncheckedFloorsAsync();
    Task<FloorStatistics> GetStatisticsAsync();
}

public record FloorStatistics(
    string CheckedFloors,
    string UncheckedFloors,
    int TotalChecks,
    int UniqueFloors,
    int MostCheckedFloor,
    int LastCheckedFloor,
    double CheckedPercentage
);
