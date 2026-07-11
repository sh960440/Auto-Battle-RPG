using UnityEngine;

namespace Data
{
    /// <summary>
    /// Static equipment template.
    /// </summary>
    [CreateAssetMenu(fileName = "Equipment_", menuName = "Game Specific/Equipment Definition")]
    public class EquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private EquipmentSlot _slot;
        [SerializeField] private EquipmentQuality _quality;
        [SerializeField] private StatType _mainStatType;
        [SerializeField] private int _baseMainStat = 1;
        [SerializeField] private EquipmentGrowthCurve _growthCurve;

        public string DisplayName => _displayName;

        public EquipmentSlot Slot => _slot;

        public EquipmentQuality Quality => _quality;

        public StatType MainStatType => _mainStatType;

        public int BaseMainStat => _baseMainStat;

        public EquipmentGrowthCurve GrowthCurve => _growthCurve;

        /// <summary>
        /// Main stat at the given equipment level (base + curve bonus).
        /// </summary>
        public int GetMainStatAtLevel(int level)
        {
            var clampedLevel = Mathf.Max(1, level);
            var bonus = _growthCurve != null ? _growthCurve.GetBonusFromLevel(clampedLevel) : 0;
            return _baseMainStat + bonus;
        }
    }
}