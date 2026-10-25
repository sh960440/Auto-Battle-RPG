using UnityEngine;

namespace Data
{
    /// <summary>
    /// Which UI click clip to play.
    /// </summary>
    public enum UiClickKind
    {
        None = 0,
        Default = 1,
        Confirm = 2,
        Back = 3,
        LevelUp = 4,
        Equip = 5
    }

    /// <summary>
    /// Shared short SFX clips for UI and fallback combat sounds.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioBank_", menuName = "Game Specific/Audio Bank")]
    public class AudioBank : ScriptableObject
    {
        [Header("UI Clicks")]
        [SerializeField] private AudioClip _uiClick;
        [SerializeField] private AudioClip _uiConfirm;
        [SerializeField] private AudioClip _uiBack;
        [SerializeField] private AudioClip _uiLevelUp;
        [SerializeField] private AudioClip _uiEquip;

        [Header("Combat")]
        [SerializeField] private AudioClip _attack;
        [SerializeField] private AudioClip _skill;
        [SerializeField] private AudioClip _victory;
        [SerializeField] private AudioClip _defeat;

        [Header("Exploration")]
        [SerializeField] private AudioClip _advanceFootsteps;

        public AudioClip UiClick => _uiClick;
        public AudioClip UiConfirm => _uiConfirm;
        public AudioClip UiBack => _uiBack;
        public AudioClip UiLevelUp => _uiLevelUp;
        public AudioClip UiEquip => _uiEquip;
        public AudioClip Attack => _attack;
        public AudioClip Skill => _skill;
        public AudioClip Victory => _victory;
        public AudioClip Defeat => _defeat;
        public AudioClip AdvanceFootsteps => _advanceFootsteps;
    }
}