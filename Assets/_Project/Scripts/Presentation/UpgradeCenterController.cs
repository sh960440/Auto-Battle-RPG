using Core;
using Data;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Hub for roster, equipment, and gold upgrades. Loads Gameplay or MainMenu.
    /// </summary>
    public class UpgradeCenterController : MonoBehaviour
    {
        private enum HubTab
        {
            Characters,
            Equipment,
            Upgrade
        }

        private CharacterSheet _characterSheet;
        private EquipmentCenter _equipmentCenter;
        private ProgressionService _progression;
        private PlayerProfileService _profileService;
        private HubTab _tab = HubTab.Characters;

        private TMP_Text _goldLabel;
        private TMP_Text _upgradeBody;
        private Button _upgradeButton;
        private GameObject _upgradePanel;

        private void Awake()
        {
            _progression = new ProgressionService();
            EnsurePartyComponents();
            BuildChrome();
            ShowTab(HubTab.Characters);
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet(out _profileService))
            {
                _profileService.SelectionChanged += Refresh;
                _profileService.LoadoutChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (_profileService != null)
            {
                _profileService.SelectionChanged -= Refresh;
                _profileService.LoadoutChanged -= Refresh;
            }
        }

        private void EnsurePartyComponents()
        {
            _characterSheet = GetComponent<CharacterSheet>();
            if (_characterSheet == null)
                _characterSheet = gameObject.AddComponent<CharacterSheet>();

            _equipmentCenter = GetComponent<EquipmentCenter>();
            if (_equipmentCenter == null)
                _equipmentCenter = gameObject.AddComponent<EquipmentCenter>();

            _characterSheet.EnsureProfileServiceForDependents();
            ServiceLocator.TryGet(out _profileService);
        }

        private void ShowTab(HubTab tab)
        {
            _tab = tab;
            _characterSheet.Hide();
            _equipmentCenter.Hide();
            HideUpgradePanel();

            if (tab == HubTab.Characters)
                _characterSheet.Show();
            else if (tab == HubTab.Equipment)
                _equipmentCenter.Show();
            else
                ShowUpgradePanel();

            Refresh();
        }

        private void Refresh()
        {
            if (_goldLabel != null && _profileService != null)
                _goldLabel.text = $"Gold {_profileService.Profile.Gold}";

            if (_tab == HubTab.Upgrade)
                RefreshUpgradeBody();
            else if (_tab == HubTab.Characters)
                _characterSheet.Refresh();
            else
                _equipmentCenter.Refresh();
        }

        private void ShowUpgradePanel()
        {
            if (_upgradePanel != null)
                _upgradePanel.SetActive(true);

            RefreshUpgradeBody();
        }

        private void HideUpgradePanel()
        {
            if (_upgradePanel != null)
                _upgradePanel.SetActive(false);
        }

        private void RefreshUpgradeBody()
        {
            if (_upgradeBody == null)
                return;

            var character = _profileService?.Profile?.SelectedCharacter;
            if (character == null)
            {
                _upgradeBody.text = "No character selected.";
                if (_upgradeButton != null)
                    _upgradeButton.interactable = false;
                return;
            }

            var current = StatCalculator.Calculate(character);
            var next = _progression.PreviewStatsAfterUpgrade(character);
            var cost = _progression.GetNextLevelCost(character);
            var can = _progression.CanUpgrade(_profileService.Profile, character);

            if (cost <= 0)
            {
                _upgradeBody.text =
                    $"{character.DisplayName}  Lv{character.Level} (max)\n" +
                    $"HP {current.HP}   ATK {current.Attack}   DEF {current.Defense}   SPD {current.Speed}";
            }
            else
            {
                _upgradeBody.text =
                    $"{character.DisplayName}  Lv{character.Level} → {character.Level + 1}\n" +
                    $"Now   HP {current.HP}  ATK {current.Attack}  DEF {current.Defense}  SPD {current.Speed}\n" +
                    $"Next  HP {next.HP}  ATK {next.Attack}  DEF {next.Defense}  SPD {next.Speed}\n" +
                    $"Cost  {cost} gold";
            }

            if (_upgradeButton != null)
                _upgradeButton.interactable = can;
        }

        private void HandleUpgrade()
        {
            var character = _profileService?.Profile?.SelectedCharacter;
            if (character == null)
                return;

            if (!_progression.TryUpgrade(_profileService.Profile, character))
                return;

            _profileService.NotifyLoadoutChanged();
            Refresh();
        }

        private void HandlePrev() => _profileService?.SelectPrevious();

        private void HandleNext() => _profileService?.SelectNext();

        private static void LoadGameplay() => SceneManager.LoadScene(SceneNames.Gameplay);

        private static void LoadMenu() => SceneManager.LoadScene(SceneNames.MainMenu);

        private void BuildChrome()
        {
            var gold = CreateLabel("GoldLabel", 26f, new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(400f, 40f));
            _goldLabel = gold;

            CreateNavButton("TabCharacters", "Characters", new Vector2(-280f, 240f), () => ShowTab(HubTab.Characters));
            CreateNavButton("TabEquipment", "Equipment", new Vector2(-80f, 240f), () => ShowTab(HubTab.Equipment));
            CreateNavButton("TabUpgrade", "Upgrade", new Vector2(120f, 240f), () => ShowTab(HubTab.Upgrade));

            CreateNavButton("PlayButton", "Play", new Vector2(280f, -280f), LoadGameplay, new Vector2(180f, 56f));
            CreateNavButton("MenuButton", "Menu", new Vector2(-280f, -280f), LoadMenu, new Vector2(180f, 56f));

            BuildUpgradePanel();
        }

        private void BuildUpgradePanel()
        {
            var panel = new GameObject("UpgradePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 420f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.96f);
            _upgradePanel = panel;

            _upgradeBody = CreateChildLabel(panel.transform, "UpgradeBody", 22f, Vector2.zero, new Vector2(640f, 220f));
            _upgradeBody.alignment = TextAlignmentOptions.Center;

            CreateChildButton(panel.transform, "Prev", "<", new Vector2(-260f, -150f), new Vector2(72f, 48f), HandlePrev);
            CreateChildButton(panel.transform, "Next", ">", new Vector2(-160f, -150f), new Vector2(72f, 48f), HandleNext);
            _upgradeButton = CreateChildButton(panel.transform, "UpgradeButton", "Upgrade", new Vector2(200f, -150f), new Vector2(180f, 48f), HandleUpgrade);
            _upgradePanel.SetActive(false);
        }

        private TMP_Text CreateLabel(
            string objectName,
            float fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = "Gold 0";
            return tmp;
        }

        private void CreateNavButton(string objectName, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, Vector2? size = null)
        {
            var button = CreateChildButton(transform, objectName, label, anchoredPosition, size ?? new Vector2(180f, 48f), onClick);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        private static Button CreateChildButton(
            Transform parent,
            string objectName,
            string label,
            Vector2 anchoredPosition,
            Vector2 size,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f, 1f);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

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
            tmp.fontSize = 22f;
            tmp.color = Color.white;
            return button;
        }

        private static TextMeshProUGUI CreateChildLabel(
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
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }
    }
}
