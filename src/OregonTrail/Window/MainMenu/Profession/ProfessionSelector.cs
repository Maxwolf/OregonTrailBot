﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OregonTrail.Form;
using OregonTrail.MainMenu.Names;

namespace OregonTrail.MainMenu.Profession
{
    /// <summary>
    ///     Facilitates the ability for a user to select a given profession for the party leader. This will determine the
    ///     starting amount of money their party has access to when purchasing starting items for the journey on the trail path
    ///     simulation.
    /// </summary>
    [ParentWindow(typeof (MainMenu))]
    public sealed class ProfessionSelector : Form<NewGameInfo>
    {
        /// <summary>
        ///     References the string for the profession selection so it is only constructed once.
        /// </summary>
        private StringBuilder _professionChooser;

        private List<Person.Profession> _professions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfessionSelector" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public ProfessionSelector(IWindow window) : base(window)
        {
        }

        /// <summary>
        ///     Determines if user input is currently allowed to be typed and filled into the input buffer.
        /// </summary>
        /// <remarks>Default is FALSE. Setting to TRUE allows characters and input buffer to be read when submitted.</remarks>
        public override bool InputFillsBuffer
        {
            get { return true; }
        }

        public override object MenuCommands
        {
            get { return new[] {"1", "2", "3", "4"}; }
        }

        /// <summary>
        ///     Fired after the state has been completely attached to the simulation letting the state know it can browse the user
        ///     data and other properties below it.
        /// </summary>
        public override void OnFormPostCreate()
        {
            base.OnFormPostCreate();

            // Set the profession to default value in case we are retrying this.
            UserData.PlayerProfession = Person.Profession.Banker;
            UserData.StartingMonies = 1600;

            // Pass the game data to the simulation for each new game Windows state.
            UserData.Game.SetStartInfo(UserData);

            // String builder that will hold our representation of all possible professions player can choose from.
            _professionChooser = new StringBuilder();
            _professionChooser.AppendLine($"{Environment.NewLine}Many kinds of people made the");
            _professionChooser.AppendLine($"trip to Oregon.{Environment.NewLine}");
            _professionChooser.AppendLine($"You may:{Environment.NewLine}");

            // Loop through all the profession enumeration values and grab their description attribute for selection purposes.
            _professions =
                new List<Person.Profession>(Enum.GetValues(typeof (Person.Profession)).Cast<Person.Profession>());
            for (var index = 0; index < _professions.Count; index++)
            {
                // Get the current profession choice enumeration value we casted into list.
                var professionChoice = _professions[index];

                // Last line should not print new line.
                if (index == (_professions.Count - 1))
                {
                    _professionChooser.AppendLine(
                        $"  {(int) professionChoice}. {professionChoice.ToDescriptionAttribute()}");
                    _professionChooser.AppendLine($"  {_professions.Count + 1}. Find out the differences");
                    _professionChooser.Append("     between these choices");
                }
                else
                {
                    _professionChooser.AppendLine(
                        $"  {(int) professionChoice}. {professionChoice.ToDescriptionAttribute()}");
                }
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
            return _professionChooser.ToString();
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game Windows.</param>
        public override void OnInputBufferReturned(string input)
        {
            // Skip if the input is null or empty.
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                return;

            // Attempt to cast string to enum value, can be characters or integer.
            Person.Profession professionChoice;
            Enum.TryParse(input, out professionChoice);

            // Once a profession is selected, we need to confirm that is what the user wanted.
            switch (professionChoice)
            {
                case Person.Profession.Banker:
                    UserData.PlayerProfession = Person.Profession.Banker;
                    UserData.StartingMonies = 1600;
                    UserData.PlayerNameIndex = 0;
                    SetForm(typeof (InputPlayerNames));
                    break;
                case Person.Profession.Carpenter:
                    UserData.PlayerProfession = Person.Profession.Carpenter;
                    UserData.StartingMonies = 800;
                    UserData.PlayerNameIndex = 0;
                    SetForm(typeof (InputPlayerNames));
                    break;
                case Person.Profession.Farmer:
                    UserData.PlayerProfession = Person.Profession.Farmer;
                    UserData.StartingMonies = 400;
                    UserData.PlayerNameIndex = 0;
                    SetForm(typeof (InputPlayerNames));
                    break;
                default:
                    UserData.PlayerProfession = Person.Profession.Banker;
                    UserData.StartingMonies = 1600;
                    UserData.PlayerNameIndex = 0;
                    SetForm(typeof (ProfessionHelp));
                    break;
            }
        }
    }
}