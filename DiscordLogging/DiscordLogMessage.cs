using System.IO;
using Discord;

namespace DiscordLogging
{
    public class DiscordLogMessage
    {
        public string Message { get; set; }

        public Embed[] Embeds { get; set; }

        public string FileName { get; set; }

        public Stream File { get; set; }
    }
}
