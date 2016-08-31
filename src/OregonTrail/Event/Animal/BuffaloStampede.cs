﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using OregonTrail.Director;
using OregonTrail.Prefab;

namespace OregonTrail.Animal
{
    /// <summary>
    ///     The buffalo stampede by the vehicle and can destroy items and trample people to death.
    /// </summary>
    [DirectorEvent(EventCategory.Animal)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class BuffaloStampede : ItemDestroyer
    {
        /// <summary>Fired by the item destroyer event prefab before items are destroyed.</summary>
        /// <param name="destroyedItems">Items that were destroyed from the players inventory.</param>
        /// <param name="game"></param>
        /// <returns>The <see cref="string" />.</returns>
        protected override string OnPostDestroyItems(IDictionary<Entities, int> destroyedItems, GameSimulationApp game)
        {
            // Change event text depending on if items were destroyed or not.
            return destroyedItems.Count > 0
                ? TryKillPassengers("trampled", game)
                : "no loss of items.";
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
            firePrompt.AppendLine("Buffalo stampede!");
            firePrompt.Append("Resulting in ");
            return firePrompt.ToString();
        }
    }
}