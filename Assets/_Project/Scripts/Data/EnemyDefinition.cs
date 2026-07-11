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
        [SerializeField] private GameObject _prefab;

        public string DisplayName => _displayName;

        public StatBlock Stats => _stats;

        public GameObject Prefab => _prefab;
    }
}