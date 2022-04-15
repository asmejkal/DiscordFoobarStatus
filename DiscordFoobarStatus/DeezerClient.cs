using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DiscordFoobarStatus
{
    public class DeezerClient : IDeezerClient
    {
        private readonly HttpClient _client;

        public DeezerClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<Uri?> FindThumbnailAsync(string artist, string album)
        {
            var result = await _client.GetStringAsync($"search?strict=on&q=artist:\"{artist}\" album:\"{album}\"");
            
            var json = JObject.Parse(result);
            var item = json["data"]?.AsJEnumerable().FirstOrDefault();
            if (item == null)
                return null;

            var albumToken = item["album"];
            if (albumToken != null)
            {
                var url = (string?)(albumToken?["cover_medium"] ?? albumToken?["cover_big"] ?? albumToken?["cover_xl"] ?? albumToken?["cover_small"]);
                if (url != null)
                    return new(url);
            }

            var artistToken = item["artist"];
            if (artistToken != null)
            {
                var url = (string?)(artistToken?["picture_medium"] ?? artistToken?["picture_big"] ?? artistToken?["picture_xl"] ?? artistToken?["picture_small"]);
                if (url != null)
                    return new(url);
            }

            return null;
        }
    }
}
