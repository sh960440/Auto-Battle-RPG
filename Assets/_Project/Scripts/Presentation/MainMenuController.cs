using Core;
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
                _newGameButton.onClick.AddListener(OpenUpgradeCenter);
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OpenUpgradeCenter);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(HandleSettings);
        }

        private void OnDisable()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.RemoveListener(OpenUpgradeCenter);
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OpenUpgradeCenter);
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(HandleSettings);
        }

        private static void OpenUpgradeCenter()
        {
            SceneManager.LoadScene(SceneNames.UpgradeCenter);
        }

        private static void HandleSettings()
        {
            Debug.Log("[MainMenu] Settings is not wired yet.");
        }

        private static Button FindButton(string objectName)
        {
            var go = GameObject.Find(objectName);
            return go != null ? go.GetComponent<Button>() : null;
        }
    }
}
