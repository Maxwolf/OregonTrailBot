// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Shows the player information about what the various starting months mean.
    /// </summary>
    [ParentWindow(typeof (MainMenu))]
    public sealed class StartMonthHelp : InputForm<NewGameInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="StartMonthHelp" /> class.</summary>
        /// <param name="window">The window.</param>
        public StartMonthHelp(IWindow window) : base(window)
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
            // Inform the user about a decision they need to make.
            var startMonth = new StringBuilder();
            startMonth.AppendLine(
                $"You attend a public meeting held for \"folks with the California - Oregon fever.\" You're told:{Environment.NewLine}");
            startMonth.AppendLine(
                "If you leave too early, there won't be any grass for your oxen to eat. If you leave too late, you may not get to Oregon before winter comes. If you leave at just the right time, there will be green grass and the weather will still be cool.");
            return startMonth.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // parentGameMode.State = new SelectStartingMonthState(parentGameMode, UserData);
            SetForm(typeof (SelectStartingMonthState));
        }
    }
}