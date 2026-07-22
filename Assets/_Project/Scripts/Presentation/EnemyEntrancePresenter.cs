using System;
using System.Collections;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Enemies appear from the horizon, grow larger as they approach, and stop at slot position.
    /// </summary>
    public class EnemyEntrancePresenter : MonoBehaviour
    {
        [SerializeField] private CombatStage _combatStage;
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private float _staggerSeconds = 0.12f;
        [SerializeField] private float _startScaleMultiplier = 0.25f;
        [SerializeField] private float _startYOffset = 4.5f;
        [SerializeField] private bool _fadeIn = true;
        [SerializeField] private bool _playOnStart;

        private Coroutine _running;
        private Action _onComplete;

        private void Start()
        {
            if (_playOnStart)
                PlayEntrance();
        }

        /// <summary>
        /// Plays approach for every active enemy slot under CombatStage.
        /// </summary>
        /// <param name="onComplete">Optional callback when all approaches finish.</param>
        public void PlayEntrance(Action onComplete = null)
        {
            if (_combatStage == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (_running != null)
                StopCoroutine(_running);

            _onComplete = onComplete;
            _running = StartCoroutine(PlayEntranceRoutine());
        }

        /// <summary>
        /// Plays approach for one slot index.
        /// </summary>
        public void PlayEntranceForSlot(int index)
        {
            if (_combatStage == null)
                return;

            var slot = _combatStage.GetEnemySlot(index);
            if (slot == null || !slot.gameObject.activeInHierarchy)
                return;

            StartCoroutine(AnimateSlot(slot, 0f));
        }

#if UNITY_EDITOR
        [ContextMenu("Preview Entrance")]
        private void PreviewEntrance()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("EnemyEntrancePresenter: enter Play Mode, then use Preview Entrance.");
                return;
            }

            PlayEntrance();
        }
#endif

        private IEnumerator PlayEntranceRoutine()
        {
            var slots = _combatStage.EnemySlots;
            if (slots == null)
            {
                FinishEntrance();
                yield break;
            }

            var activeCount = 0;
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null || !slot.gameObject.activeInHierarchy)
                    continue;

                StartCoroutine(AnimateSlot(slot, activeCount * _staggerSeconds));
                activeCount++;
            }

            if (activeCount > 0)
            {
                var totalWait = _duration + Mathf.Max(0, activeCount - 1) * _staggerSeconds;
                yield return new WaitForSeconds(totalWait);
            }

            FinishEntrance();
        }

        private void FinishEntrance()
        {
            _running = null;
            var complete = _onComplete;
            _onComplete = null;
            complete?.Invoke();
        }

        private IEnumerator AnimateSlot(Transform slot, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            var endLocalPos = slot.localPosition;
            var endLocalScale = slot.localScale;
            var startLocalPos = endLocalPos + new Vector3(0f, _startYOffset, 0f);
            var startLocalScale = endLocalScale * _startScaleMultiplier;

            var renderers = slot.GetComponentsInChildren<SpriteRenderer>(true);
            var endColors = new Color[renderers.Length];
            for (var i = 0; i < renderers.Length; i++)
                endColors[i] = renderers[i].color;

            slot.localPosition = startLocalPos;
            slot.localScale = startLocalScale;
            if (_fadeIn)
                SetRenderersAlpha(renderers, endColors, 0f);

            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, _duration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                // Ease-out so they settle naturally at the stop point.
                var eased = 1f - (1f - t) * (1f - t);

                slot.localPosition = Vector3.LerpUnclamped(startLocalPos, endLocalPos, eased);
                slot.localScale = Vector3.LerpUnclamped(startLocalScale, endLocalScale, eased);
                if (_fadeIn)
                    SetRenderersAlpha(renderers, endColors, eased);

                yield return null;
            }

            slot.localPosition = endLocalPos;
            slot.localScale = endLocalScale;
            if (_fadeIn)
                SetRenderersAlpha(renderers, endColors, 1f);
        }

        private static void SetRenderersAlpha(SpriteRenderer[] renderers, Color[] endColors, float alpha)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                var color = endColors[i];
                color.a = alpha;
                renderers[i].color = color;
            }
        }
    }
}