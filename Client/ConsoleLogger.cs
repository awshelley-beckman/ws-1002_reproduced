using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Client
{
  public class ConsoleLoggerProvider : ILoggerProvider
  {
    private readonly LogLevel _logLevel;
    private readonly IndentableWriter _indentor = new IndentableWriter("  ");

    public ConsoleLoggerProvider(string filePath, LogLevel logLevel)
    {
      _logLevel = logLevel;
    }

    public ILogger CreateLogger(string categoryName)
      => new ConsoleLogger(categoryName, _indentor, _logLevel);

    public void Dispose() { }
  }

  public class IndentableWriter
  {
    public int IndentLevel { get; private set; }
    private readonly string _indentSequence;

    public IndentableWriter(string indentSequence)
    {
      _indentSequence = indentSequence;
    }

    public void IncreaseIndent() => IndentLevel++;

    public void DecreaseIndent()
    {
      IndentLevel--;

      if (IndentLevel < 0)
        IndentLevel = 0;
    }

    public string ApplyIndentation(string text)
    {
      // Prepend the indent to the line
      var builder = new StringBuilder();
      for (int i = 0; i < IndentLevel; i++)
        builder.Append(_indentSequence);

      builder.Append(text);

      return builder.ToString();
    }
  }

  public class ConsoleLogger : ILogger
  {
    private class DelegateDisposable : IDisposable
    {
      public Action DisposeAction;
      public void Dispose() => DisposeAction();

      public DelegateDisposable(Action action)
        => DisposeAction = action;
    }

    private readonly string _categoryName;
    private readonly IndentableWriter _indentor;
    private readonly LogLevel _logLevel;

    public ConsoleLogger(
      string categoryName,
      IndentableWriter indentor,
      LogLevel logLevel
    )
    {
      _categoryName = categoryName;
      _indentor = indentor;
      _logLevel = logLevel;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      _indentor.IncreaseIndent();
      return new DelegateDisposable(EndScope);
    }

    private void EndScope() => _indentor.DecreaseIndent();

    public bool IsEnabled(LogLevel logLevel)
    {
      // TEMPORARY: The preview version of AtlasClient.NET is logging some
      // things at the None level, in an attempt to disable them.  That'll be
      // fixed in the next release of AtlasClient.NET.  In the meantime, we'll
      // interpret None to mean "don't log this message".
      if (logLevel == LogLevel.None)
        return false;

      return logLevel >= _logLevel;
    }

    public void Log<TState>(
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception exception,
      Func<TState, Exception, string> formatter
    )
    {
      // Only log things that are enabled
      if (!IsEnabled(logLevel))
        return;

      string logMessage = formatter(state, exception);
      int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
      string decoratedMessage = $"[{logLevel}][PID: {processId}][{_categoryName}, {DateTimeOffset.UtcNow}]: {logMessage}";

      if (exception != null)
      {
        decoratedMessage += Environment.NewLine + "  " + exception.ToString();
      }

      string[] lines = decoratedMessage.Split(System.Environment.NewLine);

      // Indent all the lines.  All lines after the first one get an extra indent,
      // to show that they still belong to the same message.
      lines[0] = _indentor.ApplyIndentation(lines[0]);

      using (BeginScope(state))
      {
        for (int i = 1; i < lines.Length; i++)
          lines[i] = _indentor.ApplyIndentation(lines[i]);
      }

      foreach (var line in lines)
        Console.WriteLine(line);
    }
  }
}
