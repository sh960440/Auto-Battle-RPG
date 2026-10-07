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
    /// Equipment tab: inventory slots, hover compare, click-to-equip.
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

        [Header("Optional legacy hooks")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        private PlayerProfileService _profileService;
        private EquipmentService _equipmentService;
        private bool _entryAvailable = true;
        private RectTransform _tooltipRect;

        /// <summary>
        /// Whether the equipment panel is visible.
        /// </summary>
        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        public bool IsEntryAvailable => _entryAvailable;

        private void Awake()
        {
            EnsureServices();
            if (_itemSlotTemplate != null)
                _itemSlotTemplate.SetActive(false);
            HideTooltip();
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EnsureServices();
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
            if (_descriptionText != null && (profile == null || count == 0))
                _descriptionText.text = string.Empty;
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

            var canEquip = character != null &&
                           _equipmentService.CanEquipFromInventory(_profileService.Profile, character, item);

            var button = slot.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = canEquip;
                var captured = item;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => HandleEquip(captured));
            }

            WireHover(slot, item, character);
        }

        private void WireHover(GameObject slot, EquipmentInstance item, CharacterInstance character)
        {
            var trigger = slot.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = slot.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => ShowItemDetails(item, character));
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => HideTooltip());
            trigger.triggers.Add(exit);
        }

        private void ShowItemDetails(EquipmentInstance item, CharacterInstance character)
        {
            if (_descriptionText != null)
            {
                _descriptionText.text =
                    $"{item.DisplayName}\n" +
                    $"Slot: {item.Slot}\n" +
                    $"Class: {CharacterClassRules.GetDisplayName(item.RequiredClass)}\n" +
                    $"Lv{item.Level}  {item.Quality}  +{item.GetMainStat()} {item.MainStatType}";
            }

            if (_itemTooltip == null || _tooltipText == null)
                return;

            _tooltipText.text = BuildTooltip(item, character);
            if (_tooltipRect == null)
                _tooltipRect = _itemTooltip.GetComponent<RectTransform>();

            _itemTooltip.SetActive(true);
        }

        private void HideTooltip()
        {
            if (_itemTooltip != null)
                _itemTooltip.SetActive(false);
        }

        private void HandleEquip(EquipmentInstance item)
        {
            EnsureServices();
            var profile = _profileService?.Profile;
            var character = profile?.SelectedCharacter;
            if (profile == null || character == null)
                return;

            if (!_equipmentService.TryEquipFromInventory(profile, character, item))
                return;

            HideTooltip();
            _profileService.NotifyLoadoutChanged();
            if (ServiceLocator.TryGet(out SaveSystem saveSystem))
                saveSystem.SaveCurrent();
            Refresh();
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

        private static string BuildTooltip(EquipmentInstance item, CharacterInstance character)
        {
            var main = $"+{item.GetMainStat()} {item.MainStatType}";
            var header =
                $"{item.DisplayName}\n" +
                $"{item.Slot}  |  {CharacterClassRules.GetDisplayName(item.RequiredClass)}\n" +
                $"Lv{item.Level}  {item.Quality}  {main}";

            if (character == null)
                return header;

            if (!character.CanEquip(item))
                return header + "\nWrong class.";

            if (character.TryGetEquipment(item.Slot, out var equipped))
            {
                var delta = item.GetMainStat() - equipped.GetMainStat();
                var deltaText = delta >= 0 ? $"+{delta}" : delta.ToString();
                return header +
                       $"\nEquipped: {equipped.DisplayName} Lv{equipped.Level} +{equipped.GetMainStat()} {equipped.MainStatType}" +
                       $"\nΔ {deltaText} {item.MainStatType}";
            }

            return header + $"\nEquipped: (empty)\n→ {main}";
        }
    }
}