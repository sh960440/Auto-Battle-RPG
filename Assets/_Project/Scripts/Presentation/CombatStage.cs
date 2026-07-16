using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Combat stage placeholders: player hands/weapon and up to three enemy slots in front.
    /// </summary>
    public class CombatStage : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private Transform _playerHandsRoot;
        [SerializeField] private Transform[] _enemySlots = new Transform[3];

        [Header("Placeholder Sprites")]
        [SerializeField] private Sprite _handSprite;
        [SerializeField] private Sprite _weaponSprite;
        [SerializeField] private Sprite _enemySprite;

        public Transform PlayerHandsRoot => _playerHandsRoot;

        public Transform[] EnemySlots => _enemySlots;

        /// <summary>
        /// Returns the enemy slot at <paramref name="index"/>, or null if missing.
        /// </summary>
        public Transform GetEnemySlot(int index)
        {
            if (_enemySlots == null || index < 0 || index >= _enemySlots.Length)
                return null;

            return _enemySlots[index];
        }

        /// <summary>
        /// Shows or hides an enemy slot placeholder.
        /// </summary>
        public void SetEnemySlotActive(int index, bool active)
        {
            var slot = GetEnemySlot(index);
            if (slot != null)
                slot.gameObject.SetActive(active);
        }

#if UNITY_EDITOR
        [ContextMenu("Build 2D Placeholders")]
        private void BuildPlaceholders()
        {
            EnsureRoots();
            BuildHands();
            BuildEnemySlots();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void EnsureRoots()
        {
            if (_playerHandsRoot == null)
            {
                var hands = transform.Find("PlayerHands");
                if (hands == null)
                {
                    var go = new GameObject("PlayerHands");
                    go.transform.SetParent(transform, false);
                    hands = go.transform;
                }

                _playerHandsRoot = hands;
            }

            var slotsRoot = transform.Find("EnemySlots");
            if (slotsRoot == null)
            {
                var go = new GameObject("EnemySlots");
                go.transform.SetParent(transform, false);
                slotsRoot = go.transform;
            }

            if (_enemySlots == null || _enemySlots.Length != 3)
                _enemySlots = new Transform[3];
        }

        private void BuildHands()
        {
            _playerHandsRoot.localPosition = new Vector3(0f, -3.2f, 0f);

            CreateOrUpdateSpriteChild(_playerHandsRoot, "LeftHand", new Vector3(-1.4f, 0f, 0f), new Vector3(1.2f, 0.9f, 1f), _handSprite);
            CreateOrUpdateSpriteChild(_playerHandsRoot, "RightHand", new Vector3(1.4f, 0f, 0f), new Vector3(1.2f, 0.9f, 1f), _handSprite);
            CreateOrUpdateSpriteChild(_playerHandsRoot, "Weapon", new Vector3(1.8f, 0.6f, 0f), new Vector3(0.5f, 1.6f, 1f), _weaponSprite);
        }

        private void BuildEnemySlots()
        {
            var slotsRoot = transform.Find("EnemySlots");
            var positions = new[]
            {
                new Vector3(-2.5f, 1.2f, 0f),
                new Vector3(0f, 1.5f, 0f),
                new Vector3(2.5f, 1.2f, 0f)
            };

            for (var i = 0; i < 3; i++)
            {
                var name = $"EnemySlot{i + 1}";
                var slot = slotsRoot.Find(name);
                if (slot == null)
                {
                    var go = new GameObject(name);
                    go.transform.SetParent(slotsRoot, false);
                    slot = go.transform;
                }

                slot.localPosition = positions[i];
                CreateOrUpdateSpriteChild(slot, "Visual", Vector3.zero, new Vector3(1.8f, 2.2f, 1f), _enemySprite);
                _enemySlots[i] = slot;
            }
        }

        private static void CreateOrUpdateSpriteChild(
            Transform parent,
            string childName,
            Vector3 localPos,
            Vector3 localScale,
            Sprite sprite)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent, false);
                child = go.transform;
                go.AddComponent<SpriteRenderer>();
            }

            child.localPosition = localPos;
            child.localScale = localScale;

            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = child.gameObject.AddComponent<SpriteRenderer>();

            if (sprite != null)
                renderer.sprite = sprite;

            renderer.sortingOrder = childName.StartsWith("Enemy") || parent.name.StartsWith("Enemy") ? 5 : 10;
        }
#endif
    }
}