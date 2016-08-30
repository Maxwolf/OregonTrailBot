﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using OregonTrail.Location.Point;

namespace OregonTrail.Travel.Toll
{
    /// <summary>
    ///     Generates a new toll amount and keeps track of the location to be inserted if the deal goes through with the
    ///     player. Otherwise this information will be destroyed.
    /// </summary>
    public sealed class TollGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:OregonTrail.Travel.Toll.TollGenerator" /> class.
        /// </summary>
        /// <param name="tollRoad">Location that is going to cost the player money in order to use the path to travel to it.</param>
        /// <param name="game">Simulation instance.</param>
        public TollGenerator(TollRoad tollRoad, GameSimulationApp game)
        {
            Cost = game.Random.Next(1, 13);
            Road = tollRoad;
        }

        /// <summary>
        ///     Location that is going to cost the player money in order to use the path to travel to it.
        /// </summary>
        public TollRoad Road { get; }

        /// <summary>
        ///     Gets the total toll for the cost road the player must pay before they will be allowed on the cost road.
        /// </summary>
        public int Cost { get; }
    }
}