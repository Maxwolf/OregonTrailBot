﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Information about what fording a river means and how it works for the player vehicle and their party members.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class FordRiverHelp : InputForm<TravelInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FordRiverHelp" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public FordRiverHelp(IWindow window) : base(window)
        {
        }

        public override object MenuCommands
        {
            get { return new[] {"Ok"}; }
        }

        /// <summary>
        ///     Fired when dialog prompt is attached to active game Windows and would like to have a string returned.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        protected override string OnDialogPrompt()
        {
            var fordRiver = new StringBuilder();
            fordRiver.AppendLine(
                $"To ford a river means to pull your wagon across a shallow part of the river, with the oxen still attached.{Environment.NewLine}");
            return fordRiver.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            SetForm(typeof (CaulkRiverHelp));
        }
    }
}