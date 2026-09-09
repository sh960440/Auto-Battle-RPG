using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// One equipment option in a loot table, with a relative drop weight.
    /// </summary>
    [Serializable]
    public class LootEntry
    {
        [SerializeField] private EquipmentDefinition _equipment;
        [SerializeField] [Min(0)] private int _weight = 1;

        public EquipmentDefinition Equipment => _equipment;

        public int Weight => _weight;

        /// <summary>
        /// Creates an entry for tests or runtime tables.
        /// </summary>
        public static LootEntry Create(EquipmentDefinition equipment, int weight = 1)
        {
            return new LootEntry
            {
                _equipment = equipment,
                _weight = Math.Max(0, weight)
            };
        }
    }
}