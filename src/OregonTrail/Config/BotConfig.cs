using System.IO;
using Newtonsoft.Json;

namespace OregonTrail
{
    /// <summary>
    ///     Class that is used to manually control the loading and saving of data related to the simulation and bot keys used
    ///     to control the simulation and allow access to a Telegram API bot.
    /// </summary>
    public class BotConfig
    {
        /// <summary>
        ///     Starts looking for configuration files in the application startup path.
        /// </summary>
        public BotConfig(string startupPath)
        {
            // Two configuration files can be found. One will be loaded before the other if found, the dev always comes first before the user.
            var devKeyPath = Path.Combine(startupPath, "oregon_dev.json");
            var userKeyPath = Path.Combine(startupPath, "oregon.json");

            // Look for the dev key.
            if (File.Exists(devKeyPath))
            {
                LoadedKey = JsonConvert.DeserializeObject<BotInfo>(File.ReadAllText(devKeyPath))?.BotKey;
                return;
            }

            // Look for the user key.
            if (File.Exists(userKeyPath))
            {
                LoadedKey = JsonConvert.DeserializeObject<BotInfo>(File.ReadAllText(userKeyPath))?.BotKey;
                return;
            }

            // Attempts to create configuration files if we get this far and nothing happens.
            if (!File.Exists(userKeyPath))
                Save(userKeyPath);
        }

        /// <summary>
        ///     Key which was loaded from one of the valid configuration files found in the application startup path.
        /// </summary>
        public string LoadedKey { get; }

        /// <summary>
        ///     Saves out a copy of the configuration files. This is used to generate them if they do not exist, by default only
        ///     the user configuration file will get generated.
        /// </summary>
        /// <param name="userKeyPath">Path to where the configuration file will live on the disk.</param>
        private void Save(string userKeyPath)
        {
            // Remove a file if it already exists, checks are done before this one.
            if (File.Exists(userKeyPath))
                File.Delete(userKeyPath);

            // Use default message if one does not exist, otherwise use loaded key.
            var key = "Telegram Bot API Key Goes Here";
            if (!string.IsNullOrEmpty(LoadedKey))
                key = LoadedKey;

            // Serialize the bot configuration into JSON after creating a new one.
            var config = new BotInfo(key);
            var configText = JsonConvert.SerializeObject(config);
            File.WriteAllText(userKeyPath, configText);
        }
    }
}