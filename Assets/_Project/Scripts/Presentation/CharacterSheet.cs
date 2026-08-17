using System.Text;
using Core;
using Data;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Switches roster characters and inspects class / level / stats / equipment / skill.
    /// </summary>
    public class CharacterSheet : MonoBehaviour
    {
        [Header("Starter Roster (used when no PlayerProfileService exists)")]
        [SerializeField] private CharacterDefinition[] _starterCharacters;
        [SerializeField] private int _startingGold = 100;
        [SerializeField] private bool _createHudOpenButton = true;

        [Header("Optional UI Hooks")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _classLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _statsLabel;
        [SerializeField] private TMP_Text _equipmentLabel;
        [SerializeField] private TMP_Text _skillLabel;

        private PlayerProfileService _profileService;
        private bool _uiBuilt;
        private bool _entryAvailable = true;

        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        public bool IsEntryAvailable => _entryAvailable;

        private void Awake()
        {
            EnsureProfileService();
            EnsureUi();
            Hide();
        }

        private void OnEnable()
        {
            EnsureProfileService();
            EnsureUi();
            BindButtons(true);

            if (_profileService != null)
            {
                _profileService.SelectionChanged += Refresh;
                _profileService.LoadoutChanged += Refresh;
                _profileService.DeploymentLockChanged += RefreshSwitchControls;
            }

            RefreshSwitchControls();
        }

        private void OnDisable()
        {
            BindButtons(false);

            if (_profileService != null)
            {
                _profileService.SelectionChanged -= Refresh;
                _profileService.LoadoutChanged -= Refresh;
                _profileService.DeploymentLockChanged -= RefreshSwitchControls;
            }
        }

        /// <summary>
        /// Opens the sheet and refreshes the selected character.
        /// </summary>
        public void Show()
        {
            if (!_entryAvailable)
                return;

            EnsureProfileService();
            EnsureUi();

            var equipmentCenter = GetComponent<EquipmentCenter>();
            if (equipmentCenter != null && equipmentCenter.IsOpen)
                equipmentCenter.Hide();

            if (_panelRoot != null)
                _panelRoot.SetActive(true);

            if (_openButton != null)
                _openButton.gameObject.SetActive(false);

            Refresh();
        }

        /// <summary>
        /// Closes the sheet.
        /// </summary>
        public void Hide()
        {
            EnsureUi();

            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            RefreshOpenButtonVisibility();
        }

        /// <summary>
        /// Enables or disables the main HUD Characters entry.
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
        /// Rebuilds labels from the selected character.
        /// </summary>
        public void Refresh()
        {
            EnsureProfileService();
            EnsureUi();

            var character = _profileService?.Profile?.SelectedCharacter;
            if (character == null)
            {
                SetText(_nameLabel, "No characters");
                SetText(_classLabel, string.Empty);
                SetText(_levelLabel, string.Empty);
                SetText(_statsLabel, string.Empty);
                SetText(_equipmentLabel, string.Empty);
                SetText(_skillLabel, string.Empty);
                return;
            }

            var stats = StatCalculator.Calculate(character);
            var rosterIndex = _profileService.Profile.SelectedIndex + 1;
            var rosterCount = _profileService.Profile.Characters.Count;

            SetText(_nameLabel, $"{character.DisplayName}  ({rosterIndex}/{rosterCount})");
            SetText(
                _classLabel,
                $"Class: {CharacterClassRules.GetDisplayName(character.CharacterClass)}  " +
                $"(Primary: {CharacterClassRules.GetPrimaryStat(character.CharacterClass)})");
            SetText(_levelLabel, $"Level: {character.Level}");
            SetText(
                _statsLabel,
                $"HP {stats.HP}   ATK {stats.Attack}   DEF {stats.Defense}   SPD {stats.Speed}");
            SetText(_equipmentLabel, BuildEquipmentText(character));
            SetText(_skillLabel, BuildSkillText(character));
            RefreshSwitchControls();
        }

        private void RefreshSwitchControls()
        {
            var locked = _profileService != null && _profileService.IsDeploymentLocked;
            if (_prevButton != null)
                _prevButton.interactable = !locked;
            if (_nextButton != null)
                _nextButton.interactable = !locked;
        }

        /// <summary>
        /// Ensures the shared profile service exists.
        /// </summary>
        public void EnsureProfileServiceForDependents()
        {
            EnsureProfileService();
        }

        private void EnsureProfileService()
        {
            if (_profileService != null)
            {
                EnsureRosterPopulated(_profileService.Profile);
                return;
            }

            if (ServiceLocator.TryGet(out PlayerProfileService existing))
            {
                _profileService = existing;
                EnsureRosterPopulated(_profileService.Profile);
                return;
            }

            var profile = PlayerProfile.CreateStarter(_startingGold, ResolveStarterCharacters());
            _profileService = new PlayerProfileService(profile);
            ServiceLocator.Register(_profileService);
        }

        private void EnsureRosterPopulated(PlayerProfile profile)
        {
            if (profile == null || profile.Characters.Count > 0)
                return;

            var definitions = ResolveStarterCharacters();
            for (var i = 0; i < definitions.Length; i++)
            {
                if (definitions[i] != null)
                    profile.AddCharacter(new CharacterInstance(definitions[i], level: 1));
            }

            if (profile.Characters.Count > 0)
                _profileService.SelectByIndex(0);
        }

        private CharacterDefinition[] ResolveStarterCharacters()
        {
            if (_starterCharacters != null && _starterCharacters.Length > 0)
            {
                var assigned = 0;
                for (var i = 0; i < _starterCharacters.Length; i++)
                {
                    if (_starterCharacters[i] != null)
                        assigned++;
                }

                if (assigned > 0)
                    return _starterCharacters;
            }

            return new[]
            {
                CharacterDefinition.CreateRuntime(
                    "Knight",
                    CharacterClass.Knight,
                    new StatBlock { HP = 120, Attack = 10, Defense = 4, Speed = 9 },
                    UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }),
                    SkillDefinition.CreateRuntime(
                        "Heavy Strike",
                        energyCost: 80,
                        damageMultiplier: 1.5f,
                        description: "A focused blow that deals heavy single-target damage.")),
                CharacterDefinition.CreateRuntime(
                    "Swordsman",
                    CharacterClass.Swordsman,
                    new StatBlock { HP = 90, Attack = 16, Defense = 3, Speed = 12 },
                    UpgradeCurve.CreateRuntime(new StatBlock { HP = 2, Attack = 3, Defense = 0, Speed = 1 }),
                    SkillDefinition.CreateRuntime(
                        "Blade Flurry",
                        energyCost: 90,
                        damageMultiplier: 1.8f,
                        description: "Rapid strikes that prioritize raw Attack damage.")),
                CharacterDefinition.CreateRuntime(
                    "Shield Guard",
                    CharacterClass.ShieldGuard,
                    new StatBlock { HP = 110, Attack = 8, Defense = 10, Speed = 8 },
                    UpgradeCurve.CreateRuntime(new StatBlock { HP = 3, Attack = 0, Defense = 3 }),
                    SkillDefinition.CreateRuntime(
                        "Shield Bash",
                        energyCost: 70,
                        damageMultiplier: 1.2f,
                        description: "A defensive slam that scales well with high Defense builds."))
            };
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

            if (_prevButton != null)
            {
                _prevButton.onClick.RemoveListener(HandlePrevious);
                if (bind)
                    _prevButton.onClick.AddListener(HandlePrevious);
            }

            if (_nextButton != null)
            {
                _nextButton.onClick.RemoveListener(HandleNext);
                if (bind)
                    _nextButton.onClick.AddListener(HandleNext);
            }
        }

        private void HandlePrevious()
        {
            _profileService?.SelectPrevious();
        }

        private void HandleNext()
        {
            _profileService?.SelectNext();
        }

        private void EnsureUi()
        {
            if (_uiBuilt && _panelRoot != null)
                return;

            if (_createHudOpenButton && _openButton == null)
                _openButton = CreateCornerButton("CharacterSheetOpenButton", "Characters", new Vector2(120f, -40f));

            if (_panelRoot == null)
                BuildPanel();

            _uiBuilt = true;
        }

        private void BuildPanel()
        {
            var panel = new GameObject("CharacterSheetPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(transform, false);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(720f, 520f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.09f, 0.12f, 0.96f);

            _panelRoot = panel;

            _nameLabel = CreateLabel(panel.transform, "NameLabel", 28f, new Vector2(0f, 220f), new Vector2(640f, 40f));
            _classLabel = CreateLabel(panel.transform, "ClassLabel", 22f, new Vector2(0f, 175f), new Vector2(640f, 32f));
            _levelLabel = CreateLabel(panel.transform, "LevelLabel", 22f, new Vector2(0f, 140f), new Vector2(640f, 32f));
            _statsLabel = CreateLabel(panel.transform, "StatsLabel", 22f, new Vector2(0f, 95f), new Vector2(640f, 36f));
            _equipmentLabel = CreateLabel(panel.transform, "EquipmentLabel", 20f, new Vector2(0f, -20f), new Vector2(640f, 180f));
            _equipmentLabel.alignment = TextAlignmentOptions.TopLeft;
            _skillLabel = CreateLabel(panel.transform, "SkillLabel", 20f, new Vector2(0f, -180f), new Vector2(640f, 90f));
            _skillLabel.alignment = TextAlignmentOptions.TopLeft;

            _prevButton = CreatePanelButton(panel.transform, "PrevButton", "<", new Vector2(-300f, -230f));
            _nextButton = CreatePanelButton(panel.transform, "NextButton", ">", new Vector2(-200f, -230f));
            _closeButton = CreatePanelButton(panel.transform, "CloseButton", "Close", new Vector2(260f, -230f), new Vector2(140f, 48f));
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
            Vector2? size = null)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(72f, 48f);
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
            tmp.fontSize = 22f;
            tmp.color = Color.white;

            return button;
        }

        private static TextMeshProUGUI CreateLabel(
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
                label.text = value ?? string.Empty;
        }

        private static string BuildEquipmentText(CharacterInstance character)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Equipment:");

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (character.TryGetEquipment(slot, out var item))
                {
                    sb.AppendLine(
                        $"  {slot}: {item.DisplayName}  Lv{item.Level}  " +
                        $"+{item.GetMainStat()} {item.MainStatType}");
                }
                else
                {
                    sb.AppendLine($"  {slot}: (empty)");
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildSkillText(CharacterInstance character)
        {
            var skill = character.DefaultSkill;
            if (skill == null)
                return "Skill: (none)";

            return $"Skill: {skill.DisplayName}\n{skill.Description}";
        }
    }
}