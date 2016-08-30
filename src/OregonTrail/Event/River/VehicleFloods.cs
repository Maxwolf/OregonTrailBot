// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.Collections.Generic;
using System.Text;
using OregonTrail.Director;
using OregonTrail.Prefab;
using OregonTrail.RandomEvent;

namespace OregonTrail.River
{
    /// <summary>
    ///     When crossing a river there is a chance that your wagon will flood if you choose to caulk and float across the
    ///     river.
    /// </summary>
    [DirectorEvent(EventCategory.RiverCross, EventExecution.ManualOnly)]
    public sealed class VehicleFloods : ItemDestroyer
    {
        /// <summary>Fired by the item destroyer event prefab before items are destroyed.</summary>
        /// <param name="destroyedItems">Items that were destroyed from the players inventory.</param>
        /// <param name="game"></param>
        /// <returns>The <see cref="string" />.</returns>
        protected override string OnPostDestroyItems(IDictionary<Entities, int> destroyedItems, GameSimulationApp game)
        {
            return destroyedItems.Count > 0
                ? TryKillPassengers("drowned", game)
                : "no loss of items.";
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
            vehicle?.ReduceMileage(20 - 20*eventExecutor.Game.Random.Next());
        }

        /// <summary>
        ///     Fired by the item destroyer event prefab after items are destroyed.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        protected override string OnPreDestroyItems()
        {
            var floodPrompt = new StringBuilder();
            floodPrompt.Clear();
            floodPrompt.AppendLine("Vehicle floods");
            floodPrompt.AppendLine("while crossing the");
            floodPrompt.Append("river results in");
            return floodPrompt.ToString();
        }
    }
}