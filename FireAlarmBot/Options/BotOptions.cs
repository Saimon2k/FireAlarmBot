public class BotOptions
{
    public const string SectionName = "BotConfiguration";
    
    public string BotToken { get; set; } = string.Empty;
    public int MinFloor { get; set; } = 2;
    public int MaxFloor { get; set; } = 25;
    public TimeSpan LogsClearInterval { get; set; } = TimeSpan.FromHours(1);
}
