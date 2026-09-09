using System;

namespace Data
{
    /// <summary>
    /// Gold and equipment granted after a victory.
    /// </summary>
    public class LootDrop
    {
        public LootDrop(int gold, EquipmentInstance item)
        {
            Gold = Math.Max(0, gold);
            Item = item;
        }

        public int Gold { get; }

        public EquipmentInstance Item { get; }

        public bool HasItem => Item != null;

        /// <summary>
        /// Builds the result popup lines for gold and item name.
        /// </summary>
        public string FormatSummary()
        {
            if (Gold <= 0 && !HasItem)
                return "No rewards";

            if (!HasItem)
                return $"+{Gold} Gold";

            var itemLine = $"{Item.DisplayName}  Lv{Item.Level}";
            return Gold > 0 ? $"+{Gold} Gold\n{itemLine}" : itemLine;
        }
    }
}