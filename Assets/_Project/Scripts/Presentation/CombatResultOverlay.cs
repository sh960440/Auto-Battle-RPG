using System;
using Combat;
using Core;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Temporary result popup
    /// </summary>
    public class CombatResultOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private TMP_Text _stageLabel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _returnButton;
        [SerializeField] private string _victoryText = "Victory";
        [SerializeField] private string _defeatText = "Defeat";
        [SerializeField] private string _continueLabel = "Continue";
        [SerializeField] private string _returnLabel = "Character";

        private CombatSimulator _simulator;

        /// <summary>
        /// Fired when the player chooses to continue exploring.
        /// </summary>
        public event Action ContinueRequested;

        /// <summary>
        /// Fired when the player chooses to return to the character page / main menu.
        /// </summary>
        public event Action ReturnRequested;

        /// <summary>
        /// Alias for <see cref="ContinueRequested"/>.
        /// </summary>
        public event Action Continued
        {
            add => ContinueRequested += value;
            remove => ContinueRequested -= value;
        }

        private void Awake()
        {
            EnsureReturnButton();
            Hide();
        }

        private void OnEnable()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(HandleContinue);

            if (_returnButton != null)
                _returnButton.onClick.AddListener(HandleReturn);

            // Only re-show for a still-bound finished combat. Do not use the persisted
            // stage result here, or a fresh combat would immediately pop the overlay.
            if (_simulator != null && _simulator.IsFinished && _simulator.Result.HasValue)
                Show(_simulator.Result.Value);
        }

        private void OnDisable()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(HandleContinue);

            if (_returnButton != null)
                _returnButton.onClick.RemoveListener(HandleReturn);
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
            EnsureReturnButton();
            gameObject.SetActive(true);

            if (_root != null)
                _root.SetActive(true);

            if (_resultLabel != null)
                _resultLabel.text = result == CombatResult.Victory ? _victoryText : _defeatText;

            if (_stageLabel != null && ServiceLocator.TryGet(out StageProgressService progress))
            {
                _stageLabel.text = $"Stage {progress.StageBeforeLastChange} → {progress.CurrentStage}";
            }

            SetButtonLabel(_continueButton, _continueLabel);
            SetButtonLabel(_returnButton, _returnLabel);
        }

        private void Hide()
        {
            if (_root != null)
                _root.SetActive(false);
        }

        private void HandleContinue()
        {
            ContinueRequested?.Invoke();
            Hide();
            UnbindCombatEvents();
        }

        private void HandleReturn()
        {
            ReturnRequested?.Invoke();
            Hide();
            UnbindCombatEvents();
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        private void EnsureReturnButton()
        {
            if (_returnButton != null || _continueButton == null)
                return;

            var existing = transform.Find("ReturnButton");
            if (existing != null)
            {
                _returnButton = existing.GetComponent<Button>();
                if (_returnButton != null)
                    return;
            }

            var template = _continueButton.gameObject;
            var clone = Instantiate(template, template.transform.parent);
            clone.name = "ReturnButton";

            var rect = clone.GetComponent<RectTransform>();
            if (rect != null)
            {
                var continueRect = template.GetComponent<RectTransform>();
                if (continueRect != null)
                    rect.anchoredPosition = continueRect.anchoredPosition + new Vector2(0f, -90f);
            }

            _returnButton = clone.GetComponent<Button>();
            SetButtonLabel(_returnButton, _returnLabel);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null || string.IsNullOrEmpty(label))
                return;

            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
                tmp.text = label;
        }
    }
}