using System;
using Microsoft.Extensions.Logging;
using Qwr.ComponentInterface;

namespace DiscordFoobarStatus.Logging
{
    public class FoobarConsoleLogger : ILogger
    {
        private readonly IConsole _foobarConsole;
        private readonly string _categoryName;
        private readonly string? _componentName;

        public FoobarConsoleLogger(IConsole foobarConsole, string categoryName, string? componentName)
        {
            _foobarConsole = foobarConsole ?? throw new ArgumentNullException(nameof(foobarConsole));
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _componentName = componentName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _foobarConsole.Log($"{_componentName} ({_categoryName}) {logLevel}: {formatter(state, exception)} {exception}");
        }
    }
}
