using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DiscordFoobarStatus.Core.Models;
using DiscordFoobarStatus.Core.Utility;
using Microsoft.Extensions.Logging;

namespace DiscordFoobarStatus
{
    public class DiscordClient : IDiscordClient
    {
        private const string ClientProcessFriendlyName = "DiscordFoobarStatus.Client";
        private const string ClientProcessName = ClientProcessFriendlyName + ".exe";
        private const string ClientDirectoryName = "Client";
        private const string ClientWindowTitle = "Music";

        private readonly ILogger<DiscordClient> _logger;
        private readonly MessageIds _messageIds = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly string _clientDirectoryPath;
        private readonly int _currentSessionId = Process.GetCurrentProcess().SessionId;

        private Process? _statusProcess;

        public DiscordClient(ILogger<DiscordClient> logger)
        {
            _logger = logger;
            var pluginDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) 
                ?? throw new InvalidOperationException("Can't find plugin directory");

            _clientDirectoryPath = Path.Combine(pluginDirectoryPath, ClientDirectoryName);
        }

        public async Task ClearActivityAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // Clear stragglers (e.g. when foobar crashed)
                var stragglers = Process.GetProcessesByName(ClientProcessFriendlyName);
                foreach (var straggler in stragglers.Where(x => x.Id != _statusProcess?.Id && x.SessionId == _currentSessionId))
                {
                    _logger.LogWarning("Killing straggler process {ProcessId}", straggler.Id);
                    straggler.Kill();
                }

                if (_statusProcess != null)
                {
                    _logger.LogInformation("Shutting down process {ProcessId}", _statusProcess.Id);
                    if (WindowFinder.TryFindProcessWindow(_statusProcess.Id, ClientWindowTitle, out var handle))
                    {
                        if (Interop.SendMessageTimeout(handle, _messageIds.Shutdown, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _) == 0)
                            _logger.LogError("Failed to shut down process {ProcessId} with error code {LastError}", _statusProcess.Id, Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        _logger.LogWarning("Couldn't find window of process {ProcessId}, killing process", _statusProcess.Id);
                        _statusProcess.Kill();
                    }

                    _statusProcess.Dispose();
                    _statusProcess = null;
                }
                else
                {
                    _logger.LogInformation("Activity already cleared, no process running");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateActivityAsync(ActivitySetDto activity)
        {
            await _semaphore.WaitAsync();
            try
            {
                IntPtr handle;
                if (_statusProcess == null || !WindowFinder.TryFindProcessWindow(_statusProcess.Id, ClientWindowTitle, out handle))
                {
                    if (_statusProcess != null)
                    {
                        _logger.LogWarning("Couldn't find window of process {ProcessId}, killing process", _statusProcess.Id);
                        _statusProcess.Kill();
                        _statusProcess.Dispose();
                    }

                    var pci = new ProcessStartInfo(Path.Combine(_clientDirectoryPath, ClientProcessName))
                    {
                        UseShellExecute = true,
                        WorkingDirectory = _clientDirectoryPath
                    };

                    _statusProcess = Process.Start(pci)!;
                    _logger.LogInformation("Started process {ProcessId} for activity update", _statusProcess.Id);

                    int attempts = 0;
                    while (!WindowFinder.TryFindProcessWindow(_statusProcess.Id, ClientWindowTitle, out handle) && attempts++ < 20)
                        await Task.Delay(200); // The process may need some time to spin up   

                    if (handle == default)
                    {
                        _logger.LogError("Couldn't find window of new process {ProcessId}, killing process", _statusProcess.Id);
                        _statusProcess.Kill();
                        return;
                    }
                }

                _logger.LogInformation("Sending activity update to process {ProcessId}", _statusProcess.Id);
                var result = Interop.CopyData.Send(handle, (int)_messageIds.UpdateActivity, JsonSerializer.Serialize(activity), Unicode: true);
                if (result != 0)
                    _logger.LogError("Failed to update activity for process {ProcessId} with error code {LastError}", _statusProcess.Id, result);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
