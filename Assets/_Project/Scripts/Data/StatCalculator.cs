using System;
using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// Combines base stats, level bonuses, and equipped main stats into a final StatBlock.
    /// </summary>
    public static class StatCalculator
    {
        /// <summary>
        /// Final stats for a character.
        /// </summary>
        public static StatBlock Calculate(CharacterInstance character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            return Calculate(
                character.BaseStats,
                character.Level,
                character.Definition.UpgradeCurve,
                EnumerateEquipped(character));
        }

        /// <summary>
        /// Final stats from explicit inputs.
        /// </summary>
        public static StatBlock Calculate(
            StatBlock baseStats,
            int level,
            UpgradeCurve upgradeCurve,
            IEnumerable<EquipmentInstance> equipment)
        {
            var result = baseStats;

            if (upgradeCurve != null)
                result += upgradeCurve.GetTotalStatBonusAtLevel(level);

            if (equipment == null)
                return result;

            foreach (var item in equipment)
            {
                if (item == null)
                    continue;

                result = Add(result, item.MainStatType, item.GetMainStat());
            }

            return result;
        }

        private static IEnumerable<EquipmentInstance> EnumerateEquipped(CharacterInstance character)
        {
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (character.TryGetEquipment(slot, out var item))
                    yield return item;
            }
        }

        private static StatBlock Add(StatBlock block, StatType type, int amount)
        {
            block.Set(type, block.Get(type) + amount);
            return block;
        }
    }
}