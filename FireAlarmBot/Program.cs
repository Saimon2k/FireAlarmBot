using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configure options
        services.Configure<BotOptions>(
            context.Configuration.GetSection(BotOptions.SectionName));

        // Register bot client
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BotOptions>>();
            return new TelegramBotClient(options.Value.BotToken);
        });

        // Register services
        services.AddSingleton<IFloorService, FloorService>();
        services.AddSingleton<CommandService>();

        // Register command handlers
        services.AddSingleton<ICommandHandler, CheckCommandHandler>();
        services.AddSingleton<ICommandHandler, SummaryCommandHandler>();
        services.AddSingleton<ICommandHandler, StatsCommandHandler>();

        // Register background service
        //services.AddHostedService<BotService>();
    })
    .Build();

await host.RunAsync();
