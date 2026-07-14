using System;
using Data;

namespace Combat
{
    /// <summary>
    /// Runtime battle participant.
    /// </summary>
    public class CombatUnit
    {
        public const float AtbThreshold = 100f;
        public const int DefaultMaxEnergy = 200;

        private readonly string _displayName;
        private readonly StatBlock _stats;
        private readonly bool _isPlayerSide;
        private readonly int _maxEnergy;

        private int _currentHp;
        private float _atbGauge;
        private int _energy;

        public CombatUnit(string displayName, StatBlock stats, bool isPlayerSide)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", nameof(displayName));

            _displayName = displayName;
            _stats = stats;
            _isPlayerSide = isPlayerSide;
            _maxEnergy = stats.MaxEnergy > 0 ? stats.MaxEnergy : DefaultMaxEnergy;

            ResetCombatState();
        }

        public string DisplayName => _displayName;

        // Combat stats snapshot for this fight.
        public StatBlock Stats => _stats;

        public bool IsPlayerSide => _isPlayerSide;

        public int MaxHp => _stats.HP;

        public int CurrentHp => _currentHp;

        public float AtbGauge => _atbGauge;

        public int Energy => _energy;

        public int MaxEnergy => _maxEnergy;

        public bool IsAlive => _currentHp > 0;

        public bool IsAtbReady => _atbGauge >= AtbThreshold;

        /// <summary>
        /// Resets HP, ATB, and energy for combat start.
        /// </summary>
        public void ResetCombatState()
        {
            _currentHp = _stats.HP;
            _atbGauge = 0f;
            _energy = 0;
        }

        /// <summary>
        /// Adds ATB up to <see cref="AtbThreshold"/>.
        /// </summary>
        public void AddAtb(float amount) => _atbGauge += amount;

        /// <summary>
        /// Consumes all ATB.
        /// </summary>
        public void ConsumeAtb() => _atbGauge = 0f;

        /// <summary>
        /// Adds energy up to <see cref="MaxEnergy"/>.
        /// </summary>
        public int AddEnergy(int amount)
        {
            if (amount <= 0)
                return 0;

            var space = _maxEnergy - _energy;
            if (space <= 0)
                return 0;

            var gained = Math.Min(amount, space);
            _energy += gained;
            return gained;
        }

        /// <summary>
        /// Spends energy up to <see cref="Energy"/>.
        /// </summary>
        public void SpendEnergy(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (amount > _energy)
                throw new InvalidOperationException("Not enough energy.");

            _energy -= amount;
        }

        /// <summary>
        /// Applies damage and returns the amount actually dealt.
        /// </summary>
        public int TakeDamage(int amount)
        {
            if (amount <= 0)
                return 0;

            var dealt = Math.Min(amount, _currentHp);
            _currentHp -= dealt;
            return dealt;
        }
    }
}