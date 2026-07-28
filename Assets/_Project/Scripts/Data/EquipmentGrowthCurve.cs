using UnityEngine;

namespace Data
{
    /// <summary>
    /// Defines how an equipment's main stat grows.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipGrowth_", menuName = "Game Specific/Equipment Growth Curve")]
    public class EquipmentGrowthCurve : ScriptableObject
    {
        [SerializeField] private int _tier1MaxLevel = 10;
        [SerializeField] private int _tier1BonusPerLevel = 1;
        [SerializeField] private int _tier2MaxLevel = 20;
        [SerializeField] private int _tier2BonusPerLevel = 2;

        /// <summary>
        /// Creates a runtime curve for temporary setups.
        /// </summary>
        public static EquipmentGrowthCurve CreateRuntime(
            int tier1MaxLevel = 10,
            int tier1BonusPerLevel = 1,
            int tier2MaxLevel = 20,
            int tier2BonusPerLevel = 2)
        {
            var curve = CreateInstance<EquipmentGrowthCurve>();
            curve.name = "EquipGrowth_Runtime";
            curve._tier1MaxLevel = tier1MaxLevel;
            curve._tier1BonusPerLevel = tier1BonusPerLevel;
            curve._tier2MaxLevel = tier2MaxLevel;
            curve._tier2BonusPerLevel = tier2BonusPerLevel;
            return curve;
        }

        public int GetBonusFromLevel(int level)
        {
            if (level <= 1)
                return 0;

            var bonus = 0;
            for (var lv = 2; lv <= level; lv++)
            {
                bonus += lv <= _tier1MaxLevel
                    ? _tier1BonusPerLevel
                    : _tier2BonusPerLevel;
            }

            return bonus;
        }
    }
}