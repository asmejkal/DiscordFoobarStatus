using System;
using System.Threading.Tasks;

namespace DiscordFoobarStatus
{
    public interface IDeezerClient
    {
        Task<Uri?> FindThumbnailAsync(string artist, string album);
    }
}