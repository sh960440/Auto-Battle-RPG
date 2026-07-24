using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Maps stage ranges to enemy composition pools.
    /// </summary>
    [CreateAssetMenu(fileName = "StageEncounterTable_", menuName = "Game Specific/Stage Encounter Table")]
    public class StageEncounterTable : ScriptableObject
    {
        [SerializeField] private StageEncounterBand[] _bands;

        /// <summary>
        /// Configured stage bands, ordered as authored.
        /// </summary>
        public IReadOnlyList<StageEncounterBand> Bands => _bands;

        /// <summary>
        /// Picks a random non-empty enemy list for the given stage.
        /// Falls back to the last band when the stage is past all closed ranges.
        /// </summary>
        public List<EnemyDefinition> RollEnemies(int stage, System.Random random = null)
        {
            random ??= new System.Random();
            var composition = RollComposition(stage, random);
            var result = new List<EnemyDefinition>();

            if (composition?.Enemies == null)
                return result;

            for (var i = 0; i < composition.Enemies.Count; i++)
            {
                if (composition.Enemies[i] != null)
                    result.Add(composition.Enemies[i]);
            }

            return result;
        }

        /// <summary>
        /// Picks a random composition for the given stage.
        /// </summary>
        public EnemyComposition RollComposition(int stage, System.Random random = null)
        {
            random ??= new System.Random();
            var band = FindBand(stage);
            if (band?.Compositions == null || band.Compositions.Count == 0)
                return null;

            var valid = new List<EnemyComposition>();
            for (var i = 0; i < band.Compositions.Count; i++)
            {
                var composition = band.Compositions[i];
                if (composition?.Enemies == null || composition.Enemies.Count == 0)
                    continue;

                var hasEnemy = false;
                for (var e = 0; e < composition.Enemies.Count; e++)
                {
                    if (composition.Enemies[e] != null)
                    {
                        hasEnemy = true;
                        break;
                    }
                }

                if (hasEnemy)
                    valid.Add(composition);
            }

            if (valid.Count == 0)
                return null;

            return valid[random.Next(valid.Count)];
        }

        private StageEncounterBand FindBand(int stage)
        {
            if (_bands == null || _bands.Length == 0)
                return null;

            StageEncounterBand last = null;
            for (var i = 0; i < _bands.Length; i++)
            {
                var band = _bands[i];
                if (band == null)
                    continue;

                last = band;
                if (band.Contains(stage))
                    return band;
            }

            // Past every closed band: keep rotating the final pool.
            return last;
        }
    }
}