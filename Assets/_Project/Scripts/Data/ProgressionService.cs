using System;

namespace Data
{
    /// <summary>
    /// Spends gold to raise a character's level using its upgrade curve.
    /// </summary>
    public class ProgressionService
    {
        /// <summary>
        /// Gold needed to raise <paramref name="character"/> by one level, or 0 if blocked.
        /// </summary>
        public int GetNextLevelCost(CharacterInstance character)
        {
            if (character == null)
                return 0;

            var curve = character.Definition.UpgradeCurve;
            if (curve == null || !curve.CanUpgrade(character.Level))
                return 0;

            return curve.GetGoldCostForNextLevel(character.Level);
        }

        /// <summary>
        /// Whether the character can be upgraded with the current gold.
        /// </summary>
        public bool CanUpgrade(PlayerProfile profile, CharacterInstance character)
        {
            if (profile == null || character == null)
                return false;

            var cost = GetNextLevelCost(character);
            return cost > 0 && profile.Gold >= cost;
        }

        /// <summary>
        /// Spends gold and raises the character's level by one.
        /// </summary>
        public bool TryUpgrade(PlayerProfile profile, CharacterInstance character)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            var cost = GetNextLevelCost(character);
            if (cost <= 0)
                return false;

            if (!profile.TrySpendGold(cost))
                return false;

            character.SetLevel(character.Level + 1);
            return true;
        }

        /// <summary>
        /// Final stats if the character were one level higher (does not spend gold).
        /// </summary>
        public StatBlock PreviewStatsAfterUpgrade(CharacterInstance character)
        {
            if (character == null)
                return StatBlock.Zero;

            var current = StatCalculator.Calculate(character);
            var curve = character.Definition.UpgradeCurve;
            if (curve == null || !curve.CanUpgrade(character.Level))
                return current;

            var saved = character.Level;
            character.SetLevel(saved + 1);
            var preview = StatCalculator.Calculate(character);
            character.SetLevel(saved);
            return preview;
        }
    }
}
