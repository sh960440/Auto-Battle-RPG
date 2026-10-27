using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Endslessly scrolls two stacked copies of a sprite downward.
    /// </summary>
    public class VerticalSpriteScroller : MonoBehaviour
    {
        [SerializeField] private Transform _grass1;
        [SerializeField] private Transform _grass2;
        [SerializeField] [Min(0f)] private float _speed = 1.5f;

        private float _startY;
        private bool _scrolling;

        private void Awake()
        {
            _startY = _grass2 != null ? _grass2.localPosition.y : 0f;
        }

        private void Update()
        {
            if (!_scrolling || _speed <= 0f || _grass1 == null || _grass2 == null)
                return;

            var delta = _speed * Time.deltaTime;
            MoveDown(_grass1, delta);
            MoveDown(_grass2, delta);

            var upper = _grass1.localPosition.y >= _grass2.localPosition.y ? _grass1 : _grass2;
            var lower = upper == _grass1 ? _grass2 : _grass1;
            if (upper.localPosition.y <= 0f)
            {
                var position = lower.localPosition;
                position.y = _startY;
                lower.localPosition = position;
            }
        }

        /// <summary>
        /// Starts or stops scrolling.
        /// </summary>
        public void SetScrolling(bool scrolling) => _scrolling = scrolling;

        private static void MoveDown(Transform strip, float delta)
        {
            var position = strip.localPosition;
            position.y -= delta;
            strip.localPosition = position;
        }
    }
}