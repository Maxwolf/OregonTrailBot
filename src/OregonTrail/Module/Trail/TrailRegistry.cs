// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.Collections.Generic;

namespace OregonTrail
{
    /// <summary>
    ///     Complete trails the player can travel on using the simulation. Some are remakes and others new.
    /// </summary>
    public class TrailRegistry
    {
        private GameSimulationApp _game;

        public TrailRegistry(GameSimulationApp game)
        {
            _game = game;
        }

        /// <summary>
        ///     Original Oregon trail which was in the 1986 Apple II version of the game.
        /// </summary>
        public Trail OregonTrail()
        {
            var oregonTrail = new Location[]
            {
                new Settlement("Independence", Climate.Moderate, _game),
                new RiverCrossing("Kansas River Crossing", Climate.Continental, _game, RiverOption.FerryOperator),
                new RiverCrossing("Big Blue River Crossing", Climate.Continental, _game),
                new Settlement("Fort Kearney", Climate.Continental, _game),
                new Landmark("Chimney Rock", Climate.Moderate, _game),
                new Settlement("Fort Laramie", Climate.Moderate, _game),
                new Landmark("Independence Rock", Climate.Moderate, _game),
                new ForkInRoad("South Pass", Climate.Dry, new List<Location>
                {
                    new Settlement("Fort Bridger", Climate.Dry, _game),
                    new Landmark("Green River Shortcut", Climate.Dry, _game)
                }, _game),
                new RiverCrossing("Green River Crossing", Climate.Dry, _game),
                new Landmark("Soda Springs", Climate.Dry, _game),
                new Settlement("Fort Hall", Climate.Moderate, _game),
                new RiverCrossing("Snake River Crossing", Climate.Moderate, _game, RiverOption.IndianGuide),
                new Settlement("Fort Boise", Climate.Polar, _game),
                new ForkInRoad("Blue Mountains", Climate.Polar, new List<Location>
                {
                    new Settlement("Fort Walla Walla", Climate.Polar, _game),
                    new ForkInRoad("The Dalles", Climate.Polar, new List<Location>
                    {
                        new RiverCrossing("Columbia River", Climate.Moderate, _game),
                        new TollRoad("Barlow Toll Road", Climate.Moderate, _game)
                    }, _game)
                }, _game),
                new Settlement("Oregon City", Climate.Moderate, _game)
            };

            return new Trail(oregonTrail, 32, 164, _game);
        }

        /// <summary>
        ///     Debugging and testing trail that is used to quickly iterate over the different location types.
        /// </summary>
        public Trail TestTrail()
        {
            var testTrail = new Location[]
            {
                new Settlement("Start Settlement", Climate.Moderate, _game),
                new ForkInRoad("Fork In Road", Climate.Polar, new List<Location>
                {
                    new Settlement("Inserted Settlement", Climate.Polar, _game),
                    new ForkInRoad("Inserted Fork In Road", Climate.Polar, new List<Location>
                    {
                        new RiverCrossing("Inserted River Crossing (default)", Climate.Moderate, _game),
                        new TollRoad("Inserted Toll Road", Climate.Moderate, _game)
                    }, _game)
                }, _game),
                new Landmark("Landmark", Climate.Dry, _game),
                new TollRoad("Toll Road", Climate.Moderate, _game),
                new RiverCrossing("River Crossing (with ferry)", Climate.Continental, _game, RiverOption.FerryOperator),
                new RiverCrossing("River Crossing (with Indian)", Climate.Continental, _game, RiverOption.IndianGuide),
                new Settlement("End Settlement", Climate.Moderate, _game)
            };

            return new Trail(testTrail, 50, 100, _game);
        }

        /// <summary>
        ///     Debugging trail for quickly getting to the end of the game for points tabulation and high-score tests.
        /// </summary>
        public Trail WinTrail()
        {
            var testPoints = new Location[]
            {
                new Settlement("Start Of Test", Climate.Moderate, _game),
                new Settlement("End Of Test", Climate.Dry, _game)
            };

            return new Trail(testPoints, 50, 100, _game);
        }

        /// <summary>
        ///     Debugging trail for quickly drowning the player and killing them off so tombstones and epitaphs can be tested.
        /// </summary>
        public Trail FailTrail()
        {
            var testFail = new Location[]
            {
                new Settlement("Start Of Test", Climate.Moderate, _game),
                new RiverCrossing("Wolf River Crossing", Climate.Continental, _game, RiverOption.IndianGuide),
                new RiverCrossing("Fox River Crossing", Climate.Moderate, _game, RiverOption.IndianGuide),
                new RiverCrossing("Otter River Crossing", Climate.Tropical, _game, RiverOption.FerryOperator),
                new RiverCrossing("Coyote River Crossing", Climate.Polar, _game),
                new RiverCrossing("Deer River Crossing", Climate.Continental, _game),
                new RiverCrossing("End Of Test", Climate.Dry, _game)
            };

            return new Trail(testFail, 50, 100, _game);
        }
    }
}