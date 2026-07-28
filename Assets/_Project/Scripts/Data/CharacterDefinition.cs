using UnityEngine;

namespace Data
{
    /// <summary>
    /// Static character template.
    /// </summary>
    [CreateAssetMenu(fileName = "Character_", menuName = "Game Specific/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private CharacterClass _characterClass = CharacterClass.Warrior;
        [SerializeField] private StatBlock _baseStats;
        [SerializeField] private UpgradeCurve _upgradeCurve;
        [SerializeField] private SkillDefinition _defaultSkill;

        public string DisplayName => _displayName;

        public CharacterClass CharacterClass => _characterClass;

        public StatBlock BaseStats => _baseStats;

        public UpgradeCurve UpgradeCurve => _upgradeCurve;

        public SkillDefinition DefaultSkill => _defaultSkill;

        /// <summary>
        /// Creates a runtime definition for temporary setups.
        /// </summary>
        public static CharacterDefinition CreateRuntime(
            string displayName,
            CharacterClass characterClass,
            StatBlock baseStats,
            UpgradeCurve upgradeCurve = null,
            SkillDefinition defaultSkill = null)
        {
            var definition = CreateInstance<CharacterDefinition>();
            definition.name = $"Character_{displayName}_Runtime";
            definition._displayName = displayName;
            definition._characterClass = characterClass;
            definition._baseStats = baseStats;
            definition._upgradeCurve = upgradeCurve;
            definition._defaultSkill = defaultSkill;
            return definition;
        }
    }
}