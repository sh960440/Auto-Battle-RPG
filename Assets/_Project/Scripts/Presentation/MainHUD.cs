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
        [SerializeField] private Sprite _advanceButtonSprite;
        [SerializeField] private bool _showAdvanceLabel = true;

        /// <summary>
        /// Fired when the player presses Advance.
        /// </summary>
        public event Action AdvanceClicked;

        private void Awake()
        {
            EnsureButtons();
        }

        private void OnEnable()
        {
            EnsureButtons();

            if (_advanceButton != null)
                _advanceButton.onClick.AddListener(HandleAdvanceClicked);
        }

        private void OnDisable()
        {
            if (_advanceButton != null)
                _advanceButton.onClick.RemoveListener(HandleAdvanceClicked);
        }

        /// <summary>
        /// Shows the Advance button.
        /// </summary>
        public void ShowAdvanceOnly()
        {
            EnsureButtons();
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