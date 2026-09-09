using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Rolls victory loot from a <see cref="LootTable"/>.
    /// </summary>
    public class LootService
    {
        /// <summary>
        /// Rolls loot.
        /// </summary>
        public LootDrop Roll(
            LootTable table,
            int stage,
            CharacterClass? preferredClass = null,
            System.Random random = null)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            random ??= new System.Random();
            var gold = table.RollGold(random);
            var item = TryRollItem(table, stage, preferredClass, random);
            return new LootDrop(gold, item);
        }

        /// <summary>
        /// Relative pick weight for a quality at the given stage.
        /// </summary>
        public static float GetQualityWeightMultiplier(EquipmentQuality quality, int stage)
        {
            var t = Mathf.Clamp01((Mathf.Max(1, stage) - 1) / 40f);
            return quality switch
            {
                EquipmentQuality.Common => Mathf.Lerp(1.2f, 0.35f, t),
                EquipmentQuality.Uncommon => Mathf.Lerp(0.9f, 1.0f, t),
                EquipmentQuality.Rare => Mathf.Lerp(0.25f, 1.1f, t),
                EquipmentQuality.Epic => Mathf.Lerp(0.08f, 0.7f, t),
                EquipmentQuality.Legendary => Mathf.Lerp(0.02f, 0.4f, t),
                _ => 1f
            };
        }

        private static EquipmentInstance TryRollItem(
            LootTable table,
            int stage,
            CharacterClass? preferredClass,
            System.Random random)
        {
            if (random.NextDouble() > table.EquipmentDropChance)
                return null;

            var picked = PickEntry(table, stage, preferredClass, random);
            if (picked?.Equipment == null)
                return null;

            return EquipmentInstance.CreateForStage(picked.Equipment, stage);
        }

        private static LootEntry PickEntry(
            LootTable table,
            int stage,
            CharacterClass? preferredClass,
            System.Random random)
        {
            var entries = table.Entries;
            if (entries == null || entries.Count == 0)
                return null;

            var total = 0f;
            var weights = new float[entries.Count];

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var weight = GetEffectiveWeight(entry, table, stage, preferredClass);
                weights[i] = weight;
                total += weight;
            }

            if (total <= 0f)
                return null;

            var roll = (float)(random.NextDouble() * total);
            var running = 0f;
            for (var i = 0; i < weights.Length; i++)
            {
                running += weights[i];
                if (roll <= running)
                    return entries[i];
            }

            return entries[entries.Count - 1];
        }

        private static float GetEffectiveWeight(
            LootEntry entry,
            LootTable table,
            int stage,
            CharacterClass? preferredClass)
        {
            if (entry == null || entry.Equipment == null || entry.Weight <= 0)
                return 0f;

            var weight = entry.Weight * GetQualityWeightMultiplier(entry.Equipment.Quality, stage);
            if (preferredClass.HasValue && entry.Equipment.RequiredClass == preferredClass.Value)
                weight *= table.SameClassWeightMultiplier;

            return weight;
        }
    }
}