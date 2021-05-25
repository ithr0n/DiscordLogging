namespace DiscordLogging
{
    public interface ICanHandleMessages
    {
        void AddMessage(DiscordLogMessage message);
    }
}