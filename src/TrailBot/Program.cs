using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly TelegramBotClient Bot =
            new TelegramBotClient("227031115:AAElTAMDxnUxJ5_VuDn5LUUvQpNHnyT_V2Q");

        /// <summary>
        ///     Stores all of the currently running game sessions based on room ID.
        /// </summary>
        private static Dictionary<long, GameSimulationApp> _sessions;

        private static int _lastSessionCount;

        private static void Main(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;
            _sessions = new Dictionary<long, GameSimulationApp>();

            Console.Title = me.Username;

            // Create console with title, no cursor, make CTRL-C act as input.
            Console.Title = "Oregon Trail Clone";
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
            Console.WriteLine($"Oregon Trail Clone{Environment.NewLine}Sessions: {_sessions.Count.ToString("N0")}");
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

                //if (message.Text.StartsWith("/photo")) // send a photo
                //{
                //    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);
                //    const string file = @"C:\OregonTrailBot\bin\test.png";
                //    var fileName = file.Split('\\').Last();

                //    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                //    {
                //        var fts = new FileToSend(fileName, fileStream);
                //        await Bot.SendPhotoAsync(message.Chat.Id, fts, "Nice Picture");
                //    }
                //}
            }
            else
            {
                // Skip messages that are internal mode switching or empty (populating) windows and forms.
                if (messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_DEFAULT_TUI) ||
                    messageEventArgs.Message.Text.Contains(SceneGraph.GAMEMODE_EMPTY_TUI))
                    return;

                // Tell the players what we are doing.
                await Bot.SendTextMessageAsync(message.Chat.Id,
                    $"Creating new Oregon Trail session with token: {message.Chat.Id}",
                    replyMarkup: new ReplyKeyboardHide());

                // Create a new game session using chat room ID as key in our dictionary.
                _sessions.Add(message.Chat.Id, new GameSimulationApp(message.Chat.Id));

                // Hook delegate event for knowing when that simulation is updated.
                if (_sessions.ContainsKey(message.Chat.Id))
                    _sessions[message.Chat.Id].SceneGraph.ScreenBufferDirtyEvent += SceneGraphOnScreenBufferDirtyEvent;
            }
        }

        private async static void SceneGraphOnScreenBufferDirtyEvent(string content, int[] commands, long gameID)
        {
            // Check if there are multiple commands that can be pressed (or dialog with continue only).
            if ((commands != null && commands.Length <= 0) || commands == null)
            {
                // Instruct the program that it can pass along screen buffer when it changes.
                await Bot.SendTextMessageAsync(gameID, content,
                    replyMarkup: new ReplyKeyboardHide());
            }
            else if (commands.Length > 0)
            {
                // Get half of the commands.
                var halfCommandCount = commands.Length / 2;

                // Check if less than zero.
                if (halfCommandCount <= 0)
                {
                    // Send custom keyboard.
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                                    new[] // Single commands get a continue button.
                                    {
                                        new KeyboardButton("Continue")
                                    }
                                });

                    await Bot.SendTextMessageAsync(gameID, content,
                        replyMarkup: keyboard);
                }
                else
                {
                    // First row is first half of menu options.
                    var topRow = new List<KeyboardButton>();
                    for (var i = 0; i < halfCommandCount; i++)
                        topRow.Add(new KeyboardButton(commands[i].ToDescriptionAttribute()));

                    // Second row is last half of menu options.
                    var bottomRow = new List<KeyboardButton>();
                    for (var i = halfCommandCount; i < commands.Length; i++)
                        bottomRow.Add(new KeyboardButton(commands[i].ToDescriptionAttribute()));

                    // Send custom keyboard.
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                                    topRow.ToArray(),
                                    bottomRow.ToArray()
                                });

                    // Send the message to chat room.
                    await Bot.SendTextMessageAsync(
                        gameID,
                        content,
                        replyMarkup: keyboard);
                }
            }
        }
    }
}