﻿using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OregonTrail
{
    /// <summary>
    ///     Maintains all of the data required to keep track of a session such as the chat room ID, the from ID, text user
    ///     interface, and previous messages. There are also features to control lifetime and cache invalidation so inactive
    ///     sessions can be removed from the list with the normal ticking operations and simple boolean checks.
    /// </summary>
    public sealed class BotSession
    {
        /// <summary>
        ///     Creates a new game session for the Oregon trail simulation.
        /// </summary>
        public BotSession(Message message)
        {
            // Chat ID is the conversation (such as group).
            ChatID = message.Chat.Id;

            // User is the leader of the simulation and starter of game.
            UserID = message.From.Id;

            // Figure out if this is a group chat game or a private session between two people.
            GameType = message.Chat.Type;

            // Check for first name, if none check last, else use username.
            if (!string.IsNullOrEmpty(message.From.FirstName))
                LeaderName = message.From.FirstName;
            else if (!string.IsNullOrEmpty(message.From.LastName) && string.IsNullOrEmpty(message.From.FirstName))
                LeaderName = message.From.LastName;
            else
                LeaderName = message.From.Username;

            // Used to make users reply directly to the bot by making their clients auto reply to messages from him due to being @mentioned in the message context.
            LeaderUsername = message.From.Username;

            // Previous message is always blank.
            PreviousMessage = string.Empty;

            // Actually creates the game simulation using information contained in this object so far.
            Session = new GameSimulationApp(MemberwiseClone() as BotSession);
        }

        /// <summary>
        ///     Used to make users reply directly to the bot by making their clients auto reply to messages from him due to being
        ///     @mentioned in the message context.
        /// </summary>
        public string LeaderUsername { get; set; }

        /// <summary>
        ///     Used to determine if the current session is in a group or a private session between the bot and a single user. The
        ///     differences are subtle and change how the simulation begins with users being able to /join a leader on his journey.
        /// </summary>
        public ChatType GameType { get; }

        /// <summary>
        ///     Unique ID for the chat room or private correspondence between the bot and the user talking to him. This number is
        ///     always unique to the conversation and the session that is opened with the bot to ensure that others in a group chat
        ///     for example cannot start a session after another has already been started.
        /// </summary>
        public long ChatID { get; }

        /// <summary>
        ///     Unique ID for the leader and starter of the current game session. Keyboard events are sent to this user and they
        ///     are the ones whom will be controlling the game experience while others in the room are just along for the ride.
        /// </summary>
        public int UserID { get; }

        /// <summary>
        ///     Name of the leader, typically this is the first name of the username in question.
        /// </summary>
        public string LeaderName { get; }

        /// <summary>
        ///     Contains all of the text user interface that was generated by the simulation, updated in the scene graph and then
        ///     passed along to the bot manager for controlling this session.
        /// </summary>
        public string PreviousMessage { get; }

        /// <summary>
        ///     Contains the current game session as it is known by the user whom is the leader and starter of it. They have the
        ///     ability to quit and restart the game at any time but others in the room cannot influence the simulation in anyway
        ///     only cheer the player on.
        /// </summary>
        public GameSimulationApp Session { get; }
    }
}