using System;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// Picks combat targets.
    /// </summary>
    public static class TargetSelector
    {
        private static readonly Random _defaultRandom = new();

        /// <summary>
        /// Picks one unit at random from <paramref name="candidates"/>.
        /// </summary>
        /// <param name="candidates">Living legal targets. Must not be empty.</param>
        /// <param name="random">Optional RNG for tests. Uses a shared default when null.</param>
        public static CombatUnit SelectRandom(IReadOnlyList<CombatUnit> candidates, Random random = null)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));
            if (candidates.Count == 0)
                throw new ArgumentException("No living targets available.", nameof(candidates));

            var rng = random ?? _defaultRandom;
            return candidates[rng.Next(candidates.Count)];
        }

        /// <summary>
        /// Picks one living opponent of <paramref name="attacker"/> at random.
        /// Returns null when no living opponents remain.
        /// </summary>
        public static CombatUnit SelectRandomOpponent(
            CombatContext context,
            CombatUnit attacker,
            List<CombatUnit> scratch,
            Random random = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker));
            if (scratch == null)
                throw new ArgumentNullException(nameof(scratch));

            context.GetLivingUnits(!attacker.IsPlayerSide, scratch);
            if (scratch.Count == 0)
                return null;

            return SelectRandom(scratch, random);
        }

        /// <summary>
        /// Fills <paramref name="results"/> with every living opponent of <paramref name="attacker"/>.
        /// Used by AOE skills.
        /// </summary>
        public static void SelectAllOpponents(CombatContext context, CombatUnit attacker, List<CombatUnit> results)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker));
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            context.GetLivingUnits(!attacker.IsPlayerSide, results);
        }
    }
}