using Data;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Plays clips from <see cref="AudioBank"/>.
    /// </summary>
    public class AudioService
    {
        private const string HostName = "AudioServiceHost";

        private static GameObject _host;

        private readonly AudioBank _bank;
        private readonly AudioSource _oneShot;
        private readonly AudioSource _loop;

        /// <summary>
        /// Creates a service bound to <paramref name="bank"/>.
        /// </summary>
        public AudioService(AudioBank bank)
        {
            _bank = bank ?? throw new System.ArgumentNullException(nameof(bank));

            if (_host != null)
                Object.Destroy(_host);

            _host = new GameObject(HostName);
            Object.DontDestroyOnLoad(_host);
            _oneShot = CreateSource();
            _loop = CreateSource();
            _loop.loop = true;
        }

        /// <summary>
        /// Plays the UI click clip for <paramref name="kind"/>.
        /// </summary>
        public void PlayUiClick(UiClickKind kind)
        {
            switch (kind)
            {
                case UiClickKind.Confirm:
                    Play(_bank.UiConfirm);
                    return;
                case UiClickKind.Back:
                    Play(_bank.UiBack);
                    return;
                case UiClickKind.LevelUp:
                    Play(_bank.UiLevelUp);
                    return;
                case UiClickKind.Equip:
                    Play(_bank.UiEquip);
                    return;
                case UiClickKind.Default:
                    Play(_bank.UiClick);
                    return;
                default:
                    return;
            }
        }

        /// <summary>Plays <paramref name="clip"/>.</summary>
        public void PlayAttack(AudioClip clip) => Play(clip != null ? clip : _bank.Attack);

        /// <summary>Plays <paramref name="clip"/>.</summary>
        public void PlaySkill(AudioClip clip) => Play(clip != null ? clip : _bank.Skill);

        /// <summary>Plays the victory clip when assigned.</summary>
        public void PlayVictory() => Play(_bank.Victory);

        /// <summary>Plays the defeat clip when assigned.</summary>
        public void PlayDefeat() => Play(_bank.Defeat);

        /// <summary>Loops the Advance footstep clip until <see cref="StopAdvanceFootsteps"/>.</summary>
        public void PlayAdvanceFootsteps()
        {
            var clip = _bank.AdvanceFootsteps;
            if (clip == null || _loop == null)
                return;

            _loop.clip = clip;
            _loop.Play();
        }

        /// <summary>Stops looping Advance footsteps.</summary>
        public void StopAdvanceFootsteps()
        {
            if (_loop == null || !_loop.isPlaying)
                return;

            _loop.Stop();
            _loop.clip = null;
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || _oneShot == null)
                return;

            _oneShot.PlayOneShot(clip);
        }

        private static AudioSource CreateSource()
        {
            var source = _host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}