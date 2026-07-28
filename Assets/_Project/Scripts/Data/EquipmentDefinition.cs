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
        [SerializeField] private CharacterClass _requiredClass = CharacterClass.Knight;
        [SerializeField] private int _baseMainStat = 1;
        [SerializeField] private EquipmentGrowthCurve _growthCurve;

        public string DisplayName => _displayName;

        public EquipmentSlot Slot => _slot;

        public EquipmentQuality Quality => _quality;

        public CharacterClass RequiredClass => _requiredClass;

        public StatType MainStatType => EquipmentMainStatRules.GetMainStatType(_slot);

        public int BaseMainStat => _baseMainStat;

        public EquipmentGrowthCurve GrowthCurve => _growthCurve;

        /// <summary>
        /// Creates a runtime definition for temporary setups.
        /// </summary>
        public static EquipmentDefinition CreateRuntime(
            string displayName,
            EquipmentSlot slot,
            CharacterClass requiredClass,
            int baseMainStat,
            EquipmentGrowthCurve growthCurve = null,
            EquipmentQuality quality = EquipmentQuality.Common)
        {
            var definition = CreateInstance<EquipmentDefinition>();
            definition.name = $"Equipment_{displayName}_Runtime";
            definition._displayName = displayName;
            definition._slot = slot;
            definition._quality = quality;
            definition._requiredClass = requiredClass;
            definition._baseMainStat = baseMainStat;
            definition._growthCurve = growthCurve;
            return definition;
        }

        /// <summary>
        /// Returns whether a character of the given class may equip this item.
        /// </summary>
        public bool CanBeEquippedBy(CharacterClass characterClass)
        {
            return characterClass == _requiredClass;
        }

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