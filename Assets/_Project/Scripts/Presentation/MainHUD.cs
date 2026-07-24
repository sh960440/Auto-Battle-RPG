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
        [SerializeField] private TMP_Text _stageLabel;
        [SerializeField] private Sprite _advanceButtonSprite;
        [SerializeField] private bool _showAdvanceLabel = true;
        [SerializeField] private string _stageFormat = "Stage {0}";

        /// <summary>
        /// Fired when the player presses Advance.
        /// </summary>
        public event Action AdvanceClicked;

        private void Awake()
        {
            EnsureButtons();
            EnsureStageLabel();
        }

        private void OnEnable()
        {
            EnsureButtons();
            EnsureStageLabel();

            if (_advanceButton != null)
                _advanceButton.onClick.AddListener(HandleAdvanceClicked);
        }

        private void OnDisable()
        {
            if (_advanceButton != null)
                _advanceButton.onClick.RemoveListener(HandleAdvanceClicked);
        }

        /// <summary>
        /// Shows the Advance button and refreshes the stage label.
        /// </summary>
        public void ShowAdvanceOnly()
        {
            EnsureButtons();
            EnsureStageLabel();
            SetButtonActive(_advanceButton, true);
            SetAdvanceInteractable(true);
        }

        /// <summary>
        /// Hides the Advance button.
        /// </summary>
        public void HideAllActions()
        {
            SetButtonActive(_advanceButton, false);
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
            EnsureStageLabel();
            if (_stageLabel != null)
                _stageLabel.text = string.Format(_stageFormat, Mathf.Max(1, stage));
        }

        private void HandleAdvanceClicked()
        {
            AdvanceClicked?.Invoke();
        }

        private static void SetButtonActive(Button button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }

        private void EnsureButtons()
        {
            if (_advanceButton == null)
                _advanceButton = CreateActionButton("AdvanceButton", "Advance", new Vector2(640f, -320f));

            ApplyAdvanceAppearance();
        }

        private void EnsureStageLabel()
        {
            if (_stageLabel != null)
                return;

            var existing = transform.Find("StageLabel");
            if (existing != null)
            {
                _stageLabel = existing.GetComponent<TMP_Text>();
                if (_stageLabel != null)
                    return;
            }

            var go = new GameObject("StageLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(400f, 60f);
            rect.anchoredPosition = new Vector2(0f, -40f);

            _stageLabel = go.GetComponent<TextMeshProUGUI>();
            _stageLabel.alignment = TextAlignmentOptions.Center;
            _stageLabel.fontSize = 36f;
            _stageLabel.color = Color.white;
            _stageLabel.text = string.Format(_stageFormat, 1);
        }

        private void ApplyAdvanceAppearance()
        {
            if (_advanceButton == null || _advanceButtonSprite == null)
                return;

            var image = _advanceButton.targetGraphic as Image;
            if (image == null)
                image = _advanceButton.GetComponent<Image>();

            if (image == null)
                return;

            image.sprite = _advanceButtonSprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }

        private Button CreateActionButton(string objectName, string label, Vector2 anchoredPosition)
        {
            var existing = transform.Find(objectName);
            if (existing != null)
            {
                var existingButton = existing.GetComponent<Button>();
                if (existingButton != null)
                    return existingButton;
            }

            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(280f, 72f);
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.18f, 0.92f);
            if (_advanceButtonSprite != null)
            {
                image.sprite = _advanceButtonSprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);

            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 28f;
            tmp.color = Color.white;
            labelGo.SetActive(_showAdvanceLabel);

            return button;
        }
    }
}