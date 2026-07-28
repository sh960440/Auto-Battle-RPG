using System;

namespace Data
{
    /// <summary>
    /// Runtime character.
    /// </summary>
    public class CharacterInstance
    {
        private readonly CharacterDefinition _definition;
        private readonly EquipmentInstance[] _equipped;
        private int _level;

        /// <summary>
        /// Creates a character from a definition.
        /// </summary>
        public CharacterInstance(CharacterDefinition definition, int level = 1)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _level = Math.Max(1, level);
            _equipped = new EquipmentInstance[Enum.GetValues(typeof(EquipmentSlot)).Length];
        }

        public CharacterDefinition Definition => _definition;

        public string DisplayName => _definition.DisplayName;

        public CharacterClass CharacterClass => _definition.CharacterClass;

        public int Level => _level;

        public SkillDefinition DefaultSkill => _definition.DefaultSkill;

        public StatBlock BaseStats => _definition.BaseStats;

        public void SetLevel(int level) => _level = Math.Max(1, level);
        
        public EquipmentInstance GetEquipment(EquipmentSlot slot) => _equipped[ToIndex(slot)];

        /// <summary>
        /// Tries to read the item in the slot.
        /// </summary>
        public bool TryGetEquipment(EquipmentSlot slot, out EquipmentInstance item)
        {
            item = GetEquipment(slot);
            return item != null;
        }

        /// <summary>
        /// Overwrites the matching slot with <paramref name="item"/>.
        /// Returns the previous item, or null if the slot was empty.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when class or slot does not match.</exception>
        public EquipmentInstance Equip(EquipmentInstance item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!item.Definition.CanBeEquippedBy(CharacterClass))
            {
                throw new InvalidOperationException(
                    $"Cannot equip {item.DisplayName}: requires {item.RequiredClass}, character is {CharacterClass}.");
            }

            var index = ToIndex(item.Slot);
            var previous = _equipped[index];
            _equipped[index] = item;
            return previous;
        }

        // Hooks for future features.
        // Unequip is intentionally omitted: gear can only be replaced by Equip.

        private static int ToIndex(EquipmentSlot slot)
        {
            return (int)slot;
        }
    }
}