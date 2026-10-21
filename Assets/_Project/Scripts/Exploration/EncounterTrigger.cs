using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Presentation;
using UnityEngine;

namespace Exploration
{
    /// <summary>
    /// Spawns enemies ahead, plays the approach, signals ready for combat.
    /// </summary>
    public class EncounterTrigger : MonoBehaviour
    {
        [SerializeField] private CombatStage _combatStage;
        [SerializeField] private EnemyEntrancePresenter _enemyEntrance;

        private readonly List<EnemyDefinition> _pendingEnemies = new();
        private Coroutine _running;
        private bool _isReady;

        /// <summary>
        /// If is ready to begin combat.
        /// </summary>
        public bool IsReady => _isReady;

        /// <summary>
        /// Fired after enemies reach the stop point.
        /// </summary>
        public event Action EncounterReady;

        /// <summary>
        /// Hides enemy slots and resets the encounter.
        /// </summary>
        public void ResetEncounter()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }

            _isReady = false;
            _pendingEnemies.Clear();
            HideAllEnemySlots();
        }

        /// <summary>
        /// Shows the enemies and plays the approach.
        /// </summary>
        public void BeginEncounter(IReadOnlyList<EnemyDefinition> enemies)
        {
            if (_running != null)
                StopCoroutine(_running);

            _pendingEnemies.Clear();
            if (enemies != null)
            {
                for (var i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] != null)
                        _pendingEnemies.Add(enemies[i]);
                }
            }

            _isReady = false;
            _running = StartCoroutine(BeginEncounterRoutine());
        }

        private IEnumerator BeginEncounterRoutine()
        {
            _combatStage?.ApplyEncounter(_pendingEnemies);

            var completed = false;
            if (_enemyEntrance != null)
            {
                _enemyEntrance.PlayEntrance(() => completed = true);
                while (!completed)
                    yield return null;
            }
            else
            {
                yield return null;
            }

            _isReady = true;
            _running = null;
            EncounterReady?.Invoke();
        }

        private void HideAllEnemySlots()
        {
            if (_combatStage == null)
                return;

            var slots = _combatStage.EnemySlots;
            if (slots == null)
                return;

            for (var i = 0; i < slots.Length; i++)
                _combatStage.SetEnemySlotActive(i, false);
        }
    }
}