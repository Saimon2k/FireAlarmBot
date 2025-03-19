using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class CommandService
{
    private readonly ILogger<CommandService> _logger;
    private readonly Dictionary<string, ICommandHandler> _handlers;

    public CommandService(
        ILogger<CommandService> logger,
        IEnumerable<ICommandHandler> handlers)
    {
        _logger = logger;
        _handlers = handlers.ToDictionary(h => h.Command, h => h);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is null || update.Message.Type != MessageType.Text)
                return;

            var messageText = update.Message.Text;
            if (string.IsNullOrEmpty(messageText))
                return;

            var command = messageText.Split(' ')[0].ToLower();
            if (_handlers.TryGetValue(command, out var handler))
            {
                await handler.HandleAsync(botClient, update.Message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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
