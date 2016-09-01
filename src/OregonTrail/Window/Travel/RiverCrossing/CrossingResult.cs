// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using OregonTrail.Form;
using OregonTrail.Form.Input;
using OregonTrail.Travel.Dialog;
using OregonTrail.Vehicle;

namespace OregonTrail.Travel.RiverCrossing
{
    /// <summary>
    ///     Displays the final crossing result for the river crossing location. No matter what choice the player made, what
    ///     events happen along the way, this final screen will be shown to let the user know how the last leg of the journey
    ///     went. It is possible to get stuck in the mud, however most of the messages are safe and just let the user know they
    ///     finally made it across.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class CrossingResult : InputForm<TravelInfo>
    {
        /// <summary>
        ///     The crossing result.
        /// </summary>
        private StringBuilder _crossingResult;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CrossingResult" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public CrossingResult(IWindow window) : base(window)
        {
            _crossingResult = new StringBuilder();
        }

        public override object MenuCommands
        {
            get { return new[] {"Ok"}; }
        }

        /// <summary>
        ///     Fired when dialog prompt is attached to active game Windows and would like to have a string returned.
        /// </summary>
        /// <returns>
        ///     The text user interface.<see cref="string" />.
        /// </returns>
        protected override string OnDialogPrompt()
        {
            // Clear any previous crossing result prompt.
            _crossingResult.Clear();

            // Depending on crossing type we will say different things about the crossing.
            switch (UserData.River.CrossingType)
            {
                case RiverCrossChoice.Ford:
                    if (UserData.Game.Random.NextBool())
                    {
                        // No loss in time, but warning to let the player know it's dangerous.
                        _crossingResult.AppendLine($"It was a muddy crossing, but you did not get stuck.{Environment.NewLine}");
                    }
                    else
                    {
                        // Triggers event for muddy shore that makes player lose a day, forces end of crossing also.
                        FinishCrossing();
                        UserData.Game.EventDirector.TriggerEvent(UserData.Game.Vehicle,
                            typeof (StuckInMud));
                    }

                    break;
                case RiverCrossChoice.Float:
                    _crossingResult.AppendLine(UserData.River.DisasterHappened
                        ? $"Your party relieved to reach other side after trouble floating across.{Environment.NewLine}"
                        : $"You had no trouble floating the wagon across.{Environment.NewLine}");

                    break;
                case RiverCrossChoice.Ferry:
                    _crossingResult.AppendLine(UserData.River.DisasterHappened
                        ? $"The ferry operator apologizes for the rough ride.{Environment.NewLine}"
                        : $"The ferry got your party and wagon safely across.{Environment.NewLine}");

                    break;
                case RiverCrossChoice.Indian:
                    _crossingResult.AppendLine(UserData.River.DisasterHappened
                        ? $"The Indian runs away as soon as you reach the shore.{Environment.NewLine}"
                        : $"The Indian helped your wagon safely across.{Environment.NewLine}");

                    break;
                case RiverCrossChoice.None:
                case RiverCrossChoice.WaitForWeather:
                case RiverCrossChoice.GetMoreInformation:
                    throw new InvalidOperationException(
                        $"Invalid river crossing result choice {UserData.River.CrossingType}.");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Render the crossing result to text user interface.
            return _crossingResult.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            FinishCrossing();
        }

        /// <summary>
        ///     Cleans up any remaining data about this river the player just crossed.
        /// </summary>
        private void FinishCrossing()
        {
            // Destroy the river data now that we are done with it.
            UserData.DestroyRiver();

            // River crossing takes you a day.
            UserData.Game.TakeTurn(false);

            // Start going there...
            SetForm(typeof (LocationDepart));
        }
    }
}