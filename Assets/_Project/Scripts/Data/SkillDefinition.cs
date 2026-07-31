using UnityEngine;

namespace Data
{
    /// <summary>
    /// Active skill template.
    /// </summary>
    [CreateAssetMenu(fileName = "Skill_", menuName = "Game Specific/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] [TextArea] private string _description;
        [SerializeField] private int _energyCost = 80;
        [SerializeField] private float _damageMultiplier = 1.5f;
        [SerializeField] private bool _isAoe;

        public string DisplayName => _displayName;

        public string Description
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_description))
                    return _description;

                var aoe = _isAoe ? " AoE." : ".";
                return $"Costs {_energyCost} energy. Deals x{_damageMultiplier:0.##} damage{aoe}";
            }
        }

        public int EnergyCost => _energyCost;
        public float DamageMultiplier => _damageMultiplier;
        public bool IsAoe => _isAoe;

        /// <summary>
        /// Creates a runtime skill for tests or temporary setups.
        /// </summary>
        public static SkillDefinition CreateRuntime(
            string displayName,
            int energyCost = 80,
            float damageMultiplier = 1.5f,
            bool isAoe = false,
            string description = null)
        {
            var skill = CreateInstance<SkillDefinition>();
            skill.name = $"Skill_{displayName}_Runtime";
            skill._displayName = displayName;
            skill._description = description;
            skill._energyCost = energyCost;
            skill._damageMultiplier = damageMultiplier;
            skill._isAoe = isAoe;
            return skill;
        }
    }
}