using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Visual binding for one combat participant.
    /// </summary>
    public class CombatUnitView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private Image _hpFill;
        [SerializeField] private Image _atbFill;
        [SerializeField] private Image _energyFill;
        [SerializeField] private GameObject _energyRoot;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _root;

        private CombatUnit _unit;
        private float _displayedHpNormalized = 1f;
        private bool _showEnergy;

        public CombatUnit Unit => _unit;

        public float DisplayedHpNormalized => _displayedHpNormalized;

        /// <summary>
        /// Links this view to a runtime combat unit and applies an instant refresh.
        /// </summary>
        public void Bind(CombatUnit unit)
        {
            _unit = unit;
            if (_nameLabel != null)
                _nameLabel.text = unit != null ? unit.DisplayName : string.Empty;

            SetEnergyVisible(unit != null && unit.IsPlayerSide);
            SetAliveVisual(unit == null || unit.IsAlive);
            RefreshInstant();
        }

        public void Unbind()
        {
            _unit = null;
        }

        /// <summary>
        /// Shows or hides the energy bar. Enemies should stay hidden.
        /// </summary>
        public void SetEnergyVisible(bool visible)
        {
            _showEnergy = visible;

            if (_energyRoot != null)
                _energyRoot.SetActive(visible);
            else if (_energyFill != null)
                _energyFill.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Instantly syncs all bars from the bound unit.
        /// </summary>
        public void RefreshInstant()
        {
            if (_unit == null)
                return;

            _displayedHpNormalized = GetHpNormalized(_unit);
            ApplyHpFill(_displayedHpNormalized);
            ApplyAtbFill(GetAtbNormalized(_unit));
            if (_showEnergy)
                ApplyEnergyFill(GetEnergyNormalized(_unit));
            SetAliveVisual(_unit.IsAlive);
        }

        /// <summary>
        /// Instantly syncs ATB and energy only. HP is left to tween handlers.
        /// </summary>
        public void RefreshGaugesInstant()
        {
            if (_unit == null)
                return;

            ApplyAtbFill(GetAtbNormalized(_unit));
            if (_showEnergy)
                ApplyEnergyFill(GetEnergyNormalized(_unit));
        }

        /// <summary>
        /// Sets the displayed HP normalized value.
        /// </summary>
        /// <param name="normalized">The normalized value to set.</param>
        public void SetDisplayedHpNormalized(float normalized)
        {
            _displayedHpNormalized = Mathf.Clamp01(normalized);
            ApplyHpFill(_displayedHpNormalized);
        }

        /// <summary>
        /// Sets the alive visual.
        /// </summary>
        /// <param name="alive">The alive value to set.</param>
        public void SetAliveVisual(bool alive)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = alive ? 1f : 0.35f;

            if (_root != null && !alive)
                _root.SetActive(true);
        }

        private void ApplyHpFill(float normalized)
        {
            if (_hpFill != null)
                _hpFill.fillAmount = normalized;
        }

        private void ApplyAtbFill(float normalized)
        {
            if (_atbFill != null)
                _atbFill.fillAmount = normalized;
        }

        private void ApplyEnergyFill(float normalized)
        {
            if (_energyFill != null)
                _energyFill.fillAmount = normalized;
        }

        private static float GetHpNormalized(CombatUnit unit)
        {
            return unit.MaxHp <= 0 ? 0f : (float)unit.CurrentHp / unit.MaxHp;
        }

        private static float GetAtbNormalized(CombatUnit unit)
        {
            return Mathf.Clamp01(unit.AtbGauge / CombatUnit.AtbThreshold);
        }

        private static float GetEnergyNormalized(CombatUnit unit)
        {
            return unit.MaxEnergy <= 0 ? 0f : (float)unit.Energy / unit.MaxEnergy;
        }
    }
}