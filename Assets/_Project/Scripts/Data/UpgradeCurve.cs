using UnityEngine;

namespace Data
{
    /// <summary>
    /// Character level progression.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeCurve_", menuName = "Game Specific/Upgrade Curve")]
    public class UpgradeCurve : ScriptableObject
    {
        [SerializeField] private int _maxLevel = 30;
        [SerializeField] private int _baseGoldCost = 50;
        [SerializeField] private int _goldCostPerLevel = 25;
        [SerializeField] private StatBlock _statBonusPerLevel;

        public int MaxLevel => _maxLevel;

        /// <summary>
        /// Gold required to upgrade from <paramref name="currentLevel"/> to the next level.
        /// </summary>
        public int GetGoldCostForNextLevel(int currentLevel)
        {
            if (currentLevel >= _maxLevel)
                return 0;

            var targetLevel = currentLevel + 1;
            return _baseGoldCost + (targetLevel - 1) * _goldCostPerLevel;
        }

        /// <summary>
        /// Total stat bonus applied at the given character level.
        /// </summary>
        public StatBlock GetTotalStatBonusAtLevel(int level)
        {
            if (level <= 1)
                return StatBlock.Zero;

            var clampedLevel = Mathf.Min(level, _maxLevel);
            return Scale(_statBonusPerLevel, clampedLevel - 1);
        }

        /// <summary>
        /// Whether the character can be upgraded to the next level.
        /// </summary>
        public bool CanUpgrade(int currentLevel)
        {
            return currentLevel < _maxLevel;
        }

        private static StatBlock Scale(StatBlock block, int multiplier)
        {
            return new StatBlock
            {
                HP = block.HP * multiplier,
                Attack = block.Attack * multiplier,
                Defense = block.Defense * multiplier,
                Speed = block.Speed * multiplier,
                LifeSteal = block.LifeSteal * multiplier,
                Tenacity = block.Tenacity * multiplier,
                Evasion = block.Evasion * multiplier,
                Crit = block.Crit * multiplier,
                Accuracy = block.Accuracy * multiplier,
                MaxEnergy = block.MaxEnergy * multiplier
            };
        }
    }
}