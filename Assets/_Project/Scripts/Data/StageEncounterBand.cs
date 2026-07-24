using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// One enemy composition that can be rolled for a stage band.
    /// </summary>
    [Serializable]
    public class EnemyComposition
    {
        [SerializeField] private string _label;
        [SerializeField] private EnemyDefinition[] _enemies;

        public string Label => _label;

        public IReadOnlyList<EnemyDefinition> Enemies => _enemies;
    }

    /// <summary>
    /// Stage range that shares one pool of enemy compositions.
    /// </summary>
    [Serializable]
    public class StageEncounterBand
    {
        [SerializeField] private int _minStage = 1;
        [SerializeField] private int _maxStage = 30;
        [SerializeField] private bool _isOpenEnded;
        [SerializeField] private EnemyComposition[] _compositions;

        public int MinStage => _minStage;

        /// <summary>
        /// Inclusive max stage. Ignored when <see cref="IsOpenEnded"/> is true.
        /// </summary>
        public int MaxStage => _maxStage;

        /// <summary>
        /// When true, this band covers every stage at or above <see cref="MinStage"/>.
        /// </summary>
        public bool IsOpenEnded => _isOpenEnded;

        public IReadOnlyList<EnemyComposition> Compositions => _compositions;

        /// <summary>
        /// Returns whether the given stage falls inside this band.
        /// </summary>
        public bool Contains(int stage)
        {
            if (stage < _minStage)
                return false;

            return _isOpenEnded || stage <= _maxStage;
        }
    }
}