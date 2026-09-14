using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Scales enemy base stats by stage. Linear, not compound.
    /// Speed stops growing after <see cref="SpeedScaleCapStage"/> so ATB stays readable.
    /// </summary>
    public static class EnemyStageScaling
    {
        public const float HpPerStage = 0.06f;
        public const float AttackPerStage = 0.05f;
        public const float DefensePerStage = 0.05f;
        public const float SpeedPerStage = 0.02f;

        /// <summary>
        /// Speed uses this stage for every fight beyond it.
        /// </summary>
        public const int SpeedScaleCapStage = 30;

        /// <summary>
        /// Returns a copy of <paramref name="baseStats"/> grown for <paramref name="stage"/>.
        /// Stage 1 is unchanged.
        /// </summary>
        public static StatBlock Scale(StatBlock baseStats, int stage)
        {
            var safeStage = Math.Max(1, stage);
            return new StatBlock
            {
                HP = ScaleStat(baseStats.HP, safeStage, HpPerStage),
                Attack = ScaleStat(baseStats.Attack, safeStage, AttackPerStage),
                Defense = ScaleStat(baseStats.Defense, safeStage, DefensePerStage),
                Speed = ScaleStat(baseStats.Speed, Math.Min(safeStage, SpeedScaleCapStage), SpeedPerStage)
            };
        }

        private static int ScaleStat(int baseValue, int stage, float perStage)
        {
            if (baseValue <= 0)
                return baseValue;

            var multiplier = 1f + (stage - 1) * perStage;
            return Math.Max(1, Mathf.RoundToInt(baseValue * multiplier));
        }
    }
}
