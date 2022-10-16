using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using DiscordFoobarStatus.Configuration;
using DiscordFoobarStatus.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordFoobarStatus
{
    internal sealed class DiscordClientService : BackgroundService, IDisposable, IDiscordClientService
    {
        private Discord.Discord? _client;
        private readonly SemaphoreSlim _clientLock = new SemaphoreSlim(1, 1);

        private readonly ILogger<DiscordClientService> _logger;

        public DiscordClientService(ILogger<DiscordClientService> logger)
        {
            _logger = logger;
        }

        private Discord.Discord InitClient()
        {
            if (_client == null)
            {
                _client = new Discord.Discord(PreferencesDefaults.DiscordClientId, (ulong)CreateFlags.Default);
                _client.SetLogHook(Discord.LogLevel.Debug, (level, message) => _logger.Log(level.ToMicrosoft(), message));
            }

            return _client;
        }

        public override void Dispose()
        {
            base.Dispose();

            using (_clientLock.Claim())
            {
                ((IDisposable?)_client)?.Dispose();
                _client = null;
            }

            _clientLock.Dispose();
        }

        public void UpdateActivity(Activity data)
        {
            using (_clientLock.Claim())
            {
                var client = InitClient();

                // Discord's string marshaling is wrong, so we have to encode the string manually and marshal it as a byte array
                byte[] MarshalString(string x)
                {
                    var result = new byte[128];

                    var position = 0; // Leave one byte at the end as a null terminator
                    for (var i = 0; i < x.Length && position + Encoding.UTF8.GetByteCount(x, i, 1) <= result.Length - 1; ++i)
                        position += Encoding.UTF8.GetBytes(x, i, 1, result, position);

                    return result;
                }

                string PadString(string x) => x.Length == 1 ? x + " " : x; // Discord requires at least 2 characters...

                var activity = new Discord.Activity()
                {
                    State = data.State != null ? MarshalString(PadString(data.State)) : null,
                    Details = data.Details != null ? MarshalString(PadString(data.Details)) : null!
                };

                if (data.Thumbnail != null)
                {
                    activity.Assets = new() { LargeImage = data.Thumbnail };
                    if (data.ThumbnailText != null)
                        activity.Assets.LargeText = MarshalString(PadString(data.ThumbnailText));
                }

                if (data.StartTimestamp != null)
                    activity.Timestamps = new() { Start = data.StartTimestamp.Value };
                else if (data.EndTimestamp != null)
                    activity.Timestamps = new() { End = data.EndTimestamp.Value };

                client.GetActivityManager().UpdateActivity(activity, x =>
                {
                    if (x == Result.Ok)
                        _logger.LogInformation("Updated activity to {Activity}", data);
                    else
                        _logger.LogError("Failed to update activity with error {Error}", x);
                });
            }
        }

        public void ClearActivity()
        {
            using (_clientLock.Claim())
            {
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (await _clientLock.ClaimAsync())
                    {
                        if (_client != null)
                            _client.RunCallbacks();
                    }

                    await Task.Delay(200, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Shutting down
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Encountered an exception while running Discord SDK callbacks");
                }
            }
        }
    }
}
