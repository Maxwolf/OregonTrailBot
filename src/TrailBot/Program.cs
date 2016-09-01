using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using OregonTrail;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TrailBot
{
    internal class Program
    {
        /// <summary>
        ///     Used by the API to communicate with the Telegram bot API network.
        /// </summary>
        private static TelegramBotClient Bot;

        /// <summary>
        ///     Stores all of the currently running game sessions based on room ID.
        /// </summary>
        private static Dictionary<long, GameSimulationApp> _sessions;

        private static int _lastSessionCount;

        private static void Main(string[] args)
        {
            // Load bot configuration.
            var botConfig = new BotConfig(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // Complain if there is no key.
            if (string.IsNullOrEmpty(botConfig.LoadedKey))
                throw new SettingsPropertyNotFoundException(
                    "Unable to load API key from JSON configuration, check for created file oregon.json and edit it.");

            // Create bot instance using key got from configuration.
            Bot = new TelegramBotClient(botConfig.LoadedKey);

            // Register events used by bot to tell us when things happen like messages coming in.
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            // Get information about the bot the key gave us access to (also a test of systems).
            var me = Bot.GetMeAsync().Result;

            // Create empty sessions dictionary which is used to keep track of multiple running game states.
            _sessions = new Dictionary<long, GameSimulationApp>();

            // Create title for the console window so it can be properly identified in graphical environments.
            Console.Title = me.Username;

            // Create console with title, no cursor, make CTRL-C act as input.
            Console.Title = "Oregon Trail Telegram Bot Server";
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            UpdateServerInfo();
            Console.CancelKeyPress += Console_CancelKeyPress;

            // Process bot commands.
            Bot.StartReceiving();

            // Prevent console session from closing.
            while (_sessions != null)
            {
                // Only update the session count if it changes.
                if (!_lastSessionCount.Equals(_sessions.Count))
                {
                    UpdateServerInfo();
                }

                // Simulation takes any number of pulses to determine seconds elapsed.
                foreach (var sim in _sessions)
                    sim.Value.OnTick(true);

                // Do not consume all of the CPU, allow other messages to occur.
                Thread.Sleep(1);
            }

            // Make user press any key to close out the simulation completely, this way they know it closed without error.
            Console.Clear();
            Console.WriteLine("Goodbye!");
            Console.WriteLine("Press ANY KEY to close this window...");
            Console.ReadKey();

            Bot.StopReceiving();
        }

        private static void UpdateServerInfo()
        {
            _lastSessionCount = _sessions.Count;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            Console.WriteLine(
                $"Oregon Trail Telegram Bot Server{Environment.NewLine}Sessions: {_sessions.Count.ToString("N0")}");
        }

        /// <summary>
        ///     Fired when the user presses CTRL-C on their keyboard, this is only relevant to operating system tick and this view
        ///     of simulation. If moved into another framework like game engine this statement would be removed and just destroy
        ///     the simulation when the engine is destroyed using its overrides.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Destroy the simulation sessions.
            foreach (var sim in _sessions)
                sim.Value.Destroy();

            // Clear out and then destroy the dictionary which will exit the main loop and stop application.
            _sessions.Clear();
            _sessions = null;

            // Stop the operating system from killing the entire process.
            e.Cancel = true;
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            // Check if the message is null and of type text message.
            if (message == null || message.Type != MessageType.TextMessage)
                return;

            // Create new session if needed, otherwise pass message along.
            if (_sessions.ContainsKey(message.Chat.Id))
            {
                // Get the current session based on ID we know exists now.
                var game = _sessions[message.Chat.Id];
                if (game == null)
                    throw new NullReferenceException("Found session ID game instance is null!");

                // Clear anything that was in the input buffer before this message.
                game.InputManager.ClearBuffer();

                // Add each character of the message into the simulation input buffer.
                foreach (var msgChar in message.Text)
                    game.InputManager.AddCharToInputBuffer(msgChar);

                // Send whatever we got to the simulation for processing it will decide what it wants.
                game.InputManager.SendInputBufferAsCommand();
            }
            else
            {
                // Skip messages that are internal mode switching or empty (populating) windows and forms.
                if (messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) ||
                    messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                    return;

                //// Tell the players what we are doing.
                //await Bot.SendTextMessageAsync(message.Chat.Id,
                //    $"Creating new Oregon Trail session with token: {message.Chat.Id}",
                //    replyMarkup: new ReplyKeyboardHide(),
                //    parseMode: ParseMode.Markdown,
                //    disableWebPagePreview: true,
                //    disableNotification: true);

                // Makes the bot appear to be thinking.
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Create a new game session using chat room ID as key in our dictionary.
                _sessions.Add(message.Chat.Id, new GameSimulationApp(message.Chat.Id, Bot));

                // Hook delegate event for knowing when that simulation is updated.
                if (_sessions.ContainsKey(message.Chat.Id))
                    _sessions[message.Chat.Id].SceneGraph.ScreenBufferDirtyEvent += SceneGraphOnScreenBufferDirtyEvent;
            }
        }

        /// <summary>
        ///     Called when one of the running instances in the game wants to update the data that has been displayed to users.
        /// </summary>
        /// <param name="content">Text user interface that will be sent to the chat.</param>
        /// <param name="commands">
        ///     Commands which will become buttons, NULL if user input required for things like amounts or
        ///     names.
        /// </param>
        /// <param name="gameID">Unique identifier that is used to base the chat with bot to given user.</param>
        private static async void SceneGraphOnScreenBufferDirtyEvent(string content, object commands, long gameID)
        {
            // Cast the commands as a string array.
            var menuCommands = commands as string[];

            // Skip messages that are internal mode switching or empty (populating) windows and forms.
            if (content.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) ||
                content.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                return;

            // Check if there are multiple commands that can be pressed (or dialog with continue only).
            if ((menuCommands != null && menuCommands.Length <= 0) || menuCommands == null)
            {
                // Makes the bot appear to be thinking.
                await Bot.SendChatActionAsync(gameID, ChatAction.Typing);

                // Instruct the program that it can pass along screen buffer when it changes.
                await Bot.SendTextMessageAsync(gameID, content,
                    replyMarkup: new ReplyKeyboardHide(),
                    disableWebPagePreview: true,
                    disableNotification: true);
            }
            else if (menuCommands.Length > 0)
            {
                // Get half of the commands.
                var halfCommandCount = menuCommands.Length/2;

                // Check if less than zero.
                if (halfCommandCount <= 0)
                {
                    // Send custom keyboard.
                    var button = new List<KeyboardButton>();
                    foreach (var t in menuCommands)
                        button.Add(new KeyboardButton(t));

                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        button.ToArray()
                    }, true, true);

                    // Simulation can send photos to chat to help visualize locations.
                    if (_sessions[gameID].WindowManager.FocusedWindow == null)
                        return;

                    // Figures out where the image will be coming from.
                    var fileName = string.Empty;
                    var filePath = string.Empty;
                    if (!string.IsNullOrEmpty(_sessions[gameID].WindowManager.FocusedWindow.ImagePath) &&
                        _sessions[gameID].WindowManager.FocusedWindow.CurrentForm == null)
                    {
                        fileName = _sessions[gameID].WindowManager.FocusedWindow.ImagePath.Split('\\').Last();
                        filePath = _sessions[gameID].WindowManager.FocusedWindow.ImagePath;
                    }
                    else if (_sessions[gameID].WindowManager.FocusedWindow.CurrentForm != null &&
                             !string.IsNullOrEmpty(
                                 _sessions[gameID].WindowManager.FocusedWindow.CurrentForm.ImagePath))
                    {
                        fileName =
                            _sessions[gameID].WindowManager.FocusedWindow.CurrentForm.ImagePath.Split('\\').Last();
                        filePath = _sessions[gameID].WindowManager.FocusedWindow.CurrentForm.ImagePath;
                    }

                    // Abort if there is no valid filename.
                    if (string.IsNullOrEmpty(fileName))
                    {
                        // Makes the bot appear to be thinking.
                        await Bot.SendChatActionAsync(gameID, ChatAction.Typing);

                        // Text operations do not include photos.
                        await Bot.SendTextMessageAsync(gameID, content,
                            replyMarkup: keyboard,
                            disableWebPagePreview: true,
                            disableNotification: true);
                    }
                    else
                    {
                        // Grabs the picture from the given path and then loads it into the Telegram API.
                        using (
                            var fileStream = new FileStream(
                                filePath, FileMode.Open,
                                FileAccess.Read, FileShare.Read))
                        {
                            var fts = new FileToSend(fileName, fileStream);
                            await Bot.SendPhotoAsync(gameID, fts, content,
                                replyMarkup: keyboard,
                                disableNotification: true);
                        }
                    }
                }
                else
                {
                    // First row is first half of menu options.
                    var topRow = new List<KeyboardButton>();
                    for (var i = 0; i < halfCommandCount; i++)
                        topRow.Add(new KeyboardButton(menuCommands[i]));

                    // Second row is last half of menu options.
                    var bottomRow = new List<KeyboardButton>();
                    for (var i = halfCommandCount; i < menuCommands.Length; i++)
                        bottomRow.Add(new KeyboardButton(menuCommands[i]));

                    // Send custom keyboard.
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        topRow.ToArray(),
                        bottomRow.ToArray()
                    }, true, true);

                    // Figures out where the image will be coming from.
                    string fileName;
                    string filePath;
                    if (!string.IsNullOrEmpty(_sessions[gameID].WindowManager.FocusedWindow.ImagePath) &&
                        _sessions[gameID].WindowManager.FocusedWindow.CurrentForm == null)
                    {
                        filePath = _sessions[gameID].WindowManager.FocusedWindow.ImagePath;
                        fileName = _sessions[gameID].WindowManager.FocusedWindow.ImagePath.Split('\\').Last();
                        await Bot.SendChatActionAsync(gameID, ChatAction.UploadPhoto);

                        // Grabs the picture from the given path and then loads it into the Telegram API.
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                            )
                        {
                            var fts = new FileToSend(fileName, fileStream);
                            await Bot.SendPhotoAsync(gameID, fts, content,
                                replyMarkup: keyboard,
                                disableNotification: true);
                        }
                    }
                    else if (_sessions[gameID].WindowManager.FocusedWindow.CurrentForm != null &&
                             !string.IsNullOrEmpty(
                                 _sessions[gameID].WindowManager.FocusedWindow.CurrentForm.ImagePath))
                    {
                        filePath = _sessions[gameID].WindowManager.FocusedWindow.CurrentForm.ImagePath;
                        fileName =
                            _sessions[gameID].WindowManager.FocusedWindow.CurrentForm.ImagePath.Split('\\').Last();
                        await Bot.SendChatActionAsync(gameID, ChatAction.UploadPhoto);

                        // Abort if there is no valid filename.
                        if (string.IsNullOrEmpty(fileName))
                        {
                            await Bot.SendChatActionAsync(gameID, ChatAction.Typing);

                            // Send the message to chat room.
                            await Bot.SendTextMessageAsync(
                                gameID,
                                content,
                                replyMarkup: keyboard,
                                disableWebPagePreview: true,
                                disableNotification: true);
                        }
                        else
                        {
                            // Grabs the picture from the given path and then loads it into the Telegram API.
                            using (
                                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                )
                            {
                                var fts = new FileToSend(fileName, fileStream);
                                await Bot.SendPhotoAsync(gameID, fts, content,
                                    replyMarkup: keyboard,
                                    disableNotification: true);
                            }
                        }
                    }
                    else
                    {
                        await Bot.SendChatActionAsync(gameID, ChatAction.Typing);

                        // Send the message to chat room.
                        await Bot.SendTextMessageAsync(
                            gameID,
                            content,
                            replyMarkup: keyboard,
                            disableWebPagePreview: true,
                            disableNotification: true);
                    }
                }
            }
        }
    }
}