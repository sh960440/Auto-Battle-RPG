using System.Collections.Generic;
using Combat;
using Core;
using Data;
using Infrastructure;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Builds context from data, ticks the simulator, and binds presentation.
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("Encounter Data")]
        [SerializeField] private CharacterDefinition _playerCharacter;
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private EnemyDefinition[] _enemiesOverride;
        [SerializeField] private CombatRules _combatRules;

        [Header("Presentation")]
        [SerializeField] private CombatPresenter _combatPresenter;
        [SerializeField] private CombatHUD _combatHud;
        [SerializeField] private EnemyEntrancePresenter _enemyEntrance;
        [SerializeField] private CombatStage _combatStage;

        [Header("Debug")]
        [SerializeField] private bool _startOnPlay;
        [SerializeField] [Min(0.1f)] private float _combatSpeed = 1.75f;

        private CombatSimulator _simulator;
        private bool _isTicking;

        public CombatSimulator Simulator => _simulator;

        public LevelDefinition Level => _level;

        public bool IsCombatActive => _isTicking && _simulator != null && !_simulator.IsFinished;

        private void Start()
        {
            if (_startOnPlay)
                StartCombat();
        }

        private void Update()
        {
            if (!_isTicking || _simulator == null)
                return;

            if (_simulator.IsFinished)
            {
                _isTicking = false;
                return;
            }

            _simulator.Tick(Time.deltaTime * _combatSpeed);
        }

        /// <summary>
        /// Builds units from assigned data, binds HUD/presenter, and starts ticking.
        /// </summary>
        /// <param name="playEntrance">When false, enemies stay at their current stop-point pose.</param>
        public void StartCombat(bool playEntrance = true)
        {
            StopCombat();

            if (_playerCharacter == null)
            {
                Debug.LogError("CombatController: Player character is missing.", this);
                return;
            }

            if (_playerCharacter.DefaultSkill == null)
            {
                Debug.LogError("CombatController: Player character has no default skill.", this);
                return;
            }

            var enemyDefs = ResolveEnemies();
            if (enemyDefs.Count == 0)
            {
                Debug.LogError("CombatController: No enemies assigned (Level or Enemies Override).", this);
                return;
            }

            var rules = _combatRules != null ? _combatRules : CombatRules.CreateRuntimeDefaults();

            var player = new CombatUnit(
                _playerCharacter.DisplayName,
                _playerCharacter.BaseStats,
                isPlayerSide: true,
                rules);

            var enemies = new List<CombatUnit>(enemyDefs.Count);
            for (var i = 0; i < enemyDefs.Count; i++)
            {
                var def = enemyDefs[i];
                enemies.Add(new CombatUnit(def.DisplayName, def.Stats, isPlayerSide: false, rules));
            }

            var context = new CombatContext(player, enemies);
            _simulator = new CombatSimulator(context, rules);
            _simulator.OnCombatEnd += HandleCombatEnd;

            ApplyEnemySlots(enemyDefs.Count);
            _combatPresenter?.Bind(_simulator);
            _combatHud?.Bind(_simulator, _playerCharacter.DefaultSkill);

            if (playEntrance)
                _enemyEntrance?.PlayEntrance();

            _isTicking = true;
            Debug.Log($"[Combat] Started vs {enemyDefs.Count} enemies.");
        }

        /// <summary>
        /// Stops ticking and unbinds presentation.
        /// </summary>
        public void StopCombat()
        {
            _isTicking = false;

            if (_simulator != null)
                _simulator.OnCombatEnd -= HandleCombatEnd;

            _combatHud?.Unbind();
            _combatPresenter?.Unbind();

            _simulator = null;
        }

        private void HandleCombatEnd(CombatResult result)
        {
            _isTicking = false;

            if (ServiceLocator.TryGet(out GameStateMachine stateMachine))
                stateMachine.SetState(GameState.Result);
        }

        private List<EnemyDefinition> ResolveEnemies()
        {
            var result = new List<EnemyDefinition>();

            if (_enemiesOverride != null && _enemiesOverride.Length > 0)
            {
                for (var i = 0; i < _enemiesOverride.Length; i++)
                {
                    if (_enemiesOverride[i] != null)
                        result.Add(_enemiesOverride[i]);
                }

                return result;
            }

            if (_level?.Enemies == null)
                return result;

            for (var i = 0; i < _level.Enemies.Count; i++)
            {
                if (_level.Enemies[i] != null)
                    result.Add(_level.Enemies[i]);
            }

            return result;
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

        private void OnDestroy()
        {
            StopCombat();
        }
    }
}