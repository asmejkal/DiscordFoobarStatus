using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Discord;
using DiscordFoobarStatus.Core.Models;
using Microsoft.Extensions.Logging;

namespace DiscordFoobarStatus.Client
{
    internal class DiscordClient : IDisposable, IDiscordClient
    {
        private readonly Discord.Discord _client;
        private readonly ILogger<DiscordClient> _logger;

        public DiscordClient(ILogger<DiscordClient> logger)
        {
            _client = new Discord.Discord(DiscordConstants.ClientId, (ulong)CreateFlags.NoRequireDiscord);
            _logger = logger;
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
        }

        public void RunCallbacks() => _client.RunCallbacks();

        public void UpdateActivity(ActivitySetDto data)
        {
            // Discord's string marshaling is wrong, so we have to encode the string manually and marshal it as a byte array
            byte[] MarshalString(string x)
            {
                var result = new byte[128];

                var position = 0; // Leave one byte at the end as a null terminator
                for (var i = 0; i < x.Length && position + Encoding.UTF8.GetByteCount(x, i, 1) <= result.Length - 1; ++i)
                    position += Encoding.UTF8.GetBytes(x, i, 1, result, position);
                
                return result;
            }

            var activity = new Activity()
            {
                State = data.State != null ? MarshalString(data.State) : null,
                Details = data.Details != null ? MarshalString(data.Details) : null!
            };

            if (data.Thumbnail != null)
            {
                activity.Assets = new() { LargeImage = data.Thumbnail };
                if (data.ThumbnailText != null)
                    activity.Assets.LargeText = data.ThumbnailText;
            }

            if (data.StartTimestamp != null)
                activity.Timestamps = new() { Start = data.StartTimestamp.Value };
            else if (data.EndTimestamp != null)
                activity.Timestamps = new() { End = data.EndTimestamp.Value };

            _client.GetActivityManager().UpdateActivity(activity, x =>
            {
                if (x == Result.Ok)
                    _logger.LogInformation("Updated activity to {Activity}", data);
                else
                    _logger.LogError("Failed to update activity with error {Error}", x);
            });
        }

        public void ClearActivity()
        {
            _client.GetActivityManager().ClearActivity(x =>
            {
                if (x == Result.Ok)
                    _logger.LogInformation("Cleared activity");
                else
                    _logger.LogError("Failed to clear activity with error {Error}", x);
            });
        }
    }
}
