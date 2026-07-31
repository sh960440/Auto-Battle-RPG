using Core;
using Data;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Equipment center: inventory list, class filter, compare to equipped.
    /// </summary>
    public class EquipmentCenter : MonoBehaviour
    {
        private enum ClassFilter
        {
            All = 0,
            SelectedCharacter = 1,
            Knight = 2,
            Swordsman = 3,
            ShieldGuard = 4
        }

        [Header("Starter Inventory (optional)")]
        [SerializeField] private EquipmentDefinition[] _starterEquipment;
        [SerializeField] private int _starterItemStage = 5;

        [Header("Optional UI Hooks")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _filterButton;
        [SerializeField] private TMP_Text _headerLabel;
        [SerializeField] private TMP_Text _filterLabel;
        [SerializeField] private Transform _listContent;

        private PlayerProfileService _profileService;
        private EquipmentService _equipmentService;
        private ClassFilter _filter = ClassFilter.SelectedCharacter;
        private bool _uiBuilt;
        private bool _inventorySeeded;
        private bool _entryAvailable = true;

        /// <summary>
        /// Whether the equipment panel is visible.
        /// </summary>
        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        public bool IsEntryAvailable => _entryAvailable;

        private void Awake()
        {
            EnsureServices();
            EnsureUi();
            Hide();
        }

        private void OnEnable()
        {
            EnsureServices();
            EnsureUi();
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

            if (_profileService != null)
            {
                _profileService.SelectionChanged -= Refresh;
                _profileService.LoadoutChanged -= Refresh;
            }
        }

        /// <summary>
        /// Opens the equipment center.
        /// </summary>
        public void Show()
        {
            if (!_entryAvailable)
                return;

            EnsureServices();
            EnsureUi();
            SeedInventoryIfNeeded();

            var sheet = GetComponent<CharacterSheet>();
            if (sheet != null && sheet.IsOpen)
                sheet.Hide();

            if (_panelRoot != null)
                _panelRoot.SetActive(true);

            if (_openButton != null)
                _openButton.gameObject.SetActive(false);

            Refresh();
        }

        /// <summary>
        /// Closes the equipment center.
        /// </summary>
        public void Hide()
        {
            EnsureUi();

            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            RefreshOpenButtonVisibility();
        }

        /// <summary>
        /// Enables or disables the main HUD Equipment entry.
        /// </summary>
        public void SetEntryAvailable(bool available)
        {
            _entryAvailable = available;
            EnsureUi();

            if (!available && _panelRoot != null)
                _panelRoot.SetActive(false);

            RefreshOpenButtonVisibility();
        }

        private void RefreshOpenButtonVisibility()
        {
            if (_openButton == null)
                return;

            var panelOpen = _panelRoot != null && _panelRoot.activeSelf;
            _openButton.gameObject.SetActive(_entryAvailable && !panelOpen);
        }

        /// <summary>
        /// Rebuilds the filtered inventory list.
        /// </summary>
        public void Refresh()
        {
            EnsureServices();
            EnsureUi();
            SeedInventoryIfNeeded();

            var profile = _profileService?.Profile;
            var character = profile?.SelectedCharacter;

            if (_headerLabel != null)
            {
                _headerLabel.text = character != null
                    ? $"Equipment  →  {character.DisplayName} ({CharacterClassRules.GetDisplayName(character.CharacterClass)})"
                    : "Equipment";
            }

            if (_filterLabel != null)
                _filterLabel.text = $"Filter: {GetFilterLabel(_filter)}";

            RebuildList(profile, character);
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

            // Do not register an empty profile here — CharacterSheet owns roster bootstrap.
            var sheet = GetComponent<CharacterSheet>();
            if (sheet != null)
                sheet.EnsureProfileServiceForDependents();

            ServiceLocator.TryGet(out _profileService);
        }

        private void SeedInventoryIfNeeded()
        {
            if (_inventorySeeded || _profileService?.Profile == null)
                return;

            var profile = _profileService.Profile;
            if (profile.Inventory.Count > 0)
            {
                _inventorySeeded = true;
                return;
            }

            if (_starterEquipment != null && _starterEquipment.Length > 0)
            {
                for (var i = 0; i < _starterEquipment.Length; i++)
                {
                    if (_starterEquipment[i] == null)
                        continue;

                    profile.AddToInventory(
                        EquipmentInstance.CreateForStage(_starterEquipment[i], _starterItemStage));
                }
            }
            else
            {
                SeedFallbackInventory(profile);
            }

            _inventorySeeded = true;
            _profileService.NotifyLoadoutChanged();
        }

        private void SeedFallbackInventory(PlayerProfile profile)
        {
            var stage = Mathf.Max(1, _starterItemStage);
            var defs = new[]
            {
                EquipmentDefinition.CreateRuntime("Knight Sidearm", EquipmentSlot.LeftHand, CharacterClass.Knight, 2, quality: EquipmentQuality.Common),
                EquipmentDefinition.CreateRuntime("Knight Longsword", EquipmentSlot.RightHand, CharacterClass.Knight, 5, quality: EquipmentQuality.Uncommon),
                EquipmentDefinition.CreateRuntime("Padded Coat", EquipmentSlot.UpperBody, CharacterClass.Knight, 3, quality: EquipmentQuality.Uncommon),
                EquipmentDefinition.CreateRuntime("Leather Pants", EquipmentSlot.LowerBody, CharacterClass.Knight, 10, quality: EquipmentQuality.Common),
                EquipmentDefinition.CreateRuntime("Parrying Dagger", EquipmentSlot.LeftHand, CharacterClass.Swordsman, 2, quality: EquipmentQuality.Common),
                EquipmentDefinition.CreateRuntime("Iron Sword", EquipmentSlot.RightHand, CharacterClass.Swordsman, 5, quality: EquipmentQuality.Uncommon),
                EquipmentDefinition.CreateRuntime("Silk Mantle", EquipmentSlot.UpperBody, CharacterClass.Swordsman, 5, quality: EquipmentQuality.Rare),
                EquipmentDefinition.CreateRuntime("Runed Pants", EquipmentSlot.LowerBody, CharacterClass.Swordsman, 16, quality: EquipmentQuality.Rare),
                EquipmentDefinition.CreateRuntime("Wooden Shield", EquipmentSlot.LeftHand, CharacterClass.ShieldGuard, 2, quality: EquipmentQuality.Common),
                EquipmentDefinition.CreateRuntime("Tower Mace", EquipmentSlot.RightHand, CharacterClass.ShieldGuard, 5, quality: EquipmentQuality.Uncommon),
                EquipmentDefinition.CreateRuntime("Chain Mail", EquipmentSlot.UpperBody, CharacterClass.ShieldGuard, 4, quality: EquipmentQuality.Uncommon),
                EquipmentDefinition.CreateRuntime("Tower Greaves", EquipmentSlot.LowerBody, CharacterClass.ShieldGuard, 14, quality: EquipmentQuality.Uncommon)
            };

            for (var i = 0; i < defs.Length; i++)
                profile.AddToInventory(EquipmentInstance.CreateForStage(defs[i], stage));
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

            if (_filterButton != null)
            {
                _filterButton.onClick.RemoveListener(CycleFilter);
                if (bind)
                    _filterButton.onClick.AddListener(CycleFilter);
            }
        }

        private void CycleFilter()
        {
            _filter = (ClassFilter)(((int)_filter + 1) % 5);
            Refresh();
        }

        private void RebuildList(PlayerProfile profile, CharacterInstance character)
        {
            if (_listContent == null)
                return;

            for (var i = _listContent.childCount - 1; i >= 0; i--)
                Destroy(_listContent.GetChild(i).gameObject);

            if (profile == null)
                return;

            var rows = 0;
            for (var i = 0; i < profile.Inventory.Count; i++)
            {
                var item = profile.Inventory[i];
                if (item == null || !PassesFilter(item, character))
                    continue;

                CreateRow(item, character, rows);
                rows++;
            }

            if (rows == 0)
            {
                var empty = CreateRowLabel(_listContent, "EmptyLabel", "No items for this filter.", 20f);
                empty.alignment = TextAlignmentOptions.Center;
                var emptyRect = empty.rectTransform;
                emptyRect.anchorMin = new Vector2(0f, 1f);
                emptyRect.anchorMax = new Vector2(1f, 1f);
                emptyRect.pivot = new Vector2(0.5f, 1f);
                emptyRect.sizeDelta = new Vector2(0f, 40f);
                emptyRect.anchoredPosition = new Vector2(0f, -8f);
            }

            var contentRect = _listContent as RectTransform;
            if (contentRect != null)
                contentRect.sizeDelta = new Vector2(0f, Mathf.Max(360f, rows * 96f + 16f));
        }

        private bool PassesFilter(EquipmentInstance item, CharacterInstance character)
        {
            return _filter switch
            {
                ClassFilter.All => true,
                ClassFilter.SelectedCharacter => character != null && item.RequiredClass == character.CharacterClass,
                ClassFilter.Knight => item.RequiredClass == CharacterClass.Knight,
                ClassFilter.Swordsman => item.RequiredClass == CharacterClass.Swordsman,
                ClassFilter.ShieldGuard => item.RequiredClass == CharacterClass.ShieldGuard,
                _ => true
            };
        }

        private void CreateRow(EquipmentInstance item, CharacterInstance character, int index)
        {
            var row = new GameObject($"Item_{index}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            row.transform.SetParent(_listContent, false);

            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.sizeDelta = new Vector2(0f, 88f);
            rowRect.anchoredPosition = new Vector2(0f, -8f - index * 96f);

            row.GetComponent<Image>().color = new Color(0.14f, 0.15f, 0.2f, 1f);

            var info = CreateRowLabel(row.transform, "Info", BuildItemInfo(item, character), 18f);
            info.alignment = TextAlignmentOptions.TopLeft;
            var infoRect = info.rectTransform;
            infoRect.anchorMin = new Vector2(0f, 0f);
            infoRect.anchorMax = new Vector2(1f, 1f);
            infoRect.offsetMin = new Vector2(12f, 8f);
            infoRect.offsetMax = new Vector2(-130f, -8f);

            var canEquip = character != null &&
                           _equipmentService.CanEquipFromInventory(_profileService.Profile, character, item);

            var equipButton = CreateRowButton(row.transform, "EquipButton", canEquip ? "Equip" : "Locked", new Vector2(-12f, 0f));
            equipButton.interactable = canEquip;
            if (canEquip)
            {
                var captured = item;
                equipButton.onClick.AddListener(() => HandleEquip(captured));
            }
        }

        private void HandleEquip(EquipmentInstance item)
        {
            EnsureServices();
            var profile = _profileService.Profile;
            var character = profile.SelectedCharacter;
            if (character == null)
                return;

            if (!_equipmentService.TryEquipFromInventory(profile, character, item))
                return;

            _profileService.NotifyLoadoutChanged();
            Refresh();
        }

        private static string BuildItemInfo(EquipmentInstance item, CharacterInstance character)
        {
            var main = $"+{item.GetMainStat()} {item.MainStatType}";
            var line1 =
                $"{item.DisplayName}  |  {item.Slot}  |  {CharacterClassRules.GetDisplayName(item.RequiredClass)}  |  " +
                $"Lv{item.Level}  |  {item.Quality}  |  {main}";

            if (character == null)
                return line1;

            if (!character.CanEquip(item))
                return line1 + "\nWrong class for selected character.";

            if (character.TryGetEquipment(item.Slot, out var equipped))
            {
                var delta = item.GetMainStat() - equipped.GetMainStat();
                var deltaText = delta >= 0 ? $"+{delta}" : delta.ToString();
                return line1 +
                       $"\nEquipped: {equipped.DisplayName} Lv{equipped.Level} +{equipped.GetMainStat()} {equipped.MainStatType}" +
                       $"  →  Δ {deltaText} {item.MainStatType}";
            }

            return line1 + $"\nEquipped: (empty)  →  {main}";
        }

        private void EnsureUi()
        {
            if (_uiBuilt && _panelRoot != null)
                return;

            if (_openButton == null)
                _openButton = CreateCornerButton("EquipmentCenterOpenButton", "Equipment", new Vector2(120f, -110f));

            if (_panelRoot == null)
                BuildPanel();

            _uiBuilt = true;
        }

        private void BuildPanel()
        {
            var panel = new GameObject("EquipmentCenterPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(transform, false);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(860f, 560f);
            panelRect.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.96f);
            _panelRoot = panel;

            _headerLabel = CreateCenteredLabel(panel.transform, "HeaderLabel", 26f, new Vector2(0f, 240f), new Vector2(780f, 36f));
            _filterLabel = CreateCenteredLabel(panel.transform, "FilterLabel", 20f, new Vector2(-80f, 195f), new Vector2(420f, 28f));
            _filterButton = CreatePanelButton(panel.transform, "FilterButton", "Filter", new Vector2(250f, 195f), new Vector2(120f, 40f));
            _closeButton = CreatePanelButton(panel.transform, "CloseButton", "Close", new Vector2(350f, 195f), new Vector2(100f, 40f));

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(panel.transform, false);
            var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRectTransform.sizeDelta = new Vector2(800f, 380f);
            scrollRectTransform.anchoredPosition = new Vector2(0f, -20f);
            scrollGo.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 1f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 380f);
            _listContent = content.transform;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
        }

        private Button CreateCornerButton(string objectName, string label, Vector2 anchoredPosition)
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
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(200f, 56f);
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.18f, 0.92f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());

            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24f;
            tmp.color = Color.white;
            return button;
        }

        private static Button CreatePanelButton(
            Transform parent,
            string objectName,
            string label,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.22f, 0.28f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());

            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20f;
            tmp.color = Color.white;
            return button;
        }

        private static Button CreateRowButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(110f, 44f);
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.25f, 0.4f, 0.3f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());

            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18f;
            tmp.color = Color.white;
            return button;
        }

        private static TextMeshProUGUI CreateCenteredLabel(
            Transform parent,
            string objectName,
            float fontSize,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.text = string.Empty;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static TextMeshProUGUI CreateRowLabel(Transform parent, string objectName, string text, float fontSize)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static string GetFilterLabel(ClassFilter filter)
        {
            return filter switch
            {
                ClassFilter.All => "All",
                ClassFilter.SelectedCharacter => "Selected class",
                ClassFilter.Knight => "Knight",
                ClassFilter.Swordsman => "Swordsman",
                ClassFilter.ShieldGuard => "Shield Guard",
                _ => filter.ToString()
            };
        }
    }
}