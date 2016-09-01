﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Collections.Generic;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Second panel on point information, shows how the number of resources you end the game with contribute to your final
    ///     score.
    /// </summary>
    [ParentWindow(typeof (MainMenu))]
    public sealed class PointsAwardHelp : InputForm<NewGameInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PointsAwardHelp" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public PointsAwardHelp(IWindow window) : base(window)
        {
        }

        /// <summary>
        ///     Reference to points that will be given for entities of given matching types in this list.
        /// </summary>
        private static IEnumerable<Points> ResourcePoints
        {
            get
            {
                return new List<Points>
                {
                    new Points(Resources.Person),
                    new Points(Resources.Vehicle),
                    new Points(Parts.Oxen),
                    new Points(Parts.Wheel),
                    new Points(Parts.Axle),
                    new Points(Parts.Tongue),
                    new Points(Resources.Clothing),
                    new Points(Resources.Bullets),
                    new Points(Resources.Food),
                    new Points(Resources.Cash)
                };
            }
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
            var pointsHelp = new StringBuilder();
            pointsHelp.Append($"{Environment.NewLine}On Arriving in Oregon{Environment.NewLine}{Environment.NewLine}");
            pointsHelp.Append(
                $"The resources you arrive with will{Environment.NewLine} help you get started in the new{Environment.NewLine} land. You receive points for each{Environment.NewLine} item you bring safely to Oregon.{Environment.NewLine}{Environment.NewLine}");

            // Build up the table of resource points and how they work for player.
            var partyTable = ResourcePoints.ToStringTable(
                new[] {"Resources of Party", "Points per Item"},
                u => u.ToString(),
                u => u.PointsAwarded
                );

            // Print the table of how resources earn points.
            pointsHelp.AppendLine(partyTable);
            return pointsHelp.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // parentGameMode.State = new PointsMultiplyerHelp(parentGameMode, UserData);
            SetForm(typeof (PointsMultiplyerHelp));
        }
    }
}