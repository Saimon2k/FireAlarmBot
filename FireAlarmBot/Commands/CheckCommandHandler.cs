using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

public class CheckCommandHandler : ICommandHandler
{
    private readonly IFloorService _floorService;
    private readonly ILogger<CheckCommandHandler> _logger;

    public string Command => "/check";

    public CheckCommandHandler(IFloorService floorService, ILogger<CheckCommandHandler> logger)
    {
        _floorService = floorService;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var s_floor = message.Text?.Substring(Command.Length)?.Trim() ?? string.Empty;
        
        if (string.IsNullOrEmpty(s_floor))
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, 
                "Некорректный диапазон этажей. Пример: /check 12", 
                cancellationToken: cancellationToken);
            return;
        }

        try
        {
            var result = await _floorService.AddCheckedFloor(s_floor, message.From?.Id ?? 0);
            await botClient.SendTextMessageAsync(message.Chat.Id, result, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing check command");
            await botClient.SendTextMessageAsync(message.Chat.Id, 
                "Произошла ошибка при обработке команды", 
                cancellationToken: cancellationToken);
        }
    }
}
