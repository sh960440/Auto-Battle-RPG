using System.Collections;
using Core;
using Data;
using Infrastructure;
using Presentation;
using UnityEngine;

namespace Exploration
{
    /// <summary>
    /// Controls the exploration, including advancing, encountering, and entering combat.
    /// </summary>
    public class ExplorationController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private EncounterTrigger _encounterTrigger;
        [SerializeField] private CombatController _combatController;
        [SerializeField] private CombatResultOverlay _resultOverlay;
        [SerializeField] private LevelDefinition _level;

        [Header("Advance")]
        [SerializeField] [Min(0.1f)] private float _advanceDuration = 1.5f;
        [SerializeField] private Transform _scrollRoot;
        [SerializeField] private Vector3 _scrollStartLocalPosition = new Vector3(0f, 0.4f, 0f);
        [SerializeField] private Vector3 _scrollEndLocalPosition = Vector3.zero;

        [Header("Main HUD")]
        [SerializeField] private MainHUD _mainHUD;

        private GameStateMachine _stateMachine;
        private Coroutine _advanceRoutine;
        private bool _isAdvancing;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _stateMachine))
            {
                Debug.LogWarning($"{nameof(ExplorationController)}: {nameof(GameStateMachine)} is not registered.");
                return;
            }

            _stateMachine.StateEntered += HandleStateEntered;

            if (_mainHUD != null)
                _mainHUD.AdvanceClicked += HandleAdvanceClicked;

            if (_resultOverlay != null)
                _resultOverlay.Continued += HandleResultContinued;

            if (_encounterTrigger != null)
                _encounterTrigger.EncounterReady += HandleEncounterReady;

            if (_stateMachine.CurrentState == GameState.Exploration)
                HandleEnterExploration();
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.StateEntered -= HandleStateEntered;

            if (_mainHUD != null)
                _mainHUD.AdvanceClicked -= HandleAdvanceClicked;

            if (_resultOverlay != null)
                _resultOverlay.Continued -= HandleResultContinued;

            if (_encounterTrigger != null)
                _encounterTrigger.EncounterReady -= HandleEncounterReady;
        }

        private void HandleStateEntered(GameState state)
        {
            if (state == GameState.Exploration)
                HandleEnterExploration();
            else if (state == GameState.Combat)
                HandleEnterCombat();
        }

        private void HandleEnterExploration()
        {
            StopAdvance();
            _encounterTrigger?.ResetEncounter();
            _combatController?.StopCombat();
            ResetScrollVisual();
            _mainHUD?.ShowAdvanceOnly();
        }

        private void HandleEnterCombat()
        {
            _mainHUD?.HideAllActions();
            // Encounter already moved enemies to the stop point; skip a second approach.
            _combatController?.StartCombat(playEntrance: false);
        }

        private void HandleAdvanceClicked()
        {
            if (_stateMachine == null || _stateMachine.CurrentState != GameState.Exploration)
                return;

            if (_isAdvancing)
                return;

            StopAdvance();
            _advanceRoutine = StartCoroutine(AdvanceRoutine());
        }

        private void HandleEncounterReady()
        {
            if (_stateMachine == null || _stateMachine.CurrentState != GameState.Exploration)
                return;

            _stateMachine.SetState(GameState.Combat);
        }

        private void HandleResultContinued()
        {
            if (_stateMachine == null)
                return;

            _stateMachine.SetState(GameState.Exploration);
        }

        private IEnumerator AdvanceRoutine()
        {
            _isAdvancing = true;
            _mainHUD?.SetAdvanceInteractable(false);

            var duration = Mathf.Max(0.01f, _advanceDuration);
            var elapsed = 0f;

            if (_scrollRoot != null)
                _scrollRoot.localPosition = _scrollStartLocalPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = t * t * (3f - 2f * t);

                if (_scrollRoot != null)
                {
                    _scrollRoot.localPosition = Vector3.LerpUnclamped(
                        _scrollStartLocalPosition,
                        _scrollEndLocalPosition,
                        eased);
                }

                yield return null;
            }

            if (_scrollRoot != null)
                _scrollRoot.localPosition = _scrollEndLocalPosition;

            _isAdvancing = false;
            _advanceRoutine = null;

            var enemyCount = ResolveEnemyCount();
            _encounterTrigger?.BeginEncounter(enemyCount);
        }

        private int ResolveEnemyCount()
        {
            if (_level?.Enemies == null || _level.Enemies.Count == 0)
                return 1;

            var count = 0;
            for (var i = 0; i < _level.Enemies.Count; i++)
            {
                if (_level.Enemies[i] != null)
                    count++;
            }

            return Mathf.Max(1, count);
        }

        private void StopAdvance()
        {
            if (_advanceRoutine != null)
            {
                StopCoroutine(_advanceRoutine);
                _advanceRoutine = null;
            }

            _isAdvancing = false;
        }

        private void ResetScrollVisual()
        {
            if (_scrollRoot != null)
                _scrollRoot.localPosition = _scrollStartLocalPosition;
        }
    }
}