using System.IO;
using Discord;

namespace DiscordLogging
{
    public class DiscordLogModel
    {
        public string Message { get; set; }

        public Embed[] DiscordEmbeds { get; set; }

        public string FileName { get; set; }

        public Stream FileStream { get; set; }
    }
}
