using Combat;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Win/Lose overlay shown when combat ends.
    /// </summary>
    public class CombatResultOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private string _victoryText = "WIN";
        [SerializeField] private string _defeatText = "LOSE";
        [SerializeField] private bool _loadMainMenuOnContinue = true;
        [SerializeField] private UnityEvent _onContinue;

        private CombatSimulator _simulator;

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(HandleContinue);
        }

        private void OnDisable()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(HandleContinue);
        }

        /// <summary>
        /// Binds the overlay to a running combat session.
        /// </summary>
        public void Bind(CombatSimulator simulator)
        {
            Unbind();

            _simulator = simulator ?? throw new System.ArgumentNullException(nameof(simulator));
            _simulator.OnCombatEnd += HandleCombatEnd;
            Hide();

            if (_simulator.IsFinished && _simulator.Result.HasValue)
                Show(_simulator.Result.Value);
        }

        /// <summary>
        /// Clears combat bindings and hides the overlay.
        /// </summary>
        public void Unbind()
        {
            if (_simulator != null)
            {
                _simulator.OnCombatEnd -= HandleCombatEnd;
                _simulator = null;
            }

            Hide();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void HandleCombatEnd(CombatResult result)
        {
            Show(result);
        }

        private void Show(CombatResult result)
        {
            if (_root != null)
                _root.SetActive(true);

            if (_resultLabel != null)
                _resultLabel.text = result == CombatResult.Victory ? _victoryText : _defeatText;

            gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (_root != null)
                _root.SetActive(false);
        }

        private void HandleContinue()
        {
            _onContinue?.Invoke();
            Hide();
            Unbind();

            if (_loadMainMenuOnContinue)
                SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}