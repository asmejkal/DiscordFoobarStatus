using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordFoobarStatus.Configuration;
using DiscordFoobarStatus.Utility;
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
        private readonly IDiscordClientService _discordClient;
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
            IDiscordClientService discordClient,
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
            try
            {
                if (value.Disabled)
                {
                    _logger.LogInformation("Component disabled, clearing activity");
                    _discordClient.ClearActivity();
                }
                else
                {
                    _logger.LogInformation("Component re-enabled");
                    _updatePending = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle options change");
            }
        }

        private void HandleTrackPlaybackPositionChanged(object? sender, GenericEventArgs<double> e)
        {
            try
            {
                if (_updatePending)
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

                    _logger.LogInformation("Updating activity");
                    UpdateActivity(handle, e.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle playback position change");
            }
        }

        private void HandleTrackSeekPerformed(object? sender, GenericEventArgs<double> e)
        {
            _logger.LogInformation("Track seeked");
            _updatePending = true;
        }

        private void HandlePlaybackStopped(object? sender, GenericEventArgs<PlaybackStopReason> e)
        {
            try
            {
                if (e.Value != PlaybackStopReason.StartingAnother)
                {
                    _logger.LogInformation("Playback stopped, clearing activity");
                    _discordClient.ClearActivity();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle playback position change");
            }
        }

        private void HandlePlaybackPausedStateChanged(object? sender, GenericEventArgs<bool> e)
        {
            try
            {
                if (e.Value)
                {
                    _logger.LogInformation("Playback paused, clearing activity");
                    _discordClient.ClearActivity();
                }
                else
                {
                    _logger.LogInformation("Playback resumed");
                    _updatePending = true; // Can take a while for the unpause to propagate
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle playback position change");
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Clearing activity from previous run, in case Foobar crashed");
            try
            {
                _discordClient.ClearActivity();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear activity");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down, clearing activity");
            try
            {
                _discordClient.ClearActivity();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear activity");
            }

            return Task.CompletedTask;
        }

        private void UpdateActivity(IMetadbHandle handle, double positionSeconds)
        {
            var isPlaying = _controls.PlaybackControls().IsPlaying();
            string Render(string expression)
            {
                var format = _controls.TitleFormat(expression);
                return isPlaying ? format.Eval() : format.EvalWithMetadb(handle);
            }

            var topLine = Render(_formattingOptions.CurrentValue.TopLineFormat ?? PreferencesDefaults.TopLineFormat);
            var bottomLine = Render(_formattingOptions.CurrentValue.BottomLineFormat ?? PreferencesDefaults.BottomLineFormat);

            var artist = Render("[%artist%]");
            var album = Render("[%album%]");

            _activityStart = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(positionSeconds);
            var activity = new Activity()
            {
                Details = topLine,
                State = bottomLine,
                Thumbnail = "default",
                ThumbnailText = album,
                StartTimestamp = _activityStart.Value.ToUnixTimeSeconds()
            };

            // Offload to a worker thread, to not block the main thread with potentially long-running HTTP requests
            TaskHelper.FireForget(async () =>
            {
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

                _discordClient.UpdateActivity(activity);
            }, ex => _logger.LogError(ex, "Failed to update activity"));
        }

        public void Dispose()
        {
            _currentHandle?.Dispose();
        }
    }
}
