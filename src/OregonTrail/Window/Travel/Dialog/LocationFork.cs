// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OregonTrail.Form;
using OregonTrail.Location.Point;
using OregonTrail.Travel.Command;
using OregonTrail.Travel.Toll;

namespace OregonTrail.Travel.Dialog
{
    /// <summary>
    ///     Defines a location that has the player make a choice about the next location they want to travel to, it is not a
    ///     linear choice and depends on the player telling the simulation which way to fork down the path. The decisions are
    ///     pear shaped in the sense any fork will eventually lead back to the same path.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class LocationFork : Form<TravelInfo>
    {
        /// <summary>
        ///     Holds representation of the fork in the road as a decision for the player to make.
        /// </summary>
        private StringBuilder _forkPrompt;

        /// <summary>
        ///     Defines the skip choices as they will be selected from the fork form. The purpose for this is because we want the
        ///     index for selecting them to start at one not zero.
        /// </summary>
        private Dictionary<int, Location.Location> _skipChoices;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocationFork" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public LocationFork(IWindow window) : base(window)
        {
            _forkPrompt = new StringBuilder();
        }

        public override object MenuCommands
        {
            get
            {
                var menuList = new List<string>();
                for (var i = 0; i < _skipChoices.Count; i++)
                    menuList.Add((i + 1).ToString());

                return menuList.ToArray();
            }
        }

        /// <summary>
        ///     Fired after the state has been completely attached to the simulation letting the state know it can browse the user
        ///     data and other properties below it.
        /// </summary>
        public override void OnFormPostCreate()
        {
            base.OnFormPostCreate();

            // Cast the current location as a fork in the road.
            var forkInRoad = UserData.Game.Trail.CurrentLocation as ForkInRoad;
            if (forkInRoad == null)
                throw new InvalidCastException("Unable to cast current location to fork in the road.");

            // Create a dictionary that represents all the choices with index starting at one not zero.
            _skipChoices = new Dictionary<int, Location.Location>();
            for (var index = 0; index < forkInRoad.SkipChoices.Count; index++)
            {
                var skipChoice = forkInRoad.SkipChoices[index];
                _skipChoices.Add(index + 1, skipChoice);
            }
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
            // Clear the string builder and being building a new fork in the road based on current location skip choices.
            _forkPrompt.Clear();
            _forkPrompt.AppendLine($"{Environment.NewLine}The trail divides here. You may:{Environment.NewLine}");

            foreach (var skipChoice in _skipChoices)
            {
                // Last line should not print new line.
                if (skipChoice.Key == _skipChoices.Last().Key)
                {
                    // Final skip choice and special option normally done when sizing up situation.
                    _forkPrompt.AppendLine($"  {skipChoice.Key}. head for {skipChoice.Value.Name}");
                    _forkPrompt.Append($"  {skipChoice.Key + 1}. see the map");
                }
                else
                {
                    // Standard skip location entry for the list.
                    _forkPrompt.AppendLine($"  {skipChoice.Key}. head for {skipChoice.Value.Name}");
                }
            }

            // Rendering of the fork in the road as text user interface.
            return _forkPrompt.ToString();
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game mode.</param>
        public override void OnInputBufferReturned(string input)
        {
            // Parse the user input buffer as integer.
            int parsedInputNumber;
            if (!int.TryParse(input, out parsedInputNumber))
                return;

            // Number must be greater than zero.
            if (parsedInputNumber <= 0)
                return;

            // Dictionary of skip choices must contain key with input number.
            if (_skipChoices.ContainsKey(parsedInputNumber))
            {
                // Check if the selected fork is a toll road (that changes things).
                var tollRoad = _skipChoices[parsedInputNumber] as TollRoad;
                if (tollRoad != null)
                {
                    // Creates a toll and adds location we would like to fork to.
                    UserData.GenerateToll(tollRoad);
                    SetForm(typeof (TollRoadQuestion));
                }
                else
                {
                    // Insert the skip location into location list after current location.
                    UserData.Game.Trail.InsertLocation(_skipChoices[parsedInputNumber]);

                    // Start going there...
                    SetForm(typeof (LocationDepart));
                }
            }
            else
            {
                // Invalid selection will result in looking at the map screen.
                SetForm(typeof (LookAtMap));
            }
        }
    }
}