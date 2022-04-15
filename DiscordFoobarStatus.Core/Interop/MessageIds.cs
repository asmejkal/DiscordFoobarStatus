using DiscordFoobarStatus.Core.Interop;

namespace DiscordFoobarStatus.Core.Models
{
    public class MessageIds
    {
        public uint UpdateActivity { get; }
        public uint Shutdown { get; }

        public MessageIds()
        {
            UpdateActivity = User32.RegisterWindowMessage("DiscordFoobarStatus.UpdateActivity");
            Shutdown = User32.RegisterWindowMessage("DiscordFoobarStatus.Shutdown");
        }
    }
}
