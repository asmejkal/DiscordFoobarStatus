using System.Threading.Tasks;
using DiscordFoobarStatus.Core.Models;

namespace DiscordFoobarStatus
{
    public interface IDiscordClient
    {
        Task ClearActivityAsync();
        Task UpdateActivityAsync(ActivitySetDto activity);
    }
}