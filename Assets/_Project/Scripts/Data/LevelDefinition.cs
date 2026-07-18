using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Display name and the enemy wave for one battle. (Initial version)
    /// </summary>
    [CreateAssetMenu(fileName = "Level_", menuName = "Game Specific/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private EnemyDefinition[] _enemies;

        public string DisplayName => _displayName;

        /// <summary>Enemy definitions for this level's single combat wave.</summary>
        public IReadOnlyList<EnemyDefinition> Enemies => _enemies;
    }
}