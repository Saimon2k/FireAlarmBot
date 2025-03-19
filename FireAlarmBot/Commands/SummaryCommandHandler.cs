using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

public class SummaryCommandHandler : ICommandHandler
{
    private readonly IFloorService _floorService;
    private readonly ILogger<SummaryCommandHandler> _logger;

    public string Command => "/summary";

    public SummaryCommandHandler(IFloorService floorService, ILogger<SummaryCommandHandler> logger)
    {
        _floorService = floorService;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var floors = await _floorService.GetUniqueFloorsAsync();
            var uncheck_floors = await _floorService.GetUncheckedFloorsAsync();
            
            var response = "Записанные этажи: " + (floors.Any() ? string.Join(", ", floors) : "Нет таких этажей") + "\n" +
                          "Непроверенные этажи: " + (uncheck_floors.Any() ? string.Join(", ", uncheck_floors) : "Нет таких этажей");

            await botClient.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing summary command");
            await botClient.SendTextMessageAsync(message.Chat.Id, 
                "Произошла ошибка при получении статистики", 
                cancellationToken: cancellationToken);
        }
    }
}
