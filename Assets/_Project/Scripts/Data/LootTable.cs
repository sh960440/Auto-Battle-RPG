using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Gold range and weighted equipment pool used after a combat victory.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable_", menuName = "Game Specific/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [SerializeField] [Min(0)] private int _minGold = 8;
        [SerializeField] [Min(0)] private int _maxGold = 16;
        [SerializeField] [Range(0f, 1f)] private float _equipmentDropChance = 0.7f;
        [SerializeField] [Min(1f)] private float _sameClassWeightMultiplier = 1.5f;
        [SerializeField] private LootEntry[] _entries = Array.Empty<LootEntry>();

        public int MinGold => Mathf.Min(_minGold, _maxGold);

        public int MaxGold => Mathf.Max(_minGold, _maxGold);

        /// <summary>
        /// Chance to roll an equipment drop in addition to gold.
        /// </summary>
        public float EquipmentDropChance => Mathf.Clamp01(_equipmentDropChance);

        /// <summary>
        /// Extra weight for items matching the character.
        /// </summary>
        public float SameClassWeightMultiplier => Mathf.Max(1f, _sameClassWeightMultiplier);

        public IReadOnlyList<LootEntry> Entries => _entries;

        /// <summary>
        /// Creates a runtime table.
        /// </summary>
        public static LootTable CreateRuntime(
            int minGold,
            int maxGold,
            float equipmentDropChance,
            params LootEntry[] entries)
        {
            var table = CreateInstance<LootTable>();
            table.name = "LootTable_Runtime";
            table._minGold = Math.Max(0, minGold);
            table._maxGold = Math.Max(0, maxGold);
            table._equipmentDropChance = Mathf.Clamp01(equipmentDropChance);
            table._sameClassWeightMultiplier = 1.5f;
            table._entries = entries ?? Array.Empty<LootEntry>();
            return table;
        }

        /// <summary>
        /// Rolls an inclusive gold amount.
        /// </summary>
        public int RollGold(System.Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var min = MinGold;
            var max = MaxGold;
            return min == max ? min : random.Next(min, max + 1);
        }
    }
}