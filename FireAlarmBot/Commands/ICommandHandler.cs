using Telegram.Bot;
using Telegram.Bot.Types;

public interface ICommandHandler
{
    string Command { get; }
    Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}
