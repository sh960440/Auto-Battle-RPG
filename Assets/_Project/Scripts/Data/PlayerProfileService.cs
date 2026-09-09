using System;

namespace Data
{
    /// <summary>
    /// Holds the live <see cref="PlayerProfile"/>, selection, and Advance deployment lock.
    /// </summary>
    public class PlayerProfileService
    {
        private CharacterInstance _lockedDeployed;
        private StatBlock _lockedStats;
        private SkillDefinition _lockedSkill;
        private bool _isDeploymentLocked;

        public PlayerProfileService(PlayerProfile profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public PlayerProfile Profile { get; }

        /// <summary>
        /// Whether Advance has locked the deployed character for this run.
        /// </summary>
        public bool IsDeploymentLocked => _isDeploymentLocked;

        /// <summary>
        /// Character used for the current Advance / combat run.
        /// While unlocked, mirrors <see cref="PlayerProfile.SelectedCharacter"/>.
        /// </summary>
        public CharacterInstance DeployedCharacter =>
            _isDeploymentLocked ? _lockedDeployed : Profile.SelectedCharacter;

        /// <summary>
        /// Final stats for exploration / combat. Locked runs use the Advance snapshot.
        /// </summary>
        public StatBlock DeployedStats
        {
            get
            {
                if (_isDeploymentLocked)
                    return _lockedStats;

                var character = Profile.SelectedCharacter;
                return character != null ? StatCalculator.Calculate(character) : StatBlock.Zero;
            }
        }

        /// <summary>
        /// Active skill for exploration / combat. Locked runs use the Advance snapshot.
        /// </summary>
        public SkillDefinition DeployedSkill
        {
            get
            {
                if (_isDeploymentLocked)
                    return _lockedSkill;

                return Profile.SelectedCharacter?.DefaultSkill;
            }
        }

        public event Action SelectionChanged;

        public event Action LoadoutChanged;

        public event Action DeploymentLockChanged;

        /// <summary>
        /// Notifies listeners that gear or inventory changed.
        /// </summary>
        public void NotifyLoadoutChanged()
        {
            LoadoutChanged?.Invoke();
        }

        /// <summary>
        /// Adds victory loot to the profile and notifies inventory listeners.
        /// </summary>
        public void ApplyLoot(LootDrop drop)
        {
            if (drop == null)
                return;

            Profile.ApplyLoot(drop);
            if (drop.HasItem)
                NotifyLoadoutChanged();
        }

        /// <summary>
        /// Snapshots the selected character, final stats, and skill for this Advance run.
        /// </summary>
        /// <returns><c>false</c> when there is no selected character to lock.</returns>
        public bool TryLockDeployment()
        {
            var selected = Profile.SelectedCharacter;
            if (selected == null)
                return false;

            _lockedDeployed = selected;
            _lockedStats = StatCalculator.Calculate(selected);
            _lockedSkill = selected.DefaultSkill;
            _isDeploymentLocked = true;
            DeploymentLockChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Clears the Advance deployment lock.
        /// </summary>
        public void UnlockDeployment()
        {
            if (!_isDeploymentLocked && _lockedDeployed == null)
                return;

            _lockedDeployed = null;
            _lockedStats = StatBlock.Zero;
            _lockedSkill = null;
            _isDeploymentLocked = false;
            DeploymentLockChanged?.Invoke();
        }

        /// <summary>
        /// Selects a roster character by index.
        /// </summary>
        public void SelectByIndex(int index)
        {
            if (_isDeploymentLocked)
                return;

            Profile.SelectCharacter(index);
            SelectionChanged?.Invoke();
        }

        /// <summary>
        /// Selects the previous roster character.
        /// </summary>
        public void SelectPrevious()
        {
            if (_isDeploymentLocked)
                return;

            var count = Profile.Characters.Count;
            if (count <= 1)
                return;

            var index = Profile.SelectedIndex - 1;
            if (index < 0)
                index = count - 1;

            SelectByIndex(index);
        }

        /// <summary>
        /// Selects the next roster character.
        /// </summary>
        public void SelectNext()
        {
            if (_isDeploymentLocked)
                return;

            var count = Profile.Characters.Count;
            if (count <= 1)
                return;

            SelectByIndex((Profile.SelectedIndex + 1) % count);
        }
    }
}