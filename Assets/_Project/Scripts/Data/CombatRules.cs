using UnityEngine;

namespace Data
{
    /// <summary>
    /// ATB threshold and energy gain rules.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatRules_", menuName = "Game Specific/Combat Rules")]
    public class CombatRules : ScriptableObject
    {
        public const float FallbackAtbThreshold = 100f;
        public const int FallbackMaxEnergy = 200;
        public const int FallbackEnergyOnAttack = 10;
        public const int FallbackEnergyOnHit = 5;

        [SerializeField] private float _atbThreshold = FallbackAtbThreshold;
        [SerializeField] private int _defaultMaxEnergy = FallbackMaxEnergy;
        [SerializeField] private int _energyOnAttack = FallbackEnergyOnAttack;
        [SerializeField] private int _energyOnHit = FallbackEnergyOnHit;

        public float AtbThreshold => _atbThreshold;

        public int DefaultMaxEnergy => _defaultMaxEnergy;

        public int EnergyOnAttack => _energyOnAttack;

        public int EnergyOnHit => _energyOnHit;

        /// <summary>
        /// Runtime defaults when no asset is assigned.
        /// </summary>
        public static CombatRules CreateRuntimeDefaults()
        {
            var rules = CreateInstance<CombatRules>();
            rules.name = "CombatRules_RuntimeDefaults";
            rules._atbThreshold = FallbackAtbThreshold;
            rules._defaultMaxEnergy = FallbackMaxEnergy;
            rules._energyOnAttack = FallbackEnergyOnAttack;
            rules._energyOnHit = FallbackEnergyOnHit;
            return rules;
        }
    }
}