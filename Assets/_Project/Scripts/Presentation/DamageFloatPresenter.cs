using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Spawns damage popups above combat units.
    /// </summary>
    public class DamageFloatPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform _popupLayer;
        [SerializeField] private CombatUnitView _playerView;
        [SerializeField] private CombatUnitView[] _enemyViews;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 40f, 0f);

        private CombatSimulator _simulator;
        private readonly Dictionary<CombatUnit, CombatUnitView> _viewsByUnit = new();

        /// <summary>
        /// Binds damage popups to a running combat session.
        /// </summary>
        public void Bind(CombatSimulator simulator)
        {
            Unbind();

            _simulator = simulator ?? throw new System.ArgumentNullException(nameof(simulator));
            MapViews(_simulator.Context);
            _simulator.OnDamage += HandleDamage;
        }

        /// <summary>
        /// Clears combat bindings and disables the button.
        /// </summary>
        public void Unbind()
        {
            if (_simulator != null)
            {
                _simulator.OnDamage -= HandleDamage;
                _simulator = null;
            }

            _viewsByUnit.Clear();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void MapViews(CombatContext context)
        {
            _viewsByUnit.Clear();

            if (_playerView != null)
                _viewsByUnit[context.Player] = _playerView;

            var enemyIndex = 0;
            for (var i = 0; i < context.Units.Count; i++)
            {
                var unit = context.Units[i];
                if (unit.IsPlayerSide)
                    continue;

                if (_enemyViews == null || enemyIndex >= _enemyViews.Length)
                    break;

                var view = _enemyViews[enemyIndex++];
                if (view != null)
                    _viewsByUnit[unit] = view;
            }
        }

        private void HandleDamage(CombatUnit attacker, CombatUnit target, int damage)
        {
            if (_popupLayer == null)
                return;

            if (!_viewsByUnit.TryGetValue(target, out var view) || view == null)
                return;

            var spawnPos = view.transform.position + _worldOffset;
            DamageFloatPopup.CreateRuntime(_popupLayer, spawnPos, damage);
        }
    }
}