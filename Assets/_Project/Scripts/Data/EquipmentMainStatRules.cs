using System;

namespace Data
{
    /// <summary>
    /// Fixed main-stat mapping by equipment slot.
    /// </summary>
    public static class EquipmentMainStatRules
    {
        /// <summary>
        /// Main stat type for the given slot.
        /// </summary>
        public static StatType GetMainStatType(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.LeftHand => StatType.Attack,
                EquipmentSlot.RightHand => StatType.Attack,
                EquipmentSlot.UpperBody => StatType.Defense,
                EquipmentSlot.LowerBody => StatType.HP,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }
    }
}