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
        [SerializeField] private int _energyCost = 80;
        [SerializeField] private float _damageMultiplier = 1.5f;
        [SerializeField] private bool _isAoe;

        public string DisplayName => _displayName;
        public int EnergyCost => _energyCost;
        public float DamageMultiplier => _damageMultiplier;
        public bool IsAoe => _isAoe;
    }
}