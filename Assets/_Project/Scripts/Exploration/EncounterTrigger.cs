using System;
using System.Collections;
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
        [SerializeField] [Min(1)] private int _enemyCount = 1;

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
            HideAllEnemySlots();
        }

        /// <summary>
        /// Shows the enemies and plays the approach.
        /// </summary>
        /// <param name="enemyCount">The number of enemies to show.</param>
        public void BeginEncounter(int enemyCount)
        {
            if (_running != null)
                StopCoroutine(_running);

            _enemyCount = Mathf.Max(1, enemyCount);
            _isReady = false;
            _running = StartCoroutine(BeginEncounterRoutine());
        }

        /// <summary>
        /// Begins an encounter using the enemy count.
        /// </summary>
        public void BeginEncounter()
        {
            BeginEncounter(_enemyCount);
        }

        private IEnumerator BeginEncounterRoutine()
        {
            ApplyEnemySlots(_enemyCount);

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

        private void ApplyEnemySlots(int enemyCount)
        {
            if (_combatStage == null)
                return;

            var slots = _combatStage.EnemySlots;
            if (slots == null)
                return;

            for (var i = 0; i < slots.Length; i++)
                _combatStage.SetEnemySlotActive(i, i < enemyCount);
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