using System;

namespace Data
{
    /// <summary>
    /// Maps stage difficulty to equipment level at obtain time (Initial implementation).
    /// </summary>
    public static class EquipmentLevelRules
    {
        public const int MinLevel = 1;

        /// <summary>
        /// Soft cap matching EquipmentGrowthCurve tier-2 ceiling.
        /// </summary>
        public const int MaxLevel = 20;

        /// <summary>
        /// Equipment level when obtained at the given stage.
        /// Initial implementation: level tracks stage 1:1, clamped to <see cref="MinLevel"/>..<see cref="MaxLevel"/>.
        /// </summary>
        public static int GetLevelForStage(int stage)
        {
            var clampedStage = Math.Max(MinLevel, stage);
            return Math.Min(MaxLevel, clampedStage);
        }
    }
}