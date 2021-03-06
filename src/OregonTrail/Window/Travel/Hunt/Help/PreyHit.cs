﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Called when the player successfully hits an animal with the bullet and the animal was added to growing list of
    ///     animals they have killed this hunting session.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class PreyHit : InputForm<TravelInfo>
    {
        /// <summary>
        ///     Holds the string data about what we hit with our bullets.
        /// </summary>
        private StringBuilder _hitPrompt;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputForm{T}" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public PreyHit(IWindow window) : base(window)
        {
            _hitPrompt = new StringBuilder();
        }

        public override object MenuCommands
        {
            get { return new[] {"Nice"}; }
        }

        /// <summary>
        ///     Fired when dialog prompt is attached to active game Windows and would like to have a string returned.
        /// </summary>
        /// <returns>
        ///     The dialog prompt text.<see cref="string" />.
        /// </returns>
        protected override string OnDialogPrompt()
        {
            // Get the last known target.
            var target = UserData.Hunt.LastTarget;

            // Prompt for hitting an animal.
            _hitPrompt.AppendLine(target.Animal.TotalWeight > 100
                ? $"You shot a giant {target.Animal.Name.ToLowerInvariant()}. Full bellies tonight!{Environment.NewLine}"
                : $"You shot a {target.Animal.Name.ToLowerInvariant()}.{Environment.NewLine}");

            // Returns the hit message to the text renderer.
            return _hitPrompt.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            SetForm(typeof (Hunting));
        }
    }
}