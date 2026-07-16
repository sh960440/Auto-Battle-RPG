using System.Collections;
using System.Collections.Generic;
using Combat;
using Data;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Listens to CombatSimulator events and updates unit views.
    /// </summary>
    public class CombatPresenter : MonoBehaviour
    {
        [SerializeField] private CombatUnitView _playerView;
        [SerializeField] private CombatUnitView[] _enemyViews;
        [SerializeField] private float _hpTweenDuration = 0.2f;

        private CombatSimulator _simulator;
        private readonly Dictionary<CombatUnit, CombatUnitView> _viewsByUnit = new();
        private readonly Dictionary<CombatUnitView, Coroutine> _hpTweens = new();

        /// <summary>
        /// Binds to a simulator, maps units to views, and starts listening for combat events.
        /// </summary>
        public void Bind(CombatSimulator simulator)
        {
            Unbind();

            _simulator = simulator ?? throw new System.ArgumentNullException(nameof(simulator));
            MapViews(_simulator.Context);
            Subscribe(_simulator);
            RefreshAllInstant();
        }

        /// <summary>
        /// Stops listening and clears view bindings.
        /// </summary>
        public void Unbind()
        {
            if (_simulator != null)
            {
                Unsubscribe(_simulator);
                _simulator = null;
            }

            StopAllHpTweens();
            _viewsByUnit.Clear();

            if (_playerView != null)
                _playerView.Unbind();

            if (_enemyViews == null)
                return;

            for (var i = 0; i < _enemyViews.Length; i++)
            {
                if (_enemyViews[i] != null)
                    _enemyViews[i].Unbind();
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void LateUpdate()
        {
            if (_simulator == null)
                return;

            foreach (var pair in _viewsByUnit)
                pair.Value.RefreshGaugesInstant();
        }

        private void MapViews(CombatContext context)
        {
            _viewsByUnit.Clear();

            if (_playerView != null)
            {
                _playerView.gameObject.SetActive(true);
                _playerView.Bind(context.Player);
                _viewsByUnit[context.Player] = _playerView;
            }

            var enemyIndex = 0;
            for (var i = 0; i < context.Units.Count; i++)
            {
                var unit = context.Units[i];
                if (unit.IsPlayerSide)
                    continue;

                if (_enemyViews == null || enemyIndex >= _enemyViews.Length)
                    break;

                var view = _enemyViews[enemyIndex++];
                if (view == null)
                    continue;

                view.gameObject.SetActive(true);
                view.Bind(unit);
                _viewsByUnit[unit] = view;
            }

            // Hide unused enemy view slots.
            if (_enemyViews == null)
                return;

            for (var i = enemyIndex; i < _enemyViews.Length; i++)
            {
                if (_enemyViews[i] == null)
                    continue;

                _enemyViews[i].Unbind();
                _enemyViews[i].gameObject.SetActive(false);
            }
        }

        private void Subscribe(CombatSimulator simulator)
        {
            simulator.OnAttack += HandleAttack;
            simulator.OnSkill += HandleSkill;
            simulator.OnDamage += HandleDamage;
            simulator.OnDeath += HandleDeath;
            simulator.OnCombatEnd += HandleCombatEnd;
        }

        private void Unsubscribe(CombatSimulator simulator)
        {
            simulator.OnAttack -= HandleAttack;
            simulator.OnSkill -= HandleSkill;
            simulator.OnDamage -= HandleDamage;
            simulator.OnDeath -= HandleDeath;
            simulator.OnCombatEnd -= HandleCombatEnd;
        }

        private void HandleAttack(CombatUnit attacker, CombatUnit target, int damage)
        {
            // Hooks for future features. Attack animation / hit flash can plug in here.
            Debug.Log($"[Combat] {attacker.DisplayName} attacks {target.DisplayName} for {damage}");
        }

        private void HandleSkill(
            CombatUnit attacker,
            SkillDefinition skill,
            IReadOnlyList<CombatUnit> targets,
            int totalDamage)
        {
            // Hooks for future features. Skill VFX can plug in here.
            Debug.Log($"[Combat] {attacker.DisplayName} uses {skill.DisplayName} for {totalDamage} total");
        }

        private void HandleDamage(CombatUnit attacker, CombatUnit target, int damage)
        {
            if (!_viewsByUnit.TryGetValue(target, out var view))
                return;

            var targetNormalized = target.MaxHp <= 0 ? 0f : (float)target.CurrentHp / target.MaxHp;
            TweenHp(view, targetNormalized);
        }

        private void HandleDeath(CombatUnit unit)
        {
            if (!_viewsByUnit.TryGetValue(unit, out var view))
                return;

            view.SetAliveVisual(false);
            Debug.Log($"[Combat] {unit.DisplayName} defeated");
        }

        private void HandleCombatEnd(CombatResult result)
        {
            Debug.Log($"[Combat] Ended: {result}");
            // Hooks for future features. Result overlay is wired in the Combat HUD step.
        }

        private void RefreshAllInstant()
        {
            foreach (var pair in _viewsByUnit)
                pair.Value.RefreshInstant();
        }

        private void TweenHp(CombatUnitView view, float targetNormalized)
        {
            if (_hpTweens.TryGetValue(view, out var running) && running != null)
                StopCoroutine(running);

            _hpTweens[view] = StartCoroutine(TweenHpRoutine(view, targetNormalized));
        }

        private IEnumerator TweenHpRoutine(CombatUnitView view, float targetNormalized)
        {
            var start = view.DisplayedHpNormalized;
            var duration = Mathf.Max(0.01f, _hpTweenDuration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                view.SetDisplayedHpNormalized(Mathf.Lerp(start, targetNormalized, t));
                yield return null;
            }

            view.SetDisplayedHpNormalized(targetNormalized);
            _hpTweens.Remove(view);
        }

        private void StopAllHpTweens()
        {
            foreach (var pair in _hpTweens)
            {
                if (pair.Value != null)
                    StopCoroutine(pair.Value);
            }

            _hpTweens.Clear();
        }
    }
}