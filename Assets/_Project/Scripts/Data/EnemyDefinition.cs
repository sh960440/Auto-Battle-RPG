using UnityEngine;

namespace Data
{
    /// <summary>
    /// Static enemy template.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_", menuName = "Game Specific/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private StatBlock _stats;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private AudioClip _attackSfx;

        public string DisplayName => _displayName;

        public StatBlock Stats => _stats;

        public Sprite Sprite => _sprite;

        public GameObject Prefab => _prefab;

        public AudioClip AttackSfx => _attackSfx;
    }
}