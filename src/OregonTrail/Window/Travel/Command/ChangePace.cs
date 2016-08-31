// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using OregonTrail.Form;
using OregonTrail.Travel.Dialog;
using OregonTrail.Vehicle;

namespace OregonTrail.Travel.Command
{
    /// <summary>
    ///     Allows the player to alter how many 'miles' their vehicle will attempt to travel in a given day, this also changes
    ///     the rate at which random events that are considered bad will occur along with other factors in the simulation such
    ///     as making players more susceptible to disease and also making them hungry more often.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class ChangePace : Form<TravelInfo>
    {
        /// <summary>
        ///     String builder for the changing pace text.
        /// </summary>
        private StringBuilder _pace;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChangePace" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public ChangePace(IWindow window) : base(window)
        {
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

            _pace = new StringBuilder();
            _pace.AppendLine(
                $"Change pace (currently \"{UserData.Game.Vehicle.Pace}\"){Environment.NewLine}The pace at which you travel can change. Your choices are:{Environment.NewLine}");
            _pace.AppendLine("1. a steady pace");
            _pace.AppendLine("2. a strenuous pace");
            _pace.AppendLine("3. a grueling pace");
            _pace.Append("4. find out what these different paces mean");
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
            return _pace.ToString();
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game Windows.</param>
        public override void OnInputBufferReturned(string input)
        {
            switch (input.ToUpperInvariant())
            {
                case "1":
                    UserData.Game.Vehicle.ChangePace(TravelPace.Steady);
                    ClearForm();
                    break;
                case "2":
                    UserData.Game.Vehicle.ChangePace(TravelPace.Strenuous);
                    ClearForm();
                    break;
                case "3":
                    UserData.Game.Vehicle.ChangePace(TravelPace.Grueling);
                    ClearForm();
                    break;
                case "4":
                    SetForm(typeof (PaceHelp));
                    break;
                default:
                    SetForm(typeof (ChangePace));
                    break;
            }
        }
    }
}