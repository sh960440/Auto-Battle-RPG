using System;

namespace Data
{
    /// <summary>
    /// Primary combat identity for each class.
    /// </summary>
    public static class CharacterClassRules
    {
        /// <summary>
        /// Main stat the class is built around.
        /// </summary>
        public static StatType GetPrimaryStat(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Knight => StatType.HP,
                CharacterClass.Swordsman => StatType.Attack,
                CharacterClass.ShieldGuard => StatType.Defense,
                _ => throw new ArgumentOutOfRangeException(nameof(characterClass), characterClass, null)
            };
        }

        /// <summary>
        /// Short UI label for the class.
        /// </summary>
        public static string GetDisplayName(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Knight => "Knight",
                CharacterClass.Swordsman => "Swordsman",
                CharacterClass.ShieldGuard => "Shield Guard",
                _ => characterClass.ToString()
            };
        }
    }
}