using Microsoft.Extensions.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private string _path;

    public FileLoggerProvider(string path)
    {
        _path = path;
    }

    public ILogger CreateLogger(string categoryName)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return new FileLogger(_path);
    }

    public void Dispose() { }
}
