// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Shows information about what the player leader professions mean and how it affects the party, vehicle, game
    ///     difficulty, and scoring at the end (if they make it).
    /// </summary>
    [ParentWindow(typeof (MainMenu))]
    public sealed class ProfessionHelp : InputForm<NewGameInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="ProfessionHelp" /> class.</summary>
        /// <param name="window">The window.</param>
        public ProfessionHelp(IWindow window) : base(window)
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
            // Information about professions and how they work.
            var jobInfo = new StringBuilder();
            jobInfo.AppendLine($"{Environment.NewLine}Traveling to Oregon isn't easy!{Environment.NewLine}");
            jobInfo.Append(
                $"But if you're a banker, you'll have more money for supplies and services than a carpenter or a farmer.{Environment.NewLine}");

            jobInfo.AppendLine(
                $"However, the harder you have to try, the more points you deserve! Therefore, the farmer earns the greatest number of points and the banker earns the least.{Environment.NewLine}{Environment.NewLine}");
            return jobInfo.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // parentGameMode.State = new ProfessionSelector(parentGameMode, UserData);
            SetForm(typeof (ProfessionSelector));
        }
    }
}