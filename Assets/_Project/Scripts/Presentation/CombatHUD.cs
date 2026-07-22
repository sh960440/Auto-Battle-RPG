using Combat;
using Data;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Combat screen HUD.
    /// </summary>
    public class CombatHUD : MonoBehaviour
    {
        [SerializeField] private CombatUnitView _playerView;
        [SerializeField] private CombatUnitView[] _enemyViews;
        [SerializeField] private CombatSkillButton _skillButton;
        [SerializeField] private DamageFloatPresenter _damageFloatPresenter;
        [SerializeField] private CombatResultOverlay _resultOverlay;

        private void Awake()
        {
            // Do not unbind here: the HUD is bound by CombatController before it is shown,
            // and this Awake runs on first activation (which would wipe that binding).
            ApplyBarVisibilityRules();
        }

        /// <summary>
        /// Binds HUD controls to a running combat session.
        /// </summary>
        public void Bind(CombatSimulator simulator, SkillDefinition playerSkill)
        {
            ApplyBarVisibilityRules();
            _skillButton?.Bind(simulator, playerSkill);
            _damageFloatPresenter?.Bind(simulator);
            _resultOverlay?.Bind(simulator);
        }

        /// <summary>
        /// Clears combat-only HUD bindings.
        /// </summary>
        public void Unbind()
        {
            _skillButton?.Unbind();
            _damageFloatPresenter?.Unbind();
            _resultOverlay?.Unbind();
        }

        /// <summary>
        /// Applies the current bar layout rules.
        /// </summary>
        public void ApplyBarVisibilityRules()
        {
            if (_playerView != null)
                _playerView.SetEnergyVisible(true);

            if (_enemyViews == null)
                return;

            for (var i = 0; i < _enemyViews.Length; i++)
            {
                if (_enemyViews[i] != null)
                    _enemyViews[i].SetEnergyVisible(false);
            }
        }
    }
}