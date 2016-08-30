// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 12/31/2015@4:49 AM

namespace OregonTrail
{
    /// <summary>
    ///     Used to make sure that every Windows info class has a basic data structure that we can rely on for creating it via
    ///     Windows factory.
    /// </summary>
    public abstract class WindowData
    {
        public GameSimulationApp Game { get; internal set; }
    }
}