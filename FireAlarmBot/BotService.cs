using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class BotService : BackgroundService
{
    private readonly ILogger<BotService> _logger;
    private readonly TelegramBotClient _botClient;
    private readonly FloorService _floorService;

    public BotService(ILogger<BotService> logger, TelegramBotClient botClient, FloorService floorService)
    {
        _logger = logger;
        _botClient = botClient;
        _floorService = floorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("Bot started...");
        await Task.Delay(-1, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null || update.Message.Type != MessageType.Text)
            return;

        var messageText = update.Message.Text;

        if (messageText?.StartsWith("/check ") ?? false)
        {
            var s_floor = messageText.Substring("/check ".Length);
            if (s_floor.Contains('-'))
            {
                var range = s_floor.Split('-');
                if (range.Length >= 2 && int.TryParse(range.FirstOrDefault(), out int start) && int.TryParse(range.LastOrDefault(), out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        _floorService.AddFloor(i);
                    }
                    _logger.LogInformation($"id: {update.Message.From?.Id} added {start} to {end}");
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Этажи с {start} по {end} добавлены.", cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный диапазон этажей. Использование: /check 1-10", cancellationToken: cancellationToken);
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
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Этажи {sfloors} добавлены.", cancellationToken: cancellationToken);
            }
            else if (int.TryParse(s_floor, out int floor))
            {
                _floorService.AddFloor(floor);
                _logger.LogInformation($"id: {update.Message.From?.Id} added {floor}");
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Этаж {floor} добавлен.", cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный номер этажа.", cancellationToken: cancellationToken);
            }
        }
        else if (messageText == "/summary")
        {
            var floors = _floorService.GetUniqueFloors();
            var response = floors.Any() ? string.Join(", ", floors) : "Нет записанных этажей.";
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Записанные этажи: {response}", cancellationToken: cancellationToken);
        }
        else if (messageText == "/joke")
        {
            var floors = _floorService.GetUniqueFloors();
            var totalFloorsChecked = _floorService.GetTotalFloorsChecked();
            var uniqueFloorCount = _floorService.GetUniqueFloorCount();
            var mostCheckedFloor = _floorService.GetMostCheckedFloor();
            var lastCheckedFloor = _floorService.GetLastCheckedFloor();
            var checkedPercentage = _floorService.GetCheckedPercentage();

            var response = floors.Any() 
                ? string.Join(", ", floors) 
                : "Нет записанных этажей.";

            var statistics = $@"
Записанные этажи: {response}
Всего проверок: {totalFloorsChecked}
Уникальных этажей: {uniqueFloorCount}
Самый часто проверяемый этаж: {mostCheckedFloor}
Последний проверенный этаж: {lastCheckedFloor}
Процент проверенных этажей: {checkedPercentage:f2}%
                ";
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, statistics, cancellationToken: cancellationToken);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}
