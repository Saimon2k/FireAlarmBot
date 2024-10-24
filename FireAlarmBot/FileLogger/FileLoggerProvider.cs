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
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        //ExDirectory.TryCreate(Path.GetDirectoryName(_path));
        //Directory.CreateDirectory(_path);
        return new FileLogger(_path);
    }

    public void Dispose() { }
}
