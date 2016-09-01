// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Shows the player their vehicle and list of all the points in the trail they could possibly travel to. It marks the
    ///     spot they are on and all the spots they have visited, shows percentage for completion and some other basic
    ///     statistics about the journey that could only be seen from this state.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class LookAtMap : InputForm<TravelInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LookAtMap" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public LookAtMap(IWindow window) : base(window)
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
            // Create visual progress representation of the trail.
            var mapPrompt = new StringBuilder();
            mapPrompt.AppendLine($"{Environment.NewLine}Trail Progress");

            // Graphical title for main menu.
            ImagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "map.jpg");

            // Build up a table of location names and if the player has visited them.
            var locationsCompleted = UserData.Game.Trail.Locations.Count(
                location => location.Status == LocationStatus.Departed);

            var locationsPercentComplete = locationsCompleted/(decimal) UserData.Game.Trail.Locations.Count;
            mapPrompt.AppendLine(locationsPercentComplete.ToString("P"));

            return mapPrompt.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // Check if current location is a fork in the road, if so we will return to that form.
            if (UserData.Game.Trail.CurrentLocation is ForkInRoad)
            {
                SetForm(typeof (LocationFork));
                return;
            }

            // Default action is to return to travel menu.
            ClearForm();
        }
    }
}