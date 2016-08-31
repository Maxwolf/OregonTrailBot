﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using OregonTrail.Form;
using OregonTrail.Form.Input;

namespace OregonTrail.Travel.RiverCrossing.Ferry
{
    /// <summary>
    ///     The ferry help.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class FerryHelp : InputForm<TravelInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FerryHelp" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public FerryHelp(IWindow window) : base(window)
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
            var _prompt = new StringBuilder();
            _prompt.AppendLine($"{Environment.NewLine}To use a ferry means to put");
            _prompt.AppendLine("your wagon on top of a flat");
            _prompt.AppendLine("boat that belongs to someone");
            _prompt.AppendLine("else. The owner of the");
            _prompt.AppendLine("ferry will take your wagon");
            _prompt.AppendLine($"across the river.{Environment.NewLine}");
            return _prompt.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            SetForm(typeof (RiverCross));
        }
    }
}