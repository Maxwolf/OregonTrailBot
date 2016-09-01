using Newtonsoft.Json;

namespace TrailBot
{
    /// <summary>
    ///     Defines a set of data that can be used to configure the bot and store data about past events like high scores,
    ///     tombstones, and anything else that is deemed important enough to save and load again on application startup. This
    ///     class will be serialized to JSON and then reloaded again from disk.
    /// </summary>
    public class BotInfo
    {
        /// <summary>
        ///     Creates a new instance of the configuration file used by the bot. There are a couple of these files and they are
        ///     checked by name and loaded in a specific order so developer can ignore his key while still providing the user a
        ///     template by which to insert their own key into.
        /// </summary>
        /// <param name="botKey">Telegram API key from @botfather</param>
        [JsonConstructor]
        public BotInfo(string botKey)
        {
            BotKey = botKey;
        }

        /// <summary>
        ///     Key that is used by the telegram API directly to grant control to the application over a specific username we have
        ///     registered with the @botfather.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string BotKey { get; set; }
    }
}