using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private StageEncounterTable _encounterTable;
        [SerializeField] private LevelDefinition _fallbackLevel;

        [Header("Advance")]
        [SerializeField] [Min(0.1f)] private float _advanceDuration = 1.5f;
        [SerializeField] private Transform _scrollRoot;
        [SerializeField] private Vector3 _scrollStartLocalPosition = new Vector3(0f, 0.4f, 0f);
        [SerializeField] private Vector3 _scrollEndLocalPosition = Vector3.zero;

        [Header("Main HUD")]
        [SerializeField] private MainHUD _mainHUD;

        private GameStateMachine _stateMachine;
        private StageProgressService _stageProgress;
        private Coroutine _advanceRoutine;
        private bool _isAdvancing;
        private readonly List<EnemyDefinition> _rolledEnemies = new();

        private void Start()
        {
            EnsureStageProgress();

            if (!ServiceLocator.TryGet(out _stateMachine))
            {
                Debug.LogWarning($"{nameof(ExplorationController)}: {nameof(GameStateMachine)} is not registered.");
                return;
            }

            _stateMachine.StateEntered += HandleStateEntered;

            if (_mainHUD != null)
                _mainHUD.AdvanceClicked += HandleAdvanceClicked;

            if (_resultOverlay != null)
            {
                _resultOverlay.ContinueRequested += HandleContinueAdvance;
                _resultOverlay.ReturnRequested += HandleReturnToCharacter;
            }

            if (_encounterTrigger != null)
                _encounterTrigger.EncounterReady += HandleEncounterReady;

            if (_stageProgress != null)
                _stageProgress.StageChanged += HandleStageChanged;

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
            {
                _resultOverlay.ContinueRequested -= HandleContinueAdvance;
                _resultOverlay.ReturnRequested -= HandleReturnToCharacter;
            }

            if (_encounterTrigger != null)
                _encounterTrigger.EncounterReady -= HandleEncounterReady;

            if (_stageProgress != null)
                _stageProgress.StageChanged -= HandleStageChanged;
        }

        private void EnsureStageProgress()
        {
            if (ServiceLocator.TryGet(out _stageProgress))
                return;

            _stageProgress = new StageProgressService();
            ServiceLocator.Register(_stageProgress);
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
            RefreshStageHud();
            _mainHUD?.ShowAdvanceOnly();
        }

        private void HandleEnterCombat()
        {
            _mainHUD?.HideAllActions();
            _combatController?.SetEncounter(_rolledEnemies);
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

        private void HandleContinueAdvance()
        {
            if (_stateMachine == null)
                return;

            _stateMachine.SetState(GameState.Exploration);
        }

        private void HandleReturnToCharacter()
        {
            // Hook for the character page.
        }

        private void HandleStageChanged(int stage)
        {
            _mainHUD?.SetStage(stage);
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

            if (!RollEncounter())
            {
                Debug.LogError($"{nameof(ExplorationController)}: Failed to roll enemies for stage {_stageProgress?.CurrentStage}.", this);
                _mainHUD?.SetAdvanceInteractable(true);
                yield break;
            }

            _encounterTrigger?.BeginEncounter(_rolledEnemies.Count);
        }

        private bool RollEncounter()
        {
            _rolledEnemies.Clear();
            var stage = _stageProgress != null ? _stageProgress.CurrentStage : 1;

            if (_encounterTable != null)
            {
                var rolled = _encounterTable.RollEnemies(stage);
                for (var i = 0; i < rolled.Count; i++)
                    _rolledEnemies.Add(rolled[i]);
            }

            if (_rolledEnemies.Count == 0 && _fallbackLevel?.Enemies != null)
            {
                for (var i = 0; i < _fallbackLevel.Enemies.Count; i++)
                {
                    if (_fallbackLevel.Enemies[i] != null)
                        _rolledEnemies.Add(_fallbackLevel.Enemies[i]);
                }
            }

            return _rolledEnemies.Count > 0;
        }

        private void RefreshStageHud()
        {
            if (_stageProgress != null)
                _mainHUD?.SetStage(_stageProgress.CurrentStage);
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