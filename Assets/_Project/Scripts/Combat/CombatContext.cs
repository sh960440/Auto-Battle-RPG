using System;
using System.Collections.Generic;
using Data;

namespace Combat
{
    /// <summary>
    /// Mutable state for one battle: participants and the player's queued skill.
    /// </summary>
    public class CombatContext
    {
        private readonly List<CombatUnit> _units = new();
        private SkillDefinition _queuedSkill;

        /// <summary>
        /// Creates a new combat context with the given player and enemies.
        /// </summary>
        /// <param name="player">The player unit.</param>
        /// <param name="enemies">The enemy units.</param>
        public CombatContext(CombatUnit player, IEnumerable<CombatUnit> enemies)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (!player.IsPlayerSide)
                throw new ArgumentException("Player unit must be on the player side.", nameof(player));
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));

            _units.Add(player);

            var enemyCount = 0;
            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    throw new ArgumentException("Enemy list cannot contain null.", nameof(enemies));
                if (enemy.IsPlayerSide)
                    throw new ArgumentException("Enemy list cannot contain player-side units.", nameof(enemies));

                _units.Add(enemy);
                enemyCount++;
            }

            if (enemyCount == 0)
                throw new ArgumentException("At least one enemy is required.", nameof(enemies));
        }

        /// <summary>All units in this battle.</summary>
        public IReadOnlyList<CombatUnit> Units => _units;

        /// <summary>Skill that will replace the player's next basic attack. Null when none is queued.</summary>
        public SkillDefinition QueuedSkill => _queuedSkill;

        /// <summary>Whether a skill is queued for the player's next action.</summary>
        public bool HasQueuedSkill => _queuedSkill != null;

        public CombatUnit Player
        {
            get
            {
                for (var i = 0; i < _units.Count; i++)
                {
                    if (_units[i].IsPlayerSide)
                        return _units[i];
                }

                throw new InvalidOperationException("Player unit is missing from combat context.");
            }
        }

        /// <summary>
        /// Queues a skill for the player's next action. Replaces any previously queued skill.
        /// </summary>
        public void QueueSkill(SkillDefinition skill)
        {
            if (skill == null)
                throw new ArgumentNullException(nameof(skill));

            _queuedSkill = skill;
        }

        /// <summary>
        /// Clears the queued skill without consuming it.
        /// </summary>
        public void ClearQueuedSkill()
        {
            _queuedSkill = null;
        }

        /// <summary>
        /// Returns and clears the queued skill. Returns null when none was queued.
        /// </summary>
        public SkillDefinition ConsumeQueuedSkill()
        {
            var skill = _queuedSkill;
            _queuedSkill = null;
            return skill;
        }

        /// <summary>
        /// Collects living units on the given side into <paramref name="results"/>.
        /// </summary>
        public void GetLivingUnits(bool playerSide, List<CombatUnit> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            for (var i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                if (unit.IsAlive && unit.IsPlayerSide == playerSide)
                    results.Add(unit);
            }
        }

        public bool IsPlayerAlive => Player.IsAlive;

        /// <summary>
        /// Whether all enemies are dead.
        /// </summary>
        public bool AreAllEnemiesDead
        {
            get
            {
                for (var i = 0; i < _units.Count; i++)
                {
                    var unit = _units[i];
                    if (!unit.IsPlayerSide && unit.IsAlive)
                        return false;
                }

                return true;
            }
        }
    }
}