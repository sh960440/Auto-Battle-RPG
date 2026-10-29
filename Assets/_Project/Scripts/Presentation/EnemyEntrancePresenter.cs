using System;
using System.Collections;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Walks enemies up from behind the foreground, crests the horizon, then down to the combat stage.
    /// </summary>
    public class EnemyEntrancePresenter : MonoBehaviour
    {
        [SerializeField] private CombatStage _combatStage;
        [SerializeField] private Transform _foreground;
        [SerializeField] [Min(0.1f)] private float _duration = 3f;
        [SerializeField] private float _staggerSeconds = 0.12f;
        [SerializeField] [Range(0.05f, 1f)] private float _startScaleMultiplier = 0.22f;
        [SerializeField] [Range(0.1f, 1f)] private float _crestScaleMultiplier = 0.55f;
        [SerializeField] private float _behindDistance = 1.2f;
        [SerializeField] private float _walkSwayDegrees = 8f;
        [SerializeField] private float _walkSwayHz = 2.2f;
        [SerializeField] private int _behindSortingOrder = 0;
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

            StopAllCoroutines();
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

            var approach = CaptureAndHide(slot);
            StartCoroutine(AnimateSlot(approach, 0f));
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

                var approach = CaptureAndHide(slot);
                StartCoroutine(AnimateSlot(approach, activeCount * _staggerSeconds));
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

        private SlotApproach CaptureAndHide(Transform slot)
        {
            var visual = slot.Find("Visual");
            var renderers = slot.GetComponentsInChildren<SpriteRenderer>(true);
            var endColors = new Color[renderers.Length];
            var endOrders = new int[renderers.Length];
            for (var i = 0; i < renderers.Length; i++)
            {
                endColors[i] = renderers[i].color;
                endOrders[i] = renderers[i].sortingOrder;
            }

            var endWorld = slot.position;
            var endScale = slot.localScale;
            var horizonY = GetHorizonWorldY();
            var startWorld = new Vector3(endWorld.x, horizonY - _behindDistance, endWorld.z);
            var startScale = endScale * _startScaleMultiplier;

            SetSorting(renderers, _behindSortingOrder);
            SetRenderersAlpha(renderers, endColors, 0f);
            slot.position = startWorld;
            slot.localScale = startScale;
            if (visual != null)
                visual.localRotation = Quaternion.identity;

            return new SlotApproach
            {
                Slot = slot,
                Visual = visual,
                Renderers = renderers,
                EndColors = endColors,
                EndOrders = endOrders,
                EndWorld = endWorld,
                StartWorld = startWorld,
                CrestWorld = new Vector3(endWorld.x, horizonY, endWorld.z),
                EndScale = endScale,
                StartScale = startScale,
                CrestScale = endScale * _crestScaleMultiplier
            };
        }

        private IEnumerator AnimateSlot(SlotApproach approach, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            var duration = Mathf.Max(0.01f, _duration);
            var crestAt = duration * 0.5f;
            var elapsed = 0f;
            var broughtToFront = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var sway = Mathf.Sin(elapsed * Mathf.PI * 2f * _walkSwayHz) * _walkSwayDegrees;
                if (approach.Visual != null)
                    approach.Visual.localRotation = Quaternion.Euler(0f, 0f, sway);

                if (elapsed <= crestAt)
                {
                    var t = Smooth01(elapsed / crestAt);
                    approach.Slot.position = Vector3.LerpUnclamped(approach.StartWorld, approach.CrestWorld, t);
                    approach.Slot.localScale = Vector3.LerpUnclamped(approach.StartScale, approach.CrestScale, t);
                    SetRenderersAlpha(approach.Renderers, approach.EndColors, t);
                }
                else
                {
                    if (!broughtToFront)
                    {
                        SetSorting(approach.Renderers, approach.EndOrders);
                        SetRenderersAlpha(approach.Renderers, approach.EndColors, 1f);
                        broughtToFront = true;
                    }

                    var t = Smooth01((elapsed - crestAt) / (duration - crestAt));
                    approach.Slot.position = Vector3.LerpUnclamped(approach.CrestWorld, approach.EndWorld, t);
                    approach.Slot.localScale = Vector3.LerpUnclamped(approach.CrestScale, approach.EndScale, t);
                }

                yield return null;
            }

            approach.Slot.position = approach.EndWorld;
            approach.Slot.localScale = approach.EndScale;
            if (approach.Visual != null)
                approach.Visual.localRotation = Quaternion.identity;
            SetSorting(approach.Renderers, approach.EndOrders);
            SetRenderersAlpha(approach.Renderers, approach.EndColors, 1f);
        }

        private float GetHorizonWorldY()
        {
            if (_foreground == null)
                return 0f;

            var mask = _foreground.GetComponent<SpriteMask>();
            if (mask != null)
                return mask.bounds.max.y;

            return _foreground.position.y;
        }

        private static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        private static void SetSorting(SpriteRenderer[] renderers, int order)
        {
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].sortingOrder = order;
        }

        private static void SetSorting(SpriteRenderer[] renderers, int[] orders)
        {
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].sortingOrder = orders[i];
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

        private struct SlotApproach
        {
            public Transform Slot;
            public Transform Visual;
            public SpriteRenderer[] Renderers;
            public Color[] EndColors;
            public int[] EndOrders;
            public Vector3 EndWorld;
            public Vector3 StartWorld;
            public Vector3 CrestWorld;
            public Vector3 EndScale;
            public Vector3 StartScale;
            public Vector3 CrestScale;
        }
    }
}