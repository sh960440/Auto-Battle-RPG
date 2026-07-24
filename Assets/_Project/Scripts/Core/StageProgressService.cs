using System;
using Combat;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Tracks the player's current stage number and applies win / lose progression.
    /// </summary>
    public class StageProgressService
    {
        private const string PlayerPrefsKey = "StageProgress.CurrentStage";

        private int _currentStage = 1;
        private int _stageBeforeLastChange = 1;
        private CombatResult? _lastCombatResult;

        /// <summary>
        /// Creates a progress service and loads any saved stage.
        /// </summary>
        /// <param name="startingStage">Fallback stage when no save exists.</param>
        public StageProgressService(int startingStage = 1)
        {
            _currentStage = Mathf.Max(1, startingStage);
            Load();
            _stageBeforeLastChange = _currentStage;
        }

        /// <summary>
        /// The stage used when entering the next exploration.
        /// </summary>
        public int CurrentStage => _currentStage;

        /// <summary>
        /// Stage value before the most recent combat result was applied.
        /// </summary>
        public int StageBeforeLastChange => _stageBeforeLastChange;

        /// <summary>
        /// Most recent combat result that changed stage progress, if any.
        /// </summary>
        public CombatResult? LastCombatResult => _lastCombatResult;

        /// <summary>
        /// Fired after <see cref="CurrentStage"/> changes.
        /// </summary>
        public event Action<int> StageChanged;

        /// <summary>
        /// Sets the current stage.
        /// </summary>
        public void SetStage(int stage)
        {
            _currentStage = Mathf.Max(1, stage);
            _stageBeforeLastChange = _currentStage;
            Save();
            StageChanged?.Invoke(_currentStage);
        }

        /// <summary>
        /// Updates stage after combat.
        /// </summary>
        public void ApplyCombatResult(CombatResult result)
        {
            _stageBeforeLastChange = _currentStage;
            _lastCombatResult = result;

            if (result == CombatResult.Victory)
                _currentStage += 1;
            else
                _currentStage = Mathf.Max(1, _currentStage - 3);

            Save();
            StageChanged?.Invoke(_currentStage);
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return;

            _currentStage = Mathf.Max(1, PlayerPrefs.GetInt(PlayerPrefsKey, _currentStage));
        }

        private void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, _currentStage);
            PlayerPrefs.Save();
        }
    }
}