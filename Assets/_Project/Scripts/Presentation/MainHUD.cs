using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Exploration HUD.
    /// </summary>
    public class MainHUD : MonoBehaviour
    {
        [SerializeField] private Button _advanceButton;
        [SerializeField] private Button _upgradeCenterButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TMP_Text _stageLabel;
        [SerializeField] private GameObject _coinPanel;
        [SerializeField] private TMP_Text _coinAmountText;
        [SerializeField] private string _stageFormat = "{0}";

        /// <summary>
        /// Fired when the player presses Advance.
        /// </summary>
        public event Action AdvanceClicked;

        /// <summary>
        /// Fired when the player leaves Gameplay for the upgrade center.
        /// </summary>
        public event Action UpgradeCenterClicked;

        /// <summary>
        /// Fired when the player leaves Gameplay for the main menu.
        /// </summary>
        public event Action MainMenuClicked;

        private void OnEnable()
        {
            if (_advanceButton != null)
                _advanceButton.onClick.AddListener(HandleAdvanceClicked);
            if (_upgradeCenterButton != null)
                _upgradeCenterButton.onClick.AddListener(HandleUpgradeCenterClicked);
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }

        private void OnDisable()
        {
            if (_advanceButton != null)
                _advanceButton.onClick.RemoveListener(HandleAdvanceClicked);
            if (_upgradeCenterButton != null)
                _upgradeCenterButton.onClick.RemoveListener(HandleUpgradeCenterClicked);
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }

        /// <summary>
        /// Shows exploration actions and the coin panel.
        /// </summary>
        public void ShowAdvanceOnly()
        {
            SetButtonActive(_advanceButton, true);
            SetAdvanceInteractable(true);
            SetLeaveButtonsVisible(true);
            SetCoinVisible(true);
        }

        /// <summary>
        /// Hides Advance, leave buttons, and coins.
        /// </summary>
        public void HideAllActions()
        {
            SetButtonActive(_advanceButton, false);
            SetLeaveButtonsVisible(false);
            SetCoinVisible(false);
        }

        /// <summary>
        /// Hides leave buttons while Advance is in progress.
        /// </summary>
        public void SetPartyMenusAvailable(bool available)
        {
            SetLeaveButtonsVisible(available);
        }

        /// <summary>
        /// Enables or disables the Advance button.
        /// </summary>
        public void SetAdvanceInteractable(bool interactable)
        {
            if (_advanceButton != null)
                _advanceButton.interactable = interactable;
        }

        /// <summary>
        /// Updates the displayed stage number.
        /// </summary>
        public void SetStage(int stage)
        {
            if (_stageLabel != null)
                _stageLabel.text = string.Format(_stageFormat, Mathf.Max(1, stage));
        }

        /// <summary>
        /// Updates the displayed gold amount.
        /// </summary>
        public void SetGold(int gold)
        {
            if (_coinAmountText != null)
                _coinAmountText.text = Mathf.Max(0, gold).ToString();
        }

        private void HandleAdvanceClicked() => AdvanceClicked?.Invoke();

        private void HandleUpgradeCenterClicked() => UpgradeCenterClicked?.Invoke();

        private void HandleMainMenuClicked() => MainMenuClicked?.Invoke();

        private void SetLeaveButtonsVisible(bool visible)
        {
            SetButtonActive(_upgradeCenterButton, visible);
            SetButtonActive(_mainMenuButton, visible);
        }

        private void SetCoinVisible(bool visible)
        {
            if (_coinPanel != null)
                _coinPanel.SetActive(visible);
        }

        private static void SetButtonActive(Button button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }
    }
}