using Core;
using Data;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Equipment tab: inventory slots, hover tooltip, click-to-select, equip or sell.
    /// </summary>
    public class EquipmentCenter : MonoBehaviour
    {
        private const int Columns = 6;
        private const float CellHeight = 130f;
        private const float SpacingY = 12f;
        private const float PaddingTop = 24f;
        private const float PaddingBottom = 24f;

        [Header("Inventory UI")]
        [SerializeField] private Transform _listContent;
        [SerializeField] private GameObject _itemSlotTemplate;
        [SerializeField] private GameObject _itemTooltip;
        [SerializeField] private TMP_Text _tooltipText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _sellButton;

        [Header("Equipped Slots")]
        [SerializeField] private Image _leftHandSlot;
        [SerializeField] private Image _rightHandSlot;
        [SerializeField] private Image _upperBodySlot;
        [SerializeField] private Image _lowerBodySlot;

        [Header("Optional legacy hooks")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        private PlayerProfileService _profileService;
        private EquipmentService _equipmentService;
        private bool _entryAvailable = true;
        private RectTransform _tooltipRect;
        private EquipmentInstance _selectedItem;
        private GameObject _selectedSlot;
        private TMP_Text _sellButtonLabel;
        private UnityEngine.Events.UnityAction _equipAction;
        private UnityEngine.Events.UnityAction _sellAction;

        /// <summary>
        /// Whether the equipment panel is visible.
        /// </summary>
        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        public bool IsEntryAvailable => _entryAvailable;

        private void Awake()
        {
            EnsureServices();
            _equipAction = HandleEquipSelected;
            _sellAction = HandleSellSelected;
            if (_itemSlotTemplate != null)
                _itemSlotTemplate.SetActive(false);
            HideTooltip();
            ResolveSellButtonLabel();
            WireEquippedSlotHovers();
            RefreshSelectionPanel();
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EnsureServices();
            ResolveSellButtonLabel();
            BindButtons(true);

            if (_profileService != null)
            {
                _profileService.SelectionChanged += Refresh;
                _profileService.LoadoutChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            BindButtons(false);
            HideTooltip();

            if (_profileService != null)
            {
                _profileService.SelectionChanged -= Refresh;
                _profileService.LoadoutChanged -= Refresh;
            }
        }

        private void Update()
        {
            TryClearSelectionOnOutsideClick();
            UpdateTooltipPosition();
        }

        private void UpdateTooltipPosition()
        {
            if (_itemTooltip == null || !_itemTooltip.activeSelf || _tooltipRect == null)
                return;

            var mouse = Input.mousePosition;
            var size = _tooltipRect.rect.size;
            var scale = _tooltipRect.lossyScale;
            var width = size.x * scale.x;
            var height = size.y * scale.y;

            var offsetX = 12f;
            var offsetY = 12f;
            if (mouse.x + offsetX + width > Screen.width)
                offsetX = -12f - width;
            if (mouse.y + offsetY + height > Screen.height)
                offsetY = -12f - height;

            var pos = new Vector3(mouse.x + offsetX, mouse.y + offsetY, 0f);
            pos.x = Mathf.Clamp(pos.x, 0f, Mathf.Max(0f, Screen.width - width));
            pos.y = Mathf.Clamp(pos.y, 0f, Mathf.Max(0f, Screen.height - height));
            _tooltipRect.position = pos;
        }

        private void TryClearSelectionOnOutsideClick()
        {
            if (_selectedItem == null)
                return;

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;

            var current = eventSystem.currentSelectedGameObject;
            if (ShouldKeepSelection(current))
                return;

            ClearSelection();
        }

        private bool ShouldKeepSelection(GameObject current)
        {
            if (current == null)
                return false;

            if (current == _selectedSlot)
                return true;

            if (_listContent != null && current.transform.IsChildOf(_listContent))
                return true;

            return IsUnder(current, _equipButton) || IsUnder(current, _sellButton);
        }

        private static bool IsUnder(GameObject current, Button button)
        {
            if (button == null || current == null)
                return false;

            return current == button.gameObject || current.transform.IsChildOf(button.transform);
        }

        private void ClearSelection()
        {
            _selectedItem = null;
            _selectedSlot = null;
            RefreshSelectionPanel();
        }

        /// <summary>
        /// Opens the equipment panel.
        /// </summary>
        public void Show()
        {
            if (!_entryAvailable)
                return;

            EnsureServices();

            if (_panelRoot != null)
                _panelRoot.SetActive(true);

            if (_openButton != null)
                _openButton.gameObject.SetActive(false);

            Refresh();
        }

        /// <summary>
        /// Closes the equipment panel.
        /// </summary>
        public void Hide()
        {
            HideTooltip();

            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            if (_openButton != null)
                _openButton.gameObject.SetActive(_entryAvailable);
        }

        /// <summary>
        /// Enables or disables entry into the equipment UI.
        /// </summary>
        public void SetEntryAvailable(bool available)
        {
            _entryAvailable = available;
            if (!available)
                Hide();
        }

        /// <summary>
        /// Rebuilds inventory slots for the selected character.
        /// </summary>
        public void Refresh()
        {
            EnsureServices();
            RebuildList(_profileService?.Profile, _profileService?.Profile?.SelectedCharacter);
        }

        private void EnsureServices()
        {
            if (_equipmentService == null)
                _equipmentService = new EquipmentService();

            if (_profileService != null)
                return;

            if (ServiceLocator.TryGet(out PlayerProfileService existing))
            {
                _profileService = existing;
                return;
            }

            var sheet = GetComponent<CharacterSheet>();
            sheet?.ResolveProfileService();
            ServiceLocator.TryGet(out _profileService);
        }

        private void BindButtons(bool bind)
        {
            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(Show);
                if (bind)
                    _openButton.onClick.AddListener(Show);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Hide);
                if (bind)
                    _closeButton.onClick.AddListener(Hide);
            }

            BindButton(_equipButton, _equipAction, bind);
            BindButton(_sellButton, _sellAction, bind);
        }

        private void RebuildList(PlayerProfile profile, CharacterInstance character)
        {
            if (_listContent == null || _itemSlotTemplate == null)
                return;

            ClearSpawnedSlots();

            var count = 0;
            if (profile != null)
            {
                for (var i = 0; i < profile.Inventory.Count; i++)
                {
                    var item = profile.Inventory[i];
                    if (item == null)
                        continue;

                    CreateSlot(item, character);
                    count++;
                }
            }

            ApplyContentHeight(count);
            HideTooltip();
            if (!IsInInventory(profile, _selectedItem))
            {
                _selectedItem = null;
                _selectedSlot = null;
            }

            RefreshSelectionPanel();
        }

        private void ClearSpawnedSlots()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                var child = _listContent.GetChild(i).gameObject;
                if (child == _itemSlotTemplate)
                    continue;

                Destroy(child);
            }

            _itemSlotTemplate.SetActive(false);
        }

        private void CreateSlot(EquipmentInstance item, CharacterInstance character)
        {
            var slot = Instantiate(_itemSlotTemplate, _listContent);
            slot.name = $"ItemSlot_{item.DisplayName}";
            slot.SetActive(true);

            var levelLabel = FindChildText(slot.transform, "ItemLevelText");
            if (levelLabel != null)
                levelLabel.text = item.Level.ToString();

            var icon = FindChildImage(slot.transform, "ItemIcon");
            if (icon != null)
            {
                var sprite = item.Definition != null ? item.Definition.Icon : null;
                icon.sprite = sprite;
                icon.enabled = true;
                icon.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);
            }

            var button = slot.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
                var captured = item;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => HandleSelect(captured, slot));
            }

            WireHover(slot, () => item, compareToEquipped: true);
        }

        private void WireEquippedSlotHovers()
        {
            WireEquippedSlotHover(_leftHandSlot, EquipmentSlot.LeftHand);
            WireEquippedSlotHover(_rightHandSlot, EquipmentSlot.RightHand);
            WireEquippedSlotHover(_upperBodySlot, EquipmentSlot.UpperBody);
            WireEquippedSlotHover(_lowerBodySlot, EquipmentSlot.LowerBody);
        }

        private void WireEquippedSlotHover(Image slot, EquipmentSlot equipmentSlot)
        {
            if (slot == null)
                return;

            slot.raycastTarget = true;
            WireHover(slot.gameObject, () =>
            {
                var character = _profileService?.Profile?.SelectedCharacter;
                if (character == null || !character.TryGetEquipment(equipmentSlot, out var item))
                    return null;

                return item;
            }, compareToEquipped: false);
        }

        private void WireHover(GameObject slot, System.Func<EquipmentInstance> resolveItem, bool compareToEquipped)
        {
            var trigger = slot.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = slot.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ =>
            {
                var item = resolveItem != null ? resolveItem() : null;
                if (item == null)
                {
                    HideTooltip();
                    return;
                }

                ShowTooltip(item, _profileService?.Profile?.SelectedCharacter, compareToEquipped);
            });
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => HideTooltip());
            trigger.triggers.Add(exit);
        }

        private void ShowTooltip(EquipmentInstance item, CharacterInstance character, bool compareToEquipped)
        {
            if (_itemTooltip == null || _tooltipText == null || item == null)
                return;

            _tooltipText.text = BuildTooltip(item, character, compareToEquipped);
            if (_tooltipRect == null)
                _tooltipRect = _itemTooltip.GetComponent<RectTransform>();

            _itemTooltip.SetActive(true);
        }

        private void HideTooltip()
        {
            if (_itemTooltip != null)
                _itemTooltip.SetActive(false);
        }

        private void HandleSelect(EquipmentInstance item, GameObject slot)
        {
            _selectedItem = item;
            _selectedSlot = slot;
            RefreshSelectionPanel();
        }

        private void HandleEquipSelected()
        {
            EnsureServices();
            var profile = _profileService?.Profile;
            var character = profile?.SelectedCharacter;
            if (profile == null || character == null || _selectedItem == null)
                return;

            if (!_equipmentService.TryEquipFromInventory(profile, character, _selectedItem))
                return;

            _selectedItem = null;
            _selectedSlot = null;
            HideTooltip();
            _profileService.NotifyLoadoutChanged();
            if (ServiceLocator.TryGet(out SaveSystem saveSystem))
                saveSystem.SaveCurrent();
            Refresh();
        }

        private void HandleSellSelected()
        {
            EnsureServices();
            var profile = _profileService?.Profile;
            if (profile == null || _selectedItem == null)
                return;

            if (!_equipmentService.TrySellFromInventory(profile, _selectedItem))
                return;

            _selectedItem = null;
            _selectedSlot = null;
            HideTooltip();
            _profileService.NotifyLoadoutChanged();
            if (ServiceLocator.TryGet(out SaveSystem saveSystem))
                saveSystem.SaveCurrent();
            Refresh();
        }

        private void RefreshSelectionPanel()
        {
            ResolveSellButtonLabel();

            var item = _selectedItem;
            if (item == null)
            {
                if (_descriptionText != null)
                    _descriptionText.text = string.Empty;
                SetButtonVisible(_equipButton, false);
                SetButtonVisible(_sellButton, false);
                return;
            }

            if (_descriptionText != null)
                _descriptionText.text = $"{item.DisplayName}\nLv{item.Level}";

            SetButtonVisible(_equipButton, true);
            SetButtonVisible(_sellButton, true);

            var profile = _profileService?.Profile;
            var character = profile?.SelectedCharacter;
            if (_equipButton != null)
            {
                _equipButton.interactable = _equipmentService != null &&
                                           _equipmentService.CanEquipFromInventory(profile, character, item);
            }

            if (_sellButton != null)
                _sellButton.interactable = _equipmentService != null &&
                                           _equipmentService.CanSellFromInventory(profile, item);

            if (_sellButtonLabel != null)
                _sellButtonLabel.text = $"Sell ({EquipmentSellRules.GetSellPrice(item)})";
        }

        private void ResolveSellButtonLabel()
        {
            if (_sellButtonLabel == null && _sellButton != null)
                _sellButtonLabel = _sellButton.GetComponentInChildren<TMP_Text>(true);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action, bool bind)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveListener(action);
            if (bind)
                button.onClick.AddListener(action);
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
                button.gameObject.SetActive(visible);
        }

        private static bool IsInInventory(PlayerProfile profile, EquipmentInstance item)
        {
            if (profile == null || item == null)
                return false;

            var inventory = profile.Inventory;
            for (var i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == item)
                    return true;
            }

            return false;
        }

        private void ApplyContentHeight(int itemCount)
        {
            var contentRect = _listContent as RectTransform;
            if (contentRect == null)
                return;

            var rows = itemCount <= 0 ? 1 : Mathf.CeilToInt(itemCount / (float)Columns);
            var height = PaddingTop + PaddingBottom + rows * CellHeight + Mathf.Max(0, rows - 1) * SpacingY;
            var size = contentRect.sizeDelta;
            size.y = Mathf.Max(height, CellHeight + PaddingTop + PaddingBottom);
            contentRect.sizeDelta = size;
        }

        private static TMP_Text FindChildText(Transform root, string childName)
        {
            var child = root.Find(childName);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        private static Image FindChildImage(Transform root, string childName)
        {
            var child = root.Find(childName);
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static string BuildTooltip(EquipmentInstance item, CharacterInstance character, bool compareToEquipped)
        {
            var text =
                $"{item.DisplayName} Lv{item.Level}\n" +
                $"{CharacterClassRules.GetDisplayName(item.RequiredClass)} - {item.Quality}\n" +
                $"{item.GetMainStat()} {item.MainStatType}";

            if (!compareToEquipped || character == null)
                return text;

            var current = 0;
            if (character.TryGetEquipment(item.Slot, out var equipped) && equipped != null)
                current = equipped.GetMainStat();

            var delta = item.GetMainStat() - current;
            var deltaText = delta >= 0 ? $"+{delta}" : delta.ToString();
            return text + $"\nΔ {deltaText} {item.MainStatType}";
        }
    }
}