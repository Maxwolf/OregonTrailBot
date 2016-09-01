﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using OregonTrail.Form;
using OregonTrail.Form.Input;
using OregonTrail.Travel.Rest;
using OregonTrail.Vehicle;

namespace OregonTrail.Travel.RiverCrossing.Ferry
{
    /// <summary>
    ///     Explains to the user how many monies and days they will be charged to cross the river using the ferry and to
    ///     confirm by saying yes. At this point the simulation will check if they have enough money or not and jump to the
    ///     next state accordingly.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class UseFerryConfirm : InputForm<TravelInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UseFerryConfirm" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public UseFerryConfirm(IWindow window) : base(window)
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
        ///     The text user interface. <see cref="string" />.
        /// </returns>
        protected override string OnDialogPrompt()
        {
            var ferryConfirm = new StringBuilder();
            ferryConfirm.AppendLine(
                $"The ferry operator says that he will charge you {UserData.River.FerryCost.ToString("C2")} and that you will have to wait {UserData.River.FerryDelayInDays} days. Are you willing to do this?");
            return ferryConfirm.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // Player has enough money for ferry operator, and there is no more delay we can cross now.
            switch (reponse)
            {
                case DialogResponse.Yes:
                    if (UserData.River.FerryCost >=
                        UserData.Game.Vehicle.Inventory[Entities.Cash].TotalValue)
                    {
                        // Tell the player they do not have enough money to cross the river using the ferry.
                        SetForm(typeof (FerryNoMonies));
                        return;
                    }

                    // Check if the ferry operator wants player to wait a certain amount of days before they can cross.
                    if (UserData.River.FerryDelayInDays > 0)
                    {
                        UserData.Game.Vehicle.Status = VehicleStatus.Stopped;
                        UserData.DaysToRest = UserData.River.FerryDelayInDays;
                        SetForm(typeof (Resting));
                        return;
                    }

                    SetForm(typeof (CrossingTick));
                    break;
                case DialogResponse.No:
                case DialogResponse.Custom:
                    UserData.River.CrossingType = RiverCrossChoice.None;
                    SetForm(typeof (RiverCross));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reponse), reponse, null);
            }
        }
    }
}