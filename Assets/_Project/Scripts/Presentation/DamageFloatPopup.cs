using System.Collections;
using TMPro;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// One floating damage number that appears above combat units.
    /// </summary>
    public class DamageFloatPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private float _riseDistance = 60f;
        [SerializeField] private float _duration = 0.7f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            if (_label == null)
                _label = GetComponent<TMP_Text>();
        }

        /// <summary>
        /// Shows <paramref name="damage"/>.
        /// </summary>
        public void Play(int damage)
        {
            if (_label != null)
                _label.text = damage.ToString();

            StopAllCoroutines();
            StartCoroutine(AnimateRoutine());
        }

        /// <summary>
        /// Creates a damage float popup under <paramref name="parent"/>.
        /// </summary>
        public static DamageFloatPopup CreateRuntime(RectTransform parent, Vector3 worldPosition, int damage)
        {
            var go = new GameObject("DamageFloat", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(120f, 40f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.85f, 0.2f, 1f);
            tmp.raycastTarget = false;

            var popup = go.AddComponent<DamageFloatPopup>();
            popup._label = tmp;
            popup._rectTransform = rect;

            rect.position = worldPosition;
            popup.Play(damage);
            return popup;
        }

        private IEnumerator AnimateRoutine()
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;

            var start = _rectTransform.anchoredPosition;
            var end = start + Vector2.up * _riseDistance;
            var startColor = _label != null ? _label.color : Color.white;
            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, _duration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, end, t);

                if (_label != null)
                {
                    var color = startColor;
                    color.a = 1f - t;
                    _label.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}