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