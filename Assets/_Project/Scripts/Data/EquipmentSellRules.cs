using System;

namespace Data
{
    /// <summary>
    /// Sell price rules for equipment.
    /// </summary>
    public static class EquipmentSellRules
    {
        /// <summary>
        /// Sell price for <paramref name="item"/>.
        /// </summary>
        public static int GetSellPrice(EquipmentInstance item)
        {
            if (item == null)
                return 0;

            return GetQualityMultiplier(item.Quality) * Math.Max(1, item.Level);
        }

        /// <summary>
        /// Quality multipliers for equipment.
        /// </summary>
        public static int GetQualityMultiplier(EquipmentQuality quality)
        {
            return quality switch
            {
                EquipmentQuality.Common => 1,
                EquipmentQuality.Uncommon => 2,
                EquipmentQuality.Rare => 3,
                EquipmentQuality.Epic => 5,
                EquipmentQuality.Legendary => 8,
                _ => 1
            };
        }
    }
}