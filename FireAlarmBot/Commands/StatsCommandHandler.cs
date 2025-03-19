using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

public class StatsCommandHandler : ICommandHandler
{
    private readonly IFloorService _floorService;
    private readonly ILogger<StatsCommandHandler> _logger;

    public string Command => "/joke";

    public StatsCommandHandler(IFloorService floorService, ILogger<StatsCommandHandler> logger)
    {
        _floorService = floorService;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _floorService.GetStatisticsAsync();
            
            var statistics = $"Записанные этажи: {stats.CheckedFloors}\n" +
                           $"Непроверенные этажи: {stats.UncheckedFloors}\n" +
                           $"Всего проверок: {stats.TotalChecks}\n" +
                           $"Уникальных этажей: {stats.UniqueFloors}\n" +
                           $"Самый часто проверяемый этаж: {stats.MostCheckedFloor}\n" +
                           $"Последний проверенный этаж: {stats.LastCheckedFloor}\n" +
                           $"Процент проверенных этажей: {stats.CheckedPercentage:f2}%";

            await botClient.SendTextMessageAsync(message.Chat.Id, statistics, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stats command");
            await botClient.SendTextMessageAsync(message.Chat.Id, 
                "Произошла ошибка при получении статистики", 
                cancellationToken: cancellationToken);
        }
    }
}
