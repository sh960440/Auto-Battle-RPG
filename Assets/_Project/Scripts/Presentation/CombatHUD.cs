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
        [SerializeField] private GameObject _skill2Root;
        [SerializeField] private CombatSkillButton _skill2Button;
        [SerializeField] private DamageFloatPresenter _damageFloatPresenter;
        [SerializeField] private CombatResultOverlay _resultOverlay;

        private void Awake()
        {
            // Do not unbind skill1 here: CombatController may bind before first activation.
            ApplyBarVisibilityRules();
            LockSecondarySkill();
        }

        /// <summary>
        /// Binds HUD controls to a running combat session.
        /// </summary>
        public void Bind(CombatSimulator simulator, SkillDefinition playerSkill)
        {
            ApplyBarVisibilityRules();
            LockSecondarySkill();
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
            _skill2Button?.Unbind();
            _damageFloatPresenter?.Unbind();
            _resultOverlay?.Unbind();
            LockSecondarySkill();
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

        private void LockSecondarySkill()
        {
            if (_skill2Button != null)
                _skill2Button.SetLocked("Locked");

            if (_skill2Root == null)
                return;

            var canvasGroup = _skill2Root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = _skill2Root.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0.45f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}