using Core;
using Data;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Upgrade Center hub: tabs, roster select, upgrades, and scene navigation.
    /// </summary>
    public class UpgradeCenterController : MonoBehaviour
    {
        private enum HubTab
        {
            Stat = 0,
            Skills = 1,
            Equipment = 2
        }

        [Header("Tabs")]
        [SerializeField] private Button _statTabButton;
        [SerializeField] private Button _skillsTabButton;
        [SerializeField] private Button _equipmentTabButton;
        [SerializeField] private GameObject _statPanel;
        [SerializeField] private GameObject _skillsPanel;
        [SerializeField] private GameObject _equipmentPanel;

        [Header("Navigation / Gold")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TMP_Text _coinAmountText;

        [Header("Roster")]
        [SerializeField] private Button[] _characterButtons;
        [SerializeField] private TMP_Text _characterNameText;
        [SerializeField] private Image _characterImage;

        [Header("Stats / Upgrade")]
        [SerializeField] private TMP_Text _levelAmountText;
        [SerializeField] private TMP_Text _hpAmountText;
        [SerializeField] private TMP_Text _atkAmountText;
        [SerializeField] private TMP_Text _defAmountText;
        [SerializeField] private TMP_Text _spdAmountText;
        [SerializeField] private TMP_Text _costAmountText;
        [SerializeField] private Button _levelUpButton;
        [SerializeField] private Color _statNormalColor = Color.white;
        [SerializeField] private Color _statPreviewColor = new Color(0.45f, 0.95f, 0.55f, 1f);

        [Header("Equipped Slots")]
        [SerializeField] private Image _leftHandSlot;
        [SerializeField] private Image _rightHandSlot;
        [SerializeField] private Image _upperBodySlot;
        [SerializeField] private Image _lowerBodySlot;
        [SerializeField] private Color _slotEmptyColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color _slotFilledColor = new Color(1f, 1f, 1f, 1f);

        [Header("Skills")]
        [SerializeField] private GameObject _skill1Root;
        [SerializeField] private TMP_Text _skill1NameText;
        [SerializeField] private TMP_Text _skill1EnergyText;
        [SerializeField] private TMP_Text _skill1EffectText;
        [SerializeField] private TMP_Text _skill1DescriptionText;
        [SerializeField] private GameObject _skill2Root;
        [SerializeField] private TMP_Text _skill2NameText;
        [SerializeField] private TMP_Text _skill2DescriptionText;

        private CharacterSheet _characterSheet;
        private EquipmentCenter _equipmentCenter;
        private ProgressionService _progression;
        private PlayerProfileService _profileService;
        private HubTab _tab = HubTab.Stat;
        private bool _previewNextLevel;
        private readonly UnityEngine.Events.UnityAction[] _characterClickActions = new UnityEngine.Events.UnityAction[8];
        private UnityEngine.Events.UnityAction _statTabAction;
        private UnityEngine.Events.UnityAction _skillsTabAction;
        private UnityEngine.Events.UnityAction _equipmentTabAction;
        private UnityEngine.Events.UnityAction _playAction;
        private UnityEngine.Events.UnityAction _mainMenuAction;
        private UnityEngine.Events.UnityAction _levelUpAction;

        private void Awake()
        {
            _progression = new ProgressionService();
            _statTabAction = () => ShowTab(HubTab.Stat);
            _skillsTabAction = () => ShowTab(HubTab.Skills);
            _equipmentTabAction = () => ShowTab(HubTab.Equipment);
            _playAction = LoadGameplay;
            _mainMenuAction = LoadMainMenu;
            _levelUpAction = HandleLevelUp;
            EnsurePartyComponents();
            WireLevelUpHover();
            ShowTab(HubTab.Stat);
            Refresh();
        }

        private void OnEnable()
        {
            BindChrome(true);

            if (_profileService == null)
                ServiceLocator.TryGet(out _profileService);

            if (_profileService != null)
            {
                _profileService.SelectionChanged += Refresh;
                _profileService.LoadoutChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            BindChrome(false);

            if (_profileService != null)
            {
                _profileService.SelectionChanged -= Refresh;
                _profileService.LoadoutChanged -= Refresh;
            }
        }

        private void EnsurePartyComponents()
        {
            _characterSheet = GetComponent<CharacterSheet>();
            _equipmentCenter = GetComponent<EquipmentCenter>();
            _characterSheet?.EnsureProfileServiceForDependents();
            ServiceLocator.TryGet(out _profileService);
        }

        private void BindChrome(bool bind)
        {
            BindButton(_statTabButton, _statTabAction, bind);
            BindButton(_skillsTabButton, _skillsTabAction, bind);
            BindButton(_equipmentTabButton, _equipmentTabAction, bind);
            BindButton(_playButton, _playAction, bind);
            BindButton(_mainMenuButton, _mainMenuAction, bind);
            BindButton(_levelUpButton, _levelUpAction, bind);
            BindCharacterButtons(bind);
        }

        private void BindCharacterButtons(bool bind)
        {
            if (_characterButtons == null)
                return;

            for (var i = 0; i < _characterButtons.Length; i++)
            {
                var button = _characterButtons[i];
                if (button == null)
                    continue;

                if (_characterClickActions[i] != null)
                    button.onClick.RemoveListener(_characterClickActions[i]);

                if (!bind)
                    continue;

                var index = i;
                UnityEngine.Events.UnityAction action = () => HandleSelectCharacter(index);
                _characterClickActions[i] = action;
                button.onClick.AddListener(action);
            }
        }

        private void WireLevelUpHover()
        {
            if (_levelUpButton == null)
                return;

            var trigger = _levelUpButton.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = _levelUpButton.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();
            AddPointerEntry(trigger, EventTriggerType.PointerEnter, _ =>
            {
                _previewNextLevel = true;
                RefreshStatsAndCost();
            });
            AddPointerEntry(trigger, EventTriggerType.PointerExit, _ =>
            {
                _previewNextLevel = false;
                RefreshStatsAndCost();
            });
        }

        private static void AddPointerEntry(
            EventTrigger trigger,
            EventTriggerType type,
            UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }

        private void ShowTab(HubTab tab)
        {
            _tab = tab;

            if (_statPanel != null)
                _statPanel.SetActive(tab == HubTab.Stat);
            if (_skillsPanel != null)
                _skillsPanel.SetActive(tab == HubTab.Skills);
            if (_equipmentPanel != null)
                _equipmentPanel.SetActive(tab == HubTab.Equipment);

            if (tab == HubTab.Equipment)
                _equipmentCenter?.Refresh();
        }

        private void Refresh()
        {
            RefreshGold();
            RefreshRosterButtons();
            RefreshSelectedCharacter();
            RefreshStatsAndCost();
            RefreshEquipmentSlots();
            RefreshSkills();

            if (_tab == HubTab.Equipment)
                _equipmentCenter?.Refresh();
        }

        private void RefreshGold()
        {
            if (_coinAmountText == null)
                return;

            var gold = _profileService?.Profile?.Gold ?? 0;
            _coinAmountText.text = gold.ToString();
        }

        private void RefreshRosterButtons()
        {
            var characters = _profileService?.Profile?.Characters;
            var selectedIndex = _profileService?.Profile?.SelectedIndex ?? -1;
            var count = characters?.Count ?? 0;

            if (_characterButtons == null)
                return;

            for (var i = 0; i < _characterButtons.Length; i++)
            {
                var button = _characterButtons[i];
                if (button == null)
                    continue;

                var unlocked = i < count;
                button.interactable = unlocked;

                var image = button.targetGraphic as Image;
                if (image == null)
                    image = button.GetComponent<Image>();

                if (!unlocked)
                {
                    if (image != null)
                    {
                        image.sprite = null;
                        image.color = new Color(1f, 1f, 1f, 0.25f);
                    }

                    continue;
                }

                var character = characters[i];
                if (image != null)
                {
                    image.sprite = character.Definition != null ? character.Definition.Portrait : null;
                    image.color = i == selectedIndex
                        ? Color.white
                        : new Color(1f, 1f, 1f, 0.85f);
                }
            }
        }

        private void RefreshSelectedCharacter()
        {
            var character = _profileService?.Profile?.SelectedCharacter;
            if (character == null)
            {
                SetText(_characterNameText, string.Empty);
                if (_characterImage != null)
                {
                    _characterImage.sprite = null;
                    _characterImage.enabled = false;
                }

                return;
            }

            SetText(_characterNameText, CharacterClassRules.GetDisplayName(character.CharacterClass));

            if (_characterImage != null)
            {
                var portrait = character.Definition != null ? character.Definition.Portrait : null;
                _characterImage.sprite = portrait;
                _characterImage.enabled = portrait != null;
                _characterImage.color = Color.white;
            }
        }

        private void RefreshStatsAndCost()
        {
            var character = _profileService?.Profile?.SelectedCharacter;
            if (character == null)
            {
                SetText(_levelAmountText, "-");
                SetText(_hpAmountText, "-");
                SetText(_atkAmountText, "-");
                SetText(_defAmountText, "-");
                SetText(_spdAmountText, "-");
                SetText(_costAmountText, "-");
                if (_levelUpButton != null)
                    _levelUpButton.interactable = false;
                return;
            }

            var showPreview = _previewNextLevel &&
                              _progression.GetNextLevelCost(character) > 0;
            var stats = showPreview
                ? _progression.PreviewStatsAfterUpgrade(character)
                : StatCalculator.Calculate(character);
            var color = showPreview ? _statPreviewColor : _statNormalColor;

            SetText(_levelAmountText, showPreview ? (character.Level + 1).ToString() : character.Level.ToString());
            SetStatText(_hpAmountText, stats.HP, color);
            SetStatText(_atkAmountText, stats.Attack, color);
            SetStatText(_defAmountText, stats.Defense, color);
            SetStatText(_spdAmountText, stats.Speed, color);

            var cost = _progression.GetNextLevelCost(character);
            SetText(_costAmountText, cost > 0 ? cost.ToString() : "-");

            if (_levelUpButton != null)
                _levelUpButton.interactable = _progression.CanUpgrade(_profileService.Profile, character);
        }

        private void RefreshEquipmentSlots()
        {
            var character = _profileService?.Profile?.SelectedCharacter;
            ApplySlot(_leftHandSlot, character, EquipmentSlot.LeftHand);
            ApplySlot(_rightHandSlot, character, EquipmentSlot.RightHand);
            ApplySlot(_upperBodySlot, character, EquipmentSlot.UpperBody);
            ApplySlot(_lowerBodySlot, character, EquipmentSlot.LowerBody);
        }

        private void ApplySlot(Image slot, CharacterInstance character, EquipmentSlot equipmentSlot)
        {
            if (slot == null)
                return;

            var filled = character != null && character.TryGetEquipment(equipmentSlot, out _);
            slot.color = filled ? _slotFilledColor : _slotEmptyColor;
        }

        private void RefreshSkills()
        {
            if (_skill2Root != null)
                _skill2Root.SetActive(true);

            // Skill2 stays locked for v1.
            SetSkill2Locked();

            var skill = _profileService?.Profile?.SelectedCharacter?.DefaultSkill;
            if (skill == null)
            {
                SetText(_skill1NameText, string.Empty);
                SetText(_skill1EnergyText, string.Empty);
                SetText(_skill1EffectText, string.Empty);
                SetText(_skill1DescriptionText, string.Empty);
                return;
            }

            SetText(_skill1NameText, skill.DisplayName);
            SetText(_skill1EnergyText, skill.EnergyCost.ToString());
            SetText(_skill1EffectText, $"x{skill.DamageMultiplier:0.##} damage");
            SetText(_skill1DescriptionText, skill.Description);
        }

        private void SetSkill2Locked()
        {
            if (_skill2Root == null)
                return;

            var canvasGroup = _skill2Root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = _skill2Root.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0.45f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            SetText(_skill2NameText, "Locked");
            SetText(_skill2DescriptionText, "Not unlocked");
        }

        private void HandleSelectCharacter(int index)
        {
            if (_profileService == null)
                return;

            if (index < 0 || index >= _profileService.Profile.Characters.Count)
                return;

            _previewNextLevel = false;
            _profileService.SelectByIndex(index);
        }

        private void HandleLevelUp()
        {
            var character = _profileService?.Profile?.SelectedCharacter;
            if (character == null || _profileService == null)
                return;

            if (!_progression.TryUpgrade(_profileService.Profile, character))
                return;

            _profileService.NotifyLoadoutChanged();
            Refresh();
        }

        private static void LoadGameplay() => SceneManager.LoadScene(SceneNames.Gameplay);

        private static void LoadMainMenu() => SceneManager.LoadScene(SceneNames.MainMenu);

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action, bool bind)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            if (bind)
                button.onClick.AddListener(action);
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
                label.text = value ?? string.Empty;
        }

        private static void SetStatText(TMP_Text label, int value, Color color)
        {
            if (label == null)
                return;

            label.text = value.ToString();
            label.color = color;
        }
    }
}