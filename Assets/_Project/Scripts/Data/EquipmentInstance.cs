using System;

namespace Data
{
    /// <summary>
    /// Runtime equipment piece.
    /// </summary>
    public class EquipmentInstance
    {
        private readonly EquipmentDefinition _definition;
        private readonly int _level;

        /// <summary>
        /// Creates a runtime item from a definition and level.
        /// </summary>
        public EquipmentInstance(EquipmentDefinition definition, int level)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _level = Math.Max(1, level);
        }

        public EquipmentDefinition Definition => _definition;

        public int Level => _level;

        public string DisplayName => _definition.DisplayName;

        public EquipmentSlot Slot => _definition.Slot;

        public CharacterClass RequiredClass => _definition.RequiredClass;

        public EquipmentQuality Quality => _definition.Quality;

        public StatType MainStatType => _definition.MainStatType;

        /// <summary>
        /// Main stat value at this item's level.
        /// </summary>
        public int GetMainStat()
        {
            return _definition.GetMainStatAtLevel(_level);
        }
    }
}