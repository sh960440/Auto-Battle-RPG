using System;
using Combat;
using Data;
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

        /// <summary>
        /// Fired when the player confirms the result and returns to exploration.
        /// </summary>
        public event Action ContinueRequested;

        private void OnEnable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(HandleConfirm);
        }

        private void OnDisable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(HandleConfirm);
        }

        private void OnDestroy()
        {
            ContinueRequested = null;
        }

        /// <summary>
        /// Hides the overlay at the start of a combat.
        /// </summary>
        public void Bind(CombatSimulator simulator)
        {
            _ = simulator;
            Hide();
        }

        /// <summary>
        /// Hides the overlay.
        /// </summary>
        public void Unbind()
        {
            Hide();
        }

        /// <summary>
        /// Shows result and loot.
        /// </summary>
        public void Show(CombatResult result, LootDrop drop)
        {
            var window = _root != null ? _root : gameObject;
            window.SetActive(true);

            var victory = result == CombatResult.Victory;
            if (_victoryText != null)
                _victoryText.SetActive(victory);
            if (_defeatText != null)
                _defeatText.SetActive(!victory);

            if (_lootLabel == null)
                return;

            _lootLabel.enabled = true;
            _lootLabel.gameObject.SetActive(true);
            _lootLabel.text = victory && drop != null
                ? drop.FormatSummary()
                : "No rewards";
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
        }
    }
}