using DiscordFoobarStatus.Core.Models;

namespace DiscordFoobarStatus.Client
{
    public interface IDiscordClient
    {
        void ClearActivity();
        void RunCallbacks();
        void UpdateActivity(ActivitySetDto data);
    }
}