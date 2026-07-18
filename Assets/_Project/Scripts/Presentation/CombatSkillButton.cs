using Combat;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    /// <summary>
    /// Skill button. Disabled when energy is too low; click queues the skill for the next player turn.
    /// </summary>
    public class CombatSkillButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _queuedIndicator;

        private CombatSimulator _simulator;
        private SkillDefinition _skill;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        private void LateUpdate()
        {
            RefreshInteractable();
            RefreshQueuedVisual();
        }

        /// <summary>
        /// Hooks this button to a live combat session and skill asset.
        /// </summary>
        public void Bind(CombatSimulator simulator, SkillDefinition skill)
        {
            _simulator = simulator ?? throw new System.ArgumentNullException(nameof(simulator));
            _skill = skill ?? throw new System.ArgumentNullException(nameof(skill));

            if (_label != null)
                _label.text = $"{_skill.DisplayName} ({_skill.EnergyCost})";

            RefreshInteractable();
            RefreshQueuedVisual();
        }

        /// <summary>
        /// Clears combat bindings and disables the button.
        /// </summary>
        public void Unbind()
        {
            _simulator = null;
            _skill = null;

            if (_button != null)
                _button.interactable = false;

            SetQueuedVisual(false);
        }

        private void OnClicked()
        {
            if (_simulator == null || _skill == null || _simulator.IsFinished)
                return;

            var player = _simulator.Context.Player;
            if (!player.IsAlive || player.Energy < _skill.EnergyCost)
                return;

            _simulator.Context.QueueSkill(_skill);
            RefreshQueuedVisual();
            Debug.Log($"[Combat] Queued skill: {_skill.DisplayName}");
        }

        private void RefreshInteractable()
        {
            if (_button == null)
                return;

            if (_simulator == null || _skill == null || _simulator.IsFinished)
            {
                _button.interactable = false;
                return;
            }

            var player = _simulator.Context.Player;
            _button.interactable = player.IsAlive && player.Energy >= _skill.EnergyCost;
        }

        private void RefreshQueuedVisual()
        {
            var queued = _simulator != null
                         && _skill != null
                         && _simulator.Context.HasQueuedSkill
                         && _simulator.Context.QueuedSkill == _skill;

            SetQueuedVisual(queued);
        }

        private void SetQueuedVisual(bool queued)
        {
            if (_queuedIndicator != null)
                _queuedIndicator.enabled = queued;
        }
    }
}