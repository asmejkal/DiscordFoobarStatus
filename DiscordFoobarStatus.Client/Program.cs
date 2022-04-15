using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DiscordFoobarStatus.Client
{
    internal class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using var host = CreateHostBuilder().Build();
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
            try
            {
                logger.LogInformation("Starting process {ProcessId}", Process.GetCurrentProcess().Id);

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(host.Services.GetRequiredService<Music>());

                logger.LogInformation("Shutting down process {ProcessId}", Process.GetCurrentProcess().Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error");
                throw;
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddClientServices())
                .UseSerilog((_, config) =>
                {
                    config.WriteTo.RollingFile(
                        "logs/log-{Date}.txt",
                        LogEventLevel.Information,
                        fileSizeLimitBytes: 1024 * 1024 * 10,
                        retainedFileCountLimit: 3);
                });
        }
    }
}
