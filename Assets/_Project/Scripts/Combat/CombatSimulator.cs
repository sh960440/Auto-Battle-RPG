using System;
using System.Collections.Generic;
using Data;

namespace Combat
{
    /// <summary>
    /// Pure C# ATB combat loop. Call <see cref="Tick"/> each frame; subscribe to events for presentation.
    /// </summary>
    public class CombatSimulator
    {
        private readonly CombatContext _context;
        private readonly CombatRules _rules;
        private readonly Random _random;
        private readonly List<CombatUnit> _scratch = new();
        private readonly List<CombatUnit> _readyUnits = new();
        private readonly List<CombatUnit> _aoeTargets = new();
        private readonly List<(CombatUnit unit, int tieBreak)> _readySort = new();

        private bool _isFinished;

        public CombatSimulator(CombatContext context, CombatRules rules = null, Random random = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _rules = rules != null ? rules : CombatRules.CreateRuntimeDefaults();
            _random = random ?? new Random();
        }

        public CombatContext Context => _context;

        public CombatRules Rules => _rules;

        public bool IsFinished => _isFinished;

        public CombatResult? Result { get; private set; }

        public event Action<CombatUnit, CombatUnit, int> OnAttack;
        public event Action<CombatUnit, SkillDefinition, IReadOnlyList<CombatUnit>, int> OnSkill;
        public event Action<CombatUnit, CombatUnit, int> OnDamage;
        public event Action<CombatUnit> OnDeath;
        public event Action<CombatResult> OnCombatEnd;

        /// <summary>
        /// Advances ATB by <paramref name="deltaTime"/> and resolves any ready turns.
        /// Also re-checks end conditions so already-dead setups (e.g. mutual kill) resolve immediately.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_isFinished)
                return;
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            EvaluateEndCondition();
            if (_isFinished)
                return;

            if (deltaTime == 0f)
                return;

            AccumulateAtb(deltaTime);
            ResolveReadyTurns();
        }

        private void AccumulateAtb(float deltaTime)
        {
            var units = _context.Units;
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (!unit.IsAlive)
                    continue;

                unit.AddAtb(unit.Stats.Speed * deltaTime);
            }
        }

        private void ResolveReadyTurns()
        {
            CollectReadyUnitsSorted();

            for (var i = 0; i < _readyUnits.Count; i++)
            {
                if (_isFinished)
                    return;

                var unit = _readyUnits[i];
                if (!unit.IsAlive || !unit.IsAtbReady)
                    continue;

                ResolveTurn(unit);
                EvaluateEndCondition();
            }
        }

        private void CollectReadyUnitsSorted()
        {
            _readySort.Clear();
            _readyUnits.Clear();

            var units = _context.Units;
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (unit.IsAlive && unit.IsAtbReady)
                    _readySort.Add((unit, _random.Next()));
            }

            _readySort.Sort(CompareReadyOrder);

            for (var i = 0; i < _readySort.Count; i++)
                _readyUnits.Add(_readySort[i].unit);
        }

        private static int CompareReadyOrder((CombatUnit unit, int tieBreak) a, (CombatUnit unit, int tieBreak) b)
        {
            var speedCmp = b.unit.Stats.Speed.CompareTo(a.unit.Stats.Speed);
            if (speedCmp != 0)
                return speedCmp;

            return a.tieBreak.CompareTo(b.tieBreak);
        }

        private void ResolveTurn(CombatUnit actor)
        {
            actor.ConsumeAtb();

            if (actor.IsPlayerSide && _context.HasQueuedSkill)
            {
                var skill = _context.QueuedSkill;
                if (actor.Energy >= skill.EnergyCost)
                {
                    _context.ConsumeQueuedSkill();
                    actor.SpendEnergy(skill.EnergyCost);
                    PerformSkill(actor, skill);
                    return;
                }

                _context.ClearQueuedSkill();
            }

            PerformBasicAttack(actor);
        }

        private void PerformBasicAttack(CombatUnit actor)
        {
            var target = TargetSelector.SelectRandomOpponent(_context, actor, _scratch, _random);
            if (target == null)
                return;

            var damage = CalculateDamage(actor.Stats.Attack, target.Stats.Defense);
            ApplyDamage(actor, target, damage);
            actor.AddEnergy(_rules.EnergyOnAttack);
            OnAttack?.Invoke(actor, target, damage);
        }

        private void PerformSkill(CombatUnit actor, SkillDefinition skill)
        {
            _aoeTargets.Clear();

            if (skill.IsAoe)
            {
                TargetSelector.SelectAllOpponents(_context, actor, _aoeTargets);
            }
            else
            {
                var target = TargetSelector.SelectRandomOpponent(_context, actor, _scratch, _random);
                if (target != null)
                    _aoeTargets.Add(target);
            }

            if (_aoeTargets.Count == 0)
                return;

            var attackPower = (int)Math.Floor(actor.Stats.Attack * skill.DamageMultiplier);
            var totalDamage = 0;

            for (var i = 0; i < _aoeTargets.Count; i++)
            {
                var target = _aoeTargets[i];
                if (!target.IsAlive)
                    continue;

                var damage = CalculateDamage(attackPower, target.Stats.Defense);
                ApplyDamage(actor, target, damage);
                totalDamage += damage;
            }

            OnSkill?.Invoke(actor, skill, _aoeTargets.ToArray(), totalDamage);
        }

        private void ApplyDamage(CombatUnit attacker, CombatUnit target, int damage)
        {
            var dealt = target.TakeDamage(damage);
            if (dealt <= 0)
                return;

            target.AddEnergy(_rules.EnergyOnHit);
            OnDamage?.Invoke(attacker, target, dealt);

            if (!target.IsAlive)
                OnDeath?.Invoke(target);
        }

        private static int CalculateDamage(int attack, int defense)
        {
            return Math.Max(1, attack - defense);
        }

        private void EvaluateEndCondition()
        {
            if (_isFinished)
                return;

            var playerAlive = _context.IsPlayerAlive;
            var enemiesDead = _context.AreAllEnemiesDead;

            if (!playerAlive)
            {
                Finish(CombatResult.Defeat);
                return;
            }

            if (enemiesDead)
                Finish(CombatResult.Victory);
        }

        private void Finish(CombatResult result)
        {
            _isFinished = true;
            Result = result;
            OnCombatEnd?.Invoke(result);
        }
    }
}