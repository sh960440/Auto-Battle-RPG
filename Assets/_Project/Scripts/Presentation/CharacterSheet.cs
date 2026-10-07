using System.Text;
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
        private bool _entryAvailable = true;

        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        public bool IsEntryAvailable => _entryAvailable;

        private void Awake()
        {
            ResolveProfileService();
            EnsureUi();
            Hide();
        }

        private void OnEnable()
        {
            ResolveProfileService();
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

            ResolveProfileService();
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
            ResolveProfileService();
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
        /// Resolves the shared profile service.
        /// </summary>
        public void ResolveProfileService()
        {
            if (_profileService != null)
                return;

            if (ServiceLocator.TryGet(out PlayerProfileService existing))
            {
                _profileService = existing;
                return;
            }

            Debug.LogError(
                $"{nameof(CharacterSheet)}: {nameof(PlayerProfileService)} is not registered. Start from Boot.");
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