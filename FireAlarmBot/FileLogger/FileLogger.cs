using Microsoft.Extensions.Logging;
using System;
using System.IO;

public class FileLogger : ILogger, IDisposable
{
    string _path;
    static object _lock = new();
    static bool _enabled = true;

    public FileLogger(string path)
    {
        _path = path;
        _enabled = !string.IsNullOrEmpty(_path);
    }

    public IDisposable BeginScope<TState>(TState state) => this;

    public void Dispose() { }

    public bool IsEnabled(LogLevel logLevel) => _enabled;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        if (_enabled)
            lock (_lock)
                try
                {
                    File.AppendAllText(_path, formatter(state, exception) + Environment.NewLine);
                }
                catch (Exception e)
                {
                    if (e is DirectoryNotFoundException)
                        _enabled = false;
                }
    }
}
