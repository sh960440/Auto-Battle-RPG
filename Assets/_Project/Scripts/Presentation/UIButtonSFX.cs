using Core;
using Data;
using Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Plays a UI click clip for a Button.
    /// </summary>
    public class UIButtonSFX : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField] private UiClickKind _clickKind = UiClickKind.Default;

        private Button _button;
        private bool _armed;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _armed = _clickKind != UiClickKind.None && IsInteractable();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_armed)
                return;

            _armed = false;

            if (ServiceLocator.TryGet(out AudioService audio))
                audio.PlayUiClick(_clickKind);
        }

        private bool IsInteractable()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            return _button != null && _button.interactable;
        }
    }
}