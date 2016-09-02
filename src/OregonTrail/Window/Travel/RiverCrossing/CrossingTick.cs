﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Runs the player over the river based on the crossing information. Depending on what happens a message will be
    ///     printed to the screen explaining what happened before defaulting back to travel game Windows.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class CrossingTick : Form<TravelInfo>
    {
        /// <summary>
        ///     String builder that will hold all the data about our river crossing as it occurs.
        /// </summary>
        private StringBuilder _crossingPrompt;

        /// <summary>
        ///     Determines if this state has performed it's duties and helped get the players and their vehicle across the river.
        /// </summary>
        private bool _finishedCrossingRiver;

        /// <summary>
        ///     Defines the current amount of feet we have crossed of the river, this will tick up to the total length of the
        ///     river.
        /// </summary>
        private int _riverCrossingOfTotalWidth;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CrossingTick" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public CrossingTick(IWindow window) : base(window)
        {
            // Create the string builder for holding all our text about river crossing as it happens.
            _crossingPrompt = new StringBuilder();

            // Sets the crossing percentage to zero.
            _riverCrossingOfTotalWidth = 0;
            _finishedCrossingRiver = false;
        }

        /// <summary>
        ///     Determines if user input is currently allowed to be typed and filled into the input buffer.
        /// </summary>
        /// <remarks>Default is FALSE. Setting to TRUE allows characters and input buffer to be read when submitted.</remarks>
        public override bool InputFillsBuffer
        {
            // Input buffer is never filled because player cannot make choices here.
            get { return false; }
        }

        /// <summary>
        ///     Determines if this dialog state is allowed to receive any input at all, even empty line returns. This is useful for
        ///     preventing the player from leaving a particular dialog until you are ready or finished processing some data.
        /// </summary>
        public override bool AllowInput
        {
            get { return _finishedCrossingRiver; }
        }

        public override object MenuCommands
        {
            get
            {
                return _finishedCrossingRiver
                    ? new[] {"Finish Crossing"}
                    : null;
            }
        }

        /// <summary>
        ///     Fired after the state has been completely attached to the simulation letting the state know it can browse the user
        ///     data and other properties below it.
        /// </summary>
        public override void OnFormPostCreate()
        {
            base.OnFormPostCreate();

            // Park the vehicle if it is not somehow by now.
            UserData.Game.Vehicle.Status = VehicleStatus.Stopped;

            // Check if ferry operator wants players monies for trip across river.
            if (UserData.River.FerryCost > 0 &&
                UserData.Game.Vehicle.Inventory[Entities.Cash].TotalValue > UserData.River.FerryCost)
            {
                UserData.Game.Vehicle.Inventory[Entities.Cash].ReduceQuantity((int) UserData.River.FerryCost);

                // Clear out the cost for the ferry since it has been paid for now.
                UserData.River.FerryCost = 0;
            }

            // Check if the Indian guide wants his clothes for the trip that you agreed to.
            if (UserData.River.IndianCost > 0 &&
                UserData.Game.Vehicle.Inventory[Entities.Clothes].Quantity > UserData.River.IndianCost)
            {
                UserData.Game.Vehicle.Inventory[Entities.Clothes].ReduceQuantity(UserData.River.IndianCost);

                // Clear out the cost for the ferry since it has been paid for now.
                UserData.River.IndianCost = 0;
            }

            // Clears the string buffer for this render pass.
            _crossingPrompt.Clear();

            // Shows basic status of vehicle and total river crossing percentage.
            _crossingPrompt.AppendLine(
                $"{UserData.Game.Trail.CurrentLocation.Name}");
            _crossingPrompt.AppendLine(
                $"{UserData.Game.Time.Date}");
            _crossingPrompt.AppendLine(
                $"Weather: {UserData.Game.Trail.CurrentLocation.Weather.ToDescriptionAttribute()}");
            _crossingPrompt.AppendLine(
                $"Health: {UserData.Game.Vehicle.PassengerHealthStatus.ToDescriptionAttribute()}");
            _crossingPrompt.AppendLine(
                $"Crossing By: {UserData.River.CrossingType}");
            _crossingPrompt.AppendLine(
                $"River width: {UserData.River.RiverWidth.ToString("N0")} feet");
            //_crossingPrompt.AppendLine(
            //    $"River crossed: {_riverCrossingOfTotalWidth.ToString("N0")} feet");

            // Image of the wagon traveling over the water.
            ImagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "river.gif");
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
            return _crossingPrompt.ToString();
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

            // Stop crossing if we have finished.
            if (_finishedCrossingRiver)
                return;

            // Increment the amount we have floated over the river.
            _riverCrossingOfTotalWidth += UserData.Game.Random.Next(1, UserData.River.RiverWidth/4);

            // Check to see if we will finish crossing river before crossing more.
            if (_riverCrossingOfTotalWidth >= UserData.River.RiverWidth)
            {
                _riverCrossingOfTotalWidth = UserData.River.RiverWidth;
                _finishedCrossingRiver = true;
                return;
            }

            // River crossing will allow ticking of people, vehicle, and other important events but others like consuming food are disabled.
            UserData.Game.TakeTurn(true);

            // Attempt to throw a random event related to some failure happening with river crossing.
            switch (UserData.River.CrossingType)
            {
                case RiverCrossChoice.Ford:
                    if (UserData.River.RiverDepth > 3 && !UserData.River.DisasterHappened &&
                        _riverCrossingOfTotalWidth >= (UserData.River.RiverWidth/2))
                    {
                        UserData.River.DisasterHappened = true;
                        UserData.Game.EventDirector.TriggerEvent(UserData.Game.Vehicle, typeof (VehicleWashOut));
                    }
                    else
                    {
                        // Check that we don't flood the user twice, that is just annoying.
                        UserData.Game.EventDirector.TriggerEventByType(UserData.Game.Vehicle, EventCategory.RiverCross);
                    }

                    break;
                case RiverCrossChoice.Float:
                    if (UserData.River.RiverDepth > 5 && !UserData.River.DisasterHappened &&
                        _riverCrossingOfTotalWidth >= (UserData.River.RiverWidth/2) &&
                        UserData.Game.Random.NextBool())
                    {
                        UserData.River.DisasterHappened = true;
                        UserData.Game.EventDirector.TriggerEvent(UserData.Game.Vehicle, typeof (VehicleFloods));
                    }
                    else
                    {
                        // Check that we don't flood the user twice, that is just annoying.
                        UserData.Game.EventDirector.TriggerEventByType(UserData.Game.Vehicle, EventCategory.RiverCross);
                    }

                    break;
                case RiverCrossChoice.Ferry:
                case RiverCrossChoice.Indian:
                    UserData.Game.EventDirector.TriggerEventByType(UserData.Game.Vehicle, EventCategory.RiverCross);
                    break;
                case RiverCrossChoice.None:
                case RiverCrossChoice.WaitForWeather:
                case RiverCrossChoice.GetMoreInformation:
                    throw new InvalidOperationException(
                        $"Invalid river crossing result choice {UserData.River.CrossingType}.");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game Windows.</param>
        public override void OnInputBufferReturned(string input)
        {
            // Skip if we are still crossing the river.
            if (_riverCrossingOfTotalWidth < UserData.River.RiverWidth)
                return;

            SetForm(typeof (CrossingResult));
        }
    }
}