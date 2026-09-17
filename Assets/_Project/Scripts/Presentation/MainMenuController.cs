using Core;
using Data;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Wires MainMenu buttons to Upgrade Center and Gameplay.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] [Min(0)] private int _startingGold = 100;

        private void Awake()
        {
            if (_newGameButton == null)
                _newGameButton = FindButton("NewGameButton");
            if (_continueButton == null)
                _continueButton = FindButton("ContinueGameButton");
            if (_settingsButton == null)
                _settingsButton = FindButton("SettingsButton");
        }

        private void OnEnable()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(HandleNewGame);
            if (_continueButton != null)
                _continueButton.onClick.AddListener(HandleContinue);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(HandleSettings);

            RefreshContinueInteractable();
        }

        private void OnDisable()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.RemoveListener(HandleNewGame);
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(HandleContinue);
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(HandleSettings);
        }

        private void RefreshContinueInteractable()
        {
            if (_continueButton == null)
                return;

            var canContinue = ServiceLocator.TryGet(out SaveSystem saveSystem) && saveSystem.Exists();
            _continueButton.interactable = canContinue;
        }

        private void HandleNewGame()
        {
            if (!ServiceLocator.TryGet(out ContentCatalog catalog))
            {
                Debug.LogError("[MainMenu] ContentCatalog is not registered. Start from Boot.");
                return;
            }

            ServiceLocator.TryGet(out SaveSystem saveSystem);
            saveSystem?.Delete();

            ReplaceProfile(PlayerProfile.CreateStarter(_startingGold, catalog.Characters));

            if (ServiceLocator.TryGet(out StageProgressService progress))
                progress.SetStage(1);

            saveSystem?.SaveCurrent();

            SceneManager.LoadScene(SceneNames.UpgradeCenter);
        }

        private void HandleContinue()
        {
            if (!ServiceLocator.TryGet(out SaveSystem saveSystem))
            {
                Debug.LogError("[MainMenu] SaveSystem is not registered. Start from Boot.");
                return;
            }

            if (!saveSystem.TryLoad(out var profile, out var stage))
            {
                Debug.LogWarning("[MainMenu] Continue failed: save missing or corrupt.");
                RefreshContinueInteractable();
                return;
            }

            ReplaceProfile(profile);

            if (ServiceLocator.TryGet(out StageProgressService progress))
                progress.SetStage(stage);

            SceneManager.LoadScene(SceneNames.UpgradeCenter);
        }

        private static void HandleSettings()
        {
            Debug.Log("[MainMenu] Settings is not wired yet.");
        }

        private static void ReplaceProfile(PlayerProfile profile)
        {
            ServiceLocator.Unregister<PlayerProfileService>();
            ServiceLocator.Register(new PlayerProfileService(profile));
        }

        private static Button FindButton(string objectName)
        {
            var go = GameObject.Find(objectName);
            return go != null ? go.GetComponent<Button>() : null;
        }
    }
}
