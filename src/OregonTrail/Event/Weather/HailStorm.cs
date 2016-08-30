// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.Collections.Generic;
using System.Text;
using OregonTrail.Director;
using OregonTrail.Prefab;
using OregonTrail.RandomEvent;

namespace OregonTrail.Weather
{
    /// <summary>
    ///     Bad hail storm damages supplies, this uses the item destroyer prefab like the river crossings do.
    /// </summary>
    [DirectorEvent(EventCategory.Weather, EventExecution.ManualOnly)]
    public sealed class HailStorm : ItemDestroyer
    {
        /// <summary>Fired by the item destroyer event prefab before items are destroyed.</summary>
        /// <param name="destroyedItems">All the items which have been destroyed.</param>
        /// <param name="game">Simulation instance.</param>
        protected override string OnPostDestroyItems(IDictionary<Entities, int> destroyedItems, GameSimulationApp game)
        {
            // Check if there are enough clothes to keep people warm, need two sets of clothes for every person.
            return game.Vehicle.Inventory[Entities.Clothes].Quantity >= (game.Vehicle.PassengerLivingCount*2) &&
                   destroyedItems.Count < 0
                ? "no loss of items."
                : TryKillPassengers("frozen", game);
        }

        /// <summary>
        ///     Fired when the event handler associated with this enum type triggers action on target entity. Implementation is
        ///     left completely up to handler.
        /// </summary>
        /// <param name="eventExecutor">
        ///     Entities which the event is going to directly affect. This way there is no confusion about
        ///     what entity the event is for. Will require casting to correct instance type from interface instance.
        /// </param>
        public override void Execute(RandomEventInfo eventExecutor)
        {
            base.Execute(eventExecutor);

            // Cast the source entity as vehicle.
            var vehicle = eventExecutor.SourceEntity as Vehicle.Vehicle;

            // Reduce the total possible mileage of the vehicle this turn.
            vehicle?.ReduceMileage(vehicle.Mileage - 5 - eventExecutor.Game.Random.Next()*10);
        }

        /// <summary>
        ///     Fired by the item destroyer event prefab after items are destroyed.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        protected override string OnPreDestroyItems()
        {
            var hailPrompt = new StringBuilder();
            hailPrompt.Clear();
            hailPrompt.AppendLine("Severe hail storm");
            hailPrompt.Append("results in");
            return hailPrompt.ToString();
        }
    }
}