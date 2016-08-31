﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using OregonTrail.Form;
using OregonTrail.Form.Input;

namespace OregonTrail.MainMenu.Options
{
    /// <summary>
    ///     Confirm the player wishes to the destroy the current top ten list and reset it back to the hard-coded default
    ///     values.
    /// </summary>
    [ParentWindow(typeof (MainMenu))]
    public sealed class EraseCurrentTopTen : InputForm<NewGameInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EraseCurrentTopTen" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public EraseCurrentTopTen(IWindow window) : base(window)
        {
        }

        /// <summary>
        ///     Defines what type of dialog this will act like depending on this enumeration value. Up to implementation to define
        ///     desired behavior.
        /// </summary>
        protected override DialogType DialogType
        {
            get { return DialogType.YesNo; }
        }

        public override object MenuCommands
        {
            get { return new[] {"Yes", "No"}; }
        }

        /// <summary>
        ///     Fired when dialog prompt is attached to active game Windows and would like to have a string returned.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        protected override string OnDialogPrompt()
        {
            var eraseTopTen = new StringBuilder();

            // Text above the table to declare what this state is.
            eraseTopTen.AppendLine($"{Environment.NewLine}Erase Top Ten list{Environment.NewLine}");

            // Ask the user question if they really want to remove the top ten list.
            eraseTopTen.AppendLine($"If you erase the current Top Ten list, the names and scores will be replaced by those on the original list.{Environment.NewLine}");

            // Wait for use input...
            eraseTopTen.Append("Do you want to do this? Y/N");
            return eraseTopTen.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // Actually erase current top ten list.
            UserData.Game.Scoring.Reset();

            // Return to main menu.
            SetForm(typeof (ManagementOptions));
        }
    }
}