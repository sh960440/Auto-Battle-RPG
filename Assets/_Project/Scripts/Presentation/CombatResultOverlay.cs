using System;
using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Combat result window.
    /// </summary>
    public class CombatResultOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _victoryText;
        [SerializeField] private GameObject _defeatText;
        [SerializeField] private TMP_Text _lootLabel;
        [SerializeField] private Button _confirmButton;

        private CombatSimulator _simulator;

        /// <summary>
        /// Fired when the player confirms the result and returns to exploration.
        /// </summary>
        public event Action ContinueRequested;

        private void OnEnable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(HandleConfirm);

            if (_simulator != null && _simulator.IsFinished && _simulator.Result.HasValue)
                Show(_simulator.Result.Value);
        }

        private void OnDisable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(HandleConfirm);
        }

        /// <summary>
        /// Binds the overlay to a running combat.
        /// </summary>
        public void Bind(CombatSimulator simulator)
        {
            UnbindCombatEvents();

            _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
            _simulator.OnCombatEnd += HandleCombatEnd;
            Hide();

            if (_simulator.IsFinished && _simulator.Result.HasValue)
                Show(_simulator.Result.Value);
        }

        /// <summary>
        /// Clears combat bindings and hides the overlay.
        /// </summary>
        public void Unbind()
        {
            UnbindCombatEvents();
            Hide();
        }

        private void OnDestroy()
        {
            UnbindCombatEvents();
        }

        private void UnbindCombatEvents()
        {
            if (_simulator != null)
            {
                _simulator.OnCombatEnd -= HandleCombatEnd;
                _simulator = null;
            }
        }

        private void HandleCombatEnd(CombatResult result)
        {
            Show(result);
        }

        private void Show(CombatResult result)
        {
            var window = _root != null ? _root : gameObject;
            window.SetActive(true);

            var victory = result == CombatResult.Victory;
            if (_victoryText != null)
                _victoryText.SetActive(victory);
            if (_defeatText != null)
                _defeatText.SetActive(!victory);

            if (_lootLabel != null)
                _lootLabel.enabled = true;
        }

        private void Hide()
        {
            var window = _root != null ? _root : gameObject;
            window.SetActive(false);
        }

        private void HandleConfirm()
        {
            ContinueRequested?.Invoke();
            Hide();
            UnbindCombatEvents();
        }
    }
}