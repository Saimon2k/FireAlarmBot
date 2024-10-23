using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
    .ConfigureLogging((ctx, conf) => conf
        .ClearProviders()
        .AddConsole()
        .AddConfiguration(ctx.Configuration.GetSection("Logging"))
        .AddFile(ctx.Configuration.GetValue<string>("Logging:FileName")))
    .ConfigureServices((context, services) =>
    {
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(botToken))
            throw new InvalidOperationException("Необходимо указать токен бота в переменной окружения TELEGRAM_BOT_TOKEN.");

        services.AddSingleton<TelegramBotClient>(provider => new TelegramBotClient(botToken));
        services.AddSingleton<FloorService>();
        services.AddHostedService<BotService>();
    })

    .Build();

await host.RunAsync();
