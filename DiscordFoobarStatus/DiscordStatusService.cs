using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordFoobarStatus.Configuration;
using DiscordFoobarStatus.Core.Models;
using DiscordFoobarStatus.Core.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qwr.ComponentInterface;

namespace DiscordFoobarStatus
{
    public class DiscordStatusService : IHostedService, IDisposable
    {
        private readonly IControls _controls;
        private readonly IDeezerClient _deezerClient;
        private readonly IDiscordClient _discordClient;
        private readonly ILogger<DiscordStatusService> _logger;

        private readonly IOptionsMonitor<FormattingOptions> _formattingOptions;
        private readonly IOptionsMonitor<ComponentOptions> _componentOptions;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private IMetadbHandle? _currentHandle;
        private bool _updatePending;
        private DateTimeOffset? _activityStart;

        public DiscordStatusService(
            IPlaybackCallbacks callbacks,
            IControls controls,
            IDeezerClient deezerClient,
            IDiscordClient discordClient,
            ILogger<DiscordStatusService> logger, 
            IOptionsMonitor<FormattingOptions> formattingOptions,
            IOptionsMonitor<ComponentOptions> componentOptions)
        {
            _controls = controls;
            _deezerClient = deezerClient;
            _discordClient = discordClient;
            _logger = logger;
            _formattingOptions = formattingOptions;
            _componentOptions = componentOptions;

            _formattingOptions.OnChange(HandleFormattingOptionsChanged);
            _componentOptions.OnChange(HandleComponentOptionsChanged);

            callbacks.PlaybackStarting += HandlePlaybackStarting;
            callbacks.PlaybackPausedStateChanged += HandlePlaybackPausedStateChanged;
            callbacks.PlaybackStopped += HandlePlaybackStopped;
            callbacks.PlaybackAdvancedToNewTrack += HandlePlaybackAdvancedToNewTrack;
            callbacks.TrackSeekPerformed += HandleTrackSeekPerformed;
            callbacks.TrackPlaybackPositionChanged += HandleTrackPlaybackPositionChanged;
        }

        private void HandleFormattingOptionsChanged(FormattingOptions value, string _)
        {
            _logger.LogInformation("Formatting options changed");
            _updatePending = true;
        }

        private void HandleComponentOptionsChanged(ComponentOptions value, string _)
        {
            if (value.Disabled)
            {
                Handle(async () =>
                {
                    _logger.LogInformation("Component disabled, clearing activity");
                    await _discordClient.ClearActivityAsync();
                });
            }
            else
            {
                _logger.LogInformation("Component re-enabled");
                _updatePending = true;
            }
        }

        private void HandleTrackPlaybackPositionChanged(object? sender, GenericEventArgs<double> e)
        {
            // Calculate drift in displayed elapsed time (seek detection is currently broken in the library)
            var expectedStart = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(e.Value);
            var currentDifference = _activityStart.HasValue ? Math.Abs((_activityStart.Value - expectedStart).TotalSeconds) : 0;

            if (_updatePending || currentDifference > 5)
            {
                _updatePending = false;

                if (_componentOptions.CurrentValue.Disabled)
                    return;

                var handle = _currentHandle;
                if (handle == null)
                {
                    _logger.LogError("Missing current track handle");
                    return;
                }

                Handle(async () =>
                {
                    _logger.LogInformation("Updating activity");
                    await UpdateActivityAsync(handle, e.Value);
                });
            }
        }

        private void HandleTrackSeekPerformed(object? sender, GenericEventArgs<double> e)
        {
            _logger.LogInformation("Track seeked");
            _updatePending = true;
        }

        private void HandlePlaybackStopped(object? sender, GenericEventArgs<PlaybackStopReason> e)
        {
            if (e.Value != PlaybackStopReason.StartingAnother)
            {
                Handle(async () =>
                {
                    _logger.LogInformation("Playback stopped, clearing activity");
                    await _discordClient.ClearActivityAsync();
                });
            }
        }

        private void HandlePlaybackPausedStateChanged(object? sender, GenericEventArgs<bool> e)
        {
            if (e.Value)
            {
                Handle(async () =>
                {
                    _logger.LogInformation("Playback paused, clearing activity");
                    await _discordClient.ClearActivityAsync();
                });
            }
            else
            {
                _logger.LogInformation("Playback resumed");
                _updatePending = true; // Can take a while for the unpause to propagate
            }
        }

        private void HandlePlaybackAdvancedToNewTrack(object? sender, GenericEventArgs<IMetadbHandle> e)
        {
            _logger.LogInformation("Playback advanced to new track, updating handle");
            if (_currentHandle != null)
                _currentHandle.Dispose();

            _currentHandle = e.Value;
            _updatePending = true;
        }

        private void HandlePlaybackStarting(object? sender, PlaybackStartingEventArgs e)
        {
            _logger.LogInformation("Playback starting");
            _updatePending = true;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Clearing activity from previous run, in case Foobar crashed");
            try
            {
                await _discordClient.ClearActivityAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear activity");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down, clearing activity");
            try
            {
                await _discordClient.ClearActivityAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear activity");
            }
        }

        private async Task UpdateActivityAsync(IMetadbHandle handle, double positionSeconds)
        {
            var topLine = _controls.TitleFormat(_formattingOptions.CurrentValue.TopLineFormat ?? PreferencesDefaults.TopLineFormat)
                .EvalWithMetadb(handle);

            var bottomLine = _controls.TitleFormat(_formattingOptions.CurrentValue.BottomLineFormat ?? PreferencesDefaults.BottomLineFormat)
                .EvalWithMetadb(handle);

            var artist = _controls.TitleFormat("%artist%").EvalWithMetadb(handle);
            var album = _controls.TitleFormat("%album%").EvalWithMetadb(handle);

            _activityStart = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(positionSeconds);
            var activity = new ActivitySetDto()
            {
                Details = topLine,
                State = bottomLine,
                Thumbnail = "default",
                ThumbnailText = album,
                StartTimestamp = _activityStart.Value.ToUnixTimeSeconds()
            };

            try
            {
                var url = await _deezerClient.FindThumbnailAsync(artist, album);
                if (url != null)
                    activity.Thumbnail = url.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve album information for album {album} and artist {artist} from Deezer");
            }

            await _discordClient.UpdateActivityAsync(activity);
        }

        private void Handle(Func<Task> func)
        {
            // We can't block the UI thread with long running IPC or HTTP requests, so we need to offload to a worker thread
            TaskHelper.FireForget(async () =>
            {
                using (await _semaphore.ClaimAsync())
                {
                    await func();
                }
            }, ex => _logger.LogError(ex, "Uncaught exception in handler"));
        }

        public void Dispose()
        {
            _currentHandle?.Dispose();
        }
    }
}
