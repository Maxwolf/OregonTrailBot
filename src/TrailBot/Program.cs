using System;
using System.Collections.Generic;
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
    /// <summary>
    ///     Oregon Trail Telegram Bot Server
    ///     Created by Ron "Maxwolf" McDowell (ron.mcdowell@gmail.com)
    ///     Date 9/1/2016
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     Used by the API to communicate with the Telegram bot API network.
        /// </summary>
        private static TelegramBotClient _bot;

        /// <summary>
        ///     Stores all of the currently running game sessions based on room ID.
        /// </summary>
        private static Dictionary<long, BotSession> _sessions;

        /// <summary>
        ///     Last known number of unique sessions or games that are currently being served by the bot manager.
        /// </summary>
        private static int _lastSessionCount;

        private static void Main()
        {
            // Load bot configuration.
            var botConfig = new BotConfig(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // Complain if there is no key.
            if (string.IsNullOrEmpty(botConfig.LoadedKey))
            {
                Console.WriteLine(
                    "Unable to load API key from JSON configuration, check for created file oregon.json and edit it.");
                return;
            }

            // Create bot instance using key got from configuration.
            _bot = new TelegramBotClient(botConfig.LoadedKey);

            // Register events used by bot to tell us when things happen like messages coming in.
            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnMessageEdited += BotOnMessageReceived;
            _bot.OnReceiveError += BotOnReceiveError;

            // Get information about the bot the key gave us access to (also a test of systems).
            var me = _bot.GetMeAsync().Result;

            // Create empty sessions dictionary which is used to keep track of multiple running game states.
            _sessions = new Dictionary<long, BotSession>();

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
            _bot.StartReceiving();

            // Prevent console session from closing.
            while (_sessions != null)
            {
                // Only update the session count if it changes.
                if (!_lastSessionCount.Equals(_sessions.Count))
                    UpdateServerInfo();

                // Simulation takes any number of pulses to determine seconds elapsed.
                lock (_sessions)
                {
                    foreach (var sim in _sessions)
                        sim.Value.Session.OnTick(true);
                }

                // Do not consume all of the CPU, allow other messages to occur.
                Thread.Sleep(1);
            }

            // Make user press any key to close out the simulation completely, this way they know it closed without error.
            Console.Clear();
            Console.WriteLine("Goodbye!");
            Console.WriteLine("Press ANY KEY to close this window...");
            Console.ReadKey();

            _bot.StopReceiving();
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
            lock (_sessions)
            {
                foreach (var sim in _sessions)
                    sim.Value.Session.Destroy();
            }

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

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            // Check if the message is null and of type text message.
            if (message == null || message.Type != MessageType.TextMessage)
                return;

            if (_sessions.ContainsKey(message.Chat.Id) &&
                _sessions[message.Chat.Id].UserID == message.From.Id &&
                message.Text.Contains("/quit"))
            {
                // Skip messages that are internal mode switching or empty (populating) windows and forms.
                if (messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) ||
                    messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                    return;

                // Makes the bot appear to be thinking.
                _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing).Wait();

                // Send the notification to the chat room.
                _bot.SendTextMessageAsync(message.Chat.Id,
                    $"{message.From.FirstName} has quit the game, losing their progress. To start a new game type /start to become leader of new session.",
                    replyMarkup: new ReplyKeyboardRemove(),
                    disableWebPagePreview: true,
                    disableNotification: true).Wait();

                // Destroy the session.
                switch (message.Chat.Type)
                {
                    case ChatType.Private:
                        _sessions[message.From.Id].Session.Destroy();
                        break;
                    case ChatType.Group:
                    case ChatType.Channel:
                    case ChatType.Supergroup:
                        _sessions[message.Chat.Id].Session.Destroy();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Remove session from list.
                lock (_sessions)
                {
                    _sessions.Remove(message.Chat.Id);
                }
            }
            else if (_sessions.ContainsKey(message.Chat.Id) &&
                     _sessions[message.Chat.Id].UserID == message.From.Id &&
                     message.Text.Contains("/reset"))
            {
                // Skip messages that are internal mode switching or empty (populating) windows and forms.
                if (messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) ||
                    messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                    return;

                // Restart the session now.
                switch (message.Chat.Type)
                {
                    case ChatType.Private:
                        // Makes the bot appear to be thinking.
                        _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing).Wait();
                        _sessions[message.From.Id].Session.Restart();
                        // Send the notification to the chat room.
                        _bot.SendTextMessageAsync(message.Chat.Id,
                            $"{message.From.FirstName} has reset the game, losing their progress. Starting a new session now with them as the leader again.",
                            replyMarkup: new ReplyKeyboardRemove(),
                            disableWebPagePreview: true,
                            disableNotification: true)
                            .Wait();
                        break;
                    case ChatType.Group:
                    case ChatType.Channel:
                    case ChatType.Supergroup:
                        // Makes the bot appear to be thinking.
                        _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing).Wait();
                        _sessions[message.Chat.Id].Session.Restart();
                        // Send the notification to the chat room.
                        _bot.SendTextMessageAsync(message.Chat.Id,
                            $"{message.From.FirstName} has reset the game, losing their progress. Starting a new session now with them as the leader again.",
                            replyMarkup: new ReplyKeyboardRemove(),
                            disableWebPagePreview: true,
                            disableNotification: true).Wait();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (_sessions.ContainsKey(message.Chat.Id) &&
                     _sessions[message.Chat.Id].UserID != message.From.Id &&
                     message.Text.Contains("/join") &&
                     _sessions[message.Chat.Id]?.Session?.WindowManager?.FocusedWindow?.CurrentForm is InputPlayerNames)
            {
                // Get the current session based on ID we know exists now.
                var game = _sessions[message.Chat.Id];
                if (game == null)
                    throw new NullReferenceException("Found session ID game instance is null!");

                // Prevent the leader of the session from clicking or typing join on himself.
                if (message.From.Id == game.UserID)
                    return;

                // Clear anything that was in the input buffer before this message.
                game.Session.InputManager.ClearBuffer();

                // Allow user to join the session by using their name, that withholding use their last and if not that then whole username.
                if (!string.IsNullOrEmpty(message.From.FirstName))
                {
                    foreach (var msgChar in message.From.FirstName)
                        game.Session.InputManager.AddCharToInputBuffer(msgChar);
                }
                else if (!string.IsNullOrEmpty(message.From.LastName) && string.IsNullOrEmpty(message.From.FirstName))
                {
                    foreach (var msgChar in message.From.LastName)
                        game.Session.InputManager.AddCharToInputBuffer(msgChar);
                }
                else
                {
                    foreach (var msgChar in message.From.Username)
                        game.Session.InputManager.AddCharToInputBuffer(msgChar);
                }

                // Send whatever we got to the simulation for processing it will decide what it wants.
                game.Session.InputManager.SendInputBufferAsCommand();
            }
            else if (!_sessions.ContainsKey(message.Chat.Id) &&
                     message.Text.Contains("/start"))
            {
                // Skip messages that are internal mode switching or empty (populating) windows and forms.
                if (messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) ||
                    messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                    return;

                // Makes the bot appear to be thinking.
                _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing).Wait();

                lock (_sessions)
                {
                    _sessions.Add(message.Chat.Id, new BotSession(message));
                }

                _bot.SendTextMessageAsync(message.Chat.Id,
                    $"Creating new Oregon Trail session with {message.From.FirstName} as the leader since they said start first.",
                    replyMarkup: new ReplyKeyboardRemove(), disableWebPagePreview: true,
                    disableNotification: true).Wait();

                // Hook delegate event for knowing when that simulation is updated.
                _sessions[message.Chat.Id].Session.SceneGraph.ScreenBufferDirtyEvent +=
                    SceneGraphOnScreenBufferDirtyEvent;
            }
            else if (_sessions.ContainsKey(message.Chat.Id) &&
                     _sessions[message.Chat.Id].UserID == message.From.Id)
            {
                // Get the current session based on ID we know exists now.
                var game = _sessions[message.Chat.Id];
                if (game == null)
                    throw new NullReferenceException("Found session ID game instance is null!");

                // Check that session is not null.
                if (game.Session == null)
                    throw new NullReferenceException(
                        "Session for ID is currently null and trying to have input passed into it!");

                // Group sessions need to ignore the player during this stage.
                if (game.Session?.WindowManager?.FocusedWindow?.CurrentForm is InputPlayerNames &&
                    game.GameType != ChatType.Private && !message.Text.Contains("Generate Names"))
                    return;

                // Clear anything that was in the input buffer before this message.
                game.Session.InputManager.ClearBuffer();

                // Add each character of the message into the simulation input buffer.
                foreach (var msgChar in message.Text)
                    game.Session.InputManager.AddCharToInputBuffer(msgChar);

                // Send whatever we got to the simulation for processing it will decide what it wants.
                game.Session.InputManager.SendInputBufferAsCommand();
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
        /// <param name="session">Contains the same session data which should be also being used on this game.</param>
        private static void SceneGraphOnScreenBufferDirtyEvent(string content, object commands, BotSession session)
        {
            // Cast the commands as a string array.
            var menuCommands = commands as string[];

            // Skip messages that are internal mode switching or empty (populating) windows and forms.
            if (content.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) || content.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                return;

            // Check if there are multiple commands that can be pressed (or dialog with continue only).
            if ((menuCommands != null && menuCommands.Length <= 0) || menuCommands == null)
            {
                // Figures out where the image will be coming from.
                var window = _sessions[session.ChatID].Session.WindowManager.FocusedWindow;
                var fileName = string.Empty;
                var filePath = string.Empty;
                if (!string.IsNullOrEmpty(window.ImagePath) &&
                    window.CurrentForm == null)
                {
                    fileName = window.ImagePath.Split('\\').Last();
                    filePath = window.ImagePath;
                }
                else if (!string.IsNullOrEmpty(window.CurrentForm?.ImagePath))
                {
                    fileName = window.CurrentForm.ImagePath.Split('\\').Last();
                    filePath = window.CurrentForm.ImagePath;
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    // Makes the bot appear to be thinking.
                    _bot.SendChatActionAsync(session.ChatID, ChatAction.Typing).Wait();

                    if (session.GameType == ChatType.Private)
                    {
                        _bot.SendTextMessageAsync(session.ChatID, content,
                            replyMarkup: new ReplyKeyboardRemove(),
                            disableWebPagePreview: true,
                            disableNotification: true).Wait();
                    }
                    else
                    {
                        // Send the text user interface to chat room.
                        var forceReply = new ForceReply {Force = true, Selective = true};
                        _bot.SendTextMessageAsync(session.ChatID,
                            $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                            replyMarkup: forceReply,
                            disableWebPagePreview: true,
                            disableNotification: true).Wait();
                    }
                }
                else
                {
                    // Makes the bot appear to be thinking.
                    _bot.SendChatActionAsync(session.ChatID, ChatAction.UploadPhoto).Wait();

                    // Grabs the picture from the given path and then loads it into the Telegram API.
                    using (var fileStream =
                        new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var fts = new FileToSend(fileName, fileStream);

                        if (session.GameType == ChatType.Private)
                        {
                            _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                            _bot.SendTextMessageAsync(session.ChatID, content, replyMarkup: new ReplyKeyboardRemove(),
                                disableNotification: true)
                                .Wait();
                        }
                        else
                        {
                            // Send a message the user can reply to.
                            _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                            // Group messages cannot send a picture and also spawn a keyboard so make them separate.
                            _bot.SendTextMessageAsync(session.ChatID,
                                $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                                replyMarkup: new ReplyKeyboardRemove(),
                                disableWebPagePreview: true,
                                disableNotification: true).Wait();
                        }
                    }
                }
            }
            else if (menuCommands.Length > 0)
            {
                // Get half of the commands.
                var halfCommandCount = menuCommands.Length/2;

                // Check if less than zero.
                var window = _sessions[session.ChatID].Session.WindowManager.FocusedWindow;
                if (halfCommandCount <= 0)
                {
                    // Simulation can send photos to chat to help visualize locations.
                    if (window == null)
                        return;

                    // Figures out where the image will be coming from.
                    var fileName = string.Empty;
                    var filePath = string.Empty;
                    if (!string.IsNullOrEmpty(window.ImagePath) &&
                        window.CurrentForm == null)
                    {
                        fileName = window.ImagePath.Split('\\').Last();
                        filePath = window.ImagePath;
                    }
                    else if (!string.IsNullOrEmpty(window.CurrentForm?.ImagePath))
                    {
                        fileName = window.CurrentForm.ImagePath.Split('\\').Last();
                        filePath = window.CurrentForm.ImagePath;
                    }

                    // Send custom keyboard.
                    var button = new List<KeyboardButton>();
                    foreach (var t in menuCommands)
                        button.Add(new KeyboardButton(t));

                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        button.ToArray()
                    }, true, true)
                    {Selective = true, OneTimeKeyboard = true, ResizeKeyboard = true};

                    // Abort if there is no valid filename.
                    if (string.IsNullOrEmpty(fileName))
                    {
                        // Makes the bot appear to be thinking.
                        _bot.SendChatActionAsync(session.ChatID, ChatAction.Typing).Wait();

                        // Change up behavior depending on private or group chats.
                        if (session.GameType == ChatType.Private)
                        {
                            // Private session gets the keyboard and the content.
                            _bot.SendTextMessageAsync(session.ChatID, content,
                                replyMarkup: keyboard,
                                disableWebPagePreview: true,
                                disableNotification: true).Wait();
                        }
                        else
                        {
                            // Send the keyboard to the session leader.
                            _bot.SendTextMessageAsync(session.ChatID,
                                $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                                replyMarkup: keyboard,
                                disableWebPagePreview: true,
                                disableNotification: true).Wait();
                        }
                    }
                    else
                    {
                        // Makes the bot appear to be thinking.
                        _bot.SendChatActionAsync(session.ChatID, ChatAction.UploadPhoto).Wait();

                        // Grabs the picture from the given path and then loads it into the Telegram API.
                        using (var fileStream =
                            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var fts = new FileToSend(fileName, fileStream);

                            if (session.GameType == ChatType.Private)
                            {
                                _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                                _bot.SendTextMessageAsync(session.ChatID, content, replyMarkup: keyboard,
                                    disableNotification: true)
                                    .Wait();
                            }
                            else
                            {
                                // Send a message the user can reply to.
                                _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                                // Group messages cannot send a picture and also spawn a keyboard so make them separate.
                                _bot.SendTextMessageAsync(session.ChatID,
                                    $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                                    replyMarkup: keyboard,
                                    disableWebPagePreview: true,
                                    disableNotification: true).Wait();
                            }
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
                        topRow.ToArray(), bottomRow.ToArray()
                    }, true, true) {Selective = true};

                    // Figures out where the image will be coming from.
                    string fileName;
                    string filePath;
                    if (!string.IsNullOrEmpty(window.ImagePath) &&
                        window.CurrentForm == null)
                    {
                        filePath = window.ImagePath;
                        fileName = window.ImagePath.Split('\\').Last();

                        _bot.SendChatActionAsync(session.ChatID, ChatAction.UploadPhoto).Wait();

                        // Grabs the picture from the given path and then loads it into the Telegram API.
                        using (var fileStream =
                            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var fts = new FileToSend(fileName, fileStream);

                            if (session.GameType == ChatType.Private)
                            {
                                _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                                _bot.SendTextMessageAsync(session.ChatID, content, replyMarkup: keyboard,
                                    disableNotification: true).Wait();
                            }
                            else
                            {
                                // Send a message the user can reply to.
                                _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                                // Group messages cannot send a picture and also spawn a keyboard so make them separate.
                                _bot.SendTextMessageAsync(session.ChatID,
                                    $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                                    replyMarkup: keyboard,
                                    disableWebPagePreview: true,
                                    disableNotification: true).Wait();
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(window.CurrentForm?.ImagePath))
                    {
                        filePath = window.CurrentForm.ImagePath;
                        fileName = window.CurrentForm.ImagePath.Split('\\').Last();

                        // Abort if there is no valid filename.
                        if (string.IsNullOrEmpty(fileName))
                        {
                            // Send the message to chat room.
                            _bot.SendChatActionAsync(session.ChatID, ChatAction.Typing).Wait();

                            if (session.GameType == ChatType.Private)
                            {
                                _bot.SendTextMessageAsync(session.ChatID, content, replyMarkup: keyboard,
                                    disableWebPagePreview: true, disableNotification: true).Wait();
                            }
                            else
                            {
                                _bot.SendTextMessageAsync(session.ChatID,
                                    $"@{session.LeaderUsername}{Environment.NewLine}{content}", replyMarkup: keyboard,
                                    disableWebPagePreview: true, disableNotification: true).Wait();
                            }
                        }
                        else
                        {
                            // Grabs the picture from the given path and then loads it into the Telegram API.
                            using (
                                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                )
                            {
                                _bot.SendChatActionAsync(session.ChatID, ChatAction.UploadPhoto).Wait();

                                var fts = new FileToSend(fileName, fileStream);

                                if (session.GameType == ChatType.Private)
                                {
                                    _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                                    _bot.SendTextMessageAsync(session.ChatID, content, replyMarkup: keyboard,
                                        disableNotification: true).Wait();
                                }
                                else
                                {
                                    // Send a message the user can reply to.
                                    _bot.SendDocumentAsync(session.ChatID, fts, string.Empty).Wait();

                                    // Group messages cannot send a picture and also spawn a keyboard so make them separate.
                                    _bot.SendTextMessageAsync(session.ChatID,
                                        $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                                        replyMarkup: keyboard,
                                        disableWebPagePreview: true,
                                        disableNotification: true).Wait();
                                }
                            }
                        }
                    }
                    else
                    {
                        // Send the message to chat room.
                        _bot.SendChatActionAsync(session.ChatID, ChatAction.Typing).Wait();

                        if (session.GameType == ChatType.Private)
                        {
                            _bot.SendTextMessageAsync(session.ChatID, content,
                                replyMarkup: keyboard,
                                disableWebPagePreview: true,
                                disableNotification: true).Wait();
                        }
                        else
                        {
                            _bot.SendTextMessageAsync(session.ChatID,
                                $"@{session.LeaderUsername}{Environment.NewLine}{content}",
                                replyMarkup: keyboard,
                                disableWebPagePreview: true,
                                disableNotification: true).Wait();
                        }
                    }
                }
            }
        }
    }
}