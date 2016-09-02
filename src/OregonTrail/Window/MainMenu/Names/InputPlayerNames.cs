// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace OregonTrail
{
    /// <summary>
    ///     Gets the name of a player for a particular index in the player name user data object. This will also offer the user
    ///     a chance to confirm their selection in another state, reset if they don't like it, and also generate a random user
    ///     name if they just press enter at the prompt for a name.
    /// </summary>
    [ParentWindow(typeof (MainMenu))]
    public sealed class InputPlayerNames : Form<NewGameInfo>
    {
        /// <summary>
        ///     References the string that makes up the question about player names and also showing previous ones that have been
        ///     entered for continuity sake.
        /// </summary>
        private StringBuilder _inputNamesHelp;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputPlayerNames" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public InputPlayerNames(IWindow window) : base(window)
        {
        }

        public override object MenuCommands
        {
            get { return new[] {"Generate Names"}; }
        }

        /// <summary>
        ///     Determines if this dialog state is allowed to receive any input at all, even empty line returns. This is useful for
        ///     preventing the player from leaving a particular dialog until you are ready or finished processing some data.
        /// </summary>
        public override bool AllowInput
        {
            get { return true; }
        }

        /// <summary>
        ///     Fired after the state has been completely attached to the simulation letting the state know it can browse the user
        ///     data and other properties below it.
        /// </summary>
        public override void OnFormPostCreate()
        {
            base.OnFormPostCreate();

            // Pass the game data to the simulation for each new game Windows state.
            UserData.Game.SetStartInfo(UserData);

            // Create string builder so we only build up this data once.
            _inputNamesHelp = new StringBuilder();

            // Add the leaders current username when we begin.
            if (UserData.PlayerNames.Count <= 0 && UserData.PlayerNameIndex <= 0)
            {
                UserData.PlayerNames.Insert(UserData.PlayerNameIndex, UserData.Game.LeaderTuple.Item1);
                UserData.PlayerNameIndex++;

                switch (UserData.Game.Session.GameType)
                {
                    case ChatType.Private:
                        _inputNamesHelp.AppendLine(
                            $"What are three names of the other members of your party? Press generate to randomly fill in names.{Environment.NewLine}");
                        break;
                    case ChatType.Group:
                    case ChatType.Channel:
                    case ChatType.Supergroup:
                        _inputNamesHelp.AppendLine(
                            $"Additional players can join the session by typing /join at this time. If the leader doesn't have enough members, press generate to fill in remaining names.{Environment.NewLine}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Only print player names if we have some to actually print.
            if (UserData.PlayerNames.Count <= 0)
                return;

            // Loop through all the player names and get their current state.
            var crewNumber = 1;

            // Loop through every player and print their name.
            for (var index = 0; index < GameSimulationApp.MaxPlayers; index++)
            {
                var name = string.Empty;
                if (index < UserData.PlayerNames.Count)
                    name = UserData.PlayerNames[index];

                // First name in list is always the leader.
                var isLeader = UserData.PlayerNames.IndexOf(name) == 0 && crewNumber == 1;
                _inputNamesHelp.AppendFormat(isLeader
                    ? $" {crewNumber} - {name} (leader){Environment.NewLine}"
                    : $" {crewNumber} - {name}{Environment.NewLine}");
                crewNumber++;
            }
        }

        /// <summary>
        ///     Returns a text only representation of the current game Windows state. Could be a statement, information, question
        ///     waiting input, etc.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string OnRenderForm()
        {
            return _inputNamesHelp.ToString();
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game Windows.</param>
        public override void OnInputBufferReturned(string input)
        {
            // If player enters empty name fill out all the slots with random ones.
            if (input.Contains("Generate Names"))
            {
                // Only fill out names for slots that are empty.
                for (var i = 0; i < (GameSimulationApp.MaxPlayers - UserData.PlayerNameIndex); i++)
                    UserData.PlayerNames.Insert(UserData.PlayerNameIndex, GetPlayerName());

                // Attach state to confirm randomized name selection, skipping manual entry with the return.
                SetForm(typeof (ConfirmPlayerNames));
                return;
            }

            // Add the name to list since we will have something at this point even if randomly generated.
            UserData.PlayerNames.Insert(UserData.PlayerNameIndex, input);
            UserData.PlayerNameIndex++;

            // Change the state to either confirm or input the next name based on index of name we are entering.
            SetForm(UserData.PlayerNameIndex < GameSimulationApp.MaxPlayers
                ? typeof (InputPlayerNames)
                : typeof (ConfirmPlayerNames));
        }

        /// <summary>
        ///     Returns a random name if there is an empty name returned, we assume the player doesn't care and just give him one.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private string GetPlayerName()
        {
            string[] names =
            {
                "Bob", "Joe", "Sally", "Tim", "Steve", "Zeke", "Suzan", "Rebekah", "Young", "Margret", "Kristy", "Bush",
                "Joanna", "Chrystal", "Gene", "Angela", "Ruthann", "Viva", "Iris", "Anderson", "Siobhan", "Trump",
                "Jolie", "Carlene", "Kerry", "Buck"
            };
            return names[UserData.Game.Random.Next(names.Length)];
        }
    }
}