using System;
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

        [Header("Loot")]
        [SerializeField] private LootTable _lootTable;

        [Header("Debug")]
        [SerializeField] private bool _startOnPlay;
        [SerializeField] [Min(0.1f)] private float _combatSpeed = 1.75f;

        private CombatSimulator _simulator;
        private readonly LootService _lootService = new();
        private readonly List<EnemyDefinition> _pendingEnemies = new();
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
        /// Sets the enemy list used by the next <see cref="StartCombat"/> call.
        /// </summary>
        public void SetEncounter(IReadOnlyList<EnemyDefinition> enemies)
        {
            _pendingEnemies.Clear();
            if (enemies == null)
                return;

            for (var i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null)
                    _pendingEnemies.Add(enemies[i]);
            }
        }

        /// <summary>
        /// Builds units from assigned data, binds HUD/presenter, and starts ticking.
        /// </summary>
        /// <param name="playEntrance">When false, enemies stay at their current stop-point pose.</param>
        public void StartCombat(bool playEntrance = true)
        {
            StopCombat();

            if (!TryResolvePlayer(out var playerName, out var playerStats, out var playerSkill))
                return;

            var enemyDefs = ResolveEnemies();
            if (enemyDefs.Count == 0)
            {
                Debug.LogError("CombatController: No enemies assigned (encounter table, Level, or Override).", this);
                return;
            }

            var rules = _combatRules != null ? _combatRules : CombatRules.CreateRuntimeDefaults();

            var player = new CombatUnit(
                playerName,
                playerStats,
                isPlayerSide: true,
                rules);

            var stage = ResolveCurrentStage();
            var enemies = new List<CombatUnit>(enemyDefs.Count);
            for (var i = 0; i < enemyDefs.Count; i++)
            {
                var def = enemyDefs[i];
                var scaledStats = EnemyStageScaling.Scale(def.Stats, stage);
                enemies.Add(new CombatUnit(def.DisplayName, scaledStats, isPlayerSide: false, rules));
            }

            var context = new CombatContext(player, enemies);
            _simulator = new CombatSimulator(context, rules);
            _simulator.OnCombatEnd += HandleCombatEnd;

            ApplyEnemySlots(enemyDefs.Count);
            _combatPresenter?.Bind(_simulator);
            _combatHud?.Bind(_simulator, playerSkill);

            if (playEntrance)
                _enemyEntrance?.PlayEntrance();

            _isTicking = true;
            Debug.Log(
                $"[Combat] Started as {playerName} " +
                $"(HP {playerStats.HP}/ATK {playerStats.Attack}/DEF {playerStats.Defense}/SPD {playerStats.Speed}) " +
                $"vs {enemyDefs.Count} enemies. Skill={playerSkill.DisplayName}");
        }

        /// <summary>
        /// Resolves the deployed character stats/skill.
        /// </summary>
        private bool TryResolvePlayer(out string displayName, out StatBlock stats, out SkillDefinition skill)
        {
            displayName = null;
            stats = StatBlock.Zero;
            skill = null;

            if (ServiceLocator.TryGet(out PlayerProfileService profileService) &&
                profileService.DeployedCharacter != null)
            {
                var deployed = profileService.DeployedCharacter;
                displayName = deployed.DisplayName;
                stats = profileService.DeployedStats;
                skill = profileService.DeployedSkill;

                if (skill == null)
                {
                    Debug.LogError(
                        $"CombatController: Deployed character '{displayName}' has no skill.",
                        this);
                    return false;
                }

                return true;
            }

            if (_playerCharacter == null)
            {
                Debug.LogError(
                    "CombatController: No deployed character and no inspector player fallback.",
                    this);
                return false;
            }

            if (_playerCharacter.DefaultSkill == null)
            {
                Debug.LogError("CombatController: Player character has no default skill.", this);
                return false;
            }

            displayName = _playerCharacter.DisplayName;
            stats = _playerCharacter.BaseStats;
            skill = _playerCharacter.DefaultSkill;
            return true;
        }

        /// <summary>
        /// Stops ticking and unbinds presentation.
        /// </summary>
        public void StopCombat()
        {
            _isTicking = false;

            if (_simulator != null)
                _simulator.OnCombatEnd -= HandleCombatEnd;

            if (_combatHud != null)
                _combatHud.Unbind();

            _combatPresenter?.Unbind();
            _simulator = null;
        }

        private void HandleCombatEnd(CombatResult result)
        {
            _isTicking = false;

            var drop = TryGrantVictoryLoot(result);

            if (_combatHud != null)
                _combatHud.ShowResult(result, drop);

            if (ServiceLocator.TryGet(out StageProgressService progress))
                progress.ApplyCombatResult(result);

            if (ServiceLocator.TryGet(out GameStateMachine stateMachine))
                stateMachine.SetState(GameState.Result);
        }

        private LootDrop TryGrantVictoryLoot(CombatResult result)
        {
            if (result != CombatResult.Victory)
                return null;

            if (_lootTable == null)
            {
                Debug.LogWarning("CombatController: No loot table assigned.", this);
                return null;
            }

            var stage = ResolveCurrentStage();

            ServiceLocator.TryGet(out PlayerProfileService profileService);
            CharacterClass? preferredClass = profileService?.DeployedCharacter?.CharacterClass;

            var drop = _lootService.Roll(_lootTable, stage, preferredClass);
            profileService?.ApplyLoot(drop);
            return drop;
        }

        private static int ResolveCurrentStage()
        {
            if (ServiceLocator.TryGet(out StageProgressService progress))
                return progress.CurrentStage;

            return 1;
        }

        private List<EnemyDefinition> ResolveEnemies()
        {
            var result = new List<EnemyDefinition>();

            if (_pendingEnemies.Count > 0)
            {
                result.AddRange(_pendingEnemies);
                return result;
            }

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