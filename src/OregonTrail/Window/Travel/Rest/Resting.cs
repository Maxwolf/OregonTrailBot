﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Keeps track of a set number of days and every time the game Windows is ticked a day is simulated and days to rest
    ///     subtracted until we are at zero, then the player can close the window but until then input will not be accepted.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class Resting : Form<TravelInfo>
    {
        /// <summary>
        ///     References the number of days the player has reset, this ticks up each time we rest a day and will be used for
        ///     display purposes to user.
        /// </summary>
        private int _daysRested;

        /// <summary>
        ///     Holds the message that is printed to the text renderer for debugging about the number of days rested.
        /// </summary>
        private StringBuilder _restMessage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Resting" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public Resting(IWindow window) : base(window)
        {
            _restMessage = new StringBuilder();
        }

        /// <summary>
        ///     Determines if user input is currently allowed to be typed and filled into the input buffer.
        /// </summary>
        /// <remarks>Default is FALSE. Setting to TRUE allows characters and input buffer to be read when submitted.</remarks>
        public override bool InputFillsBuffer
        {
            get { return false; }
        }

        public override object MenuCommands
        {
            get { return UserData.DaysToRest <= 0 ? new[] {"Ok"} : null; }
        }

        /// <summary>
        ///     Called when the simulation is ticked by underlying operating system, game engine, or potato. Each of these system
        ///     ticks is called at unpredictable rates, however if not a system tick that means the simulation has processed enough
        ///     of them to fire off event for fixed interval that is set in the core simulation by constant in milliseconds.
        /// </summary>
        /// <remarks>Default is one second or 1000ms.</remarks>
        /// <param name="systemTick">
        ///     TRUE if ticked unpredictably by underlying operating system, game engine, or potato. FALSE if
        ///     pulsed by game simulation at fixed interval.
        /// </param>
        /// <param name="skipDay">
        ///     Determines if the simulation has force ticked without advancing time or down the trail. Used by
        ///     special events that want to simulate passage of time without actually any actual time moving by.
        /// </param>
        public override void OnTick(bool systemTick, bool skipDay)
        {
            base.OnTick(systemTick, skipDay);

            // Skip system ticks.
            if (systemTick)
                return;

            // Not ticking when days to rest is zero.
            if (UserData.DaysToRest <= 0)
                return;

            // Check if we are at a river crossing and need to subtract from ferry days also.
            if (UserData.River != null &&
                UserData.River.FerryDelayInDays > 0 &&
                UserData.Game.Trail.CurrentLocation is RiverCrossing)
                UserData.River.FerryDelayInDays--;

            // Decrease number of days needed to rest, increment number of days rested.
            UserData.DaysToRest--;

            // Increment the number of days reset.
            _daysRested++;

            // Simulate the days to rest in time and event system, this will trigger random event game Windows if required.
            UserData.Game.TakeTurn(false);
        }

        /// <summary>
        ///     Fired after the state has been completely attached to the simulation letting the state know it can browse the user
        ///     data and other properties below it.
        /// </summary>
        public override void OnFormPostCreate()
        {
            base.OnFormPostCreate();

            // Only change the vehicle status to stopped if it is moving, it could just be stuck.
            if (UserData.Game.Vehicle.Status == VehicleStatus.Moving)
                UserData.Game.Vehicle.Status = VehicleStatus.Stopped;
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
            // String that holds message about resting, it can change depending on location.
            _restMessage.Clear();

            // Change up resting prompt depending on location category to give it some context.
            if (UserData.Game.Trail.CurrentLocation is ForkInRoad)
            {
                if (_daysRested > 1)
                    _restMessage.AppendLine($"You camp near the river for {_daysRested.ToString("N0")} days.");
                else if (_daysRested == 1)
                    _restMessage.AppendLine("You camp near the river for a day.");
                //else if (_daysRested <= 0)
                //    _restMessage.AppendLine("Preparing to camp near the river...");
            }
            else
            {
                if (_daysRested > 1)
                    _restMessage.AppendLine($"You rest for {_daysRested.ToString("N0")} days");
                else if (_daysRested == 1)
                    _restMessage.AppendLine("You rest for a day.");
                //else if (_daysRested <= 0)
                //    _restMessage.AppendLine("Preparing to rest...");
            }

            // Prints out the message about resting for however long this cycle was.
            return _restMessage.ToString();
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game Windows.</param>
        public override void OnInputBufferReturned(string input)
        {
            // Figure out what to do with response.
            if (_daysRested > 0)
                StopResting();
        }

        /// <summary>
        ///     Forces the resting period to end and days to rest reset to zero even if there was time remaining.
        /// </summary>
        private void StopResting()
        {
            // Determine if we should bounce back to travel menu or some special Windows.
            if (UserData.River == null)
            {
                ClearForm();
                return;
            }

            // Check if we have already departed from current location, so we just return to travel menu.
            if (UserData.Game.Trail.CurrentLocation.ArrivalFlag &&
                UserData.Game.Trail.CurrentLocation.Status == LocationStatus.Departed)
            {
                ClearForm();
                return;
            }

            // Locations can return to a special state if required based on the category of the location.
            if (UserData.Game.Trail.CurrentLocation is Landmark ||
                UserData.Game.Trail.CurrentLocation is Settlement)
            {
                ClearForm();
            }
            else if (UserData.Game.Trail.CurrentLocation is RiverCrossing)
            {
                UserData.DaysToRest = 0;

                // Player might be crossing a river, so we check if they made a decision and are waiting for ferry operator.
                if (UserData.River != null &&
                    UserData.River.CrossingType == RiverCrossChoice.Ferry &&
                    UserData.River.FerryDelayInDays <= 0 &&
                    UserData.River.FerryCost >= 0)
                {
                    // If player was waiting for ferry operator to let them cross we will jump right to that.
                    SetForm(typeof (CrossingTick));
                }
                else
                {
                    // Alternative is player just was waiting for weather.
                    SetForm(typeof (RiverCross));
                }
            }
            else if (UserData.Game.Trail.CurrentLocation is ForkInRoad)
            {
                SetForm(typeof (LocationFork));
            }
        }
    }
}