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
        [SerializeField] private StatBlock _baseStats;
        [SerializeField] private SkillDefinition _defaultSkill;

        public string DisplayName => _displayName;

        public StatBlock BaseStats => _baseStats;

        public SkillDefinition DefaultSkill => _defaultSkill;
    }
}