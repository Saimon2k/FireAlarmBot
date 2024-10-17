using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(botToken))
        {
            throw new InvalidOperationException("Необходимо указать токен бота в переменной окружения TELEGRAM_BOT_TOKEN.");
        }

        services.AddSingleton<TelegramBotClient>(provider => new TelegramBotClient(botToken));

        services.AddSingleton<FloorService>();
        services.AddHostedService<BotService>();
    })
    .Build();

await host.RunAsync();
