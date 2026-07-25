using System;

namespace Combat
{
    /// <summary>
    /// Pure stage progression rules.
    /// </summary>
    public static class StageProgressRules
    {
        /// <summary>
        /// Returns the next stage after combat.
        /// </summary>
        public static int GetNextStage(int currentStage, CombatResult result)
        {
            var stage = Math.Max(1, currentStage);

            if (result == CombatResult.Victory)
                return stage + 1;

            return Math.Max(1, stage - 3);
        }
    }
}