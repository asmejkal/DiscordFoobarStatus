using System.Threading.Tasks;

namespace DiscordFoobarStatus
{
    public interface IDiscordClientService
    {
        void ClearActivity();
        void UpdateActivity(Activity data);
    }
}