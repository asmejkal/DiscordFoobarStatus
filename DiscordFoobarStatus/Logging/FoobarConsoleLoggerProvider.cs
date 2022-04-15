using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qwr.ComponentInterface;

namespace DiscordFoobarStatus.Logging
{
    public class FoobarConsoleLoggerProvider : ILoggerProvider
    {
        private readonly IConsole _foobarConsole;
        private readonly FoobarConsoleLoggerOptions _options;

        public FoobarConsoleLoggerProvider(IConsole foobarConsole, IOptions<FoobarConsoleLoggerOptions> options)
        {
            _foobarConsole = foobarConsole ?? throw new ArgumentNullException(nameof(foobarConsole));
            _options = options.Value;
        }

        public ILogger CreateLogger(string categoryName) => new FoobarConsoleLogger(_foobarConsole, categoryName, _options.ComponentName);

        public void Dispose()
        {
        }
    }
}
