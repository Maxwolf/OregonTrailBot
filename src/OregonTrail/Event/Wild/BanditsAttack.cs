// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Deals with a random event that involves strangers approaching your vehicle. Once they do this the player is given
    ///     several choices about what they would like to do, they can attack them, try to outrun them, or circle the vehicle
    ///     around them to try and get them to leave.
    /// </summary>
    [DirectorEvent(EventCategory.Wild)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class BanditsAttack : ItemDestroyer
    {
        /// <summary>Fired by the item destroyer event prefab before items are destroyed.</summary>
        /// <param name="destroyedItems">Items that were destroyed from the players inventory.</param>
        /// <param name="game"></param>
        /// <returns>The <see cref="string" />.</returns>
        protected override string OnPostDestroyItems(IDictionary<Entities, int> destroyedItems, GameSimulationApp game)
        {
            // Ammo used to kill the bandits is randomly generated.
            game.Vehicle.Inventory[Entities.Ammo].ReduceQuantity(game.Random.Next(3, 15));

            // Change event text depending on if items were destroyed or not.
            return destroyedItems.Count > 0
                ? TryKillPassengers("murdered", game)
                : "no loss of items. You drove them off!";
        }

        /// <summary>
        ///     Fired by the item destroyer event prefab after items are destroyed.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        protected override string OnPreDestroyItems()
        {
            var firePrompt = new StringBuilder();
            firePrompt.Clear();
            firePrompt.AppendLine("Bandits attack!");
            firePrompt.Append("Resulting in ");
            return firePrompt.ToString();
        }
    }
}